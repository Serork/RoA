using Microsoft.Xna.Framework;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class CottonFiber : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(5);

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: false);

        Projectile.SetSizeValues(24);
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.tileCollide = false;

        Projectile.timeLeft = TIMELEFT;

        Projectile.aiStyle = -1;

        Projectile.manualDirectionChange = true;
    }

    public override void AI() {
        Projectile.velocity *= 0.97f;

        if (Projectile.velocity.Length() > 0.1f) {
            Projectile.SetDirection(Projectile.velocity.X.GetDirection());
        }

        Projectile.rotation += 0.5f * Projectile.spriteDirection * MathUtils.Clamp01(Projectile.SpeedX());

        if (Projectile.SpeedX() < 1f) {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, 0.025f);
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDraw(lightColor * Projectile.Opacity);

        return false;
    }
}
