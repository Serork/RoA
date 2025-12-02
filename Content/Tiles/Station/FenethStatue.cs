using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Sets;
using RoA.Common.Tiles;
using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Solid.Backwoods;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Station;

sealed class FenethStatue : ModTile {
    private static Asset<Texture2D> _glowTexture = null!;

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
        if (!Main.dedServ) {
            _glowTexture = ModContent.Request<Texture2D>(TileLoader.GetTile(Type).Texture + "_Glow");
        }

        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileLighted[Type] = true;

        Main.tileSpelunker[Type] = true;

        TileID.Sets.GeneralPlacementTiles[Type] = false;

        Main.tileOreFinderPriority[Type] = 750;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 4;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Origin = new Point16(1, 3);
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;
        TileSets.ShouldKillTileBelow[Type] = false;
        TileSets.PreventsSlopesBelow[Type] = true;
        CanBeSlopedTileSystem.Included[Type] = true;

        AddMapEntry(new Color(191, 107, 87), CreateMapEntryName());

        DustType = DustID.MeteorHead;
        MinPick = 150;
        MineResist = 4f;
    }

    public override bool CanExplode(int i, int j) => false;

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        for (int k = 0; k < 5; k++) {
            Dust.NewDustPerfect(new Point(i + (frameX == 0 ? 3 : 0), j).ToWorldCoordinates() + new Vector2(frameX == 0 ? -16f : 0, 0f), ModContent.DustType<Dusts.Fireblossom2>());
        }
    }

    public override void RandomUpdate(int x, int y) {
        int num868 = x;
        int num869 = y;
        var genRand = Main.rand;
        for (int i = num868 - 10; i < num868 + 11; i++) {
            for (int j = num869 - 10; j < num869 + 11; j++) {
                if (genRand.NextBool(20)) {
                    List<int> grass = [TileID.Ash, 2, 23, 109, 199, 477, 492, 633, ModContent.TileType<BackwoodsGrass>()];
                    if (ModLoader.TryGetMod("TheDepths", out Mod theDepths) && theDepths.TryFind<ModTile>("NightmareGrass", out ModTile NightmareGrass)) {
                        grass.Add(NightmareGrass.Type);
                    }
                    if (Main.tile[i, j].HasTile && (grass.Contains(Main.tile[i, j].TileType)) &&
                        !Main.tile[i, j].IsHalfBlock && Main.tile[i, j].Slope == 0 && !Main.tile[i, j - 1].HasTile) {
                        int flowersType = ModContent.TileType<FenethStatueFlowers>();
                        WorldGen.PlaceTile(i, j - 1, flowersType, style: genRand.Next(4), mute: true);
                        Tile tile = Main.tile[i, j - 1];
                        if (tile.TileType == flowersType) {
                            tile.TileColor = Main.tile[x, y].TileColor;
                        }
                    }
                }
            }
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameY == 0 && (tile.TileFrameX == 72 || tile.TileFrameX == 36)) {
            Color color = Color.Orange;
            color *= 0.4f;
            Lighting.AddLight(new Point(i, j).ToWorldCoordinates() - Vector2.One * 8f, new Vector3(color.R / 255f, color.G / 255f, color.B / 255f));
        }

        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        // not cool 
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

        Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i, j);
        texture ??= TextureAssets.Tile[Type].Value;

        Main.spriteBatch.Draw(texture,
                              new Vector2(i * 16 - (int)Main.screenPosition.X + offsetX, j * 16 - (int)Main.screenPosition.Y - (flag ? 4 : 0)) + zero,
                              new Rectangle(frameX, frameY, width, height),
                              Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        Main.spriteBatch.Draw(_glowTexture.Value,
                              new Vector2(i * 16 - (int)Main.screenPosition.X + offsetX, j * 16 - (int)Main.screenPosition.Y - (flag ? 4 : 0)) + zero,
                              new Rectangle(frameX, frameY, width, height),
                              Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        return false;
    }
    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Station.FenethStatue>());
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => false;

    //public override void NumDust(int i, int j, bool fail, ref int num) => num = 5;
}