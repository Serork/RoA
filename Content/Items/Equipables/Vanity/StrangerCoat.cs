using Microsoft.Xna.Framework;

using RoA.Content.Items.Materials;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Body)]
sealed class StrangerCoat : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Stranger's Coat");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 30; int height = 26;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.value = Item.sellPrice(gold: 10);
		Item.vanity = true;
	}
}
