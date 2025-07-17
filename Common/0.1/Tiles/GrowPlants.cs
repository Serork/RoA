using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Common.Tiles;

sealed class GrowPlants : ILoadable {
    public void Load(Mod mod) {
        On_WorldGen.PlantAlch += On_WorldGen_PlantAlch;
    }

    public void Unload() { }

    private void On_WorldGen_PlantAlch(On_WorldGen.orig_PlantAlch orig) {
        UnifiedRandom genRand = WorldGen.genRand;
        int num = genRand.Next(20, Main.maxTilesX - 20);
        int num2 = 0;
        for (num2 = (Main.remixWorld ? genRand.Next(20, Main.maxTilesY - 20) : ((genRand.Next(40) == 0) ? genRand.Next((int)(Main.rockLayer + (double)Main.maxTilesY) / 2, Main.maxTilesY - 20) : ((genRand.Next(10) != 0) ? genRand.Next((int)Main.worldSurface, Main.maxTilesY - 20) : genRand.Next(20, Main.maxTilesY - 20)))); num2 < Main.maxTilesY - 20 && !Main.tile[num, num2].HasTile; num2++) {
        }
        int i = num, j = num2;
        if (Main.tile[i, j].HasUnactuatedTile && !Main.tile[i, j - 1].HasTile && !Main.tile[i, j - 1].AnyLiquid()) {
            for (int k = TileID.Count; k < TileLoader.TileCount; k++) {
                if (TileLoader.GetTile(k) is TileHooks.IGrowAlchPlantRandom growRandomlyTile) {
                    growRandomlyTile.OnGlobalRandomUpdate(i, j);
                }
            }
        }

        orig();
    }
}
