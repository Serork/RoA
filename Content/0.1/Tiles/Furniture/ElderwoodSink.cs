using Microsoft.Xna.Framework;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Furniture;

sealed class ElderwoodSink : ModTile {
    public override void SetStaticDefaults() {
        TileID.Sets.CountsAsWaterSource[Type] = true;

        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.CoordinateHeights = [18, 18];
        TileObjectData.newTile.Direction = Terraria.Enums.TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = Terraria.Enums.TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(191, 142, 111), Terraria.Localization.Language.GetText("MapObject.Sink"));

        AdjTiles = [TileID.Sinks];

        RegisterItemDrop(ModContent.ItemType<Items.Placeable.Furniture.ElderwoodSink>());
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Furniture.ElderwoodSink>());
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;
}