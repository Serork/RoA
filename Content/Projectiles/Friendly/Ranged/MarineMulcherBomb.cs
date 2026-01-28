using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Projectiles;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class MarineMulcherBomb : ModProjectile, ISpawnCopies {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(5);

    float ISpawnCopies.CopyDeathFrequency => 0.1f;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling[Type] = true;

        ProjectileID.Sets.Explosive[Type] = true;

        Projectile.SetTrail(2, 4);
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(12);

        Projectile.friendly = true;
        Projectile.penetrate = 3;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.timeLeft = TIMELEFT;

        Projectile.Opacity = 0f;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override void PrepareBombToBlow() {
        Projectile.tileCollide = false;
        Projectile.alpha = 255;

        Projectile.Resize(150, 150);
        Projectile.knockBack = 8f;
    }

    public override void AI() {
        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.075f);

        Lighting.AddLight(Projectile.Center, new Color(252, 144, 144).ToVector3() * 0.75f);

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

            CopyHandler.InitializeCopies(Projectile, 10);
        }

        Projectile.SetTrail(0, 4);

        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
            Projectile.PrepareBombToBlow();
        }

        Projectile.ai[0] += 1f;
        if (Projectile.Opacity > 0.5f && Projectile.localAI[2]++ % 2 == 0) {
            CopyHandler.MakeCopy(Projectile);
        }
        if (Projectile.ai[0] > 15f) {
            if (Projectile.velocity.Y == 0f) {
                Projectile.velocity.X *= 0.95f;
            }
            Projectile.velocity.Y += 0.2f;
        }
        Projectile.rotation += Projectile.velocity.X * 0.1f;

        if (Projectile.localAI[2] > 20f && Main.rand.NextBool(4)) {
            var fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<MarineMulcherTentacleDust>(), 0f, 0f, 100, default, 1.6f + Main.rand.NextFloatRange(0.4f));
            fireDust.noGravity = true;
            fireDust.position = Projectile.Center + Main.rand.RandomPointInArea(5);
            fireDust.velocity = -Projectile.oldVelocity * Main.rand.NextFloat(5f, 10f);
            fireDust.velocity *= 0.1f;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if ((double)Projectile.velocity.X != (double)oldVelocity.X)
            Projectile.velocity.X = (float)(-(double)oldVelocity.X * 1);
        if ((double)Projectile.velocity.Y != (double)oldVelocity.Y)
            Projectile.velocity.Y = (float)(-(double)oldVelocity.Y * 1);

        Projectile.ai[0] = 15f * 1;

        Projectile.timeLeft -= 60;

        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.IsOwnerLocal()) {
            int count = 8;
            for (int i = 0; i < count; i++) {
                float angle = MathHelper.TwoPi * i / count;
                float bulletSpeed = 4f;
                bulletSpeed *= Main.rand.NextFloat(0.75f, 1.25f);
                Vector2 velocity = Vector2.One.RotatedBy(angle + MathHelper.PiOver4 * 0.5f * Main.rand.NextFloatDirection()) * bulletSpeed;
                ProjectileUtils.SpawnPlayerOwnedProjectile<MarineMulcherTentacle>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_Death()) {
                    Position = Projectile.Center,
                    Velocity = velocity,
                    Damage = Projectile.damage,
                    KnockBack = 0f
                });
            }
        }

        SoundEngine.PlaySound(SoundID.Item62, Projectile.position);

        Projectile.Resize(34, 34);

        for (int i = 0; i < 30; i++) {
            var smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
            smoke.velocity *= 1.4f;
        }


        for (int j = 0; j < 10; j++) {
            var fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<MarineMulcherTentacleDust>(), 0f, 0f, 100, default, 3.5f);
            fireDust.noGravity = true;
            fireDust.velocity *= 7f;
            fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<MarineMulcherTentacleDust>(), 0f, 0f, 100, default, 1.5f);
            fireDust.velocity *= 3f;
        }

        for (int j = 0; j < 10; j++) {
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
        if (Projectile.Opacity < 0.5f) {
            return false;
        }

        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        var handler = Projectile.GetGlobalProjectile<CopyHandler>();
        var copyData = handler.CopyData;
        int width = texture.Width,
            height = texture.Height;
        Color shadowColor = Color.White;
        shadowColor = Color.Lerp(shadowColor, shadowColor with { A = 50 }, 1f - Projectile.timeLeft / (float)TIMELEFT);
        for (int i = 0; i < 10; i++) {
            CopyHandler.CopyInfo copyInfo = copyData![i];
            if (copyInfo.Opacity <= 0f) {
                continue;
            }
            if (MathUtils.Approximately(copyInfo.Position, Projectile.Center, 2f)) {
                continue;
            }
            batch.Draw(texture, copyInfo.Position, DrawInfo.Default with {
                Color = shadowColor * MathUtils.Clamp01(copyInfo.Opacity) * 0.5f,
                Rotation = copyInfo.Rotation,
                Scale = Vector2.One * MathF.Max(copyInfo.Scale, 1f),
                Origin = new Vector2(width, height) / 2f,
                Clip = new Rectangle(0, copyInfo.UsedFrame * height, width, height)
            });
        }

        Projectile.QuickDrawShadowTrails(shadowColor, 0.5f, 1, 0f);
        Projectile.QuickDraw(shadowColor);

        return false;
    }
}
