using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class VengefulSpirit : ModProjectile {
    public override Color? GetAlpha(Color drawColor) => new Color(200, 200, 200, 100) * (1f - Projectile.alpha / 255f);

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 4;
    }

    public override void SetDefaults() {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.aiStyle = -1;
        Projectile.scale = 1f;
        Projectile.minion = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.hostile = false;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + (float)Math.PI;
            Projectile.localAI[0] = 1f;
        }

        float maxDetectRadius = 600f;
        float homingSpeed = 5;

        int num = 4;
        Player player = Main.player[Projectile.owner];
        int num2 = Main.maxTilesY * 16;
        int num3 = 0;
        if (Projectile.ai[0] >= 0f)
            num3 = (int)(Projectile.ai[1] / (float)num2);

        bool flag = Projectile.ai[0] == -1f || Projectile.ai[0] == -2f;

        if (Projectile.owner == Main.myPlayer) {
            if (Projectile.ai[0] >= 0f) {
                Projectile.netUpdate = true;
                Projectile.ai[0] = -1f;
                Projectile.ai[1] = -1f;
            }

            if (flag && Projectile.ai[1] == -1f) {
                int num5 = Projectile.FindTargetWithLineOfSight();
                if (num5 != -1) {
                    Projectile.ai[1] = num5;
                    Projectile.netUpdate = true;
                }
            }
        }

        Vector2? vector = null;
        float amount = 1f;
        if (Projectile.ai[0] > 0f && Projectile.ai[1] > 0f)
            vector = new Vector2(Projectile.ai[0], Projectile.ai[1] % (float)num2);

        if (flag && Projectile.ai[1] >= 0f) {
            int num6 = (int)Projectile.ai[1];
            if (Main.npc.IndexInRange(num6)) {
                NPC nPC = Main.npc[num6];
                if (nPC.CanBeChasedBy(this)) {
                    vector = nPC.Center;
                    float t = Projectile.Distance(vector.Value);
                    float num7 = Utils.GetLerpValue(0f, 100f, t, clamped: true) * Utils.GetLerpValue(600f, 400f, t, clamped: true);
                    amount = MathHelper.Lerp(0f, 0.2f, Utils.GetLerpValue(200f, 20f, 1f - num7, clamped: true));
                }
                else {
                    Projectile.ai[1] = -1f;
                    Projectile.netUpdate = true;
                }
            }
        }

        bool flag2 = false;
        if (flag)
            flag2 = true;

        if (vector.HasValue) {
            Vector2 value = vector.Value;
            if (Projectile.Distance(value) >= 64f) {
                flag2 = true;
                Vector2 v = value - Projectile.Center;
                Vector2 vector2 = v.SafeNormalize(Vector2.Zero);
                float num8 = Math.Min(num, v.Length());
                Vector2 value2 = vector2 * num8;
                if (Projectile.velocity.Length() < 4f)
                    Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(0.7853981852531433).SafeNormalize(Vector2.Zero) * 4f;

                if (Projectile.velocity.HasNaNs())
                    Projectile.Kill();

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, value2, amount);
            }
            else {
                Projectile.velocity *= 0.3f;
                Projectile.velocity += (value - Projectile.Center) * 0.3f;
                flag2 = Projectile.velocity.Length() >= 2f;
            }

            if (Projectile.timeLeft < 60)
                Projectile.timeLeft = 60;
        }

        if (flag && Projectile.ai[1] < 0f) {
            if (Projectile.velocity.Length() != (float)num)
                Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.velocity.SafeNormalize(Vector2.UnitY) * num, 4f);

            if (Projectile.timeLeft > 300)
                Projectile.timeLeft = 300;
        }

        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + (float)Math.PI, 0.2f);

        Vector3 rgb3 = new Vector3(0f, 1f, 0.1f) * 0.35f;
        Lighting.AddLight(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f, rgb3);

        if (Main.rand.NextBool(4)) {
            int dust = Dust.NewDust(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 40f, Projectile.width, Projectile.height, DustID.Clentaminator_Green, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 100, default, 0.8f);
            Main.dust[dust].noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D projectileTexture = Projectile.GetTexture();
        int frameHeight = projectileTexture.Height / Main.projFrames[Type];
        Rectangle frameRect = new Rectangle(0, Projectile.frame * frameHeight, projectileTexture.Width, frameHeight);
        Vector2 drawOrigin = new Vector2(0f, projectileTexture.Height / Main.projFrames[Projectile.type] * 0.5f);
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Color color = Projectile.GetAlpha(lightColor);
        spriteBatch.Draw(projectileTexture, drawPos, frameRect, color, Projectile.rotation, drawOrigin, 1f, Projectile.velocity.X > 0f ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);

        return false;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        return false;
    }

    public override bool PreAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter > 4) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }
        if (Projectile.frame >= 4) {
            Projectile.frame = 0;

        }
        return true;
    }

    public override void OnKill(int timeLeft) {
        Vector2 velocity = Projectile.oldVelocity.SafeNormalize(Vector2.Zero) * 4f;
        for (int num351 = 0; num351 < 20; num351++) {
            int num352 = Dust.NewDust(Projectile.position - Projectile.velocity.SafeNormalize(Vector2.Zero) * 30f, Projectile.width, Projectile.height, DustID.Clentaminator_Green, velocity.X, velocity.Y, 0, default, Main.rand.NextFloat(1.1f, 1.3f));
            Dust dust = Main.dust[num352];
            dust.velocity *= 2f;
            Main.dust[num352].noGravity = true;
            Main.dust[num352].scale = 1.4f;
        }

        SoundEngine.PlaySound(SoundID.NPCDeath6, Projectile.Center);
    }
}
