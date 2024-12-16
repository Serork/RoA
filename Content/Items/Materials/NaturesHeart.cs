using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class NaturesHeart : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Nature's Heart");
		//Tooltip.SetDefault("'Seems like is was a source of life for higher beings...'");
		Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 6));
		ItemID.Sets.AnimatesAsSoul[Item.type] = true;

		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
	}

	public override void SetDefaults() {
		int width = 20; int height = 28;
		Item.Size = new Vector2(width, height);

		Item.maxStack = 999;
		Item.value = Item.sellPrice(silver: 10);
		Item.rare = ItemRarityID.Green;
	}

	public override void PostUpdate() => Lighting.AddLight(Item.Center, Color.WhiteSmoke.ToVector3() * 0.5f * Main.essScale);
}