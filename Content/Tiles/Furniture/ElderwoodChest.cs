using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.Enums;
using Terraria.Audio;
using Terraria.GameContent.ObjectInteractions;

using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core.Utility;

namespace RoA.Content.Tiles.Furniture;

sealed class ElderwoodChest : ModTile {
	public override void SetStaticDefaults()  {
		Main.tileSpelunker[Type] = true;
		Main.tileContainer[Type] = true;
		Main.tileShine2[Type] = true;
		Main.tileShine[Type] = 1200;
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileOreFinderPriority[Type] = 500;

		TileID.Sets.HasOutlines[Type] = true;
		TileID.Sets.BasicChest[Type] = true;
		TileID.Sets.DisableSmartCursor[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.Origin = new Point16(0, 1);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
		TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.AnchorInvalidTiles = [TileID.MagicalIceBlock];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.addTile(Type);

        AdjTiles = [TileID.Containers];

        Color mapColor = new(110, 91, 74);
        AddMapEntry(mapColor, CreateMapEntryName());
        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Furniture>();
		HitSound = SoundID.Dig;
	}

    //public override IEnumerable<Item> GetItemDrops(int i, int j) {
    //    Tile tile = Main.tile[i, j];
    //    int style = TileObjectData.GetTileStyle(tile);
    //    if (style == 0) {
    //        yield return new Item(ModContent.ItemType<CemeteryBiomeChestItem>());
    //    }
    //    if (style == 1) {
    //        yield return new Item(ModContent.ItemType<CemeteryBiomeChestItem>());
    //    }
    //}

	public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override bool IsLockedChest(int i, int j) => !NPC.downedBoss2;

	public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual) {
		bool flag = false;
		if (flag) {
			return false;
		}

		return true;
	}

	public static string MapChestName(string name, int i, int j) {
		int left = i;
		int top = j;
		Tile tile = Main.tile[i, j];
		if (tile.TileFrameX % 36 != 0)  {
			left--;
		}

		if (tile.TileFrameY != 0)  {
			top--;
		}

		int chest = Chest.FindChest(left, top);
		if (chest < 0)  {
			return Language.GetTextValue("LegacyChestType.0");
		}

		if (Main.chest[chest].name == string.Empty) {
			return name;
		}

		return name + ": " + Main.chest[chest].name;
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = 3;

	public override void KillMultiTile(int i, int j, int frameX, int frameY) => Chest.DestroyChest(i, j);

	public override bool RightClick(int i, int j)  {
		Player player = Main.LocalPlayer;
		Tile tile = Main.tile[i, j];
		Main.mouseRightRelease = false;
		int left = i;
		int top = j;
		if (tile.TileFrameX % 36 != 0) {
			left--;
		}

		if (tile.TileFrameY != 0) {
			top--;
		}

		if (player.sign >= 0) {
			SoundEngine.PlaySound(SoundID.MenuClose);
			player.sign = -1;
			Main.editSign = false;
			Main.npcChatText = string.Empty;
		}

		if (Main.editChest) {
			SoundEngine.PlaySound(SoundID.MenuTick);
			Main.editChest = false;
			Main.npcChatText = string.Empty;

        }

		if (player.editedChestName) {
			NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
			player.editedChestName = false;
		}

		bool isLocked = IsLockedChest(left, top);
		if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked)  {
			if (left == player.chestX && top == player.chestY && player.chest >= 0)  {
				player.chest = -1;
				Recipe.FindRecipes();
				SoundEngine.PlaySound(SoundID.MenuClose);
			}
			else {
				NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top);
				Main.stackSplit = 600;
			}
		}
		else {
			if (isLocked)  {

			}
			else {
				int chest = Chest.FindChest(left, top);
				if (chest >= 0) {
					Main.stackSplit = 600;
					if (chest == player.chest) {
						player.chest = -1;
						SoundEngine.PlaySound(SoundID.MenuClose);
					}
					else {
						player.chest = chest;
						Main.playerInventory = true;
						Main.recBigList = false;
						player.chestX = left;
						player.chestY = top;
						SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
					}

					Recipe.FindRecipes();
				}
			}
		}

		return true;
	}

	public override void MouseOver(int i, int j) {
		Player player = Main.LocalPlayer;
		Tile tile = Main.tile[i, j];
		int left = i;
		int top = j;
		if (tile.TileFrameX % 36 != 0) {
			left--;
		}

		if (tile.TileFrameY != 0) {
			top--;
		}

		int chest = Chest.FindChest(left, top);
		if (chest < 0) {
			player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
		}
		else  {
            bool isLocked = IsLockedChest(left, top);
            player.cursorItemIconText = Main.chest[chest].name;
            player.cursorItemIconID = !isLocked ? (ushort)ModContent.ItemType<Items.Placeable.Furniture.ElderwoodChest>() : player.GetSelectedItem().type;
            //player.cursorItemIconText = "";
        }

		player.noThrow = 2;
		player.cursorItemIconEnabled = true;
	}

	public override void MouseOverFar(int i, int j)  {
		MouseOver(i, j);
		Player player = Main.LocalPlayer;
		if (player.cursorItemIconText == string.Empty)  {
			player.cursorItemIconEnabled = false;
			player.cursorItemIconID = 0;
		}
	}
}