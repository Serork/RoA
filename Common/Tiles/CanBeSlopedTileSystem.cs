using Terraria.ModLoader;
using Terraria.ID;
using Terraria;

namespace RoA.Common.Tiles;

sealed class CanBeSlopedTileSystem : ModSystem {
    public static bool[] Included = TileID.Sets.Factory.CreateBoolSet();

    public override void Load() {
        On_Player.ItemCheck_UseMiningTools_TryPoundingTile += On_Player_ItemCheck_UseMiningTools_TryPoundingTile;
    }

    private void On_Player_ItemCheck_UseMiningTools_TryPoundingTile(On_Player.orig_ItemCheck_UseMiningTools_TryPoundingTile orig, Player self, Item sItem, int tileHitId, ref bool hitWall, int x, int y) {
        Tile tile = Main.tile[x, y];
        if (!Included[tile.TileType]) {
            orig(self, sItem, tileHitId, ref hitWall, x, y);
        }
    }
}