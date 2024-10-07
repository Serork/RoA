using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts.Backwoods;
using RoA.Core;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Furniture;

sealed class ElderwoodChandelier : ModTile {
	public override void SetStaticDefaults() {
		Main.tileLighted[Type] = true;
		Main.tileFrameImportant[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
		TileObjectData.newTile.Origin = new Point16(1, 0);
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 1);
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.Height = 3;
		TileObjectData.newTile.Width = 3;
		TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.StyleWrapLimit = 111;
		TileObjectData.addTile(Type);

        DustType = ModContent.DustType<WoodTrash>();
        AdjTiles = new int[] { TileID.Chandeliers };

		LocalizedText name = CreateMapEntryName();
		// name.SetDefault("Elderwood Chandelier");
		AddMapEntry(new Color(111, 22, 22));

		AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
		TileID.Sets.DisableSmartCursor[Type] = true;
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
		Tile tile = Main.tile[i, j];
		if (tile.TileFrameX < 18) {
			r = 0.9f;
			g = 0.8f;
			b = 0.9f;
		}
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

	public override void KillMultiTile(int i, int j, int TileFrameX, int TileFrameY) {
		//int item = Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 34, 38, ModContent.ItemType<Items.Placeable.Furniture.ElderwoodChandelier>(), 1, false, 0, false, false);
		//if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
		//	NetMessage.SendData(21, -1, -1, null, item, 1f, 0f, 0f, 0, 0, 0);
	}

	public override void HitWire(int i, int j) {
		int num = i - Main.tile[i, j].TileFrameX / 16 % 3;
		int num2 = j - Main.tile[i, j].TileFrameY / 16 % 3;
		for (int k = num; k < num + 3; k++) {
			for (int l = num2; l < num2 + 3; l++) {
				if (Main.tile[k, l].HasTile && Main.tile[k, l].TileType == Type) {
					if (Main.tile[k, l].TileFrameX != 108) {
						Tile tile = Main.tile[k, l];
						tile.TileFrameX += 54;
					}
					if (Main.tile[k, l].TileFrameX >= 108) {
						Tile tile2 = Main.tile[k, l];
						tile2.TileFrameX -= 108;
					}
				}
			}
		}

		if (Wiring.running) {
			Wiring.SkipWire(num, num2);
			Wiring.SkipWire(num, num2 + 1);
			Wiring.SkipWire(num, num2 + 2);
			Wiring.SkipWire(num + 1, num2);
			Wiring.SkipWire(num + 1, num2 + 1);
			Wiring.SkipWire(num + 1, num2 + 2);
			Wiring.SkipWire(num + 2, num2);
			Wiring.SkipWire(num + 2, num2 + 1);
			Wiring.SkipWire(num + 2, num2 + 2);
		}
		NetMessage.SendTileSquare(-1, num, num2 + 1, 3, TileChangeType.None);
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
		Texture2D flameTexture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Flame");
		ulong speed = Main.TileFrameSeed ^ (((ulong)j << 32) | (ulong)i);
		Color color = new Color(100, 100, 100, 0);
		int TileFrameX = Main.tile[i, j].TileFrameX;
		int TileFrameY = Main.tile[i, j].TileFrameY;
		int width = 20;
		int height = 20;
		int offset = 0;

		if (WorldGen.SolidTile(i, j - 1)) {
			offset = 2;
			if (WorldGen.SolidTile(i - 1, j + 1) || WorldGen.SolidTile(i + 1, j + 1))
				offset = 4;
		}

		Vector2 vector2 = new Vector2(Main.offScreenRange, Main.offScreenRange);
		if (Main.drawToScreen) vector2 = Vector2.Zero;
		for (int index = 0; index < 5; ++index) {
			float posX = Utils.RandomInt(ref speed, -12, 13) * 0.075f;
			float posY = Utils.RandomInt(ref speed, -12, 13) * 0.075f;
			Main.spriteBatch.Draw(flameTexture, new Vector2(i * 16 - (int)Main.screenPosition.X - (float)((width - 16.0) / 2.0) + posX + 1, j * 16 - (int)Main.screenPosition.Y + offset + posY + 2) + vector2, new Rectangle?(new Rectangle(TileFrameX, TileFrameY, width, height)), color, 0.0f, new Vector2(), 1f, SpriteEffects.None, 0.0f);
		}
	}
}