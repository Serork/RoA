using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.Localization;

namespace RoA.Content.Items.Equipables.Armor.Summon;

[AutoloadEquip(EquipType.Head)]
sealed class FlinxFurUshanka : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Flinx Fur Ushanka");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 20; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Green;
		Item.value = Item.sellPrice(gold: 2);

		Item.defense = 1;
	}

	public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ItemID.FlinxFurCoat;

	public override void UpdateArmorSet(Player player) {
		player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.FlinxFurUshankaSetBonus").Value;
		player.buffImmune[BuffID.Chilled] = true;
		player.buffImmune[BuffID.Frozen] = true;
	}
}