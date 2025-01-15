using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Body)]
sealed class DevilHunterCloak : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Devil Hunter's Cloak");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 30; int height = 24;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.value = Item.sellPrice(gold: 10);
		Item.vanity = true;
	}

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<Materials.FlamingFabric>(18)
			.AddIngredient(ItemID.BlackThread)
			.AddTile(TileID.DemonAltar)
			.Register();
	}
}
