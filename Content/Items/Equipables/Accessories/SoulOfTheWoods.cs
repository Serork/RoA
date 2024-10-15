using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class SoulOfTheWoods : ModItem {
	public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 28; int height = 38;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.accessory = true;

        Item.value = Item.sellPrice(gold: 2, silver: 30);
    }

	public override void UpdateAccessory(Player player, bool hideVisual)  => player.GetModPlayer<DruidStats>().SoulOfTheWoods = true;
}