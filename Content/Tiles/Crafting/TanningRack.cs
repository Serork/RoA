using RoA.Content.Buffs;
using RoA.Content.Items.Miscellaneous;
using RoA.Core.Utility;

using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

sealed class TanningRack : ModTile {
	private ushort SkinningBuffType => (ushort)ModContent.BuffType<Skinning>();

    public override void SetStaticDefaults() {
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.Origin = new Point16(1, 1);
		TileObjectData.newTile.CoordinateHeights = [16, 16];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
		TileObjectData.addTile(Type);

		//ModTranslation name = CreateMapEntryName();
		//name.SetDefault("Tanning Rack");
		//AddMapEntry(new Color(111, 22, 22));

		//DustType = DustID.t_LivingWood;
		//TileID.Sets.DisableSmartCursor[Type] = true;
		//AdjTiles = new int[] { TileID.Dressers };
	}

	/*public override void NumDust(int i, int j, bool fail, ref int num) 
		=> num = fail ? 1 : 3;*/

	//public override void KillMultiTile(int i, int j, int frameX, int frameY) {
	//	int item = Item.NewItem(i * 16, j * 16, 28, 22, ModContent.ItemType<Items.Placeable.Crafting.TanningRack>(), 1, false, 0, false, false);
	//	if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
	//		NetMessage.SendData(21, -1, -1, null, item, 1f, 0f, 0f, 0, 0, 0);
	//	Player player = Main.player[Main.myPlayer];
	//	int type = (ushort)ModContent.BuffType<Skinning>();
	//	if (player.FindBuffIndex(type) == -1)
	//		return;
	//	player.ClearBuff(type);
	//}

	public override bool RightClick(int i, int j) {
		Player player = Main.player[Main.myPlayer];
		if (player.HasBuff(SkinningBuffType)) {
			return false;
		}

		player.AddBuff(SkinningBuffType, 18000);
		return true;
	}

	public override void MouseOver(int i, int j) {
		Player player = Main.player[Main.myPlayer];
		if (player.HasBuff(SkinningBuffType)) {
			int[] leather = [ModContent.ItemType<AnimalLeather>(), ModContent.ItemType<RoughLeather>()];
			bool flag = leather.Contains(player.GetSelectedItem().type);
            if (flag) {
				player.noThrow = 2;
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = ItemID.Leather;
			}

			return;
		}

		player.noThrow = 2;
		player.cursorItemIconEnabled = true;
		player.cursorItemIconID = ModContent.ItemType<Items.Placeable.Crafting.TanningRack>();
	}
}
