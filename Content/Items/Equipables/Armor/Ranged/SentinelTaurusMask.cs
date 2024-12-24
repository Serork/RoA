using Microsoft.Xna.Framework;

using RoA.Common.Players;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Ranged;

[AutoloadEquip(EquipType.Head)]
sealed class SentinelTaurusMask : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Sentinel Taurus Mask");
		//Tooltip.SetDefault("Increases running acceleration speed");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 26; int height = 24;
		Item.Size = new Vector2(width, height);

		Item.value = Item.sellPrice(silver: 75);
		Item.rare = ItemRarityID.Blue;
		Item.defense = 3;
	}

	public override void UpdateEquip(Player player)
		=> player.runAcceleration *= 1.5f;

	public override bool IsArmorSet(Item head, Item body, Item legs)
		=> body.type == ModContent.ItemType<SentinelBreastplate>() && legs.type == ModContent.ItemType<SentinelLeggings>();

	public override void ArmorSetShadows(Player player) 
		=> player.armorEffectDrawShadow = true;
	
	public override void UpdateArmorSet(Player player)  {
        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.SentinelSetBonus2").Value;
        player.GetModPlayer<RangedArmorSetPlayer>().TaurusArmorSet = true;
	}
}
