using Microsoft.Xna.Framework;

using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class GobletOfPain : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(121, 96, 156), CreateMapEntryName());
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Equipables.Accessories.GobletOfPain>());
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;
}
