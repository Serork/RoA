using Microsoft.Xna.Framework;

using RoA.Content.Dusts.Backwoods;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Furniture;

sealed class ElderwoodTable : ModTile {
	public override void SetStaticDefaults() {
		Main.tileSolidTop[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileTable[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileObjectData.newTile.DrawYOffset = 2;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        DustType = ModContent.DustType<WoodTrash>();
        AdjTiles = new int[] { TileID.Tables };

		LocalizedText name = CreateMapEntryName();
		// name.SetDefault("Elderwood Sink");
		AddMapEntry(new Color(111, 22, 22));

		TileID.Sets.DisableSmartCursor[Type] = true;
		TileID.Sets.IgnoredByNpcStepUp[Type] = true;
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

	public override void KillMultiTile(int i, int j, int TileFrameX, int TileFrameY) {
		//int item = Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 34, 38, ModContent.ItemType<Items.Placeable.Furniture.ElderwoodTable>(), 1, false, 0, false, false);
		//if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
		//	NetMessage.SendData(21, -1, -1, null, item, 1f, 0f, 0f, 0, 0, 0);
	}
}