using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Buffs;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

sealed class TrackingBolt : ModProjectile {
    private readonly VertexStrip _vertexStrip = new VertexStrip();

    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
        ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
    }

    public override void SetDefaults() {
        int width = 4; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = true;
        Projectile.hostile = false;

        Projectile.DamageType = DamageClass.Magic;
        Projectile.aiStyle = 0;

        Projectile.penetrate = 1;
        Projectile.timeLeft = 900;

        Projectile.extraUpdates = 1;
        Projectile.tileCollide = false;

        Projectile.Opacity = 0f;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox.Inflate(6, 6);
    }

    public override void AI() {
        if (Projectile.Opacity < 1f) {
            Projectile.Opacity += 0.1f;
        }

        float maxDetectRadius = 600f;
        float homingSpeed = 5;

        Lighting.AddLight(Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 40f, new Vector3(194, 44, 44) * 0.0035f);

        int num = 5;
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
                //int num4 = Projectile.FindTargetWithLineOfSight();
                //if (num4 != -1)
                //    Projectile.ai[1] = num4;
                //else if (Projectile.velocity.Length() < 2f)
                //    Projectile.velocity = Projectile.DirectionFrom(player.Center) * num;
                //else
                //    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * num;
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

        Projectile.rotation = Projectile.velocity.ToRotation();


        for (int num28 = Projectile.oldPos.Length - 1; num28 > 0; num28--) {
            Projectile.oldPos[num28] = Projectile.oldPos[num28 - 1];
            Projectile.oldRot[num28] = Projectile.oldRot[num28 - 1];
        }

        Projectile.oldPos[0] = Projectile.position;
        Projectile.oldRot[0] = Projectile.rotation;
    }

    public NPC FindClosestNPC(float maxDetectDistance) {
        NPC closestNPC = null;
        float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;
        for (int npc = 0; npc < Main.maxNPCs; npc++) {
            NPC target = Main.npc[npc];
            if (target.CanBeChasedBy()) {
                float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                if (sqrDistanceToTarget < sqrMaxDetectDistance) {
                    sqrMaxDetectDistance = sqrDistanceToTarget;
                    closestNPC = target;
                }
            }
        }
        return closestNPC;
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        float lifetime = 10;
        spriteBatch.DrawWithSnapshot(() => {
            MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
            miscShaderData.UseSaturation(-2.8f);
            miscShaderData.UseOpacity(1f);
            miscShaderData.Apply();
            _vertexStrip.PrepareStripWithProceduralPadding(
                Projectile.oldPos,
                Projectile.oldRot,
                p => Color.Lerp(new Color(194, 44, 44).MultiplyAlpha(lifetime * (p <= 0.25 ? p / 0.25f : 1f)) * Projectile.Opacity, new Color(250, 198, 164).MultiplyAlpha(0.5f) * Projectile.Opacity, p),
                p => (float)(35 * Projectile.scale * (1.0 - p)),
                -Main.screenPosition + Projectile.Size / 2, true);
            _vertexStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }, sortMode: SpriteSortMode.Immediate, blendState: BlendState.Additive, samplerState: SamplerState.LinearClamp, depthStencilState: DepthStencilState.Default, rasterizerState: RasterizerState.CullNone);
        //spriteBatch.End();
        //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
        //MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
        //miscShaderData.UseSaturation(-2.8f);
        //miscShaderData.UseOpacity(1f);
        //miscShaderData.Apply();
        //_vertexStrip.PrepareStripWithProceduralPadding(
        //    Projectile.oldPos,
        //    Projectile.oldRot,
        //    p => Color.Lerp(new Color(194, 44, 44).MultiplyAlpha(lifetime * (p <= 0.25 ? p / 0.25f : 1f)) * Projectile.Opacity, new Color(250, 198, 164).MultiplyAlpha(0.5f) * Projectile.Opacity, p),
        //    p => (float)(35 * Projectile.scale * (1.0 - p)),
        //    -Main.screenPosition + Projectile.Size / 2, true);
        //_vertexStrip.DrawTrail();
        //Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        //spriteBatch.EndBlendState();
        return false;
    }

    public override void OnKill(int timeLeft) {
        for (int num615 = 0; num615 < 12; num615++) {
            int num616 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y) - Projectile.oldVelocity.SafeNormalize(Vector2.Zero) * 15f, Projectile.width, Projectile.height, 66, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, Color.Lerp(Color.Red, Color.White, 0.5f + Main.rand.NextFloatRange(0.1f)), Main.rand.NextFloat(1.35f, 2f) * 0.75f);
            Main.dust[num616].noGravity = true;
            Dust dust2 = Main.dust[num616];
            dust2.scale *= 0.65f;
            dust2 = Main.dust[num616];
            dust2.velocity *= Main.rand.NextFloat(1.5f, 2.5f);
            dust2.velocity *= 0.5f;
        }
        float distance = 30f;
        for (int findNPC = 0; findNPC < Main.maxNPCs; findNPC++) {
            NPC npc = Main.npc[findNPC];
            if (npc.active && npc.CanBeChasedBy() && npc.life > 0 && Vector2.Distance(Projectile.Center, npc.Center) < distance) {
                npc.AddBuff(ModContent.BuffType<FlametrackerDebuff>(), 3600);
            }
        }
        SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
    }

    public override bool? CanCutTiles()
        => false;
}