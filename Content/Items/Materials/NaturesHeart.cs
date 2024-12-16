using Microsoft.Xna.Framework;

using Newtonsoft.Json.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class NaturesHeart : ModItem {
	private float _value;

	public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.5f);

    public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Nature's Heart");
		//Tooltip.SetDefault("'Seems like is was a source of life for higher beings...'");
		Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(10, 6));
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

	public override void PostUpdate() {
		float value = 0.2f;
		switch (Main.itemAnimations[Type].Frame) {
			case 1 or 5:
				value = 0.4f;
				break;
            case 2 or 4:
                value = 0.6f;
                break;
			case 3:
                value = 0.8f;
                break;
        }
		_value = MathHelper.Lerp(_value, value, 0.3f);
        Lighting.AddLight(Item.Center + Vector2.UnitX * 2f, 
			(Color.Green * _value).ToVector3() * 0.65f);
	}
}