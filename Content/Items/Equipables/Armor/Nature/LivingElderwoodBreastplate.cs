using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Body)]
sealed class LivingElderwoodBreastplate : NatureItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Living Elderwood Breastplate");
		//Tooltip.SetDefault("Increases defense by 3 while Wreath is charged");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    protected override void SafeSetDefaults() {
        int width = 30; int height = 26;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 25, 0);

        Item.defense = 4;
	}

	public override void UpdateEquip(Player player) {
		if (player.GetModPlayer<WreathHandler>().IsFull1) {
			player.statDefense += 3;
		}
	}
}