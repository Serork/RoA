using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Melee;

sealed class BloodshedAxeHeal : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(ref Color lightColor) => false;

    public override void SetDefaults() {
        int width = 4; int height = width;
        Projectile.Size = new Vector2(width, height);
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        Projectile.timeLeft = 2;
        Player player = Main.player[Projectile.owner];
        Color newColor = Main.hslToRgb((0.92f + Main.rand.NextFloat() * 0.02f) % 1f, 1f, 0.4f + Main.rand.NextFloat() * 0.25f);
        int dust = Dust.NewDust(Projectile.Center, 0, 0, ModContent.DustType<VampParticle>(), 0f, 0f, 0, newColor);
        Main.dust[dust].noGravity = true;
        Main.dust[dust].velocity = Main.rand.NextVector2Circular(2f, 2f);
        Main.dust[dust].velocity *= 0.1f;
        Projectile.SlightlyMoveTo(player.Center, 20, 5);
        if (player.Hitbox.Intersects(Projectile.Hitbox)) {
            Projectile.Kill();
            player.Heal(Projectile.damage / 2);
            player.HealEffect(Projectile.damage / 2);
        }
    }
}
