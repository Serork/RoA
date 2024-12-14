using Microsoft.Xna.Framework;

using RoA.Content.Dusts.Backwoods;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Crafting;

sealed class Elderwood : ModTile {
	public override void SetStaticDefaults() {
		Main.tileSolid[Type] = true;
		Main.tileMergeDirt[Type] = true;
		Main.tileMerge[Type][TileID.Dirt] = true;
		Main.tileMerge[Type][TileID.Grass] = true;
		Main.tileMerge[TileID.Grass][Type] = true;
		Main.tileBlendAll[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileLighted[Type] = true;

        DustType = (ushort)ModContent.DustType<WoodTrash>();
        AddMapEntry(new Color(162, 82, 45), CreateMapEntryName());
	}

	public override IEnumerable<Item> GetItemDrops(int i, int j) {
		yield return new Item(ModContent.ItemType<Items.Placeable.Crafting.Elderwood>());
	}
}