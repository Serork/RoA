using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class MushroomSpore : NatureProjectile {
    private enum State {
        Direct,
        Float
    }

    private State _state = State.Direct;

    public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

    protected override void SafeSetDefaults() {
        int width = 16; int height = width + 4;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = 1;
        Projectile.timeLeft = 600;

        Projectile.tileCollide = true;
        Projectile.friendly = true;
    }

    public override void SafePostAI() => ProjectileUtils.Animate(Projectile, 4);

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void AI() {
        //Helper.ApplyWindPhysics(Projectile.Center, ref Projectile.velocity);

        float speed = 0.05f;
        switch (_state) {
            case State.Direct:
                if (Projectile.velocity.Length() > 2f) {
                    Projectile.velocity *= 0.98f;
                }
                else {
                    Projectile.ai[0] = Math.Sign(Projectile.velocity.X);
                    float max = 5f;
                    Projectile.ai[1] = Math.Clamp(Math.Abs(Projectile.velocity.X), -max, max);
                    Projectile.ai[2] = Projectile.ai[1] * speed;
                    _state = State.Float;
                }
                break;
            case State.Float:
                Projectile.ai[2] += Projectile.ai[0] * speed;
                if (Projectile.ai[2] <= -Projectile.ai[1] || Projectile.ai[2] >= Projectile.ai[1]) {
                    Projectile.ai[0] *= -1f;
                }
                Projectile.velocity.X *= 0.98f;
                Projectile.velocity.X -= 0.02f * Projectile.ai[2];
                Projectile.position.X += Projectile.ai[2];
                Projectile.velocity.Y += 0.01f;
                Projectile.velocity.Y = Math.Min(Projectile.velocity.Y, 12f);
                break;
        }
        float minSpeed = 4.5f;
        if (Projectile.velocity.Length() > minSpeed) {
            Projectile.rotation = Helper.VelocityAngle(Projectile.velocity);
            return;
        }
        float lerp = Math.Abs(Projectile.rotation) * Math.Max(0.5f, minSpeed - 1f - Projectile.velocity.Length()) * TimeSystem.LogicDeltaTime;
        Projectile.rotation = Helper.SmoothAngleLerp(Projectile.rotation, 0f, lerp * minSpeed);
        Projectile.rotation += -Projectile.ai[2] * 0.0125f;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SoundEngine.PlaySound(SoundID.Dig with { Pitch = 1.2f, Volume = 0.5f }, Projectile.Center);

        return base.OnTileCollide(oldVelocity);
    }

    public override void OnKill(int timeLeft) {
        if (Main.netMode != NetmodeID.Server) {
            for (int i = 0; i < Main.rand.Next(3, 6); i++) {
                int dust = Dust.NewDust(Projectile.Center, 5, 5, DustID.Pumpkin, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 100, default, 1.1f);
                Main.dust[dust].velocity.Y *= 0.1f;
                Main.dust[dust].scale *= 0.8f;
            }
        }
    }
}