using System.Collections.Generic;

using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.World.Generations;
sealed class BackwoodsWorldGen : ModSystem {
    private const string GENLAYERNAME = "Backwoods", GENLAYERNAME2 = "Backwoods: Clean up", GENLAYERNAME3 = "Backwoods: Extra placements";

    private BackwoodsBiomePass BackwoodsWorldGenPass { get; set; }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Corruption"));
        genIndex += 6;
        tasks.Insert(genIndex, BackwoodsWorldGenPass = new(GENLAYERNAME, 600f));
        tasks.Insert(genIndex, new PassLegacy(GENLAYERNAME, BackwoodsWorldGenPass.GrowBackwoods, 1200f));
        genIndex += 3;
        tasks.Insert(genIndex, new PassLegacy(GENLAYERNAME3, BackwoodsWorldGenPass.BackwoodsLootRooms, 800f));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Smooth World"));
        genIndex -= 2;
        tasks.Insert(genIndex, new PassLegacy(GENLAYERNAME2, BackwoodsWorldGenPass.BackwoodsCleanup, 600f));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
        genIndex -= 3;
        tasks.Insert(genIndex, new PassLegacy(GENLAYERNAME3, BackwoodsWorldGenPass.BackwoodsOtherPlacements, 1200f));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
        genIndex += 4;
        tasks.Insert(genIndex, new PassLegacy(GENLAYERNAME3, BackwoodsWorldGenPass.BackwoodsTilesReplacement));
    }
}