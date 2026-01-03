using Microsoft.Xna.Framework;

using RoA.Content.Dusts.Backwoods;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class BrokenBookcase : ModTile {
    public override void SetStaticDefaults() {
        Main.tileSolidTop[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 18];
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 2;
        TileObjectData.addTile(Type);

        DustType = ModContent.DustType<WoodTrash>();
        AdjTiles = [TileID.Bookcases];
        AddMapEntry(new Color(191, 142, 111), Language.GetText("ItemName.Bookcase"));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Miscellaneous.BrokenBookcase>());
    }
}