using RoA.Common.Tiles;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

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

        TryPlacePlant(i, j, WorldGen.gen ? WorldGen.genRand.Next(WorldGen.genRand.NextBool() ? 3 : WorldGen.genRand.Next(2)) : 0, TileID.SnowBlock, TileID.IceBlock);
    }
}
