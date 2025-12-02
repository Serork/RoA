using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Tools;

public class MercuriumHammer : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Mercurium Hammer");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 44; int height = 38;
        Item.Size = new Vector2(width, height);

        Item.damage = 20;
        Item.DamageType = DamageClass.Melee;

        Item.useAnimation = 30;
        Item.useTime = 18;

        Item.useStyle = ItemUseStyleID.Swing;

        Item.useTurn = true;

        Item.autoReuse = true;

        Item.knockBack = 5f;
        Item.hammer = 60;

        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item1;

        Item.value = Item.sellPrice(silver: 30);
    }

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), 80);
    }

    public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo) {
        target.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), 80);
    }

    public override void MeleeEffects(Player player, Rectangle hitbox) {
        if (Main.rand.Next(5) == 0) {
            int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, ModContent.DustType<ToxicFumes>(), player.direction * 2, 0f, 0, default(Color), 1.3f);
            Main.dust[dust].customData = 0.15f;
        }
    }
}
