﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;
using RoA.Content.Dusts;
using RoA.Core.Utility;

using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Content.VisualEffects;

sealed class ClawsSlashHit : VisualEffect<ClawsSlashHit> {
    protected override void SetDefaults() {
        TimeLeft = MaxTimeLeft = 30;
    }

    public override void Update(ref ParticleRendererSettings settings) {
        float t = (float)((MaxTimeLeft - TimeLeft) / (double)MaxTimeLeft * 60.0);
        float scale = Utils.GetLerpValue(0f, MaxTimeLeft / 3f, t, true) * Utils.GetLerpValue((float)(MaxTimeLeft - MaxTimeLeft / 5f), MaxTimeLeft / 2f, t, true);
        Scale = scale;

        if (Main.rand.NextChance(0.1) && TimeLeft > 5 && TimeLeft < MaxTimeLeft - 5) {
            int type = ModContent.DustType<Slash>();
            Dust dust = Dust.NewDustPerfect(Position, type, new Vector2?((Main.rand.NextFloat() * MathHelper.TwoPi).ToRotationVector2() * Main.rand.NextFloat(2.5f)), 0, DrawColor * 1.25f, Main.rand.NextFloat(0.75f, 0.9f) * 1.2f);
            dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
            dust.noLight = dust.noLightEmittence = true;
            dust.noGravity = true;
        }

        Rotation += Main.rand.Next(10) * Main.rand.NextFloat(0.05f);

        if (--TimeLeft <= 0) {
            RestInPool();
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spriteBatch) {
        Color color = DrawColor;
        Vector2 origin = Texture.Size() / 2f;
        Vector2 position = Position - Main.screenPosition;
        SpriteEffects effects = SpriteEffects.None;
        spriteBatch.Draw(Texture, position, null, color, MathHelper.PiOver2 + Rotation, origin, Scale, effects, 0f);
        spriteBatch.Draw(Texture, position, null, color, Rotation, origin, Scale, effects, 0f);
        spriteBatch.Draw(Texture, position, null, color, MathHelper.PiOver2 + Rotation, origin, Scale * 0.6f, effects, 0f);
        spriteBatch.Draw(Texture, position, null, color, Rotation, origin, Scale * 0.6f, effects, 0f);
        spriteBatch.BeginBlendState(BlendState.Additive);
        Color shineColor = new(255, 200, 150);
        Color color3 = color * Scale * 0.5f;
        color3.A = (byte)(color3.A * (1.0 - (double)Scale));
        Color color4 = color3 * Scale * 0.5f;
        color4.G = (byte)(color4.G * (double)Scale);
        color4.B = (byte)(color4.R * (0.25 + (double)Scale * 0.75));
        spriteBatch.Draw(Texture, position, null, color4, MathHelper.PiOver2 + Rotation, origin, Scale, effects, 0f);
        spriteBatch.Draw(Texture, position, null, color3, Rotation, origin, Scale, effects, 0f);
        spriteBatch.Draw(Texture, position, null, color3, MathHelper.PiOver2 + Rotation, origin, Scale * 0.6f, effects, 0f);
        spriteBatch.Draw(Texture, position, null, color4, Rotation, origin, Scale * 0.6f, effects, 0f);
        spriteBatch.Draw(Texture, position, null, color * 0.2f, MathHelper.PiOver2 + Rotation, origin, Scale, effects, 0f);
        spriteBatch.Draw(Texture, position, null, color * 0.2f, Rotation, origin, Scale, effects, 0f);
        spriteBatch.Draw(Texture, position, null, color * 0.2f, MathHelper.PiOver2 + Rotation, origin, Scale * 0.6f, effects, 0f);
        spriteBatch.Draw(Texture, position, null, color * 0.2f, Rotation, origin, Scale * 0.6f, effects, 0f);
        //Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.Textures + "Light").Value;
        //spriteBatch.Draw(texture, position, null, color, Rotation, texture.Size() / 2f, Scale * 0.6f, effects, 0f);
        spriteBatch.EndBlendState();
    }
}
