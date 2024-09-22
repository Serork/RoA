using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.GlowMasks;
using RoA.Content.Dusts;
using RoA.Core;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

[AutoloadGlowMask]
sealed class RodOfTheCondor : ModItem {
    private static CondorWingsHandler GetHandler(Player player) => player.GetModPlayer<CondorWingsHandler>();

    private static Color LightingColor => new(42, 148, 194);

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 42; int height = width;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 24;
        Item.autoReuse = false;

        Item.noMelee = true;

        Item.mana = 17;

        Item.value = Item.buyPrice(gold: 1, silver: 10);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item1;

        Item.staff[Item.type] = true;
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame) {
        if (!Main.dedServ) {
            Lighting.AddLight(player.itemLocation + Utils.SafeNormalize(player.itemLocation.DirectionFrom(player.Center), Vector2.Zero) * Item.width, LightingColor.ToVector3() * 0.75f);
        }
    }

    public override void PostUpdate() {
        if (!Main.dedServ) {
            Lighting.AddLight(Item.getRect().TopRight(), LightingColor.ToVector3() * 0.75f);
        }
    }

    public override bool CanUseItem(Player player) => !GetHandler(player).IsActive;

    public override bool? UseItem(Player player) {
        GetHandler(player).ActivateCondor();

        return base.UseItem(player);
    }

    private class CondorWingsHandler : ModPlayer {
        private const float MAXWINGSTIME = 3f;

        private bool _active;
        private float _wingsTime;

        public float Opacity {
            get {
                float value = Utils.GetLerpValue(MAXWINGSTIME, AnimationBound, _wingsTime, true);
                if (_wingsTime < 0f) {
                    float wingsTime2 = Math.Abs(_wingsTime);
                    float value3 = -AnimationBound;
                    float value2 = -(1f - Utils.GetLerpValue(0f, value3, value3 - wingsTime2)) * 10f;
                    value *= value2;
                }
                float opacity = (float)Math.Pow(value, 0.5f);
                return opacity;
            }
        }

        public bool IsInUse => _wingsTime != 0f;
        public bool IsActive => IsInUse || _active;

        private static int _wingsSlot = -1;

        private static string WingsTextureName => ResourceManager.ItemsTextures + $"{nameof(RodOfTheCondor)}_Wings";
        private static string WingsLayerName => $"{nameof(RodOfTheCondor)}_Wings";

        private static float AnimationBound => MAXWINGSTIME - 0.25f;
        private static float AnimationBound2 => -0.15f;

        public override void PostUpdate() {
            if (IsActive) {
                Lighting.AddLight(Player.Center, LightingColor.ToVector3() * 0.75f * Opacity);

                bool flag3 = Player.CanVisuallyHoldItem(Player.HeldItem);
                bool flag4 = false;
                if (flag3 && Player.inventory[Player.selectedItem].holdStyle == 1 && (!Player.wet || !Player.inventory[Player.selectedItem].noWet) && (!Player.happyFunTorchTime || Player.inventory[Player.selectedItem].createTile != 4)) {
                    flag4 = true;
                }
                else if (flag3 && Player.inventory[Player.selectedItem].holdStyle == 2 && (!Player.wet || !Player.inventory[Player.selectedItem].noWet)) {
                    flag4 = true;
                }
                else if (flag3 && Player.inventory[Player.selectedItem].holdStyle == 3) {
                    flag4 = true;
                }
                else if (flag3 && Player.inventory[Player.selectedItem].holdStyle == 5) {
                    flag4 = true;
                }
                else if (flag3 && Player.inventory[Player.selectedItem].holdStyle == 7) {
                    flag4 = true;
                }
                else if (flag3 && Player.inventory[Player.selectedItem].holdStyle == 4 && Player.velocity.Y == 0f && Player.gravDir == 1f) {
                    flag4 = true;
                }
                bool flag = !flag4 && !Player.sandStorm && Player.swimTime <= 0 && Player.itemAnimation <= 0 && !Player.pulley &&
                            !Player.shieldRaised && !Player.mount.Active && Player.grappling[0] <= 0 && !(Player.wet && Player.ShouldFloatInWater);
                if (flag && Player.velocity.Y != 0f) {
                    if (Player.velocity.Y > 0f) {
                        if (Player.controlJump) {
                            Player.bodyFrame.Y = Player.bodyFrame.Height * 6;
                        }
                        else {
                            Player.bodyFrame.Y = Player.bodyFrame.Height * 5;
                        }
                    }
                    else {
                        Player.bodyFrame.Y = Player.bodyFrame.Height * 6;
                    }
                }
            }
        }

        public override void PostUpdateRunSpeeds() {
            if (_active && Player.velocity.Y == 0f && Player.wingTime < (int)(Player.wingTimeMax * 0.95f)) {
                _active = false;
                _wingsTime = AnimationBound2;
            }

            if (IsInUse) {
                if (!_active && Player.velocity.Y < 0f && Player.controlJump) {
                    _active = true;
                }
            }
        }

        public override void PostUpdateEquips() {
            if ((IsInUse && _active) || _wingsTime > AnimationBound) {
                if (_wingsTime < TimeSystem.LogicDeltaTime) {
                    _wingsTime = 0f;
                }
                else {
                    _wingsTime -= TimeSystem.LogicDeltaTime;
                }
            }
            if (_wingsTime < 0f) {
                if (_wingsTime >= -TimeSystem.LogicDeltaTime) {
                    _wingsTime = 0f;
                }
                else {
                    _wingsTime += TimeSystem.LogicDeltaTime;
                }
            }

            HandleCondorWings();
        }

        public override void FrameEffects() => HandleCondorWings();

        public override void SetStaticDefaults() => ArmorIDs.Wing.Sets.Stats[_wingsSlot] = new WingStats(50, 5f, 0.75f);

        public override void Load() => _wingsSlot = EquipLoader.AddEquipTexture(Mod, WingsTextureName, EquipType.Wings, name: WingsLayerName);

        public void ActivateCondor() {
            _wingsTime = MAXWINGSTIME;
            HandleCondorWings();
            Player.wingTime = Player.wingTimeMax;
        }

        private void HandleCondorWings() {
            if (IsInUse || _active) {
                Player.noFallDmg = true;
                Player.wings = -1;
                Player.wingTimeMax = Player.GetWingStats(_wingsSlot).FlyTime;
                Player.wingsLogic = _wingsSlot;
            }
        }

        private class CondorWingsPlayerLayer : PlayerDrawLayer {
            private static Asset<Texture2D> _wingsTexture;

            public override void SetStaticDefaults() {
                if (Main.dedServ) {
                    return;
                }

                _wingsTexture = ModContent.Request<Texture2D>(WingsTextureName);
            }

            public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Wings);

            public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => GetHandler(drawInfo.drawPlayer).IsActive;

            protected override void Draw(ref PlayerDrawSet drawInfo) {
                if (drawInfo.drawPlayer.dead) {
                    return;
                }

                var handler = GetHandler(drawInfo.drawPlayer);

                int num58 = 13;
                if (drawInfo.drawPlayer.direction == 1)
                    num58 = -42;

                if (Main.rand.NextBool(drawInfo.drawPlayer.velocity.Y != 0f ? 4 : 14)) {
                    int num59 = Dust.NewDust(new Vector2(drawInfo.drawPlayer.position.X + (float)(drawInfo.drawPlayer.width / 2) + (float)num58, drawInfo.drawPlayer.position.Y + 10), 26, drawInfo.drawPlayer.height / 2 + 4, ModContent.DustType<CondorDust>(), 0f, 0f, (int)(255 * handler.Opacity), default, 1.2f);
                    Main.dust[num59].velocity *= 0.15f;
                    Main.dust[num59].noLightEmittence = true;

                    num58 = 34;
                    if (drawInfo.drawPlayer.direction == 1)
                        num58 = -12;

                    num59 = Dust.NewDust(new Vector2(drawInfo.drawPlayer.position.X + (float)(drawInfo.drawPlayer.width / 2) - (float)num58, drawInfo.drawPlayer.position.Y + 10), 17, drawInfo.drawPlayer.height / 2 + 4, ModContent.DustType<CondorDust>(), 0f, 0f, (int)(255 * handler.Opacity), default, 1.2f);
                    Main.dust[num59].velocity *= 0.15f;
                    Main.dust[num59].noLightEmittence = true;
                }

                Vector2 directions = drawInfo.drawPlayer.Directions;
                Vector2 vector = drawInfo.Position - Main.screenPosition + drawInfo.drawPlayer.Size / 2f;
                Vector2 vector2 = new(0f, 7f);
                vector = drawInfo.Position - Main.screenPosition + new Vector2(drawInfo.drawPlayer.width / 2, drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height / 2) + vector2;
                float wingsTime = handler._wingsTime;
                Color colorArmorBody3 = Color.White * drawInfo.stealth * (1f - drawInfo.shadow) * handler.Opacity;
                Vector2 vector11 = new(-6f, -7f);
                Texture2D value4 = _wingsTexture.Value;
                Vector2 vec5 = vector + vector11 * directions;
                Rectangle rectangle4 = value4.Frame(1, 4, 0, drawInfo.drawPlayer.velocity.Y == 0f ? 1 : drawInfo.drawPlayer.wingFrame);
                DrawData item = new(value4, vec5.Floor(), rectangle4, colorArmorBody3, drawInfo.drawPlayer.bodyRotation, rectangle4.Size() / 2f, 1f, drawInfo.playerEffect);
                drawInfo.DrawDataCache.Add(item);
            }
        }
    }
}