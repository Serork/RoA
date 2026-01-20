using Microsoft.Xna.Framework;

using ModLiquidLib.ModLoader;

using RoA.Content.Liquids;
using RoA.Content.Tiles.Ambient;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Content.Tiles.LiquidsSpecific;

sealed class SolidifiedTar : ModTile {
    public override void SetStaticDefaults() {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileMergeDirt[Type] = true;

        DustType = ModContent.DustType<Dusts.SolidifiedTar>();

        AddMapEntry(new Color(68, 57, 77));
    }

    public override void RandomUpdate(int i, int j) {
        if (Main.tile[i, j].HasUnactuatedTile) {
            UnifiedRandom unifiedRandom = (WorldGen.gen ? WorldGen.genRand : Main.rand);
            if (!Main.tile[i, j].HasTile || i < 95 || i > Main.maxTilesX - 95 || j < 95 || j > Main.maxTilesY - 95)
                return;

            var tarLiquid = LiquidLoader.LiquidType<Tar>();
            if (!Main.tile[i, j - 3].HasTile && Main.tile[i, j - 3].LiquidType == tarLiquid && Main.tile[i, j - 3].LiquidAmount == 255 &&
                !Main.tile[i, j - 2].HasTile && Main.tile[i, j - 2].LiquidType == tarLiquid && Main.tile[i, j - 2].LiquidAmount == 255 &&
                Main.tile[i, j - 1].LiquidType == tarLiquid && Main.tile[i, j - 1].LiquidAmount == 255) {
                if (!Main.tile[i, j - 1].HasTile) {
                    ushort swellingTarTileType = (ushort)ModContent.TileType<SwellingTar>();
                    int check = 4;
                    bool flag = false;
                    for (int k2 = -check; k2 < check; k2++) {
                        if (flag) {
                            break;
                        }
                        for (int j2 = -check; j2 <= check; j2++) {
                            Tile tile2 = Main.tile[i + j2, j - 1 + k2];
                            if (tile2.TileType == swellingTarTileType) {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag) {
                        if (Main.rand.Next(2500) == 0) {
                            WorldGenHelper.Place2x2(i, j - 1, swellingTarTileType, WorldGen.genRand.NextBool().ToInt(), 0);
                            if (Main.tile[i - 2, j - 2].TileType != swellingTarTileType) {
                                ModContent.GetInstance<SwellingTarTE>().Place(i - 1, j - 2);
                            }
                        }
                    }
                }
            }
        }
    }
}
