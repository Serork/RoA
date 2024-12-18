using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Head)]
sealed class LivingWoodHelmet : NatureItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Living Wood Helmet");
		//Tooltip.SetDefault("2% increased potential speed");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	protected override void SafeSetDefaults() {
		int width = 24; int height = 24;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(silver: 20);

		Item.defense = 2;
	}

    public override void UpdateEquip(Player player) => player.GetModPlayer<DruidStats>().DruidPotentialUseTimeMultiplier -= 0.04f;

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<LivingWoodChestplate>() && legs.type == ModContent.ItemType<LivingWoodGreaves>();

	public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetTextValue("Mods.RoA.Items.Tooltips.LivingWoodSetBonus");
        player.GetModPlayer<DruidStats>().DruidDamageExtraIncreaseValueMultiplier += 0.1f;
    }

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient(ItemID.Wood, 10)
	//		.AddIngredient<Galipot>(5)
	//		.AddTile(TileID.LivingLoom)
	//		.Register();
	//}
}