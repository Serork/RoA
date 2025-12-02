using RoA.Content.Items.Miscellaneous;
using RoA.Content.Tiles.Furniture;

using System.Collections.Generic;

using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

sealed class BackwoodsDungeonChestGen_Remnants : ModSystem {
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        if (!ModLoader.HasMod("Remnants")) {
            return;
        }

        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
        if (genIndex != -1) {
            tasks.Insert(genIndex + 30, new BackwoodsDungeonChestPass("Backwoods Dungeon Chest", 12.8337f));
        }
    }

    private class BackwoodsDungeonChestPass(string name, float loadWeight) : GenPass(name, loadWeight) {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
            while (true) {
                int i = WorldGen.genRand.Next(0, Main.maxTilesX);
                int j = WorldGen.genRand.Next((int)GenVars.worldSurfaceLow, Main.maxTilesY);
                if (Main.wallDungeon[Main.tile[i, j].WallType]) {
                    ushort chestTileType = (ushort)ModContent.TileType<BackwoodsDungeonChest>();
                    int contain = ModContent.ItemType<IOU>();
                    if (WorldGen.AddBuriedChest(i, j - 1, contain, notNearOtherChests: false, 0, trySlope: false, chestTileType)) {
                        break;
                    }
                }
            }
        }
    }
}
