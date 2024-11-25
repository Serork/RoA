using Microsoft.Xna.Framework;
using RoA.Content.Dusts;
using RoA.Content.Dusts.Backwoods;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Furniture;

sealed class ElderwoodBookcase : ModTile {
	public override void SetStaticDefaults() {
		Main.tileSolidTop[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileTable[Type] = true;
		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
		TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
		TileID.Sets.DisableSmartCursor[Type] = true;
		TileObjectData.addTile(Type);

        DustType = ModContent.DustType<WoodTrash>();
        AdjTiles = new int[] { TileID.Bookcases };

		LocalizedText name = CreateMapEntryName();
		// name.SetDefault("Elderwood Bookcase");
		AddMapEntry(new Color(111, 22, 22));
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = (fail ? 8 : 3);

	public override void KillMultiTile(int i, int j, int TileFrameX, int TileFrameY) {
		//int item = Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 34, 38, ModContent.ItemType<Items.Placeable.Furniture.ElderwoodBookcase>(), 1, false, 0, false, false);
		//if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
		//	NetMessage.SendData(21, -1, -1, null, item, 1f, 0f, 0f, 0, 0, 0);
	}
}