using Microsoft.Xna.Framework;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class GraveDangerSplinter : ModProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(20f);

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(3);

        Projectile.SetTrail(0, 2);
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(14);

        Projectile.friendly = true;
        Projectile.penetrate = 3;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.tileCollide = true;

        Projectile.timeLeft = TIMELEFT;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        return false;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            Projectile.frame = Main.rand.Next(3);
        }

        Projectile.Opacity = Utils.GetLerpValue(0, 50, Projectile.timeLeft, true);

        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] > 15f) {
            if (Projectile.velocity.Y == 0f) {
                Projectile.velocity.X *= 0.95f;
            }
            Projectile.velocity.Y += 0.2f;
        }
        Projectile.rotation += Projectile.velocity.X * 0.1f;
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawShadowTrails(lightColor * Projectile.Opacity, 0.5f, 1, 0f);
        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity, 0f);

        return false;
    }
}
