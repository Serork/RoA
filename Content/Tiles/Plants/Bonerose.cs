using RoA.Common.Tiles;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Plants;

sealed class Bonerose : PlantBase, TileHooks.IGlobalRandomUpdate {
    protected override void SafeSetStaticDefaults() {
        AddMapEntry(new(178, 178, 137), CreateMapEntryName());

        DustType = DustID.Bone;
        HitSound = SoundID.NPCHit1;

        DropItem = (ushort)ModContent.ItemType<Items.Materials.Bonerose>();

        RootsDrawing.ShouldDraw[Type] = true;
    }

    public void OnGlobalRandomUpdate(int i, int j) {
        if (j < Main.worldSurface) {
            return;
        }

        if (!Main.wallDungeon[Main.tile[i, j].WallType]/* || !WorldGen.genRand.NextBool(150)*/) {
            return;
        }

        TryPlacePlant(i, j, WorldGen.gen ? 2 : 0,  TileID.PinkDungeonBrick, TileID.GreenDungeonBrick, TileID.BlueDungeonBrick);
    }
}
