using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class CottonBollSmall : NatureProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(15);

    private Vector2 _velocity;

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: false);

        Projectile.SetSizeValues(20);
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;

        Projectile.timeLeft = TIMELEFT;

        Projectile.aiStyle = -1;

        Projectile.manualDirectionChange = true;

        Projectile.Opacity = 0f;
    }

    public override void AI() {
        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.2f);

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            Projectile.SetDirection(Main.rand.NextBool().ToDirectionInt());
        }

        float offsetY = 0.1f;
        Projectile.localAI[0] = Helper.Wave(-offsetY, offsetY, 2.5f, Projectile.identity);
        Projectile.velocity.Y += Projectile.localAI[0] * 0.1f;
        Projectile.rotation = Projectile.localAI[0] * 1f;

        Projectile.velocity *= 0.97f;

        Projectile.OffsetTheSameProjectile(.05f);
    }

    public override void OnKill(int timeLeft) {
        
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDraw(lightColor * Projectile.Opacity);

        return false;
    }
}
