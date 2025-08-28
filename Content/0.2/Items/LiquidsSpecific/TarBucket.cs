﻿using ModLiquidLib.ID;
using ModLiquidLib.ModLoader;

using RoA.Content.Liquids;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.LiquidsSpecific;

//This is an example of a modded bucket
//Here we do some extra logic to make our bucket dispense liquid
//While also using a new ID Set added by ModLiquid Library to allow buckets to create this item
sealed class TarBucket : ModItem {
	//The SetStaticDefaults of a bucket
	public override void SetStaticDefaults() {
		ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
		ItemID.Sets.AlsoABuildingItem[Type] = true; //Unused ID Set, but may be useful for other modders or when the game updates
		ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<BottomlessTarBucket>();
		ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;

		//Here is how we make buckets create this item
		//Using the CreateLiquidBucketItem ID Set, we are able to set which liquid creates an item as well as which item its creates
		//NOTE: a liquid can only create 1 bucket item at a time
		LiquidID_TLmod.Sets.CreateLiquidBucketItem[LiquidLoader.LiquidType<Tar>()] = Type;

		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
	}

	//The normal SetDefaults for a bucket
	public override void SetDefaults() {
		Item.width = 24;
		Item.height = 22;
		Item.maxStack = Item.CommonMaxStack;
		Item.useTurn = true;
		Item.autoReuse = true;
		Item.useAnimation = 15;
		Item.useTime = 10;
		Item.useStyle = ItemUseStyleID.Swing;
	}

	//Here is how we do the bucket logic. Calculating when the player is able to place a liquid tile or not.
	//This is taken from the Lava bucket logic
	//We use HoldItem, as its the hook/method ran just before the bucket logic is ran.
	//This is so no other extra item logic is run inbetween this hook/method and when we would normally have bucket logic called
	public override void HoldItem(Player player) {
		if (!player.JustDroppedAnItem)//make sure the player hasn't just dropped an item recently
		{
			if (player.whoAmI != Main.myPlayer) //make sure to only run on clients (we sync multiplayer data later down the line)
			{
				return;
			}
			//We make sure the player is in range to be able to place the liquid tile
			if (player.noBuilding ||
				!(player.position.X / 16f - (float)Player.tileRangeX - (float)Item.tileBoost <= (float)Player.tileTargetX) ||
				!((player.position.X + (float)player.width) / 16f + (float)Player.tileRangeX + (float)Item.tileBoost - 1f >= (float)Player.tileTargetX) ||
				!(player.position.Y / 16f - (float)Player.tileRangeY - (float)Item.tileBoost <= (float)Player.tileTargetY) ||
				!((player.position.Y + (float)player.height) / 16f + (float)Player.tileRangeY + (float)Item.tileBoost - 2f >= (float)Player.tileTargetY)) {
				return;
			}
			//We set the cursor's item to be of the bucket item. To show the player that they are able to place a liquid tile at that coordinate normally
			if (!Main.GamepadDisableCursorItemIcon) {
				player.cursorItemIconEnabled = true;
				Main.ItemIconCacheUpdate(Item.type);
			}
			//Make sure that the item is being used to place liquid
			if (!player.ItemTimeIsZero || player.itemAnimation <= 0 || !player.controlUseItem) {
				return;
			}
			//Get the tile position of the cursor
			Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
			if (tile.LiquidAmount >= 200) {
				return;
			}
			//Do additional tile check to make sure the target tile isn't a grate
			if (tile.HasUnactuatedTile) {
				bool[] tileSolid = Main.tileSolid;
				if (tileSolid[tile.TileType]) {
					bool[] tileSolidTop = Main.tileSolidTop;
					if (!tileSolidTop[tile.TileType]) {
						if (tile.TileType != TileID.Grate) {
							return;
						}
					}
				}
			}
			//We then check if the liquid at the position is ours if the liquid amount isn't zero
			if (tile.LiquidAmount != 0) {
				if (tile.LiquidType != LiquidLoader.LiquidType<Tar>()) {
					return;
				}
			}

			//After all of that, we are able to place the liquid 
			//In which we...
			SoundEngine.PlaySound(SoundID.SplashWeak, player.position); //...play a sound
			tile.LiquidType = LiquidLoader.LiquidType<Tar>(); //...create a liquid tile...
			tile.LiquidAmount = byte.MaxValue; //...at full liquid capacity
			WorldGen.SquareTileFrame(Player.tileTargetX, Player.tileTargetY); //...frame the tile to update the liquid
			Item.stack--; //...remove the item's count
			player.PutItemInInventoryFromItemUsage(ItemID.EmptyBucket, player.selectedItem); //...create a bucket item
			player.ApplyItemTime(Item); //...do item usetime

			if (Main.netMode == NetmodeID.MultiplayerClient)//...lastly, if the game is MP, then sync the liquid plavement
			{
				NetMessage.sendWater(Player.tileTargetX, Player.tileTargetY);
			}
		}
	}
}
