using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class MercuriumNugget : ModItem {
	public override void SetStaticDefaults() {
        //Tooltip.SetDefault("'Shiny, but dangerous'");

        ItemID.Sets.SortingPriorityMaterials[Item.type] = 59;

        Item.ResearchUnlockCount = 25;
	}

	public override void SetDefaults() {
		int width = 20; int height = 20;
		Item.Size = new Vector2(width, height);

		Item.value = Item.sellPrice(silver: 30);
		Item.rare = ItemRarityID.Blue;
		Item.maxStack = Item.CommonMaxStack;
	}
}