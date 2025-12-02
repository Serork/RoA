using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Melee;

sealed class MercuriumSword : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        int width = 42; int height = width;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 25;
        Item.autoReuse = false;
        Item.useTurn = false;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 14;
        Item.knockBack = 3f;

        Item.rare = ItemRarityID.Blue;

        Item.UseSound = SoundID.Item1;

        Item.value = Item.sellPrice(0, 0, 25, 0);
    }

    public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo) {
        target.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), 90);
    }

    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), 90);

        if (!target.CanActivateOnHitEffect()) {
            return;
        }

        Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 0.5f;
        ushort type = (ushort)ModContent.ProjectileType<Projectiles.Friendly.Melee.MercuriumFumes>();
        Projectile.NewProjectile(target.GetSource_FromAI(), target.Center.X, target.Center.Y, velocity.X, velocity.Y, type, Item.damage / 3, Item.knockBack / 3f, player.whoAmI);
        Projectile.NewProjectile(target.GetSource_FromAI(), target.Center.X, target.Center.Y, -velocity.X, -velocity.Y, type, Item.damage / 3, Item.knockBack / 3f, player.whoAmI);
    }

    public override void MeleeEffects(Player player, Rectangle hitbox) {
        if (Main.rand.Next(5) == 0) {
            int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, ModContent.DustType<ToxicFumes>(), player.direction * 2, 0f, 0, default(Color), 1.3f);
            Main.dust[dust].customData = 0.15f;
        }
    }
}