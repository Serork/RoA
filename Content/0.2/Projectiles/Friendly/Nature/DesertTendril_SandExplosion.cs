using Microsoft.Xna.Framework;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class SandExplosion : NatureProjectile {
    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(5);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(30);

        Projectile.friendly = true;
        Projectile.aiStyle = -1;

        Projectile.tileCollide = false;
    }

    public override void AI() {
        if (Projectile.frameCounter++ > 2) {
            Projectile.frameCounter = 0;

            Projectile.frame++;
            if (Projectile.frame > Projectile.GetFrameCount()) {
                Projectile.Kill();
            }
            if (Projectile.frame == Projectile.GetFrameCount() - 2) {
                if (Projectile.IsOwnerLocal()) {
                    ProjectileUtils.SpawnPlayerOwnedProjectile<SandBall>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromAI()) {
                        Position = Projectile.Center,
                        Damage = Projectile.damage,
                        KnockBack = Projectile.knockBack
                    });
                }
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        ProjectileUtils.QuickDrawAnimated(Projectile, lightColor);

        return false;
    }
}
