using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Tiles;

sealed class CantBeSlopedTileSystem : ILoadable {
    public static bool[] Included = TileID.Sets.Factory.CreateBoolSet();

    public void Load(Mod mod) {
        On_Player.ItemCheck_UseMiningTools_TryPoundingTile += On_Player_ItemCheck_UseMiningTools_TryPoundingTile;
    }

    public void Unload() { }

    private void On_Player_ItemCheck_UseMiningTools_TryPoundingTile(On_Player.orig_ItemCheck_UseMiningTools_TryPoundingTile orig, Player self, Item sItem, int tileHitId, ref bool hitWall, int x, int y) {
        Tile tile = Main.tile[x, y];
        if (!Included[tile.TileType]) {
            orig(self, sItem, tileHitId, ref hitWall, x, y);
        }
    }
}