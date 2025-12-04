using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class Crates : ModTile {
    public override void SetStaticDefaults() {
        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileTable[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(160, 120, 92));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;
}