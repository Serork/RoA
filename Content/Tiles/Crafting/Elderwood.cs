using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Crafting;

sealed class Elderwood : ModTile {
    public override void SetStaticDefaults() {
        TileHelper.Solid(Type, false, false);
        Main.tileMergeDirt[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileLighted[Type] = true;

        TileID.Sets.ChecksForMerge[Type] = true;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Furniture>();
        AddMapEntry(new Color(162, 82, 45), CreateMapEntryName());

        MineResist = 1.5f;
        MinPick = 55;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Solid.Elderwood>());
    }
}