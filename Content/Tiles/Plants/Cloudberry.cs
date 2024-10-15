using RoA.Common.Tiles;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Plants;

sealed class Cloudberry : PlantBase, TileHooks.IGlobalRandomUpdate {
    protected override void SafeSetStaticDefaults() {
        AddMapEntry(new(235, 150, 12), CreateMapEntryName());

        DustType = DustID.Shiverthorn;

        DropItem = (ushort)ModContent.ItemType<Items.Materials.Cloudberry>();
    }

    public void OnGlobalRandomUpdate(int i, int j) {
        if (j >= Main.worldSurface) {
            return;
        }

        if (WorldGen.genRand.NextBool()) {
            return;
        }

        TryPlacePlant(i, j, WorldGen.gen ? WorldGen.genRand.Next(3) : 0, TileID.SnowBlock);
    }
}
