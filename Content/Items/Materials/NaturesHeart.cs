using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
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
		Main.RegisterItemAnimation(Type, new DrawAnimationVertical(10, 6));
		ItemID.Sets.AnimatesAsSoul[Type] = true;

		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
	}

	public override void SetDefaults() {
		int width = 20; int height = 28;
		Item.Size = new Vector2(width, height);

		Item.maxStack = 999;
		Item.value = Item.sellPrice(silver: 10);
		Item.rare = ItemRarityID.Green;
	}

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
        float value = 0.2f;
        switch (Main.itemAnimations[Type].GetFrame(TextureAssets.Item[Type].Value, Main.itemFrameCounter[whoAmI]).Y / Item.height) {
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
        Lighting.AddLight(Item.Center + Vector2.UnitX * 2f,
            (Color.Green * value).ToVector3() * 0.6f);
    }
}