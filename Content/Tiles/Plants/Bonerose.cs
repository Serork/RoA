using RoA.Common.Tiles;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Plants;

sealed class Bonerose : PlantBase, TileHooks.IGrowPlantRandom {
    protected override int PlantDrop => DropItem;

    protected override int[] AnchorValidTiles => [TileID.PinkDungeonBrick, TileID.GreenDungeonBrick, TileID.BlueDungeonBrick];

    protected override void SafeSetStaticDefaults() {
        AddMapEntry(new(178, 178, 137), CreateMapEntryName());

        DustType = DustID.Bone;
        HitSound = SoundID.NPCHit2;

        DropItem = (ushort)ModContent.ItemType<Items.Materials.Bonerose>();

        RootsDrawing.ShouldDraw[Type] = true;
    }

    void TileHooks.IGrowPlantRandom.OnGlobalRandomUpdate(int i, int j) {
        if (j < Main.worldSurface && !WorldGen.remixWorldGen) {
            return;
        }

        if (!Main.wallDungeon[Main.tile[i, j].WallType]) {
            return;
        }

        if (WorldGen.gen) {
            return;
        }

        if (!NPC.downedBoss3) {
            return;
        }

        TryPlacePlant(i, j, Type, 0, checkRadius: 25, maxAlchNearby: 2, validTiles: [TileID.PinkDungeonBrick, TileID.GreenDungeonBrick, TileID.BlueDungeonBrick]);
    }
}
