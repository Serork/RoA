using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Common.VisualEffects;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Content.VisualEffects;

sealed class HardmodeClawsSlashHit : VisualEffect<HardmodeClawsSlashHit> {
    private float _baseScale;

    protected override void SetDefaults() {
        TimeLeft = 35;

        _baseScale = Scale * 0.85f;

        DontEmitLight = true;
    }

    public override void Update(ref ParticleRendererSettings settings) {
        //float t = (float)((MaxTimeLeft - TimeLeft) / (double)MaxTimeLeft * 60.0);
        //float scale = Utils.GetLerpValue(0f, MaxTimeLeft / 3f, t, true) * Utils.GetLerpValue((float)(MaxTimeLeft - MaxTimeLeft / 5f), MaxTimeLeft / 2f, t, true) * t3;
        //Scale = scale * _baseScale;

        if (Main.rand.NextChance(0.4) && TimeLeft > 15 && TimeLeft < MaxTimeLeft - 5) {
            int type = ModContent.DustType<Slash>();
            Dust dust = Dust.NewDustPerfect(Position + Main.rand.NextVector2Circular(20f, 10f), type, new Vector2?((Main.rand.NextFloat() * MathHelper.TwoPi).ToRotationVector2() * Main.rand.NextFloat(2.5f)), 0, DrawColor * 1.25f, Main.rand.NextFloat(0.75f, 0.9f) * 1.5f);
            //dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15) / 2f;
            dust.noLight = dust.noLightEmittence = true;
            if (ShouldFullBright) {
                dust.customData = BrightnessModifier;
            }
            dust.noGravity = true;
        }

        float t = (float)((MaxTimeLeft - TimeLeft) / (double)MaxTimeLeft * 60.0);
        float scale = Utils.GetLerpValue(0f, MaxTimeLeft / 2f, t, true) * Utils.GetLerpValue(MaxTimeLeft, MaxTimeLeft - MaxTimeLeft / 2f, t, true) * 1f;
        Rotation = Utils.AngleLerp(Rotation, -0.07f + Main.rand.NextFloatRange(0.1f), 0.5f) * scale;

        if (--TimeLeft <= 0) {
            RestInPool();
        }

        if (!DontEmitLight) {
            Lighting.AddLight(Position, DrawColor.ToVector3() * 0.5f * scale);
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spriteBatch) {
        Color color = DrawColor * 1f;
        Vector2 origin = Texture.Size() / 2f;
        Vector2 position = Position - Main.screenPosition;
        SpriteEffects effects = SpriteEffects.None;

        color *= MathHelper.Lerp(0.75f, 0.875f, 0.5f);

        float t = (float)((MaxTimeLeft - TimeLeft) / (double)MaxTimeLeft * 60.0);
        float t2 = Utils.GetLerpValue(0f, MaxTimeLeft / 2f, t, true) * Utils.GetLerpValue(MaxTimeLeft, MaxTimeLeft - MaxTimeLeft / 2f, t, true) * 0.75f;
        float scale = MathF.Max(0.25f, t2);
        color *= Utils.GetLerpValue(0f, MaxTimeLeft / 4f, t, true) * Utils.GetLerpValue(MaxTimeLeft, MaxTimeLeft - MaxTimeLeft / 4f, t, true) * 0.9f;

        Texture2D sparkle = ResourceManager.DefaultSparkle;

        spriteBatch.Draw(sparkle, position, null, color * 1f, MathHelper.PiOver2 + Rotation, origin, Scale * scale, effects, 0f);
        spriteBatch.Draw(sparkle, position, null, color * 1f, Rotation, origin, Scale * scale, effects, 0f);
        spriteBatch.Draw(sparkle, position, null, color * 1f, MathHelper.PiOver2 + Rotation, origin, Scale * 0.6f * scale, effects, 0f);
        spriteBatch.Draw(sparkle, position, null, color * 1f, Rotation, origin, Scale * 0.6f * scale, effects, 0f);

        SpriteBatchSnapshot snapshot = spriteBatch.CaptureSnapshot();
        spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
        Color shineColor = new Color(255, 200, 150) * 0.9f;
        Color color3 = color * Scale * scale * 0.5f;
        color3.A = (byte)(color3.A * (1.0 - (double)Scale * scale));
        Color color4 = color3 * Scale * scale * 0.5f;
        color4.G = (byte)(color4.G * (double)Scale * scale);
        color4.B = (byte)(color4.R * (0.25 + (double)Scale * scale * 0.75));

        float t3 = 1.5f;

        spriteBatch.Draw(Texture, position, null, color4, MathHelper.PiOver2 + Rotation, origin, new Vector2(Scale * scale * t3, Scale * 0.75f), effects, 0f);
        spriteBatch.Draw(Texture, position, null, color3, Rotation, origin, new Vector2(Scale, Scale * scale), effects, 0f);
        spriteBatch.Draw(Texture, position, null, color3, MathHelper.PiOver2 + Rotation, origin, new Vector2(Scale * scale * t3, Scale * 0.75f) * 0.6f, effects, 0f);
        spriteBatch.Draw(Texture, position, null, color4, Rotation, origin, new Vector2(Scale, Scale * scale) * 0.6f, effects, 0f);

        spriteBatch.Draw(Texture, position, null, color * 1f, MathHelper.PiOver2 + Rotation, origin, new Vector2(Scale * scale * t3, Scale), effects, 0f);
        spriteBatch.Draw(Texture, position, null, color * 1f, Rotation, origin, new Vector2(Scale, Scale * scale), effects, 0f);
        spriteBatch.Draw(Texture, position, null, color * 1f, MathHelper.PiOver2 + Rotation, origin, new Vector2(Scale * scale * t3, Scale) * 0.6f, effects, 0f);
        spriteBatch.Draw(Texture, position, null, color * 1f, Rotation, origin, new Vector2(Scale, Scale * scale) * 0.6f, effects, 0f);

        spriteBatch.Draw(sparkle, position, null, color4 * 0.5f, MathHelper.PiOver2 + Rotation, origin, new Vector2(Scale * scale * t3, Scale), effects, 0f);
        spriteBatch.Draw(sparkle, position, null, color3 * 0.5f, Rotation, origin, new Vector2(Scale, Scale * scale), effects, 0f);
        spriteBatch.Draw(sparkle, position, null, color3 * 0.5f, MathHelper.PiOver2 + Rotation, origin, new Vector2(Scale * scale * t3, Scale) * 0.6f, effects, 0f);
        spriteBatch.Draw(sparkle, position, null, color4 * 0.5f, Rotation, origin, new Vector2(Scale, Scale * scale) * 0.6f, effects, 0f);
        spriteBatch.Draw(sparkle, position, null, color * 0.5f, MathHelper.PiOver2 + Rotation, origin, new Vector2(Scale * scale * t3, Scale), effects, 0f);
        spriteBatch.Draw(sparkle, position, null, color * 0.5f, Rotation, origin, new Vector2(Scale, Scale * scale), effects, 0f);
        spriteBatch.Draw(sparkle, position, null, color * 0.5f, MathHelper.PiOver2 + Rotation, origin, new Vector2(Scale * scale * t3, Scale) * 0.6f, effects, 0f);
        spriteBatch.Draw(sparkle, position, null, color * 0.5f, Rotation, origin, new Vector2(Scale, Scale * scale) * 0.6f, effects, 0f);
        //Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.TexturesPerType + "Light").Value;
        //spriteBatch.DrawSelf(texture, position, null, color, Rotation, texture.Size() / 2f, Scale * 0.6f, effects, 0f);
        spriteBatch.Begin(snapshot, true);
    }
}

