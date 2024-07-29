using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common;

[Autoload(Side = ModSide.Client)]
sealed class BackwoodsVars : ModSystem {
    public static int FirstTileYAtCenter { get; internal set; }
    public static int BackwoodsTileForBackground { get; internal set; }

    public override void ClearWorld() => ResetAllFlags();

    public override void SaveWorldData(TagCompound tag) {
        tag[nameof(FirstTileYAtCenter)] = FirstTileYAtCenter;
        tag[nameof(BackwoodsTileForBackground)] = BackwoodsTileForBackground;
    }

    public override void LoadWorldData(TagCompound tag) {
        FirstTileYAtCenter = tag.GetInt(nameof(FirstTileYAtCenter));
        BackwoodsTileForBackground = tag.GetInt(nameof(BackwoodsTileForBackground));
    }

    private static void ResetAllFlags() {
        FirstTileYAtCenter = BackwoodsTileForBackground = 0;
    }
}
