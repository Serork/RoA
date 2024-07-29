using System.Collections.Generic;

using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.World.Generations;

sealed class BackwoodsWorldGen : ModSystem {
    private const string GENLAYERNAME = "Backwoods", GENLAYERNAME2 = "Backwoods: Clean up", GENLAYERNAME3 = "Backwoods: Extra placements";
    private const float LAYERWEIGHT = 600f;

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        BackwoodsBiomePass backwoodsWorldGen;

        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Corruption"));
        genIndex += 7;
        tasks.Insert(genIndex, backwoodsWorldGen = new(GENLAYERNAME, LAYERWEIGHT));
        genIndex += 3;
        tasks.Insert(genIndex, new PassLegacy(GENLAYERNAME3, backwoodsWorldGen.BackwoodsLootRooms));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Smooth World"));
        genIndex -= 2;
        tasks.Insert(genIndex, new PassLegacy(GENLAYERNAME2, backwoodsWorldGen.BackwoodsCleanup));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
        genIndex -= 3;
        tasks.Insert(genIndex, new PassLegacy(GENLAYERNAME3, backwoodsWorldGen.BackwoodsOtherPlacements));
    }
}