using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class ScholarsDesk : ModTile, TileHooks.IGetTileDrawData {
    void TileHooks.IGetTileDrawData.GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {

    }

    public override void Load() {
        On_TileObject.DrawPreview += On_TileObject_DrawPreview;
    }

    private void On_TileObject_DrawPreview(On_TileObject.orig_DrawPreview orig, SpriteBatch sb, TileObjectPreviewData op, Vector2 position) {
        if (TileLoader.GetTile(op.Type) is ScholarsDesk) {
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

                    num5 = 0;

                    if (!WorldGen.SolidTile(Framing.GetTileSafely(num9, num10 + 1))) {
                        num5 = -4;
                    }

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

                        sb.Draw(sourceRectangle: new Rectangle(x, num8, coordinateWidth, num12), texture: value, 
                            position: new Vector2(num9 * 16 - (int)(position.X + (float)(coordinateWidth - 16) / 2f) + drawXOffset, num10 * 16 - (int)position.Y + num5), color: color, rotation: 0f, origin: Vector2.Zero, scale: 1f, effects: spriteEffects, layerDepth: 0f);
                        num8 += num12 + tileData.CoordinatePadding;
                    }
                }
            }

            return;
        }

        orig(sb, op, position);
    }

    public override void SetStaticDefaults() {
        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.GeneralPlacementTiles[Type] = false;
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;

        TileObjectData.newTile.DrawYOffset = -2;
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(1, 1);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [20, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        AddMapEntry(new Color(191, 142, 111), Language.GetText("MapObject.Table"));

        AdjTiles = [TileID.Tables];
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        if (Main.tile[i, j + 1].TileType == Type) {
            height = 20;
            offsetY = -4;
        }
        else {
            height = 16;
            offsetY = 0;
        }
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Miscellaneous.ScholarsDesk>());
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;
}
