using Microsoft.Xna.Framework;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using RoA.Content.Items.Materials;

namespace RoA.Content.Items.Tools;

sealed class MercuriumAxe : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Mercurium Axe");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 34; int height = width;
		Item.Size = new Vector2(width, height);

		Item.damage = 18;
		Item.DamageType = DamageClass.Melee;

		Item.useTime = Item.useAnimation = 24;
		Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = true;

        Item.knockBack = 5f;
		Item.axe = 80 / 5;

		Item.value = Item.sellPrice(silver: 26);
		Item.rare = ItemRarityID.Blue;
		Item.UseSound = SoundID.Item1;
	}

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient(ModContent.ItemType<MercuriumNugget>(), 14)
	//		.AddTile(TileID.Anvils)
	//		.Register();
	//}
}
