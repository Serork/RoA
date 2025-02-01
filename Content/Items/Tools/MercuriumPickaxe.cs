using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Tools;

sealed class MercuriumPickaxe : ModItem {
	public override void SetStaticDefaults() {
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 36; int height = width;
		Item.Size = new Vector2(width, height);

		Item.damage = 8;
		Item.DamageType = DamageClass.Melee;

		Item.useTime = Item.useAnimation = 22;
		Item.useStyle = ItemUseStyleID.Swing;
		Item.autoReuse = true;

		Item.knockBack = 5f;
		Item.pick = 65;

		Item.value = Item.buyPrice(silver: 42);
		Item.rare = ItemRarityID.Blue;
		Item.UseSound = SoundID.Item1;
	}

    public override void MeleeEffects(Player player, Rectangle hitbox) {
        if (Main.rand.Next(5) == 0)
            Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, ModContent.DustType<ToxicFumes>(), player.direction * 2, 0f, 0, default(Color), 1.3f);
    }

    public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<Materials.MercuriumNugget>(16)
			.AddTile(TileID.Anvils)
			.Register();
	}
}
