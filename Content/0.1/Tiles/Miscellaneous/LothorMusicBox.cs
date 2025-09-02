using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class LothorMusicBox : MusicBox {
    protected override int CursorItemType => ModContent.ItemType<Items.Special.Lothor.LothorMusicBox>();

    protected override int GoreOffsetX => 4;

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX > 18) {
            float value = 1f;
            Color color = new Color(1f, 0.2f, 0.2f) * value * 0.75f;
            r = color.R / 255f * 0.75f;
            g = color.G / 255f * 0.75f;
            b = color.B / 255f * 0.75f;
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        Tile tile = Main.tile[i, j];
        //if (tile.TileFrameX > 18) {
        //    float value = 1f;
        //    Lighting.AddLight(new Vector2(i, j).ToWorldCoordinates(), new Vector3(1f, 0.2f, 0.2f) * value * 0.75f);
        //}
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        int height = tile.TileFrameY == 36 ? 18 : 16;

        Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(Main.tile[i, j], i, j);
        texture ??= TextureAssets.Tile[Type].Value;

        Vector2 drawPosition = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2) + zero;
        Color color = Lighting.GetColor(i, j);
        Main.spriteBatch.Draw(texture,
                      drawPosition,
                      new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
                      color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture + "_Glow").Value,
                              drawPosition,
                              new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
                              Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        if (Main.InSmartCursorHighlightArea(i, j, out var actuallySelected)) {
            int num = (color.R + color.G + color.B) / 3;
            if (num > 10) {
                Texture2D highlightTexture = ModContent.Request<Texture2D>(Texture + "_Highlight").Value;
                Color highlightColor = Colors.GetSelectionGlowColor(actuallySelected, num);
                Rectangle rect = new(tile.TileFrameX, tile.TileFrameY, 16, height);
                Main.spriteBatch.Draw(sourceRectangle: rect, texture: highlightTexture, position: drawPosition, color: highlightColor, rotation: 0f, origin: Vector2.Zero, scale: 1f,
                    effects: SpriteEffects.None, layerDepth: 0f);
            }
        }

        if (!(Main.gamePaused || !Main.instance.IsActive || Lighting.UpdateEveryFrame && !Main.rand.NextBool(4))) {
            if (tile.TileFrameX == 36 && (int)Main.timeForVisualEffects % 7 == 0 && Main.rand.NextBool(3)) {
                int goreType = Main.rand.Next(570, 573);
                Vector2 position = new Vector2(i * 16 + 8 + GoreOffsetX, j * 16 - 10);
                Vector2 velocity = new Vector2(Main.WindForVisuals * 2f, -0.5f);
                velocity.X *= 1f + Main.rand.NextFloat(-0.5f, 0.5f);
                velocity.Y *= 1f + Main.rand.NextFloat(-0.5f, 0.5f);
                if (goreType == 572) {
                    position.X -= 8f;
                }

                if (goreType == 571) {
                    position.X -= 4f;
                }

                if (!Main.dedServ) {
                    Gore.NewGore(new EntitySource_TileUpdate(i, j), position, velocity, goreType, 0.8f);
                }
            }
        }

        return false;
    }
}