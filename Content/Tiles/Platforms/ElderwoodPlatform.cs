using Microsoft.Xna.Framework;

using RoA.Content.Dusts.Backwoods;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Furniture;

sealed class ElderwoodPlatform : ModTile {
	public override void SetStaticDefaults() {
		Main.tileLighted[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileSolidTop[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileTable[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileID.Sets.Platforms[Type] = true;

		TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
		TileObjectData.newTile.CoordinateWidth = 16;
		TileObjectData.newTile.CoordinatePadding = 2;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.StyleMultiplier = 27;
		TileObjectData.newTile.StyleWrapLimit = 27;
		TileObjectData.newTile.UsesCustomCanPlace = false;
		TileObjectData.newTile.LavaDeath = true;
		TileObjectData.addTile(Type);

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

		DustType = ModContent.DustType<WoodTrash>();
		AdjTiles = new int[] { TileID.Platforms };

		LocalizedText name = CreateMapEntryName();
		// name.SetDefault("Elderwood Platform");
		AddMapEntry(new Color(111, 22, 22));

		//ItemDrop = ModContent.ItemType<Items.Placeable.Furniture.ElderwoodPlatform>();

		TileID.Sets.DisableSmartCursor[Type] = true;
	}

	public override void PostSetDefaults()
		=> Main.tileNoSunLight[Type] = false;
}