using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

[Autoload(false)]
sealed class WandCore : ModItem {
	public override void SetStaticDefaults() {
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
		//ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
	}

	public override void SetDefaults() {
		int width = 32; int height = 32;
		Item.Size = new Vector2(width, height);

		Item.value = Item.sellPrice(0, 1, 15, 35);
		Item.rare = ItemRarityID.Orange;
		Item.maxStack = Item.CommonMaxStack;
	}
}