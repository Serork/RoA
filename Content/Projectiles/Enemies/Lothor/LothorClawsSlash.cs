﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using RoA.Content.Dusts;
using System;

using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using RoA.Core.Utility;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class LothorClawsSlash : ModProjectile {
    public override bool? CanCutTiles()
        => Utils.PlotTileLine(Projectile.Center + (Projectile.rotation - 0.7853982f).ToRotationVector2() * 55f * Projectile.scale, Projectile.Center + (Projectile.rotation + 0.7853982f).ToRotationVector2() * 55f * Projectile.scale, 55f * Projectile.scale, new Utils.TileActionAttempt(DelegateMethods.CutTiles));

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Lothor's Claws");
    }

    public override void SetDefaults() {
        Projectile.width = Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 200;
    }

    public override void AI() {
        NPC npc = Main.npc[(int)Projectile.ai[0]];

        if (!npc.active) {
            return;
        }

        if (Projectile.localAI[1] == 0f) {
            Projectile.localAI[1] = 1f;

            float dist = Vector2.Distance(npc.Center, Main.player[npc.target].Center);
            float value = Utils.Remap(dist, 0f, 300f, 1f, 2.5f);
            Projectile.localAI[2] = Math.Max(value, 1f);
        }

        Projectile.localAI[0] += 1f;

        Projectile.ai[2] = npc.direction;

        float fromValue = Projectile.localAI[0] / Projectile.ai[1];

        Projectile.Opacity = Utils.GetLerpValue(Projectile.ai[1] / 2f, Projectile.ai[1] * 0.8f, Projectile.localAI[0], true);

        float num1 = Projectile.ai[2];
        float num2 = 0.2f;
        float num3 = Projectile.localAI[2];

        Projectile.Center = npc.Center - new Vector2(-npc.width / 3f * npc.direction, npc.height / 4f) - Projectile.velocity;

        Projectile.velocity = new Vector2(npc.direction, 0f);

        float rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver4 / 2f * Math.Sign(Projectile.velocity.X);
        Projectile.rotation = (float)(MathHelper.Pi * (double)num1 * (double)fromValue + (double)rotation + (double)num1 * MathHelper.Pi) + npc.rotation;

        Projectile.scale = num3 + fromValue * num2;

        Point pos = Projectile.Center.ToTileCoordinates();
        Color color2 = new Color(150, 20, 20);
        Color color1 = new Color(241, 100, 100);
        float num12 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
        float num22 = Utils.Remap(num12, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num12, 0.6f, 1f, 1f, 0.0f);
        float num42 = 1f/*Utils.Remap((Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * 1.5f).ToVector3().Length() / (float)Math.Sqrt(3.0), 0.6f, 1f, 0.4f, 1f)*/ * Projectile.Opacity;
        color1 *= num42 * num22;
        color2 *= num42 * num22;

        float offset = 0f;
        float f = Projectile.rotation + (float)((double)Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7);
        Vector2 rotationVector2 = (f + Projectile.ai[0] * 1.25f * MathHelper.PiOver2).ToRotationVector2();
        if (Projectile.localAI[0] >= Projectile.ai[1] * 0.7f && Projectile.localAI[0] < Projectile.ai[1] + Projectile.ai[1] * 0.2f) {
            Vector2 position = Projectile.Center + (f - offset).ToRotationVector2() * (float)((double)Main.rand.NextFloat() * 80.0 * Projectile.scale + 20.0 * Projectile.scale);
            if (position.Distance(npc.Center) > 45f) {
                int type = ModContent.DustType<Slash>();
                Dust dust = Dust.NewDustPerfect(position, type, new Vector2?(rotationVector2), 0, Color.Lerp(color1, color2, Main.rand.NextFloat() * 0.3f) * 2f, Main.rand.NextFloat(0.75f, 0.9f) * 1.3f);
                dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
                dust.noLight = dust.noLightEmittence = true;
                dust.noGravity = true;
            }
        }
        if (Projectile.localAI[0] < (double)(Projectile.ai[1] + Projectile.ai[1] * 0.3f)) {
            return;
        }
        Projectile.Kill();
    }

    public override bool? CanDamage()
        => CanFunction();

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float coneLength = 90f * Projectile.scale;
        float num1 = 0.5105088f * Projectile.localAI[0];
        float maximumAngle = 0.3926991f;
        float coneRotation = Projectile.rotation + num1;
        return targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation, maximumAngle);
    }

    private bool CanFunction() {
        bool result = false;
        NPC npc = Main.npc[(int)Projectile.ai[0]];
        if (npc.ai[0] > npc.ai[1] * 0.335f) {
            result = true;
        }
        return result;
    }

    private void DrawItself(ref Color lightColor, float? rotation = null) {
        float rot = rotation ?? Projectile.rotation;
        lightColor *= 2f;
        lightColor.A = 100;
        Vector2 position = Projectile.Center - Main.screenPosition;
        Asset<Texture2D> asset = ModContent.Request<Texture2D>(Texture);
        Rectangle r = asset.Frame(verticalFrames: 2);
        Vector2 origin = r.Size() / 2f;
        float scale = Projectile.scale * 1.1f;
        SpriteEffects effects = Projectile.ai[2] >= 0.0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
        //SpriteEffects effects = Projectile.ai[0] >= 0.0 ? ((player.gravDir == 1f) ? SpriteEffects.None : SpriteEffects.FlipVertically) : ((player.gravDir == 1f) ? SpriteEffects.FlipVertically : SpriteEffects.None);
        float num1 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
        float num2 = Utils.Remap(num1, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num1, 0.6f, 1f, 1f, 0.0f);
        float num3 = 0.975f;
        Point pos = Projectile.Center.ToTileCoordinates();
        float num4 = 1f /* Utils.Remap((Lighting.GetColor(pos) * 1.5f).ToVector3().Length() / (float)Math.Sqrt(3.0), 0.6f, 1f, 0.4f, 1f)*/ * Projectile.Opacity;
        Color color2 = new Color(150, 20, 20);
        Color color1 = new Color(241, 100, 100);
        float num12 = MathHelper.Clamp(Projectile.timeLeft / 2, 0f, 5f);
        if (CanFunction()) {
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.BeginBlendState(BlendState.NonPremultiplied);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color1 * num4 * num2 * 0.35f, Projectile.rotation + (float)(Projectile.ai[2] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            //spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color1 * num4 * num2, Projectile.rotation + (float)(Projectile.ai[0] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);
            Color shineColor = new Color(255, 200, 150);
            Color color3 = lightColor * num2 * 0.5f;
            color3.A = (byte)(color3.A * (1.0 - (double)num4));
            Color color4 = color3 * num4 * 0.5f;
            color4.G = (byte)(color4.G * (double)num4);
            color4.B = (byte)(color4.R * (0.25 + (double)num4 * 0.75));
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color4 * 0.15f, Projectile.rotation + Projectile.ai[2] * 0.01f, origin, scale, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), shineColor * num4 * num2 * 0.3f, Projectile.rotation, origin, scale, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color2 * num4 * num2 * 0.5f, Projectile.rotation, origin, scale * num3, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), lightColor * 0.6f * num2, Projectile.rotation + Projectile.ai[2] * 0.01f, origin, scale, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), lightColor * 0.5f * num2, Projectile.rotation + Projectile.ai[2] * -0.05f, origin, scale * 0.8f, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), lightColor * 0.4f * num2, Projectile.rotation + Projectile.ai[2] * -0.1f, origin, scale * 0.6f, effects, 0.0f);
            spriteBatch.BeginBlendState(BlendState.Additive);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color4 * 0.15f, Projectile.rotation + Projectile.ai[2] * 0.01f, origin, scale, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color2 * num4 * num2 * 0.5f, Projectile.rotation, origin, scale * num3, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color1 * num4 * num2, Projectile.rotation + (float)(Projectile.ai[2] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);
            for (int i = 0; i < 5; i++) {
                spriteBatch.Draw(asset.Value, position + Utils.ToRotationVector2((float)(Projectile.timeLeft * 0.1 + i * Math.PI / 5.0)) * num12, new Rectangle?(r), color1 * num4 * num2 * 0.25f, Projectile.rotation + (float)(Projectile.ai[2] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);
            }
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color4 * 0.15f * 0.15f, Projectile.rotation + Projectile.ai[2] * 0.01f, origin, scale, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color2 * 0.15f * num4 * num2 * 0.5f, Projectile.rotation, origin, scale * num3, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color1 * 0.15f * num4 * num2, Projectile.rotation + (float)(Projectile.ai[2] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);
            spriteBatch.EndBlendState();
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        DrawItself(ref lightColor);
        return false;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
        => target.AddBuff(BuffID.Bleeding, Main.expertMode ? 180 : 120);
}
