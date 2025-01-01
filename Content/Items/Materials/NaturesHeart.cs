using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

using RoA.Content.Dusts;
using RoA.Core.Utility;

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
		int width = 22; int height = 32;
		Item.Size = new Vector2(width, height);

		Item.maxStack = Item.CommonMaxStack;
		Item.value = Item.sellPrice(silver: 10);
		Item.rare = ItemRarityID.Green;
	}

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
        float value = 0.2f;
        int frame = Main.itemAnimations[Type].GetFrame(TextureAssets.Item[Type].Value, Main.itemFrameCounter[whoAmI]).Y / Item.height;
        switch (frame) {
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
        int type = ModContent.DustType<NaturesHeartDust>();
        if (Main.rand.NextBool(20)) {
            for (int num740 = 0; num740 < 1; num740++) {
                if (Main.rand.NextChance(value / 2f)) {
                    int num741 = Dust.NewDust(new Vector2(Item.position.X, Item.position.Y), Item.width, Item.height, type);
                    Dust dust2 = Main.dust[num741];
                    dust2.velocity *= 0.5f;
                    dust2.scale *= 0.9f;
                    Main.dust[num741].noGravity = true;
                }
            }
        }
        if (Main.rand.NextBool(20)) {
            for (int num742 = 0; num742 < 1; num742++) {
                if (Main.rand.NextChance(value / 2f)) {
                    int num743 = Dust.NewDust(new Vector2(Item.position.X, Item.position.Y), Item.width, Item.height, type);
                    Dust dust2 = Main.dust[num743];
                    dust2.velocity *= 2.5f;
                    dust2.velocity *= 0.5f;
                    dust2 = Main.dust[num743];
                    dust2.scale *= 0.7f;
                    Main.dust[num743].noGravity = true;
                }
            }
        }
        Lighting.AddLight(Item.Center + Vector2.UnitX * 2f,
            (Color.Green * value).ToVector3() * 0.75f);
    }
}