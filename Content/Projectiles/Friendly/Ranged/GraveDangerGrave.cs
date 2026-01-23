using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Projectiles;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class GraveDangerGrave : ModProjectile, ISpawnCopies {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(6);

    float ISpawnCopies.CopyDeathFrequency => 0.1f;

    private bool _collideX, _collideY;
    private float _copyCounter;
    private float _scale, _trailOpacity;

    public ref float DirectionXValue => ref Projectile.localAI[1];
    public ref float DirectionYValue => ref Projectile.localAI[2];

    public override void SetStaticDefaults() {
        ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;

        ProjectileID.Sets.Explosive[Type] = true;

        Projectile.SetFrameCount(2);

        Projectile.SetTrail(2, 4);
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(24);

        Projectile.friendly = true;
        Projectile.penetrate = 3;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.timeLeft = TIMELEFT;

        Projectile.Opacity = 0f;
        _scale = 0f;
    }

    public override void PrepareBombToBlow() {
        Projectile.tileCollide = false;
        Projectile.alpha = 255;

        Projectile.Resize(150, 150);
        Projectile.ai[2] = Projectile.knockBack;
        Projectile.knockBack = 8f;

        Projectile.netUpdate = true;
    }

    public override bool ShouldUpdatePosition() => true;

    public override void AI() {
        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.15f);
        _scale = Helper.Approach(_scale, 1f, 0.0875f);
        if (Projectile.Opacity >= 1f) {
            _trailOpacity = Helper.Approach(_trailOpacity, 1f, 0.075f);
        }

        if (Projectile.localAI[0] == 2f) {
            _collideX = MathF.Abs(Projectile.velocity.X - Projectile.oldVelocity.X) > 6f;
            _collideY = MathF.Abs(Projectile.velocity.Y - Projectile.oldVelocity.Y) > 6f;

            if (MathF.Abs(Projectile.velocity.Y) < 0.1f && MathF.Abs(Projectile.velocity.X) < 0.1f) {
                if (Projectile.owner == Main.myPlayer) {
                    Projectile.PrepareBombToBlow();
                }
                Projectile.Kill();
            }

            if (Projectile.ai[1] == 0f) {
                if (_collideY)
                    Projectile.ai[0] = 2f;

                if (!_collideY && Projectile.ai[0] == 2f) {
                    DirectionXValue = -DirectionXValue;
                    Projectile.ai[1] = 1f;
                    Projectile.ai[0] = 1f;
                }

                if (_collideX) {
                    DirectionYValue = -DirectionYValue;
                    Projectile.ai[1] = 1f;
                }
            }
            else {
                if (_collideX)
                    Projectile.ai[0] = 2f;

                if (!_collideX && Projectile.ai[0] == 2f) {
                    DirectionYValue = -DirectionYValue;
                    Projectile.ai[1] = 0f;
                    Projectile.ai[0] = 1f;
                }

                if (_collideY) {
                    DirectionXValue = -DirectionXValue;
                    Projectile.ai[1] = 0f;
                }
            }
            Projectile.velocity.X = 6 * DirectionXValue;
            Projectile.velocity.Y = 6 * DirectionYValue;

            Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
        }

        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            float num114 = 4f;
            for (int num115 = 0; (float)num115 < num114; num115++) {
                Vector2 spinningpoint6 = Vector2.UnitX * 0f;
                spinningpoint6 += -Vector2.UnitY.RotatedBy((float)num115 * ((float)Math.PI * 2f / num114)) * new Vector2(1f, 4f);
                spinningpoint6 = spinningpoint6.RotatedBy(Projectile.velocity.ToRotation());
                int num116 = Dust.NewDust(Projectile.Center, 0, 0, DustID.Torch);
                Main.dust[num116].scale = 1.7f;
                Main.dust[num116].noGravity = true;
                Main.dust[num116].position = Projectile.Center + spinningpoint6 + Projectile.velocity.SafeNormalize(Vector2.Zero) * 30f;
                Main.dust[num116].velocity = Main.dust[num116].velocity * 2f + spinningpoint6.SafeNormalize(Vector2.UnitY) * 0.3f + Projectile.velocity.SafeNormalize(Vector2.Zero) * 3f;
                Main.dust[num116].velocity *= 0.7f;
                Main.dust[num116].position += Main.dust[num116].velocity * 5f;
            }

            DirectionXValue = Projectile.velocity.X.GetDirection();
            DirectionYValue = Projectile.velocity.Y.GetDirection();
            Projectile.ai[0] = 1f;

            Projectile.frame = Main.rand.NextBool().ToInt();

            CopyHandler.InitializeCopies(Projectile, 10);
        }

        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
            Projectile.PrepareBombToBlow();
        }

        if (Projectile.localAI[0] != 2f) {
            Projectile.velocity.Y += 0.05f;
        }

        if (_trailOpacity >= 1f && _copyCounter++ % 4 == 0) {
            CopyHandler.MakeCopy(Projectile);
        }
        Projectile.rotation += Projectile.velocity.X * 0.04375f;

        Projectile.oldPos[0] = Projectile.position + Projectile.gfxOffY * Vector2.UnitY;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        //if (Projectile.velocity.X != oldVelocity.X) {
        //    Projectile.velocity.X = oldVelocity.X * -0.4f;
        //}
        //if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y > 0.7f) {
        //    Projectile.velocity.Y = oldVelocity.Y * -0.4f;
        //}
        if (Projectile.velocity == -oldVelocity) {
            Projectile.PrepareBombToBlow();
        }

        if (Projectile.localAI[0] != 2f) {
            DirectionXValue = Projectile.velocity.X.GetDirection();
            DirectionYValue = Projectile.velocity.Y.GetDirection();

            Projectile.velocity.Y *= 0f;
            Projectile.oldVelocity.Y *= 0f;
        }

        Projectile.localAI[0] = 2f;

        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.IsOwnerLocal()) {
            int count = 6;
            for (int i = 0; i < count; i++) {
                float angle = MathHelper.TwoPi * i / count;
                float bulletSpeed = 4f;
                bulletSpeed *= Main.rand.NextFloat(0.75f, 1.25f);
                Vector2 velocity = Vector2.One.RotatedBy(angle + MathHelper.PiOver4 * 0.5f * Main.rand.NextFloatDirection()) * bulletSpeed;
                ProjectileUtils.SpawnPlayerOwnedProjectile<GraveDangerSplinter>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_Death()) {
                    Position = Projectile.Center,
                    Velocity = velocity,
                    Damage = Projectile.damage,
                    KnockBack = 0f
                });
            }
        }

        Explode();
    }

    private void Explode() {
        SoundEngine.PlaySound(SoundID.Item62, Projectile.position);

        Projectile.Resize(34, 34);

        for (int i = 0; i < 30; i++) {
            var smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
            smoke.velocity *= 1.4f;
        }

        for (int j = 0; j < 20; j++) {
            var fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3.5f);
            fireDust.noGravity = true;
            fireDust.velocity *= 7f;
            fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
            fireDust.velocity *= 3f;
        }

        for (int k = 0; k < 2; k++) {
            float speedMulti = 0.4f;
            if (k == 1) {
                speedMulti = 0.8f;
            }

            var smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
            smokeGore.velocity *= speedMulti;
            smokeGore.velocity += Vector2.One;
            smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
            smokeGore.velocity *= speedMulti;
            smokeGore.velocity.X -= 1f;
            smokeGore.velocity.Y += 1f;
            smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
            smokeGore.velocity *= speedMulti;
            smokeGore.velocity.X += 1f;
            smokeGore.velocity.Y -= 1f;
            smokeGore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position, default, Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
            smokeGore.velocity *= speedMulti;
            smokeGore.velocity -= Vector2.One;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        var handler = Projectile.GetGlobalProjectile<CopyHandler>();
        var copyData = handler.CopyData;
        int width = texture.Width,
            height = texture.Height / 2;
        for (int i = 0; i < 10; i++) {
            CopyHandler.CopyInfo copyInfo = copyData![i];
            if (copyInfo.Opacity <= 0f) {
                continue;
            }
            if (MathUtils.Approximately(copyInfo.Position, Projectile.Center, 2f)) {
                continue;
            }
            batch.Draw(texture, copyInfo.Position, DrawInfo.Default with {
                Color = lightColor * MathUtils.Clamp01(copyInfo.Opacity) * Projectile.Opacity * 0.5f * _trailOpacity,
                Rotation = copyInfo.Rotation,
                Scale = Vector2.One * MathF.Max(copyInfo.Scale, 1f) * _scale,
                Origin = new Vector2(width, height) / 2f,
                Clip = new Rectangle(0, copyInfo.UsedFrame * height, width, height)
            });
        }

        Color shadowColor = lightColor * Projectile.Opacity;
        Projectile.QuickDrawShadowTrails(shadowColor * _trailOpacity, 0.5f, 1, 0f, scale: _scale);
        Projectile.QuickDrawAnimated(shadowColor, scale: Vector2.One * _scale);

        return false;
    }
}
