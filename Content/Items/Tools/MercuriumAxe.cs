using Microsoft.Xna.Framework;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using RoA.Content.Items.Materials;
using RoA.Content.Dusts;

namespace RoA.Content.Items.Tools;

sealed class MercuriumAxe : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Mercurium Axe");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 38; int height = width;
		Item.Size = new Vector2(width, height);

		Item.damage = 18;
		Item.DamageType = DamageClass.Melee;

		Item.useTime = Item.useAnimation = 24;
		Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = true;

        Item.knockBack = 5f;
		Item.axe = 75 / 5;

		Item.rare = ItemRarityID.Blue;
		Item.UseSound = SoundID.Item1;

        Item.value = Item.sellPrice(silver: 30);
    }

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), Main.rand.Next(40, 120));
    }

    public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo) {
        target.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), Main.rand.Next(40, 120));
    }

    public override void MeleeEffects(Player player, Rectangle hitbox) {
        if (Main.rand.Next(5) == 0)
            Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, ModContent.DustType<ToxicFumes>(), player.direction * 2, 0f, 0, default(Color), 1.3f);
    }
}
