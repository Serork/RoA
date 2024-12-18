using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class SailorHat : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Sailor Hat");
		//Tooltip.SetDefault("'Who the hell am I?'");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 20; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.value = Item.buyPrice(gold: 2, silver: 50);
		Item.value = Item.sellPrice(silver: 50);
		Item.vanity = true;
	}
}
