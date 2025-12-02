using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Tiles;
﻿using RoA.Common.World;
using RoA.Content.Dusts;
using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Solid;

sealed class BackwoodsGreenMossBrick : ModTile, IPostSetupContent {
    public static Asset<Texture2D> GlowTexture { get; private set; } = null!;

    void IPostSetupContent.PostSetupContent() {
        for (int i = 0; i < TileLoader.TileCount; i++) {
            TileObjectData objData = TileObjectData.GetTileData(i, 0);
            if (objData == null || objData.AnchorValidTiles == null || objData.AnchorValidTiles.Length == 0) {
                continue;
            }

            if (objData.AnchorValidTiles.Any(tileId => tileId == TileID.GreenMossBrick)) {
                lock (objData) {
                    int[] anchorAlternates = objData.AnchorValidTiles;
                    Array.Resize(ref anchorAlternates, anchorAlternates.Length + 1);
                    anchorAlternates[^1] = ModContent.TileType<BackwoodsGreenMossBrick>();
                    objData.AnchorValidTiles = anchorAlternates;
                }
            }
        }
    }

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }

        ushort stoneType = (ushort)ModContent.TileType<BackwoodsStone>();
        TileHelper.Solid(Type, blendAll: true);
        TileHelper.MergeWith(Type, stoneType);
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<BackwoodsStoneBrick>());

        Main.tileLighted[Type] = true;
        Main.tileMoss[Type] = false;

        TileID.Sets.tileMossBrick[Type] = true;

        TileID.Sets.Conversion.Stone[Type] = true;
        TileID.Sets.Conversion.MossBrick[Type] = true;

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = stoneType;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;

        TransformTileSystem.ReplaceToTypeOnKill[Type] = (ushort)ModContent.TileType<BackwoodsStoneBrick>();

        DustType = ModContent.DustType<TealMossDust>();
        HitSound = SoundID.Dig;
        AddMapEntry(new Color(49, 134, 114));
    }

    public override bool CanDrop(int i, int j) => false;

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => SetupLight(ref r, ref g, ref b);

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        //bool flag = true;
        //for (int x = -1; x < 2; x++) {
        //    for (int y = -1; y < 2; y++) {
        //        if (x != 0 && y != 0) {
        //            if (!WorldGen.SolidTile(i + x, j + y)) {
        //                flag = false;
        //            }
        //        }
        //    }
        //}
        //if (flag) {
        //    Tile tile = WorldGenHelper.GetTileSafely(i, j);
        //    tile.TileFrameX = 144;
        //    tile.TileFrameY = 198;
        //    return false;
        //}

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }

    public static void SetupLight(ref float r, ref float g, ref float b) {
        float value = BackwoodsFogHandler.Opacity;
        r = 49 / 255f * value;
        g = 134 / 255f * value;
        b = 114 / 255f * value;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return;
        }

        Tile tile = Main.tile[i, j];
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        int height = tile.TileFrameY == 36 ? 18 : 16;
        Main.spriteBatch.Draw(GlowTexture.Value,
                              new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2) + zero,
                              new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
                              TileDrawingExtra.BackwoodsMossGlowColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }

    public override void RandomUpdate(int i, int j) {
        Tile me = Main.tile[i, j];
        Tile left = Main.tile[i + 1, j];
        Tile right = Main.tile[i - 1, j];
        Tile down = Main.tile[i, j + 1];
        Tile up = Main.tile[i, j - 1];
        int type = ModContent.TileType<MossGrowth>();
        if (Main.rand.NextBool(6) && me.HasUnactuatedTile && !me.IsHalfBlock && !me.BottomSlope && !me.LeftSlope && !me.RightSlope && !me.TopSlope) {
            short framing = (short)(Main.rand.Next(3) * 18);
            if (!left.HasTile) {
                WorldGen.PlaceTile(i + 1, j, type, mute: true);
                left.TileFrameY = (short)(108 + framing);
                left.CopyPaintAndCoating(me);
            }
            if (!right.HasTile) {
                WorldGen.PlaceTile(i - 1, j, type, mute: true);
                right.TileFrameY = (short)(162 + framing);
                right.CopyPaintAndCoating(me);
            }
            if (!down.HasTile) {
                WorldGen.PlaceTile(i, j + 1, type, mute: true);
                down.TileFrameY = (short)(54 + framing);
                down.CopyPaintAndCoating(me);
            }
            if (!up.HasTile) {
                WorldGen.PlaceTile(i, j - 1, type, mute: true);
                up.TileFrameY = framing;
                up.CopyPaintAndCoating(me);
            }
        }

        if (Main.tile[i, j].HasUnactuatedTile && (j > Main.worldSurface - 1 || Main.rand.NextBool(2))) {
            int num = i - 1;
            int num2 = i + 2;
            int num3 = j - 1;
            int num4 = j + 2;
            int type2 = Main.tile[i, j].TileType;
            bool flag9 = false;
            TileColorCache color = Main.tile[i, j].BlockColorAndCoating();
            for (int num28 = num; num28 < num2; num28++) {
                for (int num29 = num3; num29 < num4; num29++) {
                    bool flag = Main.tile[num28, num29].TileType == ModContent.TileType<BackwoodsStoneBrick>();
                    if ((i != num28 || j != num29) && Main.tile[num28, num29].HasTile && (
                        //Main.tile[num28, num29].TileType == 1 ||
                        Main.tile[num28, num29].TileType == ModContent.TileType<BackwoodsStone>() ||
                        flag
                        /*|| Main.tile[num28, num29].TileType == 38*/)) {
                        int type3 = Main.tile[num28, num29].TileType;
                        int num30 = flag ? Type : ModContent.TileType<BackwoodsGreenMoss>();
                        WorldGen.SpreadGrass(num28, num29, Main.tile[num28, num29].TileType, num30, repeat: false, color);
                        if (Main.tile[num28, num29].TileType == num30) {
                            WorldGen.SquareTileFrame(num28, num29);
                            flag9 = true;
                        }
                    }
                }
            }

            if (Main.netMode == 2 && flag9)
                NetMessage.SendTileSquare(-1, i, j, 3);
        }
    }

    private class SpreadFromGreenMossToGrimstoneSystem : GlobalTile {
        public override void RandomUpdate(int i, int j, int type) {
            //if (type == TileID.GreenMoss || type == TileID.GreenMossBrick) {
            //    if (Main.tile[i, j].HasUnactuatedTile && (j > Main.worldSurface - 1 || WorldGen.genRand.NextBool(2))) {
            //        int num = i - 1;
            //        int num2 = i + 2;
            //        int num3 = j - 1;
            //        int num4 = j + 2;
            //        int type2 = Main.tile[i, j].TileType;
            //        bool flag9 = false;
            //        TileColorCache color = Main.tile[i, j].BlockColorAndCoating();
            //        for (int num28 = num; num28 < num2; num28++) {
            //            for (int num29 = num3; num29 < num4; num29++) {
            //                if ((i != num28 || j != num29) && Main.tile[num28, num29].HasTile && Main.tile[num28, num29].TileType == ModContent.TileType<BackwoodsStone>()) {
            //                    int type3 = Main.tile[num28, num29].TileType;
            //                    int num30 = TileHelper.MossConversion(type2, type3);
            //                    WorldGen.SpreadGrass(num28, num29, Main.tile[num28, num29].TileType, num30, repeat: false, color);
            //                    if (Main.tile[num28, num29].TileType == num30) {
            //                        WorldGen.SquareTileFrame(num28, num29);
            //                        flag9 = true;
            //                    }
            //                }
            //            }
            //        }

            //        if (Main.netMode == 2 && flag9)
            //            NetMessage.SendTileSquare(-1, i, j, 3);
            //    }
            //}
        }
    }
}