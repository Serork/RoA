using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Content.Projectiles.Friendly.Magic;

sealed class Pixie : ModProjectile {
    private static ushort TIMELEFT = 300;
    private static byte FRAMECOUNT => 4;

    private Vector2 _magnetAccelerations;
    private Vector2 _magnetPointTarget;
    private Vector2 _positionVsMagnet;
    private Vector2 _velocityVsMagnet;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float SpawnedExplosionValue => ref Projectile.localAI[1];
    public ref float AttackTimer => ref Projectile.ai[0];
    public ref float VelocityRotationValue => ref Projectile.ai[1];
    public ref float VelocitySlowFactor => ref Projectile.ai[2];

    private static readonly SoundStyle PixieDeath = new SoundStyle(ResourceManager.ItemSounds + "Pixie");

    public override Color? GetAlpha(Color lightColor) {
        int num5 = lightColor.A;
        int num2 = (int)((double)(int)lightColor.R * 1.5);
        int num3 = (int)((double)(int)lightColor.G * 1.5);
        int num4 = (int)((double)(int)lightColor.B * 1.5);
        if (num2 > 255)
            num2 = 255;

        if (num3 > 255)
            num3 = 255;

        if (num4 > 255)
            num4 = 255;

        if (num5 < 0)
            num5 = 0;

        if (num5 > 255)
            num5 = 255;

        return new Color(num2, num3, num4, num5) * Projectile.Opacity;
    }

    public override void SetStaticDefaults() => Projectile.SetFrameCount(FRAMECOUNT);

    public override void SetDefaults() {
        Projectile.SetSizeValues(34);

        Projectile.aiStyle = -1;
        Projectile.timeLeft = TIMELEFT;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;

        Projectile.manualDirectionChange = true;

        Projectile.penetrate = 5;
    }

    public override bool ShouldUpdatePosition() => true;

