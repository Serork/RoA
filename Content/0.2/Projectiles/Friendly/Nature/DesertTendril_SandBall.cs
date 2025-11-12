using Microsoft.Xna.Framework;

using RoA.Core.Defaults;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class SandBall : NatureProjectile {
    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(14);

        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.extraUpdates = 1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 120;
    }

    public override void AI() {
        Projectile.velocity.Y += 0.2f;

        if (Main.rand.Next(2) == 0) {
            int num11 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Sand);
            Main.dust[num11].velocity.X *= 0.4f;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        ProjectileUtils.QuickDrawAnimated(Projectile, lightColor);

        return false;
    }
}
