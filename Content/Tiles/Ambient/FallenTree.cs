using RoA.Content.Tiles.Solid.Backwoods;

using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria;
using Terraria.Enums;

using RoA.Content.Dusts;

namespace RoA.Content.Tiles.Ambient;

sealed class FallenTree : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileAxe[Type] = true;

        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.CoordinateHeights = [16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        DustType = ModContent.DustType<Dusts.Backwoods.WoodTrash>();
        AddMapEntry(new Microsoft.Xna.Framework.Color(91, 74, 67), CreateMapEntryName());
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 5;
}
