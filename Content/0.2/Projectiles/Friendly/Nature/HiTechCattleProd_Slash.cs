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

using static RoA.Content.Projectiles.Friendly.Nature.HiTechStar;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class HiTechSlash : NatureProjectile {
    public Vector2 OwnerCenter => new(Projectile.ai[0], Projectile.ai[1]);

    public override string Texture => ResourceManager.NatureProjectileTextures + "HiTechCattleProd_Slash";

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(3);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.timeLeft = 60;

        Projectile.tileCollide = false;

        Projectile.penetrate = -1;

        Projectile.Opacity = 0f;

        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        if (Projectile.ai[2] >= HiTechStar.SLASHCOUNT) {
            Projectile.Kill();
            return;
        }
        if (Projectile.frame < Projectile.GetFrameCount() - 1 && Projectile.localAI[0]++ > 1f) {
            Projectile.localAI[0] = 0f;
            Projectile.frame++;
        }
        Projectile.velocity *= 0.9f;
        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f * Utils.GetLerpValue(0, 20, Projectile.timeLeft, true), 0.15f);
        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        Player owner = Projectile.GetOwnerAsPlayer();
        if (Projectile.timeLeft < 50) {
            if (Projectile.localAI[1] == 0f) {
                Projectile.localAI[1] = 1f;
                if (Projectile.IsOwnerLocal()) {
                    ProjectileUtils.SpawnPlayerOwnedProjectile<HiTechSlash>(new ProjectileUtils.SpawnProjectileArgs(owner, Projectile.GetSource_FromAI()) {
                        Damage = Projectile.damage,
                        KnockBack = Projectile.knockBack,
                        Position = Projectile.Center,
                        Velocity = Projectile.Center.DirectionTo(OwnerCenter).RotatedByRandom(MathHelper.PiOver2) * 10f,
                        AI0 = Projectile.ai[0],
                        AI1 = Projectile.ai[1],
                        AI2 = Projectile.ai[2] + 1
                    });
                }
            }
        }
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
        for (int i2 = 0; i2 < 2; i2++) {
            float wave = Helper.Wave(0.25f, 1f, 10f, i2);
            batch.Draw(texture, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color * wave,
                Scale = scale,
                Rotation = rotation
            });
            batch.DrawWithSnapshot(() => {
                batch.Draw(texture, position, DrawInfo.Default with {
                    Clip = clip,
                    Origin = origin,
                    Color = color * 0.5f,
                    Scale = scale * 1.1f,
                    Rotation = rotation
                });
            }, blendState: BlendState.Additive);
        }

        return false;
    }
}
