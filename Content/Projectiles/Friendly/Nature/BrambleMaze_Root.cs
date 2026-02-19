using Microsoft.Xna.Framework;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BrambleMazeRoot : NatureProjectile {
    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(4);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(24, 16);

        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.tileCollide = false;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        if (Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = 1f;

            Projectile.frame = Main.rand.Next(4);
        }

        if (Projectile.IsOwnerLocal() && Projectile.localAI[0] == 0f && Projectile.ai[0] < 10f) {
            Projectile.localAI[0] = 1f;

            Projectile.ai[0]++;

            Vector2 position = Projectile.Center + Vector2.UnitX * Projectile.width;
            int damage = Projectile.damage;
            float knockBack = Projectile.knockBack;
            ProjectileUtils.SpawnPlayerOwnedProjectile<BrambleMazeRoot>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromThis()) {
                Position = position,
                Damage = damage,
                KnockBack = knockBack,
                AI0 = Projectile.ai[0]
            });
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor);

        return false;
    }

    public override void OnKill(int timeLeft) {
        
    }
}
