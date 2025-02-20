using System.Collections.Generic;

using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.World.Generations;

sealed class BackwoodsWorldGen : ModSystem {
    private const float LAYERWEIGHT = 5000f;

    public static BackwoodsBiomePass BackwoodsWorldGenPass { get; private set; }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Corruption"));
        genIndex += 6;
        tasks.Insert(genIndex, BackwoodsWorldGenPass = new("Backwoods", LAYERWEIGHT));
        genIndex += 3;
        tasks.Insert(genIndex, new PassLegacy("Backwoods 1", BackwoodsWorldGenPass.BackwoodsLootRooms, 1500f));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Smooth World"));
        genIndex -= 2;
        tasks.Insert(genIndex, new PassLegacy("Backwoods 2", BackwoodsWorldGenPass.BackwoodsCleanup, 600f));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
        genIndex -= 3;
        tasks.Insert(genIndex, new PassLegacy("Backwoods 3", BackwoodsWorldGenPass.BackwoodsOtherPlacements, 3000f));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
        genIndex += 1;
        tasks.Insert(genIndex, new PassLegacy("Backwoods 4", BackwoodsWorldGenPass.BackwoodsTilesReplacement));

        tasks.Insert(tasks.Count - 4, new PassLegacy("Backwoods 5", BackwoodsWorldGenPass.BackwoodsOnLast0));
        tasks.Insert(tasks.Count - 2, new PassLegacy("Backwoods 6", BackwoodsWorldGenPass.BackwoodsOnLast));

        tasks.Add(new PassLegacy("Backwoods 7", BackwoodsWorldGenPass.BackwoodsOnLast1));
    }
}