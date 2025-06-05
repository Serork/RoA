using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

partial class Tapper : ModTile {
    public override void Load() {
        On_TileObject.DrawPreview += On_TileObject_DrawPreview;
    }

    private void On_TileObject_DrawPreview(On_TileObject.orig_DrawPreview orig, SpriteBatch sb, TileObjectPreviewData op, Vector2 position) {
        if (ImATapper[op.Type]) {
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
                    bool flag = false;
                    ushort type = (ushort)ModContent.TileType<Tapper>();
                    bool flag3 = WorldGenHelper.ActiveTile(num9, num10 - 1, type) || WorldGenHelper.ActiveTile(num9, num10 + 1, type);
                    bool flag4 = (WorldGenHelper.ActiveTile(num9 - 1, num10 - 1, TileID.Trees) || WorldGenHelper.ActiveTile(num9 + 1, num10 - 1, TileID.Trees));
                    if (num11 != 1) {
                        if (num11 != 2)
                            continue;

                        color = Color.Red * 0.7f;
                        flag = true;
                    }
                    else if (flag4) {
                        color = Color.White;
                    }
                    else {
                        color = Color.Red * 0.7f;
                    }

                    if (flag3) {
                        color = Color.Red * 0.7f;
                    }

                    bool flag5 = false;
                    for (int testJ = num10; testJ > num10 - 8; testJ--) {
                        if (WorldGenHelper.ActiveTile(num9, testJ, type)) {
                            flag5 = true;
                        }
                    }
                    for (int testJ = num10; testJ < num10 + 8; testJ++) {
                        if (WorldGenHelper.ActiveTile(num9, testJ, type)) {
                            flag5 = true;
                        }
                    }

                    if (flag5) {
                        color = Color.Red * 0.7f;
                    }

                    color *= 0.5f;
                    if (i >= op.ObjectStart.X && i < op.ObjectStart.X + tileData.Width && j >= op.ObjectStart.Y && j < op.ObjectStart.Y + tileData.Height) {
                        int coordinateWidth = tileData.CoordinateWidth;
                        int num12 = tileData.CoordinateHeights[j - op.ObjectStart.Y];
                        if (op.Type == 114 && j == 1)
                            num12 += 2;
                        SpriteEffects spriteEffects = SpriteEffects.None;
                        if (WorldGenHelper.ActiveTile(num9 - 1, num10, TileID.Trees)/* || flag*/) {
                            spriteEffects = SpriteEffects.FlipHorizontally;
                        }
                        else {
                            bool flag2 = !WorldGenHelper.ActiveTile(num9 + 1, num10, TileID.Trees);
                            if (num9 * 16 > Main.LocalPlayer.position.X) {
                                spriteEffects = flag2 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                            }
                            else if (!WorldGenHelper.ActiveTile(num9 + 1, num10, TileID.Trees)) {
                                drawXOffset += 18;
                            }
                        }
                        if (WorldGenHelper.ActiveTile(num9 + 1, num10, TileID.Trees)) {
                            drawXOffset += 1;
                        }
                        if (WorldGenHelper.ActiveTile(num9 - 1, num10, TileID.Trees)) {
                            drawXOffset -= 1;
                        }

                        //if (tileData.DrawFlipHorizontal && num9 % 2 == 0)
                        //    spriteEffects |= SpriteEffects.FlipHorizontally;

                        //if (tileData.DrawFlipVertical && num10 % 2 == 0)
                        //    spriteEffects |= SpriteEffects.FlipVertically;


                        sb.Draw(sourceRectangle: new Rectangle(0, 0, coordinateWidth, num12), texture: value, position: new Vector2(num9 * 16 - (int)(position.X + (float)(coordinateWidth - 16) / 2f) + drawXOffset, num10 * 16 - (int)position.Y + num5), color: color, rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: spriteEffects, layerDepth: 0f);
                        num8 += num12 + tileData.CoordinatePadding;
                    }
                }
            }
            return;
        }

        orig(sb, op, position);
    }
}
