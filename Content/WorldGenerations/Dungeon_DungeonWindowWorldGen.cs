using RoA.Common.World;
using RoA.Core;

using Terraria.ModLoader;

namespace RoA.Content.WorldGenerations;

sealed class DungeonWindowWorldGen : IInitializer {
    void ILoadable.Load(Mod mod) {
        WorldCommon.ModifyWorldGenTasksEvent += WorldCommon_ModifyWorldGenTasksEvent;
    }

    private void WorldCommon_ModifyWorldGenTasksEvent(System.Collections.Generic.List<Terraria.WorldBuilding.GenPass> tasks, ref double totalWeight) {
        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Underworld"));
    }
}
