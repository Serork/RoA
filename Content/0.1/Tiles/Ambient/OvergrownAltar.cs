using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Sets;
using RoA.Common.Tiles;
using RoA.Common.WorldEvents;
using RoA.Content.Tiles.Solid;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Walls;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class OvergrownAltar : ModTile {
    public override void Load() {
        //On_WorldGen.SpreadGrass += On_WorldGen_SpreadGrass;

        On_WorldGen.PlaceTile += On_WorldGen_PlaceTile;
        On_TileObject.Place += On_TileObject_Place;
        On_WorldGen.Convert_int_int_int_int += On_WorldGen_Convert_int_int_int_int;
    }

    private bool On_TileObject_Place(On_TileObject.orig_Place orig, TileObject toBePlaced) {
        ushort type = (ushort)ModContent.TileType<OvergrownAltar>();
        if (toBePlaced.type == type) {
            TileObjectData tileData = TileObjectData.GetTileData(toBePlaced.type, toBePlaced.style, toBePlaced.alternate);
            if (tileData == null)
                return false;

            WorldGen.PlaceTile(toBePlaced.xCoord + tileData.Origin.X, toBePlaced.yCoord + tileData.Origin.Y, type);
        }

        return orig(toBePlaced);
    }

    private bool On_WorldGen_PlaceTile(On_WorldGen.orig_PlaceTile orig, int i, int j, int Type, bool mute, bool forced, int plr, int style) {
        ushort type = (ushort)ModContent.TileType<OvergrownAltar>();
        if (Type == type) {
            if (i >= 0 && j >= 0 && i < Main.maxTilesX && j < Main.maxTilesY) {
                Tile tile = Main.tile[i, j];
                if (forced || Collision.EmptyTile(i, j) || !Main.tileSolid[Type]) {
                    if (tile.LiquidAmount > 0 || tile.CheckingLiquid) {
                        switch (Type) {
                            case 4:
                                if (style != 8 && style != 11 && style != 17)
                                    return false;
                                break;
                            case int _ when TileID.Sets.Torch[Type]:
                                if (TileObjectData.GetTileData(Type, style).WaterPlacement != LiquidPlacement.Allowed)
                                    return false;
                                break;
                            case 3:
                            case int _ when TileID.Sets.TreeSapling[Type]:
                            case 24:
                            case 27:
                            case 32:
                            case 51:
                            case 69:
                            case 72:
                            case 201:
                            case 352:
                            case 529:
                            case 624:
                            case 637:
                            case 656:
                                return false;
                        }
                    }
                    if (TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] && (!tile.HasTile || !Main.tileFrameImportant[tile.TileType])) {
                        tile.IsHalfBlock = false;
                        tile.TileFrameY = 0;
                        tile.TileFrameX = 0;
                    }
                    for (int xSize = 0; xSize < 3; xSize++) {
                        for (int ySize = 0; ySize < 2; ySize++) {
                            ModContent.GetInstance<OvergrownAltarTE>().Place(i + xSize - 1, j - 1 + ySize);
                        }
                    }
                    return true;
                }
            }
        }

        return orig(i, j, Type, mute, forced, plr, style);
    }

    private void On_WorldGen_Convert_int_int_int_int(On_WorldGen.orig_Convert_int_int_int_int orig, int i, int j, int conversionType, int size) {
        if (WorldGen.IsGeneratingHardMode) {
            for (int type = TileID.Count; type < TileLoader.TileCount; type++) {
                if (type == ModContent.TileType<BackwoodsGrass>()) {
                    TileID.Sets.Conversion.Grass[type] = false;
                }
                if (type == ModContent.TileType<BackwoodsStone>()) {
                    TileID.Sets.Conversion.Stone[type] = false;
                }
                if (type == ModContent.TileType<BackwoodsStoneBrick>()) {
                    TileID.Sets.Conversion.Stone[type] = false;
                }
                if (type == ModContent.TileType<BackwoodsGreenMoss>()) {
                    Main.tileMoss[type] = false;
                    TileID.Sets.Conversion.Stone[Type] = false;
                    TileID.Sets.Conversion.Moss[Type] = false;
                }
                if (type == ModContent.TileType<BackwoodsGreenMossBrick>()) {
                    TileID.Sets.Conversion.Stone[Type] = false;
                    TileID.Sets.Conversion.MossBrick[type] = false;
                }
            }
            for (int type = WallID.Count; type < WallLoader.WallCount; type++) {
                if (type == ModContent.WallType<BackwoodsGrassWall>()) {
                    WallID.Sets.Conversion.Grass[type] = false;
                }
                if (type == ModContent.WallType<BackwoodsFlowerGrassWall>()) {
                    WallID.Sets.Conversion.Grass[type] = false;
                }
            }
        }

        orig(i, j, conversionType, size);

        if (WorldGen.IsGeneratingHardMode) {
            for (int type = TileID.Count; type < TileLoader.TileCount; type++) {
                if (type == ModContent.TileType<BackwoodsGrass>()) {
                    TileID.Sets.Conversion.Grass[type] = true;
                }
                if (type == ModContent.TileType<BackwoodsStone>()) {
                    TileID.Sets.Conversion.Stone[type] = true;
                }
                if (type == ModContent.TileType<BackwoodsStoneBrick>()) {
                    TileID.Sets.Conversion.Stone[type] = true;
                }
                if (type == ModContent.TileType<BackwoodsGreenMoss>()) {
                    Main.tileMoss[type] = true;
                    TileID.Sets.Conversion.Stone[Type] = true;
                    TileID.Sets.Conversion.Moss[Type] = true;
                }
                if (type == ModContent.TileType<BackwoodsGreenMossBrick>()) {
                    TileID.Sets.Conversion.Stone[Type] = true;
                    TileID.Sets.Conversion.MossBrick[type] = true;
                }
            }
            for (int type = WallID.Count; type < WallLoader.WallCount; type++) {
                if (type == ModContent.WallType<BackwoodsGrassWall>()) {
                    WallID.Sets.Conversion.Grass[type] = true;
                }
                if (type == ModContent.WallType<BackwoodsFlowerGrassWall>()) {
                    WallID.Sets.Conversion.Grass[type] = true;
                }
            }
        }
    }

    //private void On_WorldGen_SpreadGrass(On_WorldGen.orig_SpreadGrass orig, int i, int j, int dirt, int grass, bool repeat, TileColorCache color) {
    //    if (grass == TileID.CorruptGrass || grass == TileID.CrimsonGrass || grass == TileID.CrimsonJungleGrass || grass == TileID.) {
    //        Point origin = new(i, j);
    //        int distance = 30;
    //        double num = 20;
    //        double num2 = num;
    //        int leftX = (int)(origin.X - distance * 1);
    //        int rightX = (int)(origin.X + distance * 1);
    //        int topY = (int)(origin.Y - distance * 1);
    //        int bottomY = (int)(origin.Y + distance * 1);
    //        for (int x2 = leftX; x2 < rightX; x2++) {
    //            for (int y2 = topY; y2 < bottomY; y2++) {
    //                double num9 = Math.Abs((double)x2 - origin.X);
    //                double num10 = Math.Abs((double)y2 - origin.Y);
    //                if (Math.Sqrt(num9 * num9 + num10 * num10) < num2) {
    //                    if (WorldGenHelper.ActiveTile(i, j, ModContent.TileType<OvergrownAltar>())) {
    //                        return;
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    orig(i, j, dirt, grass, repeat, color);
    //}

    public override void SetStaticDefaults() {
        AnimationFrameHeight = 36;

        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLighted[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.Origin = new Point16(1, 1);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<OvergrownAltarTE>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.addTile(Type);

        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;

        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TileSets.ShouldKillTileBelow[Type] = false;
        TileSets.CanPlayerMineMe[Type] = false;
        TileSets.PreventsSlopesBelow[Type] = true;

        CanBeSlopedTileSystem.Included[Type] = true;

        AddMapEntry(new Color(197, 254, 143), CreateMapEntryName());

        DustType = 59;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => false;

    public override bool CanExplode(int i, int j) => false;

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override void PlaceInWorld(int i, int j, Item item) => ModContent.GetInstance<OvergrownAltarTE>().Place(i, j);

    public override void KillMultiTile(int i, int j, int frameX, int frameY) => ModContent.GetInstance<OvergrownAltarTE>().Kill(i, j);

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        if (!NPC.downedBoss2) {
            return;
        }

        if (!IsValid(i, j)) {
            return;
        }

        OvergrownAltarTE overgrownAltarTE = TileHelper.GetTE<OvergrownAltarTE>(i, j);
        if (overgrownAltarTE != null) {
            float counting = overgrownAltarTE.Counting2 * 0.95f;
            float altarFactor = AltarHandler.GetAltarFactor();
            float value = 0.5f + 0.3f * altarFactor;
            value += (1f - MathHelper.Clamp(counting, 0f, 0.98f)) * (0.5f + 0.35f * altarFactor);
            bool flag = LothorSummoningHandler.PreArrivedLothorBoss.Item1 || LothorSummoningHandler.PreArrivedLothorBoss.Item2;
            float altarStrength = AltarHandler.GetAltarStrength();
            float mult = flag ? 1f : Helper.EaseInOut3(MathHelper.Clamp(altarStrength * 2f, 0f, 1f));
            float r2 = MathHelper.Lerp(0.45f, 0.9f, mult);
            float g2 = MathHelper.Lerp(0.85f, 0.2f, mult);
            float b2 = MathHelper.Lerp(0.4f, 0.3f, mult);
            float altarStrength2 = altarStrength * 1.5f;
            value *= Math.Max(0.75f, 1f - (altarStrength2 > 0.5f ? 1f - altarStrength2 : altarStrength2));
            r = r2 * value;
            g = g2 * value;
            b = b2 * value;
        }
    }

    private bool IsValid(int i, int j, bool onlyOne = false) {
        bool flag = false;
        if (WorldGenHelper.GetTileSafely(i - 1, j).ActiveTile(Type) && WorldGenHelper.GetTileSafely(i + 1, j).ActiveTile(Type) && ((onlyOne && !WorldGenHelper.GetTileSafely(i, j - 1).ActiveTile(Type)) || !onlyOne)) {
            flag = true;
        }

        return flag;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        OvergrownAltarTE overgrownAltarTE = TileHelper.GetTE<OvergrownAltarTE>(i, j);
        if (overgrownAltarTE != null) {
            if (!TileDrawing.IsVisible(Main.tile[i, j])) {
                return false;
            }

            float counting = MathHelper.Clamp(overgrownAltarTE.Counting, 0f, 0.98f);
            float factor = counting;
            float strength = AltarHandler.GetAltarStrength();
            Color color = Lighting.GetColor(i, j);
            Color color2 = new(255, 255, 200, 200);
            Color color3 = Color.Lerp(color, new(255, 155, 130, 200), 0.75f);
            if (strength < 1f) {
                color = Color.Lerp(color, color.MultiplyRGB(color3), MathUtils.YoYo(Ease.QuadOut(strength)));
            }
            Tile tile = Main.tile[i, j];
            bool flag = LothorSummoningHandler.PreArrivedLothorBoss.Item1 || LothorSummoningHandler.PreArrivedLothorBoss.Item2;
            int frame = (int)(factor * 6) + (flag || strength > 0.3f ? 6 : 0);
            Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen) {
                zero = Vector2.Zero;
            }
            Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i, j);
            texture ??= TextureAssets.Tile[Type].Value;
            Rectangle rectangle = new(tile.TileFrameX, !NPC.downedBoss2 ? tile.TileFrameY + 36 * 2 : tile.TileFrameY + 36 * frame, 16, 16);
            Vector2 position = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2f) + zero;
            spriteBatch.Draw(texture, position, rectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            if (!NPC.downedBoss2) {
                return false;
            }

            if (!IsValid(i, j)) {
                return false;
            }

            texture = ModContent.Request<Texture2D>(ResourceManager.TileTextures + "OvergrownAltar_Glow").Value;
            float mult = flag ? 1f : Helper.EaseInOut3(strength);
            float factor3 = flag ? 1f : AltarHandler.GetAltarFactor();
            float factor4 = factor3 * 1.5f;
            factor3 *= Math.Max(0.75f, 1f - (factor4 > 0.5f ? 1f - factor4 : factor4));
            spriteBatch.Draw(texture, position, rectangle, color2 * MathHelper.Lerp(0f, 1f, factor3), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            for (float i2 = -MathHelper.Pi; i2 <= MathHelper.Pi; i2 += MathHelper.PiOver2) {
                spriteBatch.Draw(texture, position + Utils.RotatedBy(Utils.ToRotationVector2(i2), Main.GlobalTimeWrappedHourly, new Vector2()) * Helper.Wave(0f, 1.5f, speed: factor3), rectangle, (color2 * factor3).MultiplyAlpha(MathHelper.Lerp(0f, 1f, factor3)).MultiplyAlpha(0.35f).MultiplyAlpha(Helper.Wave(0.25f, 0.75f, speed: factor3)) * factor3, Main.rand.NextFloatRange(0.1f * factor3), Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            float factor2 = mult;
            if (factor2 > 0f) {
                factor3 = 1f;
                spriteBatch.Draw(texture, position, rectangle, color2 * factor2 * MathHelper.Lerp(0f, 1f, factor3), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                for (float i2 = -MathHelper.Pi; i2 <= MathHelper.Pi; i2 += MathHelper.Pi) {
                    spriteBatch.Draw(texture, position + Utils.RotatedBy(Utils.ToRotationVector2(i2), Main.GlobalTimeWrappedHourly, new Vector2()) * Helper.Wave(0f, 1.5f, speed: factor3), rectangle, (color2 * factor3).MultiplyAlpha(MathHelper.Lerp(0f, 1f, factor3)).MultiplyAlpha(0.35f).MultiplyAlpha(Helper.Wave(0.25f, 0.75f, speed: factor3)) * factor3 * factor2, Main.rand.NextFloatRange(0.1f * factor3), Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }

        return false;
    }
}
