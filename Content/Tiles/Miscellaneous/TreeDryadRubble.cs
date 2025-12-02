using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Materials;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class TreeDryadRubble : ModTile {
    public override string Texture => TileHelper.GetTileTexture<TreeDryad>();

    public override void Load() {
        On_TileObject.DrawPreview += On_TileObject_DrawPreview;
    }

    private void On_TileObject_DrawPreview(On_TileObject.orig_DrawPreview orig, SpriteBatch sb, TileObjectPreviewData op, Vector2 position) {
        if (op.Type == ModContent.TileType<TreeDryadRubble>()) {
            Point16 coordinates = op.Coordinates;
            Texture2D value = TextureAssets.Tile[op.Type].Value;
            TileObjectData tileData = TileObjectData.GetTileData(op.Type, op.Style, op.Alternate);
            int num = 0;
            int num2 = 0;
            int num3 = tileData.CalculatePlacementStyle(op.Style, op.Alternate, op.Random);
            int num4 = 0;
            int num5 = tileData.DrawYOffset;
            int drawXOffset = tileData.DrawXOffset;
            num3 += tileData.DrawStyleOffset;
            int num6 = tileData.StyleWrapLimit;
            int num7 = tileData.StyleLineSkip;
            if (tileData.StyleWrapLimitVisualOverride.HasValue)
                num6 = tileData.StyleWrapLimitVisualOverride.Value;

            if (tileData.styleLineSkipVisualOverride.HasValue)
                num7 = tileData.styleLineSkipVisualOverride.Value;

            if (num6 > 0) {
                num4 = num3 / num6 * num7;
                num3 %= num6;
            }

            if (tileData.StyleHorizontal) {
                num = tileData.CoordinateFullWidth * num3;
                num2 = tileData.CoordinateFullHeight * num4;
            }
            else {
                num = tileData.CoordinateFullWidth * num4;
                num2 = tileData.CoordinateFullHeight * num3;
            }

            for (int i = 0; i < op.Size.X; i++) {
                int x = num + (i - op.ObjectStart.X) * (tileData.CoordinateWidth + tileData.CoordinatePadding);
                int num8 = num2;
                for (int j = 0; j < op.Size.Y; j++) {
                    int num9 = coordinates.X + i;
                    int num10 = coordinates.Y + j;
                    if (j == 0 && tileData.DrawStepDown != 0 && WorldGen.SolidTile(Framing.GetTileSafely(num9, num10 - 1)))
                        num5 += tileData.DrawStepDown;

                    if (op.Type == 567)
                        num5 = ((j != 0) ? tileData.DrawYOffset : (tileData.DrawYOffset - 2));

                    int num11 = op[i, j];
                    Color color;
                    if (num11 != 1) {
                        if (num11 != 2)
                            continue;

                        color = Color.Red * 0.7f;
                    }
                    else {
                        color = Color.White;
                    }

                    color *= 0.5f;
                    if (i >= op.ObjectStart.X && i < op.ObjectStart.X + tileData.Width && j >= op.ObjectStart.Y && j < op.ObjectStart.Y + tileData.Height) {
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        if (tileData.DrawFlipHorizontal && num9 % 2 == 0)
                            spriteEffects |= SpriteEffects.FlipHorizontally;

                        if (tileData.DrawFlipVertical && num10 % 2 == 0)
                            spriteEffects |= SpriteEffects.FlipVertically;

                        int coordinateWidth = tileData.CoordinateWidth;
                        int num12 = tileData.CoordinateHeights[j - op.ObjectStart.Y];
                        if (op.Type == 114 && j == 1)
                            num12 += 2;

                        int offsetX = 0;
                        int offsetY = -2;
                        int width = coordinateWidth;
                        int height = num12;
                        int frameX = x;
                        int frameY = num8;

                        bool flag = frameY == 0;
                        frameY = !flag ? frameY + 4 : 0;
                        height = flag ? 22 : 16;
                        frameX = frameX;

                        sb.Draw(sourceRectangle: new Rectangle(frameX, frameY, width, height), texture: value,
                            position: new Vector2(num9 * 16 - (int)(position.X + (float)(width - 16) / 2f) + drawXOffset + offsetX,
                            num10 * 16 - (int)position.Y + num5 + offsetY - (flag ? 4 : 0) + 2), color: color, rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: spriteEffects, layerDepth: 0f);
                        num8 += num12 + tileData.CoordinatePadding;
                    }
                }
            }

            return;
        }

        orig(sb, op, position);
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileLighted[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.addTile(Type);

        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<NaturesHeart>(), Type, 0, 1, 2, 3);

        AddMapEntry(new Color(191, 143, 111));

        DustType = DustID.WoodFurniture;

        RegisterItemDrop(ModContent.ItemType<NaturesHeart>());
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        Tile tile = Main.tile[i, j];
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        bool flag = tile.TileFrameY == 0;
        int frameY = !flag ? tile.TileFrameY + 4 : 0;
        int height = flag ? 22 : 16;
        int frameX = tile.TileFrameX;
        Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i, j);
        texture ??= TextureAssets.Tile[Type].Value;
        Main.spriteBatch.Draw(texture,
                              new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y - (flag ? 4 : 0) + 2) + zero,
                              new Rectangle(frameX, frameY, 16, height),
                              Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        return false;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        Vector2 position = new Point(i, j).ToWorldCoordinates();
        if (Main.netMode != NetmodeID.Server) {
            int dustType = DustID.WoodFurniture;
            for (int k = 0; k < 20; k++) {
                Dust.NewDust(position, 36, 54, dustType, 2.5f * Main.rand.NextFloatDirection(), 2.5f * Main.rand.NextFloatDirection());
            }
        }
    }
}