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

        Projectile.Opacity = 0f;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            Projectile.SetDirection(Main.rand.NextBool().ToDirectionInt());
        }

        if (Main.rand.NextBool(100)) {
            for (int i = 0; i < 1; i++) {
                Vector2 position = Projectile.Center - Vector2.UnitY * Projectile.height / 3 + Main.rand.RandomPointInArea(4f);
                Vector2 velocity = -Vector2.UnitY * Main.rand.NextFloat(1f, 2f) + Vector2.UnitX * Main.rand.NextFloat(-1f, 1f);
                velocity.Y *= 0.25f;
                Dust dust = Dust.NewDustPerfect(position, ModContent.DustType<Dusts.CottonDust>(), velocity, Alpha: 25);
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.scale *= 0.75f;
                dust.alpha = Projectile.alpha;
            }
        }

        float offsetY = 0.1f;
        Projectile.localAI[0] = Helper.Wave(-offsetY, offsetY, 2.5f, Projectile.identity);
        Projectile.velocity.Y += Projectile.localAI[0] * 0.1f;
        Projectile.rotation = Projectile.localAI[0] * 1f + Projectile.velocity.X * 0.1f;

        Projectile.velocity *= 0.97f;

        Projectile.OffsetTheSameProjectile(0.05f);

        if (Projectile.timeLeft < 20) {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, 0.1f);
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }
        else {
            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.2f);
        }
    }

    public override void OnKill(int timeLeft) {
        
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDraw(lightColor * Projectile.Opacity);

        return false;
    }
}
