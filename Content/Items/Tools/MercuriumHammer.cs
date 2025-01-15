using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Tools;

public class MercuriumHammer : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Mercurium Hammer");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 44; int height = 38;
		Item.Size = new Vector2(width, height);

		Item.damage = 20;
		Item.DamageType = DamageClass.Melee;

		Item.useTime = Item.useAnimation = 40;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.autoReuse = true;

		Item.knockBack = 5f;
		Item.hammer = 60;

		Item.value = Item.sellPrice(silver: 30);
		Item.rare = ItemRarityID.Blue;
		Item.UseSound = SoundID.Item1;
	}

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<Materials.MercuriumNugget>(14)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
