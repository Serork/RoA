using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.GlowMasks;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

[AutoloadGlowMask(shouldApplyItemAlpha: true)]
sealed class RodOfTheCondor : ModItem {
    private static CondorWingsHandler GetHandler(Player player) => player.GetModPlayer<CondorWingsHandler>();

    private static Color LightingColor => new(42, 148, 194);

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        //ItemSwapSystem.SwapToOnRightClick[Type] = (ushort)ModContent.ItemType<SphereOfCondor>();

        Item.staff[Item.type] = true;
    }

    public override void SetDefaults() {
        int width = 42; int height = 42;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 24;
        Item.autoReuse = false;

        Item.noMelee = true;

        Item.mana = 17;

        Item.value = Item.sellPrice(0, 3, 50, 0);
        Item.rare = ItemRarityID.Orange;
        //Item.UseSound = new SoundStyle(ResourceManager.ItemSounds + "WhatTheFuckIsAKilometer") { Volume = 0.9f };
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

    public override void MeleeEffects(Player player, Rectangle hitbox) {
    }

    private static void GetPointOnSwungItemPath(Player player, float spriteWidth, float spriteHeight, float normalizedPointOnPath, float itemScale, out Vector2 location, out Vector2 outwardDirection) {
        float num = (float)Math.Sqrt(spriteWidth * spriteWidth + spriteHeight * spriteHeight);
        float num2 = (float)(player.direction == 1).ToInt() * ((float)Math.PI / 2f);
        if (player.gravDir == -1f)
            num2 += (float)Math.PI / 2f * (float)player.direction;

        outwardDirection = player.itemRotation.ToRotationVector2().RotatedBy(3.926991f + num2);
        location = player.RotatedRelativePoint(player.itemLocation + outwardDirection * num * normalizedPointOnPath * itemScale);
    }

    public override bool CanUseItem(Player player) => player.whoAmI == Main.myPlayer && !GetHandler(player).IsActive;

    public override bool? UseItem(Player player) {
        if (player.ItemAnimationJustStarted) {
            GetHandler(player).ActivateCondor();
        }

        for (int i = 0; i < 2; i++) {
            GetPointOnSwungItemPath(player, 40f, 40f, 0.6f + 0.4f * Main.rand.NextFloat(), 1f, out var location, out var outwardDirection);
            Vector2 vector = outwardDirection.RotatedBy((float)Math.PI / 2f * (float)player.direction * player.gravDir);
            Dust dust = Dust.NewDustPerfect(location, ModContent.DustType<CondorDust>(), vector * 4f, 255, default(Color), 1.2f);
            dust.noGravity = true;
            dust.noLightEmittence = true;
        }

        return base.UseItem(player);
    }

    internal class CondorWingsHandler : ModPlayer {
        private bool _active;
        private float _opacity;
        private Vector2 _mousePosition;
        internal int _wingFrameCounter, _wingFrame;

        public float Opacity {
            get {
                float value = _opacity;
                float opacity = (float)Math.Pow(value, 0.5f);
                return opacity;
            }
        }

        public bool IsActive => _active;

        private static int _wingsSlot = -1;

        private static string WingsTextureName => ResourceManager.ItemTextures + $"{nameof(RodOfTheCondor)}_Wings";
        private static string WingsLayerName => $"{nameof(RodOfTheCondor)}_Wings";

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
        }

        public override void PostUpdateEquips() {
            if (!IsActive) {
                if (_opacity > 0f) {
                    _opacity -= TimeSystem.LogicDeltaTime * 2.5f;
                }
            }
            else {
                if (_opacity < 1f) {
                    _opacity += TimeSystem.LogicDeltaTime * 2.5f;
                }
            }

            if (!IsActive) {
                return;
            }

            bool flag22 = _wingFrame == 3;
            if (flag22) {
                if (!Player.flapSound)
                    SoundEngine.PlaySound(SoundID.Item32, Player.position);

                Player.flapSound = true;
            }
            else {
                Player.flapSound = false;
            }
            if (Player.ItemAnimationActive) {
                if (Player.velocity.Length() > 1f) {
                    int num27 = 4;
                    int num28 = 4;
                    int num29 = 0;
                    _wingFrameCounter++;
                    if (_wingFrameCounter > num27) {
                        _wingFrame++;
                        _wingFrameCounter = 0;
                        if (_wingFrame >= num28)
                            _wingFrame = num29;
                    }
                }
                Player.blockExtraJumps = true;
                Player.suffocating = false;
                Player.suffocateDelay = 0;
                Player.jump = 0;
                Player.runAcceleration = 0f;
                Player.runSlowdown = 0f;
                Player.maxRunSpeed = 0f;
                Player.pulley = false;
                Player.gfxOffY = 0f;
                Player.controlJump = true;
                Player.fallStart = (int)(Player.position.Y / 16f);
                Player.ChangeDir(-(Player.position - _mousePosition).X.GetDirection());
                Player.velocity += Helper.VelocityToPoint(Player.Center, _mousePosition, 1f * Ease.CircOut(_opacity));
                float max = 7.5f;
                Player.velocity.X = MathHelper.Clamp(Player.velocity.X, -max, max);
                max = 10f;
                Player.velocity.Y = MathHelper.Clamp(Player.velocity.Y, -max, max);
            }
            else {
                _active = false;
                _wingFrame = _wingFrameCounter = 0;
            }

            HandleCondorWings();
        }

        public override void FrameEffects() => HandleCondorWings();

        public override void SetStaticDefaults() => ArmorIDs.Wing.Sets.Stats[_wingsSlot] = new WingStats(60, 5f, 0.8f);

        public override void Load() {
            if (Main.dedServ) {
                return;
            }

            _wingsSlot = EquipLoader.AddEquipTexture(Mod, WingsTextureName, EquipType.Wings, name: WingsLayerName);
        }

        public void ActivateCondor() {
            _active = true;
            Player.velocity *= 0.8f;
            HandleCondorWings();

            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "WhatTheFuckIsAKilometer") { Volume = 0.9f }, Player.Center);

            if (Player.whoAmI == Main.myPlayer) {
                _mousePosition = Player.GetViableMousePosition();
            }

            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new ItemAnimationPacket(Player, Player.GetSelectedItem().useAnimation));
                MultiplayerSystem.SendPacket(new CondorPacket(Player, _active, _mousePosition));
            }

            Player.velocity += Helper.VelocityToPoint(Player.Center, _mousePosition, 1f);
        }

        internal void ReceivePacket(bool active, Vector2 mousePosition, Player player) {
            _active = active;
            _mousePosition = mousePosition;

            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "WhatTheFuckIsAKilometer") { Volume = 0.9f }, player.Center);
        }

        private void HandleCondorWings() {
            if (_active) {
                Player.noFallDmg = true;
                Player.wings = -1;
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
                if (drawInfo.hideEntirePlayer) {
                    return;
                }

                if (drawInfo.drawPlayer.dead) {
                    return;
                }

                var handler = GetHandler(drawInfo.drawPlayer);

                if (Main.rand.NextBool(drawInfo.drawPlayer.velocity.Y != 0f ? 4 : 14)) {
                    int num58 = 12;
                    if (drawInfo.drawPlayer.direction == 1)
                        num58 = -42;

                    int num59 = Dust.NewDust(new Vector2(drawInfo.drawPlayer.position.X + (float)(drawInfo.drawPlayer.width / 2) + (float)num58, drawInfo.drawPlayer.position.Y + 2), 26, drawInfo.drawPlayer.height / 2 + 4, ModContent.DustType<CondorDust>(), 0f, 0f, (int)(255 * handler.Opacity), default, 1.2f);
                    Main.dust[num59].velocity *= 0.15f;
                    Main.dust[num59].noLightEmittence = true;
                    drawInfo.DustCache.Add(Main.dust[num59].dustIndex);

                    num58 = 34;
                    if (drawInfo.drawPlayer.direction == 1)
                        num58 = -12;

                    num59 = Dust.NewDust(new Vector2(drawInfo.drawPlayer.position.X + (float)(drawInfo.drawPlayer.width / 2) - (float)num58, drawInfo.drawPlayer.position.Y + 2), 17, drawInfo.drawPlayer.height / 2 + 4, ModContent.DustType<CondorDust>(), 0f, 0f, (int)(255 * handler.Opacity), default, 1.2f);
                    Main.dust[num59].velocity *= 0.15f;
                    Main.dust[num59].noLightEmittence = true;
                    drawInfo.DustCache.Add(Main.dust[num59].dustIndex);
                }

                Vector2 directions = drawInfo.drawPlayer.Directions;
                Vector2 vector = drawInfo.Position - Main.screenPosition + drawInfo.drawPlayer.Size / 2f;
                Vector2 vector2 = new(0f, 7f);
                vector = drawInfo.Position - Main.screenPosition + new Vector2(drawInfo.drawPlayer.width / 2, drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height / 2) + vector2;
                Color colorArmorBody3 = Color.White * drawInfo.stealth * (1f - drawInfo.shadow) * handler.Opacity;
                Vector2 vector11 = new(-6f, -7f);
                Texture2D value4 = _wingsTexture.Value;
                Vector2 vec5 = vector + vector11 * directions;
                Rectangle rectangle4 = value4.Frame(1, 4, 0, drawInfo.drawPlayer.GetModPlayer<CondorWingsHandler>()._wingFrame);
                DrawData item = new(value4, vec5.Floor(), rectangle4, colorArmorBody3, drawInfo.drawPlayer.bodyRotation, rectangle4.Size() / 2f, 1f, drawInfo.playerEffect);
                drawInfo.DrawDataCache.Add(item);
            }
        }
    }
}