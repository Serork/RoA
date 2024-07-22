using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace RoA.Common.Common;

[Autoload(Side = ModSide.Client)]
sealed class BackwoodsVars : ModSystem {
    public static int FirstTileYAtCenter { get; internal set; }
    public static int FirstTileYAtCenterBackground { get; internal set; }
    public static float BackgroundOffset { get; internal set; }

    public override void ClearWorld() => ResetAllFlags();

    public override void SaveWorldData(TagCompound tag) {
        tag[nameof(FirstTileYAtCenter)] = FirstTileYAtCenter;
        tag[nameof(FirstTileYAtCenterBackground)] = FirstTileYAtCenterBackground;
        tag[nameof(BackgroundOffset)] = BackgroundOffset;
    }

    public override void LoadWorldData(TagCompound tag) {
        FirstTileYAtCenter = tag.GetInt(nameof(FirstTileYAtCenter));
        FirstTileYAtCenterBackground = tag.GetInt(nameof(FirstTileYAtCenterBackground));
        BackgroundOffset = tag.GetFloat(nameof(BackgroundOffset));
    }

    private static void ResetAllFlags() {
        FirstTileYAtCenter = FirstTileYAtCenterBackground = 0;
        BackgroundOffset = 0f;
    }
}
