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
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

using static RoA.Common.Druid.Forms.BaseForm;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

[Tracked]
sealed class PhoenixFireball : FormProjectile {
    public enum StateType : byte {
        Idle,
        Shoot,
        Count
    }

    private float _posRotation;
    private float _transitToDark;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float MeInQueueValue => ref Projectile.localAI[1];
    public ref float MainOffsetValue => ref Projectile.localAI[2];

    public ref float StateValue => ref Projectile.ai[0];
    public ref float OffsetXValue => ref Projectile.ai[1];
    public ref float OffsetYValue => ref Projectile.ai[2];

    public StateType State {
        get => (StateType)StateValue;
        set => StateValue = Utils.Clamp((byte)value, (byte)StateType.Idle, (byte)StateType.Count);
    }

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

    private static VertexStrip _vertexStrip = new VertexStrip();

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(10);

        Projectile.SetTrail(3, 30);
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(20);

        Projectile.friendly = true;
        Projectile.penetrate = -1;

        Projectile.tileCollide = false;

        Projectile.Opacity = 0f;
    }

    public override void AI() {
        Projectile.timeLeft = 2;

        Player player = Projectile.GetOwnerAsPlayer();
        int count = TrackedEntitiesSystem.GetTrackedProjectile<PhoenixFireball>(checkProjectile => checkProjectile.owner != player.whoAmI).Count();

        _posRotation = Utils.AngleLerp(_posRotation,
                    player.GetFormHandler().AttackFactor * TimeSystem.LogicDeltaTime * 0.5f + player.GetFormHandler().AttackCount * (count / (float)Phoenix.FIREBALLCOUNT),
                    1f);

        if (!player.GetFormHandler().IsInADruidicForm) {
            Projectile.Kill();
        }

        Vector2 center = player.GetPlayerCorePoint();
        if (!Init) {
            Init = true;

            Projectile.Center = center;

            MainOffsetValue = PositionOffset.Length() * 0.5f;

            MeInQueueValue = count + 1;
        }
        switch (State) {
            case StateType.Idle:
                Projectile.Center = Vector2.Lerp(Projectile.Center, center + PositionOffset, 0.3f * (1f - MathUtils.Clamp01(Projectile.velocity.Length() / 5f)));
                break;
            case StateType.Shoot:
                break;
        }

        Projectile.Center += Projectile.velocity;

        Projectile.velocity *= 0.9375f;

        PositionOffset = Vector2.Lerp(PositionOffset, Vector2.One.RotatedBy(MathHelper.TwoPi / 5 * MeInQueueValue) * MainOffsetValue, 0.1f);

        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.15f);

        if (Projectile.Opacity < 1f) {
            return;
        }

        ref int frame = ref Projectile.frame;
        ref int frameCounter = ref Projectile.frameCounter;
        if (frameCounter++ > 4) {
            frameCounter = 0;
            frame++;
        }
        if (frame > 8) {
            frame = 5;
        }

        _transitToDark = Helper.Wave(0f, 1f, 5f, Projectile.whoAmI);
    }

    private Color StripColors(float progressOnStrip) {
        progressOnStrip = 0.25f;
        float lerpValue = Utils.GetLerpValue(0f - 0.1f * _transitToDark, 0.7f - 0.2f * _transitToDark, progressOnStrip, clamped: true);
        Color result = Color.Lerp(Color.Lerp(Color.White, Color.Orange, _transitToDark * 0.5f), Color.Red, lerpValue) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
        result.A = (byte)(result.A * 0.875f);
        result *= 0.5f;
        return result;
    }

    private float StripWidth(float progressOnStrip) {
        progressOnStrip = 0.25f;
        float lerpValue = Utils.GetLerpValue(0f, 0.06f + _transitToDark * 0.01f, progressOnStrip, clamped: true);
        lerpValue = 1f - (1f - lerpValue) * (1f - lerpValue);
        float result = MathHelper.Lerp(24f + _transitToDark * 16f, 8f, Utils.GetLerpValue(0f, 1f, progressOnStrip, clamped: true)) * lerpValue;
        result *= 0.5f;
        return result;
    }

    public override bool PreDraw(ref Color lightColor) {
        Color baseColor = Color.White;

        float value = BaseFormDataStorage.GetAttackCharge(Projectile.GetOwnerAsPlayer());

        float offset = 5f;
        Vector2 position = Projectile.Center + MathF.Sin(TimeSystem.TimeForVisualEffects * 5f + Projectile.whoAmI * 10f).ToRotationVector2() * offset - new Vector2(offset, -offset) / 2f;
        Vector2 temp = Projectile.Center;
        Projectile.Center = position;

        MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
        miscShaderData.UseSaturation(-0.5f);
        miscShaderData.UseOpacity(10f);
        miscShaderData.UseOpacity(3f);
        miscShaderData.Apply();
        _vertexStrip.PrepareStripWithProceduralPadding(Projectile.oldPos, Projectile.oldRot, 
            StripColors, StripWidth,
            -Main.screenPosition + Projectile.Size / 2f, includeBacksides: true);
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
