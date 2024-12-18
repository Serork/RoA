using Microsoft.Xna.Framework;
using RoA.Content.Items.Materials;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Consumables;

sealed class SlipperyGrenade : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Slippery Grenade");
		//Tooltip.SetDefault("A small explosion that will not destroy tiles\nSlips through solid tiles");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
	}

	public override void SetDefaults() {
		int width = 16; int height = 20;
		Item.Size = new Vector2(width, height);

		Item.DamageType = DamageClass.Ranged;
		Item.damage = 60;

		Item.rare = ItemRarityID.White;
        Item.maxStack = Item.CommonMaxStack;

        Item.noUseGraphic = true;
		Item.UseSound = SoundID.Item1;

		Item.value = Item.sellPrice(copper: 20);
		Item.useStyle = ItemUseStyleID.Shoot;

		Item.useTime = Item.useAnimation = 45;
		Item.noMelee = true;

		Item.consumable = true;
		Item.autoReuse = false;

		Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Miscellaneous.SlipperyGrenade>();
		Item.shootSpeed = 5.5f;
		Item.knockBack = 8f;
	}

	public override void AddRecipes() {
		CreateRecipe(2)
			.AddIngredient(ItemID.Grenade, 2)
			.AddIngredient<Galipot>(1)
			.Register();
	}
}
