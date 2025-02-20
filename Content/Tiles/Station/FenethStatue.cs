using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Station;

sealed class FenethStatue : ModTile {
    public override void Load() {
        On_TileObject.DrawPreview += On_TileObject_DrawPreview;
    }

    private void On_TileObject_DrawPreview(On_TileObject.orig_DrawPreview orig, SpriteBatch sb, TileObjectPreviewData op, Vector2 position) {
        if (op.Type == ModContent.TileType<FenethStatue>()) {
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
                        bool flag2 = frameX == 36;
                        bool flag3 = frameY > 36;
                        frameY = !flag ? frameY + 4 : 0;
                        height = flag || flag3 ? 22 : 18;
                        width = flag2 ? 22 : 16;
                        if (frameX == 54) {
                            width += 6;
                        }
                        if (frameX > 54) {
                            offsetX -= 3;
                        }
                        if (frameX >= 54) {
                            frameX += 6;
                            offsetX -= 1;
                        }
                        if (frameX == 96) {
                            frameX += 6;
                            offsetX += 6;
                        }
                        if (frameX == 78) {
                            frameX += 6;
                            offsetX += 6;
                        }
                        offsetY += -(flag ? 4 : 0);
                        if (flag2) {
                            offsetX += 3;
                        }

                        sb.Draw(sourceRectangle: new Rectangle(frameX, frameY, width, height), texture: value, 
                            position: new Vector2(num9 * 16 - (int)(position.X + (float)(width - 16) / 2f) + drawXOffset + offsetX, 
                            num10 * 16 - (int)position.Y + num5 + offsetY), color: color, rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: spriteEffects, layerDepth: 0f);
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
        Main.tileLavaDeath[Type] = false;
        Main.tileLighted[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 4;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(191, 107, 87), CreateMapEntryName());

        DustType = DustID.MeteorHead;
        MinPick = 150;
        MineResist = 4f;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        // not cool 
        Tile tile = Main.tile[i, j];
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        bool flag = tile.TileFrameY == 0;
        bool flag2 = tile.TileFrameX == 36;
        bool flag3 = tile.TileFrameY > 36;
        int frameY = !flag ? tile.TileFrameY + 4 : 0;
        int height = flag || flag3 ? 22 : 18;
        int width = flag2 ? 22 : 16;
        int frameX = tile.TileFrameX;
        int offsetX = 0;
        if (frameX == 54) {
            width += 6;
        }
        if (frameX >= 54) {
            frameX += 6;
            offsetX -= 4;
        }
        if (frameX == 96) {
            frameX += 6;
            offsetX += 6;
        }
        if (frameX == 78) {
            frameX += 6;
            offsetX += 6;
        }
        Main.spriteBatch.Draw(TextureAssets.Tile[Type].Value,
                              new Vector2(i * 16 - (int)Main.screenPosition.X + offsetX, j * 16 - (int)Main.screenPosition.Y - (flag ? 4 : 0)) + zero,
                              new Rectangle(frameX, frameY, width, height),
                              Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        return false;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Station.FenethStatue>());
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => false;

    //public override void NumDust(int i, int j, bool fail, ref int num) => num = 5;
}