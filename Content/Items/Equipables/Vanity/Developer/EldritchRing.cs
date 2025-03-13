using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

sealed class EldritchRing : ModItem {
    private sealed class EldritchRingWingsLogic : ModPlayer {
        public bool IsWingsActive;

        private bool flapSound;

        public override void ResetEffects() {
            IsWingsActive = false;
        }

        public override void FrameEffects() {
            if (!IsWingsActive) {
                return;
            }

            Player.wingTimeMax = 150;

            if (((Player.velocity.Y == 0f || Player.sliding) && Player.releaseJump) || (Player.autoJump && Player.justJumped)) {
                Player.wingTime = Player.wingTimeMax;
            }

            bool flag22 = Player.wingFrame == 3;
            if (flag22) {
                if (!flapSound)
                    SoundEngine.PlaySound(SoundID.Item24, Player.position);

                flapSound = true;
            }
            else {
                flapSound = false;
            }
        }
    }


    private static int _wingsSlot = -1;

    private static string WingsTextureName => ResourceManager.EmptyTexture;
    private static string WingsLayerName => $"{nameof(EldritchRing)}_Wings";

    public override void Load() => _wingsSlot = EquipLoader.AddEquipTexture(Mod, WingsTextureName, EquipType.Wings, name: WingsLayerName);

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

        Item.vanity = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<EldritchRingWingsLogic>().IsWingsActive = true;

        player.noFallDmg = true;
        player.wings = -1;
        player.wingsLogic = _wingsSlot;
        player.flapSound = true;
    }

    private sealed class EldritchRingDrawLayer : ILoadable {
        private Vector2 runePosition;
        private float runeRotation;

        void ILoadable.Load(Mod mod) {
            On_PlayerDrawLayers.DrawPlayer_08_Backpacks += On_PlayerDrawLayers_DrawPlayer_08_Backpacks;
        }

        private void On_PlayerDrawLayers_DrawPlayer_08_Backpacks(On_PlayerDrawLayers.orig_DrawPlayer_08_Backpacks orig, ref PlayerDrawSet drawInfo) {
            Player player = drawInfo.drawPlayer;
            int itemType = ModContent.ItemType<EldritchRing>();
            if (drawInfo.shadow == 0) {
                for (int i = 3; i <= 19; i++) {
                    if (player.armor[i].type == itemType) {
                        var asset = ModContent.Request<Texture2D>(ResourceManager.ItemsTextures + "YellowSignRune");
                        Player _player = drawInfo.drawPlayer;
                        Texture2D texture = asset.Value;
                        Vector2 origin = new(texture.Width * 0.5f, texture.Height * 0.5f);
                        Color _color = new(255, 215, 50, 180);
                        Vector2 _position2 = runePosition - Main.screenPosition;
                        Vector2 _position = new(drawInfo.Center.X, drawInfo.Center.Y);
                        if (_player.gravDir == -1.0) _position.Y += 60f;
                        if (!Main.gamePaused) {
                            runePosition = new((float)(int)_position.X, (float)(int)_position.Y);
                            runeRotation += (_player.direction > 0 ? 0.04f : -0.04f) + _player.velocity.X * 0.02f;
                        }
                        int index = i;
                        if (index > 9) {
                            index -= 10;
                        }
                        DrawData drawData = new(texture, _position2 - new Vector2(3f * _player.direction, 0f), new Rectangle?(), _color, runeRotation, origin, 1f, SpriteEffects.None, 0);
                        drawData.shader = GameShaders.Armor.GetShaderIdFromItemId(player.dye[index].type);
                        drawInfo.DrawDataCache.Add(drawData);
                    }
                }
            }

            orig(ref drawInfo);
        }

        void ILoadable.Unload() { }
    }
}