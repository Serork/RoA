using System;
using System.Collections.Generic;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Sets;

sealed class TileSets : ModSystem {
    public static HashSet<ushort> Paintings;

    public override void PostSetupContent() {
        Paintings = [];
        for (ushort type = 1; type < TileLoader.TileCount; type++) {
            if (TileID.Sets.FramesOnKillWall[type]) {
                Paintings.Add(type);
            }
        }
    }

    public override void Unload() {
        Paintings.Clear();
        Paintings = null;
    }
}
