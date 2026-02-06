using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Content.Forms;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;

using static RoA.Common.Druid.Forms.BaseForm;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

[Tracked]
sealed class PhoenixFireball : FormProjectile {
    private float _transitToDark;
    private bool _phoenixDashed;
    private int _targetWhoAmI;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float MainOffsetValue => ref Projectile.localAI[2];

    public ref float MeInQueueValue => ref Projectile.ai[0];
    public ref float OffsetXValue => ref Projectile.ai[1];
    public ref float OffsetYValue => ref Projectile.ai[2];

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
    }

    public Vector2 PositionOffset {
        get => new(OffsetXValue, OffsetYValue);   
        set  {
            OffsetXValue = value.X;
            OffsetYValue = value.Y;
        }
    }

    public bool ShotFromSpawn => MeInQueueValue < 0f;

    private static VertexStrip _vertexStrip = new VertexStrip();

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(10);

        Projectile.SetTrail(3, 30);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(20);

        Projectile.friendly = true;
        Projectile.penetrate = 3;

        Projectile.tileCollide = false;

        Projectile.ignoreWater = true;

        Projectile.Opacity = 0f;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.NPCDeath3, Projectile.Center);
        for (int num700 = 0; num700 < 15; num700++) {
            int num701 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, (0f - Projectile.velocity.X) * 0.2f, (0f - Projectile.velocity.Y) * 0.2f, 0, default(Color), Main.rand.NextFloat(1.5f, 2f));
            Main.dust[num701].position += Main.dust[num701].position.DirectionTo(Projectile.Center) * 2f;
            Main.dust[num701].noGravity = true;
            Dust dust2 = Main.dust[num701];
            dust2.velocity *= Main.rand.NextFloat(1.5f, 2f);
            num701 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, (0f - Projectile.velocity.X) * 0.2f, (0f - Projectile.velocity.Y) * 0.2f, 0, default(Color), Main.rand.NextFloat(1f, 1.5f));
            dust2 = Main.dust[num701];
            dust2.velocity *= Main.rand.NextFloat(1.5f, 2f);
        }
    }

    public override void AI() {
        Player player = Projectile.GetOwnerAsPlayer();
        float dashSpeed = 20f;
        if (!_phoenixDashed && ShotFromSpawn) {
            _phoenixDashed = true;

            Projectile.Opacity = 1f;

            if (ShotFromSpawn) {
                dashSpeed *= 0.5f;
            }

            Projectile.velocity -= player.GetFormHandler().SavedVelocity.SafeNormalize().RotatedBy(MeInQueueValue + MathHelper.PiOver2) * dashSpeed;
        }

        if (!_phoenixDashed) {
            Projectile.timeLeft = 120;
        }

        Lighting.AddLight(player.Center, 0.5f * new Color(254, 158, 135).ToVector3() * MathHelper.Lerp(1f, 1.5f, BaseFormDataStorage.GetAttackCharge(player)));

        if (!_phoenixDashed && player.GetFormHandler().AttackFactor2 >= 4f && player.GetFormHandler().AttackFactor2 < 10f) {
            _phoenixDashed = true;

            Projectile.velocity += player.GetFormHandler().SavedVelocity.SafeNormalize() * dashSpeed;
        }

        if (!_phoenixDashed && !player.GetFormHandler().IsInADruidicForm) {
            Projectile.Kill();
        }

        if (Main.rand.NextBool(_phoenixDashed ? 1 : 7)) {
            if (Main.rand.NextBool()) {
                int num495 = 8;
                int num496 = Dust.NewDust(new Vector2(Projectile.position.X + (float)num495, Projectile.position.Y + (float)num495), Projectile.width - num495 * 2, Projectile.height - num495 * 2, 6);
                Main.dust[num496].position = Projectile.Center + Main.rand.RandomPointInArea(!_phoenixDashed ? 8f : 4f);
                if (!_phoenixDashed) {
                    Main.dust[num496].position.Y -= 10f * Main.rand.NextFloat(0.75f, 1f);
                }
                Dust dust2 = Main.dust[num496];
                dust2.velocity *= 0.5f;
                dust2 = Main.dust[num496];
                dust2.velocity += Projectile.velocity * 0.5f;
                Main.dust[num496].noGravity = true;
                Main.dust[num496].noLight = false;
                Main.dust[num496].scale = 1.4f;
                if (!_phoenixDashed) {
                    Main.dust[num496].velocity.Y -= 2f * Main.rand.NextFloat(0.75f, 1f);
                    Main.dust[num496].velocity = Main.dust[num496].velocity.RotatedBy(0.075f * Main.rand.NextFloatDirection());
                }
            }
        }

        Vector2 center = player.GetPlayerCorePoint();
        if (!Init) {
            Init = true;

            if (!ShotFromSpawn) {
                Projectile.Center = center;
            }

            MainOffsetValue = PositionOffset.Length() * 0.5f;

            {
                for (int i = 0; i < 10; i++) {
                    if (Main.rand.NextBool()) {
                        continue;
                    }
                    int num1021 = 1;
                    int num1030 = Utils.SelectRandom<int>(Main.rand, 6, 259, 158);
                    float num127 = Main.rand.NextFloat(0.75f, 1.25f);
                    num127 *= Main.rand.NextFloat(1.25f, 1.5f);
                    int width = 14;
                    Color color = Color.Lerp(new Color(255, 165, 53), new Color(255, 247, 147), Main.rand.NextFloat());
                    if (Main.rand.NextBool()) {
                        color = Color.Lerp(new Color(255, 53, 53), new Color(255, 147, 147), Main.rand.NextFloat());
                    }
                    if (num1030 != 6) {
                        color = default;
                        num127 = 1f;
                    }
                    num127 *= 1.75f;
                    Vector2 position = Projectile.Center;
                    int num131 = Dust.NewDust(new Vector2(position.X, position.Y), 6, 6, num1030, 0f, 0f, 0, color, num127);
                    Main.dust[num131].position = position + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(Projectile.velocity.ToRotation()) * width / 3f;
                    Main.dust[num131].position += Projectile.velocity * 1f;
                    Main.dust[num131].customData = num1021;
                    if (Main.rand.Next(4) != 0)
                        Main.dust[num131].velocity.Y -= 0.2f;
                    Main.dust[num131].noGravity = true;
                    Dust dust2 = Main.dust[num131];
                    dust2.velocity *= 0.5f;
                    dust2 = Main.dust[num131];
                    dust2.velocity += position.DirectionTo(Main.dust[num131].position) * Main.rand.NextFloat(2f, 5f) * 0.8f;
                    dust2.velocity += Projectile.velocity * Main.rand.NextFloat(2f, 5f) * 0.25f * 0.8f;
                }
            }
        }
        ref int frame = ref Projectile.frame;
        if (!_phoenixDashed) {
            Projectile.Center = Vector2.Lerp(Projectile.Center, center + PositionOffset, 0.3f * (1f - MathUtils.Clamp01(Projectile.velocity.Length() / 5f)));
            Projectile.Center += Projectile.velocity;

            Projectile.velocity *= 0.9375f;

            PositionOffset = Vector2.Lerp(PositionOffset, Vector2.One.RotatedBy(MathHelper.TwoPi / 5 * MeInQueueValue) * MainOffsetValue, 0.1f);

            Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.15f);

            if (Projectile.Opacity < 1f) {
                return;
            }

            ref int frameCounter = ref Projectile.frameCounter;
            if (frameCounter++ > 4) {
                frameCounter = 0;
                frame++;
            }
            if (frame > 8) {
                frame = 5;
            }
        }

        _transitToDark = Helper.Wave(0f, 1f, 5f, Projectile.whoAmI * 3);

        if (!_phoenixDashed) {
            return;
        }

        Projectile.velocity = Projectile.velocity.NormalizeWithMaxLength(dashSpeed);

        NPC nPC2 = Main.npc[_targetWhoAmI];
        if (nPC2.CanBeChasedBy(this)) {
            float num487 = dashSpeed;
            Vector2 center2 = Projectile.Center;
            float num488 = nPC2.Center.X - center2.X;
            float num489 = nPC2.Center.Y - center2.Y;
            float num490 = (float)Math.Sqrt(num488 * num488 + num489 * num489);
            float num491 = num490;
            num490 = num487 / num490;
            num488 *= num490;
            num489 *= num490;
            Projectile.velocity.X = (Projectile.velocity.X * 14f + num488) / 15f;
            Projectile.velocity.Y = (Projectile.velocity.Y * 14f + num489) / 15f;
        }
        else {
            float num492 = 1000f;
            for (int num493 = 0; num493 < 200; num493++) {
                NPC nPC3 = Main.npc[num493];
                if (nPC3.CanBeChasedBy(this)) {
                    float x4 = nPC3.Center.X;
                    float y4 = nPC3.Center.Y;
                    float num494 = Math.Abs(Projectile.Center.X - x4) + Math.Abs(Projectile.Center.Y - y4);
                    if (num494 < num492 && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, nPC3.position, nPC3.width, nPC3.height)) {
                        num492 = num494;
                        _targetWhoAmI = num493;
                    }
                }
            }
        }

        //Projectile.velocity *= 0.95f;
        frame = 9;

        //int num486 = (int)this.ai[0];
        //NPC nPC2 = Main.npc[num486];
        //if (nPC2.CanBeChasedBy(this) && !NPCID.Sets.CountsAsCritter[nPC2.type]) {
        //    float num487 = 8f;
        //    Vector2 center2 = base.Center;
        //    float num488 = nPC2.Center.X - center2.X;
        //    float num489 = nPC2.Center.Y - center2.Y;
        //    float num490 = (float)Math.Sqrt(num488 * num488 + num489 * num489);
        //    float num491 = num490;
        //    num490 = num487 / num490;
        //    num488 *= num490;
        //    num489 *= num490;
        //    velocity.X = (velocity.X * 14f + num488) / 15f;
        //    velocity.Y = (velocity.Y * 14f + num489) / 15f;
        //}
        //else {
        //    float num492 = 1000f;
        //    for (int num493 = 0; num493 < 200; num493++) {
        //        NPC nPC3 = Main.npc[num493];
        //        if (nPC3.CanBeChasedBy(this) && !NPCID.Sets.CountsAsCritter[nPC3.type]) {
        //            float x4 = nPC3.Center.X;
        //            float y4 = nPC3.Center.Y;
        //            float num494 = Math.Abs(base.Center.X - x4) + Math.Abs(base.Center.Y - y4);
        //            if (num494 < num492 && Collision.CanHit(base.position, width, height, nPC3.position, nPC3.width, nPC3.height)) {
        //                num492 = num494;
        //                this.ai[0] = num493;
        //            }
        //        }
        //    }
        //}

        //int num495 = 8;
        //int num496 = Dust.NewDust(new Vector2(base.position.X + (float)num495, base.position.Y + (float)num495), width - num495 * 2, height - num495 * 2, 6);
        //Dust dust2 = Main.dust[num496];
        //dust2.velocity *= 0.5f;
        //dust2 = Main.dust[num496];
        //dust2.velocity += velocity * 0.5f;
        //Main.dust[num496].noGravity = true;
        //Main.dust[num496].noLight = true;
        //Main.dust[num496].scale = 1.4f;
    }

    private Color StripColors(float progressOnStrip) {
        progressOnStrip = 0.25f;
        float lerpValue = Utils.GetLerpValue(0f - 0.1f * _transitToDark, 0.7f - 0.2f * _transitToDark, progressOnStrip, clamped: true);
        Color result = Color.Lerp(Color.Lerp(Color.White, Color.Orange, _transitToDark * 0.5f), Color.Red, lerpValue) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
        result.A = (byte)(result.A * 0.875f);
        if (!_phoenixDashed) {
            result *= 0.5f;
        }
        return result;
    }

    private float StripWidth(float progressOnStrip) {
        progressOnStrip = 0.25f;
        float lerpValue = Utils.GetLerpValue(0f, 0.06f + _transitToDark * 0.01f, progressOnStrip, clamped: true);
        lerpValue = 1f - (1f - lerpValue) * (1f - lerpValue);
        float result = MathHelper.Lerp(24f + _transitToDark * 16f, 8f, Utils.GetLerpValue(0f, 1f, progressOnStrip, clamped: true)) * lerpValue;
        result *= _phoenixDashed ? 0.625f : 0.5f;
        return result;
    }

    public override bool PreDraw(ref Color lightColor) {
        Color baseColor = Color.White;
        if (_phoenixDashed) {
            baseColor.A /= 2;
        }

        float value = BaseFormDataStorage.GetAttackCharge(Projectile.GetOwnerAsPlayer());

        float offset = 5f;
        Vector2 position = Projectile.Center +
            (MathF.Sin(TimeSystem.TimeForVisualEffects * 5f + Projectile.whoAmI * 10f).ToRotationVector2() * offset - new Vector2(offset, -offset) / 2f) * (!_phoenixDashed).ToInt();
        Vector2 temp = Projectile.Center;
        Projectile.Center = position;

        MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
        miscShaderData.UseSaturation(_phoenixDashed ? -2f : -0.5f);
        miscShaderData.UseOpacity(10f);
        miscShaderData.UseOpacity(3f);
        miscShaderData.Shader.Parameters["uTime"]?.SetValue(TimeSystem.TimeForVisualEffects + (1f + Projectile.whoAmI) * 10);
        miscShaderData.Apply();
        _vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, 
            StripColors, StripWidth,
            -Main.screenPosition + Projectile.Size / 2f + (position - Projectile.Center), includeBacksides: true);
        _vertexStrip.DrawTrail();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        Projectile.QuickDrawAnimated(baseColor * MathHelper.Lerp(0.9f, 1f, value) * Projectile.Opacity);
        for (float num6 = 0f; num6 < 4f; num6 += 1f) {
            float num3 = ((float)(TimeSystem.TimeForVisualEffects * 60f + Projectile.whoAmI * 10) / 40f * ((float)Math.PI * 2f)).ToRotationVector2().X * 3f;
            Color color2 = new Color(80, 70, 40, 0) * (num3 / 8f + 0.5f) * 0.8f * value;
            Vector2 position2 = Projectile.Center + (num6 * ((float)Math.PI / 2f)).ToRotationVector2() * num3;

            Vector2 temp2 = Projectile.Center;
            Projectile.Center = position2;
            Projectile.QuickDrawAnimated(color2 * Projectile.Opacity);
            Projectile.Center = temp2;
        }

        Projectile.QuickDrawAnimated(baseColor * 1f * Projectile.Opacity);
        for (float num6 = 0f; num6 < 4f; num6 += 1f) {
            float num3 = ((float)(TimeSystem.TimeForVisualEffects * 60f + Projectile.whoAmI * 10) / 40f * ((float)Math.PI * 2f)).ToRotationVector2().X * 3f;
            Color color2 = new Color(80, 70, 40, 0) * (num3 / 8f + 0.5f) * 0.8f;
            Vector2 position2 = Projectile.Center + (num6 * ((float)Math.PI / 2f)).ToRotationVector2() * num3;

            Vector2 temp2 = Projectile.Center;
            Projectile.Center = position2;
            Projectile.QuickDrawAnimated(color2 * Projectile.Opacity);
            Projectile.Center = temp2;
        }

        Projectile.Center = temp;

        return false;
    }
}