    public override void AI() {
        Projectile.tileCollide = Projectile.timeLeft <= TIMELEFT - 30;

        int timeLeft = Projectile.timeLeft;
        int appearTime = 2;
        Projectile.Opacity = Utils.GetLerpValue(timeLeft, TIMELEFT, TIMELEFT - appearTime, true) * Utils.GetLerpValue(timeLeft, 0f, appearTime * 2, true);

        if (InitValue == 0f) {
            InitValue = 1f;

            Projectile.Opacity = 0f;

            Projectile.direction = Projectile.velocity.X.GetDirection();

            if (Projectile.IsOwnerLocal()) {
                float num3 = 50f;
                UnifiedRandom random = Main.rand;
                Vector2 vector2 = new Vector2(1f, 1f) * 0.15f;
                float x = MathF.Abs(random.NextFloat() * 2f - 1f) * Projectile.direction;
                float y = random.NextFloat() * 2f - 1f;
                if (MathF.Abs(y) < 0.25f) {
                    y = 0.25f * y.GetDirection();
                }
                Vector2 targetOffset = new Vector2(x, y) * num3;
                SetMagnetization(vector2, targetOffset);
            }

            Projectile.spriteDirection = -Projectile.direction;

            float rotation = Projectile.velocity.ToRotation();
            if (!Projectile.FacedRight()) {
                rotation += MathHelper.Pi;
            }
            VelocityRotationValue = rotation;
            VelocitySlowFactor = 1f;
        }

        float timeToSpawnAttack = TIMELEFT / 3;
        if (AttackTimer++ > timeToSpawnAttack) {
            AttackTimer = 0f;
        }
        //float velocitySlowFactor = Utils.Clamp(Utils.GetLerpValue(AttackTimer, timeToSpawnAttack * 0.75f, timeToSpawnAttack, true), 0.5f, 1f);
        //VelocitySlowFactor = Helper.Approach(VelocitySlowFactor, velocitySlowFactor, 0.1f);
        //if (VelocitySlowFactor <= 0.5f && SpawnedExplosionValue == 0f) {
        //    SpawnedExplosionValue = 1f;
        //    int num11 = Projectile.width / 2;
        //    for (float num17 = 0f; num17 < 1f; num17 += Main.rand.NextFloat(0.1f) + 0.3f * (1f - Projectile.scale)) {
        //        Vector2 vector10 = Vector2.UnitX.RotatedBy((float)Math.PI * 2f * num17);
        //        _ = vector10 * ((float)num11 * Projectile.scale);
        //        vector10 = vector10.RotatedByRandom(MathHelper.PiOver4 * 0.5f);
        //        Vector2 center = Projectile.Center;
        //        float num18 = Main.rand.NextFloat(1f, 2f);
        //        Dust.NewDustPerfect(center, DustID.Pixie, vector10 * num18, Alpha: 200);
        //    }
        //    Projectile.scale -= 0.15f;
        //    if (timeLeft <= timeToSpawnAttack) {
        //        Projectile.Kill();
        //    }
        //}
        //if (velocitySlowFactor >= 1f) {
        //    SpawnedExplosionValue = 0f;
        //}

        Vector2 vector = _magnetAccelerations * new Vector2(Math.Sign(_magnetPointTarget.X - _positionVsMagnet.X), Math.Sign(_magnetPointTarget.Y - _positionVsMagnet.Y));
        _velocityVsMagnet += vector;
        _positionVsMagnet += _velocityVsMagnet;
        float x2 = 3f * Projectile.direction;
        Projectile.velocity = new Vector2(x2, 0f) + _velocityVsMagnet;
        Projectile.velocity = Projectile.velocity.RotatedBy(VelocityRotationValue) * VelocitySlowFactor;
        //Projectile.position += Projectile.velocity.RotatedBy(VelocityRotationValue) * VelocitySlowFactor;

        ProjectileUtils.Animate(Projectile, 4);

        Projectile.rotation = Projectile.velocity.X * 0.05f;

        if (Main.rand.Next(12) == 0) {
            int num311 = Dust.NewDust(Projectile.position + Vector2.One * 4, Projectile.width - 4, Projectile.height - 4, DustID.Pixie, 0f, 0f, 200, default);
            Dust dust = Main.dust[num311];
            dust.velocity *= 0.3f;
        }

        float num94 = Projectile.scale * 0.8f;
        num94 *= 0.5f;
        Lighting.AddLight(Projectile.Center, num94, num94, num94 * 0.6f);

        //if (Main.rand.Next(100) == 0)
        //    SoundEngine.PlaySound(SoundID.Pixie, Projectile.position);
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.WriteVector2(_magnetAccelerations);
        writer.WriteVector2(_magnetPointTarget);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _magnetAccelerations = reader.ReadVector2();
        _magnetPointTarget = reader.ReadVector2();
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;
        hitboxCenterFrac = Vector2.One * 0.5f;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.Opacity < 0.5f) {
            return;
        }

        SoundEngine.PlaySound(PixieDeath with { PitchVariance = 0.1f }, Projectile.Center);

        int num11 = Projectile.width;
        for (float num17 = 0f; num17 < 1f; num17 += 0.15f) {
            Vector2 vector10 = Vector2.UnitX.RotatedBy((float)Math.PI * 2f * num17);
            _ = vector10 * ((float)num11 * Projectile.scale);
            vector10 = vector10.RotatedByRandom(MathHelper.PiOver4 * 0.5f);
            Vector2 center = Projectile.Center;
            float num18 = Main.rand.NextFloat(0.5f, 2f);
            Dust.NewDustPerfect(center, DustID.Pixie, vector10 * num18, Alpha: 200);
        }
    }

    private void SetMagnetization(Vector2 accelerations, Vector2 targetOffset) {
        _magnetAccelerations = accelerations;
        _magnetPointTarget = targetOffset;
        Projectile.netUpdate = true;
    }
}
