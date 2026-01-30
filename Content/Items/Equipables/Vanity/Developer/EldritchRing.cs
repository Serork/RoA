using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

sealed class EldritchRing : ModItem {
    private class EldritchRingWingsLogic : ModPlayer {
        public bool IsWingsActive;

        private bool flapSound;

        public override void ResetEffects() {
            IsWingsActive = false;
        }

        public override void FrameEffects() {
            if (IsWingsActive) {
                Player.wingTimeMax = 150;

                if ((Player.IsGrounded() && Player.releaseJump) || (Player.autoJump && Player.justJumped)) {
                    Player.wingTime = Player.wingTimeMax;
                }
            }

            bool flag24 = false;
            for (int i = 13; i < 20; i++) {
                if (!Player.armor[i].IsEmpty() && Player.armor[i].wingSlot > 0) {
                    flag24 = true;
                }
            }

            bool flag23 = Player.wingsLogic == _wingsSlot;
            if (flag23 && !flag24) {
                Player.flapSound = true;

                bool flag22 = Player.wingFrame == 3 && Player.wingTime != Player.wingTimeMax;
                if (flag22) {
                    if (!flapSound)
                        SoundEngine.PlaySound(SoundID.Item24, Player.position);

                    flapSound = true;
                }
                else {
                    flapSound = false;
                }
            }

            bool flag21 = false;
            if (Player.controlJump && Player.wingTime != Player.wingTimeMax && Player.jump == 0 && Player.velocity.Y != 0f)
                flag21 = true;

            if (flag21 && flag23) {
                if (Main.rand.NextBool(2)) {
                    int num = 4;
                    if (Player.direction == 1)
                        num = -40;
                    int num2 = Dust.NewDust(new Vector2(Player.position.X + (float)(Player.width / 4) + (float)num / 2 - num / 8,
                        Player.position.Y + (float)(Player.height / 2)), 30, 30,
                        ModContent.DustType<SnowDust2>(), 0f, 0f, 50, new(255, 215, 50, 180), 0.6f);
                    Main.dust[num2].fadeIn = 1.1f;
                    Main.dust[num2].noGravity = true;
                    Main.dust[num2].noLight = true;
                    Main.dust[num2].velocity *= 0.3f;
                    Main.dust[num2].shader = GameShaders.Armor.GetSecondaryShader(Player.cWings, Player);
                }
            }
        }

        public override void PostUpdate() {
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


    private static int _wingsSlot = -1;

    private static string WingsTextureName => ResourceManager.EmptyTexture;
    private static string WingsLayerName => $"{nameof(EldritchRing)}_Wings";

    public override void Load() {
        _wingsSlot = EquipLoader.AddEquipTexture(Mod, WingsTextureName, EquipType.Wings, name: WingsLayerName);
    }

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        ArmorIDs.Wing.Sets.Stats[_wingsSlot] = new WingStats(150, 7f);
    }

    public override void SetDefaults() {
        int width = 30; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.sellPrice(gold: 5);

        Item.accessory = true;

        //Item.vanity = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<EldritchRingWingsLogic>().IsWingsActive = true;

        player.noFallDmg = true;
        player.wings = -1;
        player.wingsLogic = _wingsSlot;

        player.GetModPlayer<EldritchRingDrawLayer.EldritchRingHandler>().shouldDraw = !hideVisual;
    }

    public override void UpdateVanity(Player player) {
        player.GetModPlayer<EldritchRingDrawLayer.EldritchRingHandler>().shouldDraw = true;
    }

    private class EldritchRingDrawLayer : ILoadable {
        private static Asset<Texture2D> _yellowSignRune = null!;

        internal class EldritchRingHandler : ModPlayer {
            public Vector2 runePosition;
            public float runeRotation;
            public bool shouldDraw;

            public override void ResetEffects() => shouldDraw = false;

            public override void PostUpdateEquips() {
                int itemType = ModContent.ItemType<EldritchRing>();
                if (shouldDraw) {
                    Vector2 position2 = runePosition - Main.screenPosition;
                    Vector2 position = Player.GetPlayerCorePoint();
                    if (!Main.gamePaused) {
                        runePosition = new((float)(int)position.X, (float)(int)position.Y);
                        runeRotation += ((Player.direction > 0 ? 0.04f : -0.04f) + Player.velocity.X * 0.02f) * Player.gravDir;
                    }
                }
            }
        }

        void ILoadable.Load(Mod mod) {
            On_PlayerDrawLayers.DrawPlayer_08_Backpacks += On_PlayerDrawLayers_DrawPlayer_08_Backpacks;

            if (!Main.dedServ) {
                _yellowSignRune = ModContent.Request<Texture2D>(ResourceManager.DeveloperEquipableTextures + "YellowSignRune");
            }
        }

        private void On_PlayerDrawLayers_DrawPlayer_08_Backpacks(On_PlayerDrawLayers.orig_DrawPlayer_08_Backpacks orig, ref PlayerDrawSet drawInfo) {
            Player player = drawInfo.drawPlayer;
            var handler = player.GetModPlayer<EldritchRingHandler>();
            if (handler.shouldDraw) {
                if (drawInfo.shadow == 0) {
                    if (!drawInfo.hideEntirePlayer && !player.dead) {
                        Vector2 position2 = handler.runePosition - Main.screenPosition;
                        Vector2 position = new(drawInfo.Center.X, drawInfo.Center.Y);
                        var asset = _yellowSignRune;
                        Texture2D texture = asset.Value;
                        Vector2 origin = new(texture.Width * 0.5f, texture.Height * 0.5f);
                        Color color = new(255, 215, 50, 180);
                        if (player.gravDir == -1.0) position.Y += 60f;
                        DrawData drawData = new(texture, position2 - new Vector2(3f * player.direction, 0f), new Rectangle?(), color, handler.runeRotation, origin, 1f, SpriteEffects.None, 0);
                        drawData.shader = drawInfo.cWings;
                        drawInfo.DrawDataCache.Add(drawData);
                    }
                }
            }

            orig(ref drawInfo);
        }

        void ILoadable.Unload() { }
    }
}