using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Body)]
sealed class DreadheartCorruptionChestplate : NatureItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Dreadheart Chestplate");
		//Tooltip.SetDefault("8% increased nature potential damage" + "\nIncreases nature knockback");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 26; int height = 20;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Green;
		Item.value = Item.sellPrice(silver: 60);

		Item.defense = 5;
	}

	public override void UpdateEquip(Player player) {
        player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.08f;
		player.GetKnockback(DruidClass.NatureDamage) += 1f;
	}

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient(ItemID.ShadowScale, 20)
	//		.AddIngredient<NaturesHeart>()
	//		.AddTile<OvergrownAltar>()
	//		.Register();
	//}
}