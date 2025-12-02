using Microsoft.Xna.Framework;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

sealed class Corruptor : ModProjectile {
    private static ushort TIMELEFT => 300;

    public override void SetStaticDefaults() => Projectile.SetTrail(2, 3);

    public override void SetDefaults() {
        Projectile.SetSizeValues(40);

        Projectile.aiStyle = -1;
        Projectile.timeLeft = TIMELEFT;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;

        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.penetrate = -1;
    }

    public override void AI() {
        ushort type = (ushort)ModContent.DustType<Dusts.Corruptor2>();
        if (Main.rand.NextBool(8)) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, type);
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(1f, 1.5f);
        }

        ref float initValue = ref Projectile.localAI[0];
        if (initValue == 0f) {
            initValue = 1f;
            int dustCount = 16;
            int areaSize = 7;
            for (int i = 0; i < dustCount; i++) {
                Vector2 dustVelocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(areaSize);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, type, dustVelocity);
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
                dust.velocity *= Main.rand.NextFloat();
                dust.velocity += Projectile.velocity * Main.rand.NextFloat();
                dust.noGravity = true;
            }
        }

        ref Vector2 velocity = ref Projectile.velocity;
        velocity += velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize() * 0.25f;

        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
    }

    public override void OnKill(int timeLeft) {
        ushort type = (ushort)ModContent.DustType<Dusts.Corruptor2>();
        for (int i = 0; i < 14; i++) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, type);
            dust.noGravity = true;
            dust.scale *= Main.rand.NextFloat(1f, 1.5f);
        }
        SoundEngine.PlaySound(SoundID.NPCHit2 with { Volume = 0.7f, Pitch = 0.3f, MaxInstances = 3 }, Projectile.Center);
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawShadowTrails(lightColor * Projectile.Opacity, 0.5f, 1, 0f);
        Projectile.QuickDraw(lightColor * Projectile.Opacity, 0f);

        return false;
    }
}
