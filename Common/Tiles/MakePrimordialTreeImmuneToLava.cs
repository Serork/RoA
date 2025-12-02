using RoA.Content.Tiles.Ambient.LargeTrees;
using RoA.Content.Tiles.Trees;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Common.Tiles;

sealed class MakePrimordialTreeImmuneToLava : ILoadable {
    void ILoadable.Load(Mod mod) {
        On_WorldGen.WaterCheck += On_WorldGen_WaterCheck;
        On_Liquid.AddWater += On_Liquid_AddWater;
    }

    // also look for On_Liquid_DelWater in BackwoodsLilypad
    private bool CheckTile(int checkX, int checkY) {
        bool result = false;
        if (Main.tile[checkX, checkY].HasTile) {
            if ((Main.tile[checkX, checkY].TileType == TileID.Trees && PrimordialTree.IsPrimordialTree(checkX, checkY) && !NPC.downedBoss2) || (Main.tile[checkX, checkY].TileType == ModContent.TileType<BackwoodsBigTree>() && !Main.hardMode)) {
                result = true;
            }
        }
        return result;
    }

    private void On_Liquid_AddWater(On_Liquid.orig_AddWater orig, int x, int y) {
        Tile tile = Main.tile[x, y];
        if (Main.tile[x, y] == null || tile.CheckingLiquid || x >= Main.maxTilesX - 5 || y >= Main.maxTilesY - 5 || x < 5 || y < 5 || tile.LiquidAmount == 0 || (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && tile.TileType != 546 && !Main.tileSolidTop[tile.TileType]))
            return;

        if (Liquid.numLiquid >= Liquid.curMaxLiquid - 1) {
            LiquidBuffer.AddBuffer(x, y);
            return;
        }

        tile.CheckingLiquid = true;
        tile.SkipLiquid = false;
        Main.liquid[Liquid.numLiquid].kill = 0;
        Main.liquid[Liquid.numLiquid].x = x;
        Main.liquid[Liquid.numLiquid].y = y;
        Main.liquid[Liquid.numLiquid].delay = 0;
        Liquid.numLiquid++;
        if (Main.netMode == 2)
            Liquid.NetSendLiquid(x, y);

        if (!tile.HasTile || WorldGen.gen)
            return;

        bool flag = false;
        if (tile.LiquidType == LiquidID.Lava) {
            if (TileObjectData.CheckLavaDeath(tile) && !CheckTile(x, y))
                flag = true;
        }
        else if (TileObjectData.CheckWaterDeath(tile)) {
            flag = true;
        }

        if (flag) {
            WorldGen.KillTile(x, y);
            if (Main.netMode == 2)
                NetMessage.SendData(17, -1, -1, null, 0, x, y);
        }

        return;

        //Tile tile = Main.tile[x, y];
        //if (Main.tile[x, y] == null || tile.CheckingLiquid || x >= Main.maxTilesX - 5 || y >= Main.maxTilesY - 5 || x < 5 || y < 5 || tile.LiquidAmount == 0 || (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && tile.TileType != 546 && !Main.tileSolidTop[tile.TileType]))
        //    return;

        //if (Liquid.numLiquid >= Liquid.curMaxLiquid - 1) {
        //    LiquidBuffer.AddBuffer(x, y);
        //    return;
        //}

        //tile.CheckingLiquid = true;
        //tile.SkipLiquid = false;
        //Main.liquid[Liquid.numLiquid].kill = 0;
        //Main.liquid[Liquid.numLiquid].x = x;
        //Main.liquid[Liquid.numLiquid].y = y;
        //Main.liquid[Liquid.numLiquid].delay = 0;
        //Liquid.numLiquid++;
        //if (Main.netMode == 2)
        //    Liquid.NetSendLiquid(x, y);

        //if (!tile.HasTile || WorldGen.gen)
        //    return;

        //bool flag = false;
        //if (tile.LiquidAmount == LiquidID.Lava) {
        //    if (TileObjectData.CheckLavaDeath(tile) && CheckTile(x, y))
        //        flag = true;
        //}
        //else if (TileObjectData.CheckWaterDeath(tile)) {
        //    flag = true;
        //}

        //if (CheckTile(x, y)) {
        //    flag = false;
        //}

        //if (flag) {
        //    WorldGen.KillTile(x, y);
        //    if (Main.netMode == 2)
        //        NetMessage.SendData(17, -1, -1, null, 0, x, y);
        //}
    }

    private void On_WorldGen_WaterCheck(On_WorldGen.orig_WaterCheck orig) {
        Liquid.tilesIgnoreWater(ignoreSolids: true);
        Liquid.numLiquid = 0;
        LiquidBuffer.numLiquidBuffer = 0;
        for (int i = 1; i < Main.maxTilesX - 1; i++) {
            for (int num = Main.maxTilesY - 2; num > 0; num--) {
                Tile tile = Main.tile[i, num];
                tile.CheckingLiquid = false;
                if (tile.LiquidAmount > 0 && tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) {
                    tile.LiquidAmount = 0;
                }
                else if (tile.LiquidAmount > 0) {
                    if (tile.HasTile) {
                        if (tile.LiquidType == LiquidID.Lava) {
                            if (TileObjectData.CheckLavaDeath(tile) && CheckTile(i, num)) {
                                WorldGen.KillTile(i, num);
                            }
                        }
                        else if (TileObjectData.CheckWaterDeath(tile)) {
                            WorldGen.KillTile(i, num);
                        }
                    }

                    Tile tile2 = Main.tile[i, num + 1];
                    if ((!tile2.HasUnactuatedTile || !Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType]) && tile2.LiquidAmount < byte.MaxValue) {
                        if (tile2.LiquidAmount > 250)
                            tile2.LiquidAmount = byte.MaxValue;
                        else
                            Liquid.AddWater(i, num);
                    }

                    Tile tile3 = Main.tile[i - 1, num];
                    Tile tile4 = Main.tile[i + 1, num];
                    if ((!tile3.HasUnactuatedTile || !Main.tileSolid[tile3.TileType] || Main.tileSolidTop[tile3.TileType]) && tile3.LiquidAmount != tile.LiquidAmount)
                        Liquid.AddWater(i, num);
                    else if ((!tile4.HasUnactuatedTile || !Main.tileSolid[tile4.TileType] || Main.tileSolidTop[tile4.TileType]) && tile4.LiquidAmount != tile.LiquidAmount)
                        Liquid.AddWater(i, num);

                    if (tile.LiquidType == LiquidID.Lava) {
                        if (tile3.LiquidAmount > 0 && tile3.LiquidType != LiquidID.Lava)
                            Liquid.AddWater(i, num);
                        else if (tile4.LiquidAmount > 0 && tile4.LiquidType != LiquidID.Lava)
                            Liquid.AddWater(i, num);
                        else if (Main.tile[i, num - 1].LiquidAmount > 0 && Main.tile[i, num - 1].LiquidType != LiquidID.Lava)
                            Liquid.AddWater(i, num);
                        else if (tile2.LiquidAmount > 0 && tile2.LiquidType != LiquidID.Lava)
                            Liquid.AddWater(i, num);
                    }
                }
            }
        }

        Liquid.tilesIgnoreWater(ignoreSolids: false);
    }

    void ILoadable.Unload() { }
}
