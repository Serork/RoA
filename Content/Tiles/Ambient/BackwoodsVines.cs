using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsVinesFlower : BackwoodsVines { }

class BackwoodsVines : ModTile {
    private static readonly ushort[] ValidTilesToGrowFrom = [(ushort)ModContent.TileType<BackwoodsGrass>(), (ushort)ModContent.TileType<LivingElderwoodlLeaves>()];

    public override void SetStaticDefaults() {
        Main.tileLighted[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileID.Sets.IsVine[Type] = true;
        TileID.Sets.VineThreads[Type] = true;
        TileID.Sets.ReplaceTileBreakDown[Type] = true;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
        HitSound = SoundID.Grass;
        AddMapEntry(new Color(46, 130, 69));
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        bool intoRenderTargets = true;
        bool flag = intoRenderTargets || Main.LightingEveryFrame;

        if (flag) {
            int y = j;
            for (int num = j - 1; num > 0; num--) {
                Tile tile = Main.tile[i, num];
                if (WorldGen.SolidTile(i, num) || !tile.HasTile) {
                    y = num + 1;
                    break;
                }
            }

            Point item = new(i, y);
            TileHelper.AddVineRootPosition(item);
            TileHelper.AddSpecialPoint(i, y, 6);
        }

        return false;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        Tile tile = Framing.GetTileSafely(i, j + 1);
        if (tile.HasTile && tile.TileType == Type) {
            //WorldGen.KillTile(i, j + 1, noItem: true);
        }
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        Tile tileAbove = Framing.GetTileSafely(i, j - 1);
        int type = -1;
        if (tileAbove.HasTile && !tileAbove.BottomSlope) {
            type = tileAbove.TileType;
        }

        foreach (ushort validType in ValidTilesToGrowFrom) {
            if (type == validType) {
                return true;
            }
        }
        if (TileID.Sets.Grass[tileAbove.TileType] ||
            TileID.Sets.IsVine[tileAbove.TileType]) {
            return true;
        }
        if (type == Type) {
            return true;
        }

        WorldGen.KillTile(i, j);
        return true;
    }

    public override void RandomUpdate(int i, int j) {
        if (Main.tile[i, j].HasUnactuatedTile) {
            int num34 = 2;
            if (Main.rand.Next(num34) == 0 && WorldGen.GrowMoreVines(i, j) && !Main.tile[i, j + 1].HasTile && Main.tile[i, j + 1].LiquidType != LiquidID.Lava) {
                bool flag5 = false;
                ushort type7 = (ushort)ModContent.TileType<BackwoodsVines>();
                if (Main.tile[i, j].WallType == 68 || Main.tile[i, j].WallType == ModContent.WallType<BackwoodsGrassWall>() || Main.tile[i, j].WallType == ModContent.WallType<BackwoodsFlowerGrassWall>() || Main.tile[i, j].WallType == 65 || Main.tile[i, j].WallType == 66 || Main.tile[i, j].WallType == 63)
                    type7 = (ushort)ModContent.TileType<BackwoodsVinesFlower>();
                else if (Main.tile[i, j + 1].WallType == 68 || Main.tile[i, j + 1].WallType == ModContent.WallType<BackwoodsGrassWall>() || Main.tile[i, j + 1].WallType == ModContent.WallType<BackwoodsFlowerGrassWall>() || Main.tile[i, j + 1].WallType == 65 || Main.tile[i, j + 1].WallType == 66 || Main.tile[i, j + 1].WallType == 63)
                    type7 = (ushort)ModContent.TileType<BackwoodsVinesFlower>();

                //if (Main.remixWorld && genRand.Next(5) == 0)
                //    type7 = 382;

                for (int num35 = j; num35 > j - 10; num35--) {
                    if (Main.tile[i, num35].BottomSlope) {
                        flag5 = false;
                        break;
                    }

                    ushort[] validTiles = [(ushort)ModContent.TileType<BackwoodsGrass>(), (ushort)ModContent.TileType<LivingElderwoodlLeaves>()];
                    if (Main.tile[i, num35].HasTile && validTiles.Contains(Main.tile[i, num35].TileType) && !Main.tile[i, num35].BottomSlope) {
                        flag5 = true;
                        break;
                    }
                }

                if (flag5) {
                    int num36 = j + 1;
                    Tile tile = Main.tile[i, num36];
                    if (Main.tile[i, num36 + 1].HasTile) {
                        return;
                    }
                    Main.tile[i, num36].TileType = type7;
                    tile.HasTile = true;
                    Main.tile[i, num36].CopyPaintAndCoating(Main.tile[i, j]);
                    WorldGen.SquareTileFrame(i, num36);
                    if (Main.netMode == 2)
                        NetMessage.SendTileSquare(-1, i, num36);
                }
            }
        }
    }
}