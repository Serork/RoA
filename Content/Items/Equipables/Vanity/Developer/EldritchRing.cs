using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

sealed class EldritchRing : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 30; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.sellPrice(gold: 5);

        Item.accessory = true;

        Item.vanity = true;
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