using Microsoft.Xna.Framework;

using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

sealed class OvergrownSpear : ModProjectile {
    public override void SetDefaults() {
        int width = 16; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.DamageType = DamageClass.Melee;

        Projectile.aiStyle = 19;
        AIType = 49;

        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;

        Projectile.friendly = true;
        Projectile.hide = true;
        Projectile.ownerHitCheck = true;
        Projectile.tileCollide = false;

        //Projectile.glowMask = RoAGlowMask.Get(nameof(OvergrownSpear));
    }
}
