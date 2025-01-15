using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Ranged;

[AutoloadEquip(EquipType.Body)]
sealed class SentinelBreastplate : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Sentinel Breastplate");
		//Tooltip.SetDefault("Increases arrow damage by 10%");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 26; int height = 20;
		Item.Size = new Vector2(width, height);

		Item.value = Item.sellPrice(silver: 60);
		Item.rare = ItemRarityID.Blue;
		Item.defense = 6;
	}

	public override void UpdateEquip(Player player)	
		=> player.arrowDamage += 0.1f;

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<Materials.MercuriumNugget>(10)
			.AddIngredient(ItemID.Leather, 5)
			.AddTile(TileID.Anvils)
			.Register();
	}
}

