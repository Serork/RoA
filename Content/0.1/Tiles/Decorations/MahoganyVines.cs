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

namespace RoA.Content.Tiles.Decorations;

class MahoganyVines : ModTile {
    private static readonly ushort[] ValidTilesToGrowFrom = [TileID.LivingMahoganyLeaves];

    public override void SetStaticDefaults() {
        Main.tileLighted[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileID.Sets.IsVine[Type] = true;
        TileID.Sets.VineThreads[Type] = true;
        TileID.Sets.ReplaceTileBreakDown[Type] = true;

        DustType = DustID.JunglePlants;
        HitSound = SoundID.Grass;
        AddMapEntry(new Color(121, 176, 24));
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
                ushort type7 = (ushort)ModContent.TileType<MahoganyVines>();

                //if (Main.remixWorld && genRand.Next(5) == 0)
                //    type7 = 382;

                for (int num35 = j; num35 > j - 10; num35--) {
                    if (Main.tile[i, num35].BottomSlope) {
                        flag5 = false;
                        break;
                    }

                    ushort[] validTiles = ValidTilesToGrowFrom;
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