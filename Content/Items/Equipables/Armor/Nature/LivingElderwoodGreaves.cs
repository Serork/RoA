using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Legs)]
sealed class LivingElderwoodGreaves : NatureItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Living Elderwood Greaves");
		//Tooltip.SetDefault("4% increased nature potential damage and critical strike chance");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 22; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Green;
		Item.value = Item.sellPrice(silver: 15);

		Item.defense = 3;
	}

	public override void UpdateEquip(Player player) {
		player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.04f;
        player.GetCritChance(DruidClass.NatureDamage) += 4;
    }

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<Placeable.Crafting.Elderwood>(15)
			.AddIngredient<Materials.Galipot>(2)
			.AddTile(TileID.LivingLoom)
			.Register();
	}
}