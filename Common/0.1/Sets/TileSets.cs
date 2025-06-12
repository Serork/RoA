using RoA.Content.Tiles.Ambient.LargeTrees;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Sets;

sealed class TileSets : ModSystem {
    private class PreventsSlopesBelowSystem : GlobalTile {
        public override bool Slope(int i, int j, int type) {
            Tile aboveTile = Framing.GetTileSafely(i, j - 1);
            if (!aboveTile.HasTile) {
                return true;
            }
            if (PreventsSlopesBelow[aboveTile.TileType]) {
                return false;
            }
            return true;
        }
    }

    public static bool[] ShouldKillTileBelow = TileID.Sets.Factory.CreateBoolSet(true);
    public static bool[] CanPlayerMineMe = TileID.Sets.Factory.CreateBoolSet(true);
    public static bool[] PreventsSlopesBelow = TileID.Sets.Factory.CreateBoolSet(false);

    public static HashSet<ushort> Paintings = [];

    public override void Load() {
        On_Player.PickTile += On_Player_PickTile;
    }

    private void On_Player_PickTile(On_Player.orig_PickTile orig, Player self, int x, int y, int pickPower) {
        bool valid() {
            int i = x, j = y;
            Tile tile = Main.tile[i, j];
            if (i < 0 || j < 0 || i >= Main.maxTilesX || j >= Main.maxTilesY)
                return false;

            Tile tile2 = default;

            if (!tile.HasTile)
                return false;

            if (j >= 1)
                tile2 = Main.tile[i, j - 1];

            if (tile2.HasTile) {
                int type = tile2.TileType;
                if (!ShouldKillTileBelow[type] && (type != ModContent.TileType<BackwoodsBigTree>() ||
                    (type == ModContent.TileType<BackwoodsBigTree>() && BackwoodsBigTree.IsStart(i, j - 1)))) {
                    if (tile.TileType != type) {
                        return false;
                    }
                }
            }

            return true;
        }
        if (valid() && CanPlayerMineMe[WorldGenHelper.GetTileSafely(x, y).TileType]) {
            orig(self, x, y, pickPower);
        }
    }

    public override void PostSetupContent() {
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
