using RoA.Common.Tiles;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Plants;

sealed class Cloudberry : PlantBase, TileHooks.IGlobalRandomUpdate {
    protected override int PlantDrop => DropItem;

    protected override int[] AnchorValidTiles => [TileID.SnowBlock];

    protected override void SafeSetStaticDefaults() {
        AddMapEntry(new(235, 150, 12), CreateMapEntryName());

        DustType = DustID.Shiverthorn;
        HitSound = SoundID.Grass;

        DropItem = (ushort)ModContent.ItemType<Items.Materials.Cloudberry>();
    }

    void TileHooks.IGlobalRandomUpdate.OnGlobalRandomUpdate(int i, int j) {
        if (j >= Main.worldSurface) {
            return;
        }

        if (WorldGen.genRand.NextBool()) {
            return;
        }

        TryPlacePlant(i, j, WorldGen.gen ? WorldGen.genRand.Next(WorldGen.genRand.NextBool() ? 0 : WorldGen.genRand.NextBool() ? 1 : 2) : 0, AnchorValidTiles);
    }
}
