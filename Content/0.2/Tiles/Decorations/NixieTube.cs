using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class NixieTube : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileLighted[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
        TileObjectData.newTile.CoordinateWidth = 16;
        //TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity<NixieTubeEntity>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleWrapLimit = 36;
        TileObjectData.addTile(Type);

        DustType = DustID.WoodFurniture;

        AddMapEntry(new Color(153, 38, 0), CreateMapEntryName());
    }
}
