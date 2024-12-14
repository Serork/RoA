using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Crafting;

sealed class MercuriumOre : ModItem {
	public override void SetStaticDefaults() {
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
		ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
	}

	public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.width = 16;
        Item.height = 20;

        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ItemRarityID.Blue;

        Item.createTile = ModContent.TileType<Tiles.Crafting.MercuriumOre>();
	}

	//public override void PostUpdate() {
	//	if (Main.rand.Next(4) == 0) {
	//		int _dust = Dust.NewDust(Item.position, Item.width, Item.height / 2, ModContent.DustType<Dusts.ToxicFumes>(), 0f, -4f, 100, new Color(), 1.5f);
	//		Dust _dust2 = Main.dust[_dust];
	//		_dust2.scale *= 0.5f;
	//		_dust2 = Main.dust[_dust];
	//		_dust2.velocity *= 1.5f;
	//		_dust2 = Main.dust[_dust];
	//		_dust2.velocity.Y *= -0.5f;
	//		_dust2.noLight = false;
	//	}
	//}
}

//sealed class MercuriumOrePlayerHandler : ModPlayer {
//	public override void PostUpdateBuffs() {
//		if (Player.HasItem(ModContent.ItemType<MercuriumOre>())) Player.AddBuff(ModContent.BuffType<ToxicFumes>(), 1);
//	}
//}