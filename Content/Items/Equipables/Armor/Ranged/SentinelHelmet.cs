using Microsoft.Xna.Framework;

using RoA.Common.Players;

using System;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Ranged;

[AutoloadEquip(EquipType.Head)]
sealed class SentinelHelmet : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Sentinel Helmet");
		//Tooltip.SetDefault("10% chance to not consume arrows");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 24; int height = 26;
		Item.Size = new Vector2(width, height);

		Item.value = Item.sellPrice(silver: 75);
		Item.rare = ItemRarityID.Blue;
		Item.defense = 5;
	}

	public override void UpdateEquip(Player player)	=> player.GetModPlayer<RangedArmorSetPlayer>().ArrowConsumptionReduce += 0.1f;
	
	public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<SentinelBreastplate>() && legs.type == ModContent.ItemType<SentinelLeggings>();

    public override void ArmorSetShadows(Player player) => player.armorEffectDrawShadow = true;

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.SentinelSetBonus1").Value;
		if (Math.Abs(player.velocity.X) + Math.Abs(player.velocity.Y) < 1f) player.detectCreature = true;
	}

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient<MercuriumNugget>(10)
	//		.AddIngredient(ItemID.Leather, 4)
	//		.AddTile(TileID.Anvils)
	//		.Register();
	//}
}
