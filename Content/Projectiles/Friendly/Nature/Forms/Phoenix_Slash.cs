using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.GameContent;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

sealed class PhoenixSlash : FormProjectile_NoTextureLoad {
    public override void SetStaticDefaults() {
        Projectile.SetTrail(3, 10);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.penetrate = -1;

        Projectile.tileCollide = false;
    }

    public override void AI() {
        if (Projectile.velocity != Vector2.Zero) {
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }

        if (Projectile.localAI[0]++ < 5f) {
            Player player = Projectile.GetOwnerAsPlayer();
            Projectile.Center = player.position + new Vector2(Projectile.ai[0], Projectile.ai[1]);

            Projectile.SetTrail(1, 40);

            if (Projectile.localAI[0] < 4) {
                for (int i = 0; i < 5; i++) {
                    int num1021 = 1;
                    int num1030 = Utils.SelectRandom<int>(Main.rand, 6, 259, 158);
                    float num127 = Main.rand.NextFloat(0.75f, 1.25f);
                    num127 *= Main.rand.NextFloat(1.25f, 1.5f);
                    int width = 20;
                    Color color = Color.Lerp(new Color(255, 165, 53), new Color(255, 247, 147), Main.rand.NextFloat());
                    if (Main.rand.NextBool()) {
                        color = Color.Lerp(new Color(255, 53, 53), new Color(255, 147, 147), Main.rand.NextFloat());
                    }
                    if (num1030 != 6) {
                        color = default;
                        num127 = 1f;
                    }
                    num127 *= 2f;
                    Vector2 position = Projectile.Center;
                    int num131 = Dust.NewDust(new Vector2(position.X, position.Y), 6, 6, num1030, 0f, 0f, 0, color, num127);
                    Main.dust[num131].position = position + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(Projectile.velocity.ToRotation()) * width / 3f;
                    Main.dust[num131].position += Projectile.velocity * Main.rand.NextFloat(-50f, 120f);
                    Main.dust[num131].customData = num1021;
                    if (Main.rand.Next(4) != 0)
                        Main.dust[num131].velocity.Y -= 0.2f;
                    Main.dust[num131].noGravity = true;
                    Dust dust2 = Main.dust[num131];
                    dust2.velocity *= 0.5f;
                    dust2 = Main.dust[num131];
                    dust2.velocity += position.DirectionTo(Main.dust[num131].position) * Main.rand.NextFloat(2f, 5f) * 0.8f;
                    dust2.velocity += Projectile.velocity * Main.rand.NextFloat(2f, 5f) * 0.625f * 0.8f;
                }
            }
            else {
                for (int i = 0; i < 1; i++) {
                    int num1021 = 1;
                    int num1030 = Utils.SelectRandom<int>(Main.rand, 6, 259, 158);
                    float num127 = Main.rand.NextFloat(0.75f, 1.25f);
                    num127 *= Main.rand.NextFloat(1.25f, 1.5f);
                    int width = 20;
                    Color color = Color.Lerp(new Color(255, 165, 53), new Color(255, 247, 147), Main.rand.NextFloat());
                    if (Main.rand.NextBool()) {
                        color = Color.Lerp(new Color(255, 53, 53), new Color(255, 147, 147), Main.rand.NextFloat());
                    }
                    if (num1030 != 6) {
                        color = default;
                        num127 = 1f;
                    }
                    num127 *= 2f;
                    Vector2 position = Projectile.Center;
                    int num131 = Dust.NewDust(new Vector2(position.X, position.Y), 6, 6, num1030, 0f, 0f, 0, color, num127);
                    Main.dust[num131].position = position + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(Projectile.velocity.ToRotation()) * width / 3f;
                    Main.dust[num131].position += Projectile.velocity * Main.rand.NextFloat(0f, 100f);
                    Main.dust[num131].customData = num1021;
                    Main.dust[num131].noGravity = true;
                    if (Main.rand.Next(4) != 0)
                        Main.dust[num131].velocity.Y -= 0.2f;
                    Main.dust[num131].noGravity = true;
                    Dust dust2 = Main.dust[num131];
                    dust2.velocity *= 0.5f;
                    dust2 = Main.dust[num131];
                    dust2.velocity += position.DirectionTo(Main.dust[num131].position) * Main.rand.NextFloat(2f, 5f) * 0.8f;
                    dust2.velocity += Projectile.velocity * Main.rand.NextFloat(2f, 5f) * 0.625f * 0.8f * Main.rand.NextFloat(5, 10) * 2.5f;
                    dust2.noGravity = true;
                }
            }
        }
        else {
            Projectile.velocity = Vector2.Zero;

            Projectile.SetTrail(-1, 40);

            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, 0.1f);
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }
    }

    protected override void Draw(ref Color lightColor) {
        Texture2D texture = ResourceManager.DefaultSparkle;
        Color color = Color.Lerp(new Color(249, 75, 7), new Color(255, 231, 66), Projectile.ai[2]).MultiplyAlpha(0.5f) * Projectile.Opacity;

        Projectile projectile = Projectile;
        Texture2D mainTex = texture;

        Rectangle frameBox = mainTex.Bounds;
        SpriteEffects effects = projectile.spriteDirection.ToSpriteEffects();
        Vector2 origin = frameBox.Centered();

        int howMany = projectile.oldPos.Length;
        int step = 1;
        Color drawColor = color;
        float maxAlpha = 1f;
        float alphaStep = maxAlpha / howMany;
        for (int i = 1; i < howMany; i += step) {
            Main.spriteBatch.Draw(mainTex, projectile.oldPos[i] + projectile.Size / 2f - Main.screenPosition, frameBox,
                drawColor * (maxAlpha - (i * alphaStep)), projectile.oldRot[i] + Projectile.rotation, origin, new Vector2(1f, 5f), effects, 0);

            for (float num6 = 0f; num6 < 4f; num6 += 1f) {
                float num3 = ((float)(TimeSystem.TimeForVisualEffects * 60f + Projectile.whoAmI * 10) / 40f * ((float)Math.PI * 2f)).ToRotationVector2().X * 3f;
                Color color2 = new Color(80, 70, 40, 0) * (num3 / 8f + 0.5f) * 0.8f * Projectile.Opacity;
                Vector2 position2 = projectile.oldPos[i] + projectile.Size / 2f + (num6 * ((float)Math.PI / 2f)).ToRotationVector2() * num3;

                Main.spriteBatch.Draw(mainTex, position2 - Main.screenPosition, frameBox,
                    color2 * (maxAlpha - (i * alphaStep)), projectile.oldRot[i] + Projectile.rotation, origin, new Vector2(1f, 5f), effects, 0);
            }
        }
    }
}
