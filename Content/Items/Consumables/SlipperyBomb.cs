using Microsoft.Xna.Framework;
using RoA.Content.Items.Materials;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Consumables;

sealed class SlipperyBomb : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Slippery Bomb");
		//Tooltip.SetDefault("A small explosion that will destroy most tiles\nSlips through solid tiles");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
	}

	public override void SetDefaults() {
		int width = 20; int height = 30;
		Item.Size = new Vector2(width, height);

		Item.damage = 0;

		Item.rare = ItemRarityID.White;
		Item.maxStack = Item.CommonMaxStack;

		Item.noUseGraphic = true;
		Item.UseSound = SoundID.Item1;

		Item.value = Item.sellPrice(copper: 80);
		Item.useStyle = ItemUseStyleID.Swing;

		Item.useTime = Item.useAnimation = 25;
		Item.noMelee = true;

		Item.consumable = true;
		Item.autoReuse = false;

		Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Miscellaneous.SlipperyBomb>();
		Item.shootSpeed = 5f;
	}

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
		=> itemGroup = ContentSamples.CreativeHelper.ItemGroup.Bombs;
}
