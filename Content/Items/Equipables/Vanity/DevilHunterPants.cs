using Microsoft.Xna.Framework;
using RoA.Content.Items.Materials;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Legs)]
sealed class DevilHunterPants : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Devil Hunter's Pants");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 22; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.value = Item.sellPrice(gold: 5);
		Item.vanity = true;
	}

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<Materials.FlamingFabric>(15)
			.AddIngredient(ItemID.BlackThread)
			.AddTile(TileID.DemonAltar)
			.Register();
	}
}
