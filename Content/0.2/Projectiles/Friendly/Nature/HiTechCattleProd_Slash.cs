using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class HiTechSlash : NatureProjectile {
    public Vector2 OwnerCenter => new(Projectile.ai[0], Projectile.ai[1]);

    public override string Texture => ResourceManager.NatureProjectileTextures + "HiTechCattleProd_Slash";

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(3);

        Projectile.SetTrail(0, 6);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.timeLeft = 60;

        Projectile.tileCollide = false;

        Projectile.penetrate = -1;

        Projectile.Opacity = 0f;

        Projectile.extraUpdates = 1;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        Projectile.manualDirectionChange = true;

        SetNatureValues(Projectile, false, false);
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox.Inflate(20, 20);
    }

    public override void AI() {
        if (Projectile.ai[2] >= HiTechStar.SLASHCOUNT) {
            Projectile.Kill();
            return;
        }
        if (Projectile.localAI[2] == 0f) {
            Projectile.localAI[2] = 1f;
            Projectile.direction = Main.rand.NextBool().ToDirectionInt();
        }
        if (Projectile.Opacity >= 1f && Projectile.frame < Projectile.GetFrameCount() - 1 && Projectile.localAI[0]++ > 4f) {
            Projectile.localAI[0] = 0f;
            Projectile.frame++;
        }
        Projectile.velocity *= 0.95f;
        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true), 0.1f);
        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

        if (Main.rand.NextBool(20)) {
            int num674 = Utils.SelectRandom<int>(Main.rand, 226, 229);
            Vector2 center8 = Projectile.Center + Projectile.velocity.SafeNormalize() * 35f;
            int num676 = 20;
            int num677 = Dust.NewDust(center8 + Vector2.One * -num676, num676 * 2, num676 * 2, num674, 0f, 0f, 100, default(Color), 1f);
            Dust dust2 = Main.dust[num677];
            dust2.velocity *= 0.1f;
            if (Main.rand.Next(6) != 0)
                Main.dust[num677].noGravity = true;
        }
        //Player owner = Projectile.GetOwnerAsPlayer();
        //if (Projectile.timeLeft < 50) {
        //    if (Projectile.localAI[1] == 0f) {
        //        Projectile.localAI[1] = 1f;
        //        if (Projectile.IsOwnerLocal()) {
        //            ProjectileUtils.SpawnPlayerOwnedProjectile<HiTechSlash>(new ProjectileUtils.SpawnProjectileArgs(owner, Projectile.GetSource_FromAI()) {
        //                Damage = Projectile.damage,
        //                KnockBack = Projectile.knockBack,
        //                Position = Projectile.Center,
        //                Velocity = Projectile.Center.DirectionTo(OwnerCenter).RotatedByRandom(MathHelper.PiOver2) * 10f,
        //                AI0 = Projectile.ai[0],
        //                AI1 = Projectile.ai[1],
        //                AI2 = Projectile.ai[2] + 1
        //            });
        //        }
        //    }
        //}
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = Projectile.GetTexture();
        SpriteFrame frame = new(1, 3, 0, (byte)Projectile.frame);
        Rectangle clip = frame.GetSourceRectangle(texture);
        Vector2 origin = clip.Centered();
        SpriteBatch batch = Main.spriteBatch;
        Vector2 position = Projectile.Center;
        Color color = Color.White;
        color.A = 100;
        color *= Ease.CircIn(MathUtils.Clamp01(Projectile.Opacity));
        float rotation = Projectile.rotation;
        Vector2 scale = Vector2.One;
        SpriteEffects effects = Projectile.direction.ToSpriteEffects();
        for (int i = 0; i < 2; i++) {
            float wave = Helper.Wave(0.25f, 1f, 10f, i);
            int trailLength = Projectile.GetTrailCount();
            for (int i2 = 1; i2 < trailLength; i2++) {
                Vector2 position2 = Projectile.oldPos[i2] + Projectile.Size / 2f;
                Color color2 = color * (1f - i2 / (float)trailLength) * 0.5f;
                batch.Draw(texture, position2, DrawInfo.Default with {
                    Clip = clip,
                    Origin = origin,
                    Color = color2 * wave,
                    Scale = scale,
                    Rotation = rotation,
                    ImageFlip = effects
                });
                batch.DrawWithSnapshot(() => {
                    batch.Draw(texture, position2, DrawInfo.Default with {
                        Clip = clip,
                        Origin = origin,
                        Color = color2 * 0.5f,
                        Scale = scale * 1.1f,
                        Rotation = rotation,
                        ImageFlip = effects
                    });
                }, blendState: BlendState.Additive);
            }
            batch.Draw(texture, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color * wave,
                Scale = scale,
                Rotation = rotation,
                ImageFlip = effects
            });
            batch.DrawWithSnapshot(() => {
                batch.Draw(texture, position, DrawInfo.Default with {
                    Clip = clip,
                    Origin = origin,
                    Color = color * 0.5f,
                    Scale = scale * 1.1f,
                    Rotation = rotation,
                    ImageFlip = effects
                });
            }, blendState: BlendState.Additive);
        }

        return false;
    }
}
