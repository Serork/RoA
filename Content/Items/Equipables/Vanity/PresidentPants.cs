using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Legs)]
sealed class PresidentPants : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("President's Pants");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 30; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.value = Item.sellPrice(gold: 3, silver: 50);
		Item.vanity = true;
	}
}
