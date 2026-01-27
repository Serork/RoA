using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

sealed class MeltingStar : ModProjectile {
    private readonly VertexStrip vertexStrip = new VertexStrip();

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults() {
        int width = 18; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.DamageType = DamageClass.Melee;

        Projectile.aiStyle = 0;
        AIType = 14;

        Projectile.friendly = true;
        Projectile.ignoreWater = true;

        Projectile.penetrate = 1;
        Projectile.timeLeft = 180;

        Projectile.tileCollide = false;
        Projectile.extraUpdates = 1;

        Projectile.Opacity = 0f;
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.25f, PitchVariance = 0.5f }, Projectile.Center);
        for (int index = 0; index < 4; ++index) {
            if (Main.rand.NextChance(0.75)) {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Teleporter, Projectile.velocity.X * -0.2f, Projectile.velocity.Y * -0.2f, 150, default, Main.rand.NextFloat(1f, 1.25f));
                dust.noGravity = true;
                dust.velocity = Helper.VelocityToPoint(Projectile.Center, dust.position, Main.rand.NextFloat(8f, 12f) * 0.5f);
            }
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Vector2 hitPoint = target.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 2f;
        Vector2 normal = (-Projectile.velocity).SafeNormalize(Vector2.UnitX);
        Vector2 spinningpoint = Vector2.Reflect(Projectile.velocity, normal);
        float scale = 2.5f - Vector2.Distance(target.Center, Projectile.position) * 0.01f;
        scale = MathHelper.Clamp(scale, 0.75f, 1.15f);
        for (int i = 0; i < 4; i++) {
            if (Main.rand.NextChance(0.75)) {
                int num156 = DustID.Enchanted_Gold;
                Dust dust = Dust.NewDustPerfect(hitPoint, num156, spinningpoint.RotatedBy((float)Math.PI / 4f * Main.rand.NextFloatDirection()) * 0.6f * Main.rand.NextFloat(), 150, default, 0.5f + 0.3f * Main.rand.NextFloat());
                dust.scale *= scale;
                Dust dust2 = Dust.CloneDust(dust);
                dust2.color = Color.White;
            }
        }
        for (int i = 0; i < 2; i++) {
            if (Main.rand.NextChance(0.75)) {
                int num156 = DustID.Enchanted_Gold;
                Dust dust = Dust.NewDustPerfect(hitPoint, num156, -spinningpoint.RotatedBy((float)Math.PI / 4f * Main.rand.NextFloatDirection()) * 0.6f * Main.rand.NextFloat() * 1.5f, 150, default, 0.5f + 0.3f * Main.rand.NextFloat());
                dust.scale *= scale;
                Dust dust2 = Dust.CloneDust(dust);
                dust2.color = Color.White;
            }
        }
    }

    public override void AI() {
        if (Main.rand.Next(20) == 0 || (Main.tenthAnniversaryWorld && Main.rand.Next(15) == 0))
            for (int index = 0; index < 2; ++index) {
                if (Main.rand.Next(100) <= 35) {
                    if (Main.rand.NextChance(0.75)) {
                        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Teleporter, Projectile.velocity.X * -0.2f, Projectile.velocity.Y * -0.2f, 150, default, Main.rand.NextFloat(1f, 1.25f));
                        dust.noGravity = true;
                    }
                }
            }

        if (Projectile.Opacity < 1f) {
            Projectile.Opacity += 0.1f;
        }

        if (Main.rand.Next(20) == 0 || (Main.tenthAnniversaryWorld && Main.rand.Next(15) == 0))
            if (Main.rand.NextChance(0.75)) {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Enchanted_Gold, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default(Color), Main.rand.NextFloat(1f, 1.25f));
            }
        Projectile.ai[0]++;
        if (Projectile.ai[0] < 15f * Projectile.ai[1]) {
            Projectile.velocity *= 0.98f;
        }
        else {
            var vector = Projectile.velocity * 1.06f;
            Projectile.velocity = vector;

            float distanceFromTarget = 200f;
            Vector2 targetCenter = Projectile.position;
            bool foundTarget = false;
            // This code is required either way, used for finding a target
            for (int i = 0; i < Main.maxNPCs; i++) {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy()) {
                    float between = Vector2.Distance(npc.Center, Projectile.Center);
                    bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                    bool inRange = between < distanceFromTarget;
                    bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                    // Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
                    // The number depends on various parameters seen in the movement code below. DamageClassItemsStorage different ones out until it works alright
                    bool closeThroughWall = between < 100f;
                    if (inRange && (lineOfSight || closeThroughWall)) {
                        distanceFromTarget = between;
                        targetCenter = npc.Center;
                        foundTarget = true;
                    }
                }
            }
            if (foundTarget) {
                float speed = 10f;
                float inertia = 40f;
                Vector2 direction = targetCenter - Projectile.Center;
                direction.Normalize();
                direction *= speed;
                if (distanceFromTarget >= 40f) {
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                }
            }
        }
        float max = 15f;
        if (Projectile.velocity.Length() > max) {
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * max;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        float lifetime = Projectile.timeLeft < 15 ? Projectile.timeLeft / 15f : 1f;
        spriteBatch.BeginBlendState(BlendState.AlphaBlend, shader: true);
        GameShaders.Misc["MagicMissile"].Apply();
        vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, p => Color.Lerp(Color.OrangeRed.MultiplyAlpha(lifetime * (p <= 0.2 ? p / 0.2f : 1f)), Color.Yellow.MultiplyAlpha(0.5f), p), p => (float)(60.0 * Projectile.scale * (1.0 - p)), -Main.screenPosition + Projectile.Size / 2, true);
        vertexStrip.DrawTrail();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
        //spriteBatch.BeginBlendState(BlendState.Additive);
        //Texture2D backTexture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.TexturesPerType + "Light");
        //spriteBatch.DrawSelf(backTexture, Projectile.Center - Main.screenPosition, new Rectangle?(), DrawColor.FromNonPremultiplied(byte.MaxValue, 255, 0, 50) * Projectile.Opacity, 0.0f, backTexture.Size() / 2f, (float)(0.8f + 0.2f * Math.Sin(Main.time / 10)) * 0.75f, SpriteEffects.None, 0.0f);
        //spriteBatch.EndBlendState();
        Texture2D projectileTexture = Projectile.GetTexture();
        spriteBatch.Draw(projectileTexture, Projectile.Center - Main.screenPosition, new Rectangle?(), new Color(0.9f, 1f, 0.7f, 0.4f) * Projectile.Opacity, TimeSystem.TimeForVisualEffects * 8f * Projectile.direction + Projectile.whoAmI, projectileTexture.Size() / 2f, Projectile.scale - (float)(0.15f * Math.Sin(Main.timeForVisualEffects / 10.0)), SpriteEffects.None, 0.0f);
        spriteBatch.Draw(projectileTexture, Projectile.Center - Main.screenPosition, new Rectangle?(), new Color(0.4f, 0.1f, 0f, 0.1f), TimeSystem.TimeForVisualEffects * 8f * Projectile.direction + Projectile.whoAmI, projectileTexture.Size() / 2f, Math.Clamp(2.2f - Projectile.ai[0] / 20f, -2.5f, 2.5f), SpriteEffects.None, 0.0f);
        return false;
    }

    public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.9f) * Projectile.Opacity;
}