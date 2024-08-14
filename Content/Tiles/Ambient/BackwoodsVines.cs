using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsVines : ModTile {
    public override void SetStaticDefaults() {
        Main.tileLighted[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileID.Sets.VineThreads[Type] = true;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
        HitSound = SoundID.Grass;
        AddMapEntry(new Color(46, 130, 69));
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
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
            WorldGen.KillTile(i, j + 1);
        }
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        Tile tileAbove = Framing.GetTileSafely(i, j - 1);
        int type = -1;
        if (tileAbove.HasTile && !tileAbove.BottomSlope) {
            type = tileAbove.TileType;
        }

        if (type == ModContent.TileType<BackwoodsGrass>() || type == ModContent.TileType<LivingElderwoodlLeaves>() || type == Type) {
            return true;
        }

        WorldGen.KillTile(i, j);
        return true;
    }

    public override void RandomUpdate(int i, int j) {
        Tile tileBelow = WorldGenHelper.GetTileSafely(i, j + 1);
        if (WorldGen.genRand.NextBool(12) && !tileBelow.HasTile && tileBelow.LiquidType != LiquidID.Lava) {
            bool placed = false;
            int j2 = j;
            while (j2 > j - 12) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j2);
                if (tile.BottomSlope) {
                    break;
                }

                else if (!tile.HasTile || tile.TileType != ModContent.TileType<BackwoodsGrass>() || tile.TileType != ModContent.TileType<LivingElderwoodlLeaves>()) {
                    j2--;

                    continue;
                }

                placed = true;
                break;
            }

            if (placed) {
                tileBelow.TileType = Type;
                tileBelow.HasTile = true;
                WorldGen.SquareTileFrame(i, j + 1, true);
                if (Main.netMode == NetmodeID.Server) {
                    NetMessage.SendTileSquare(-1, i, j + 1, 3, TileChangeType.None);
                }
            }
        }
    }
}