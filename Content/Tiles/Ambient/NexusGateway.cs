using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class NexusGateway : ModTile {
    public override void SetStaticDefaults() {
        Main.tileLavaDeath[Type] = false;
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = false;
        Main.tileSolid[Type] = false;

        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.LavaDeath = false;
        int width = 7, height = 9;
        TileObjectData.newTile.Width = width;
        TileObjectData.newTile.Height = height;
        TileObjectData.newTile.CoordinateHeights = new int[height];
        for (int k = 0; k < height; k++) {
            TileObjectData.newTile.CoordinateHeights[k] = 16;
        }
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.addTile(Type);

        AddMapEntry(Color.White, CreateMapEntryName());

        DustType = ModContent.DustType<BackwoodsPotDust1>();
        HitSound = SoundID.Tink;

        TileID.Sets.DisableSmartCursor[Type] = true;
    }

    public override bool CanExplode(int i, int j) => false;

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
}
