﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using ReLogic.Content;

using System;

using Terraria;
using Terraria.ModLoader;
using Terraria.Enums;

using RoA.Content.Items.Weapons.Druidic.Claws;
using RoA.Core.Utility;
using RoA.Content.Dusts;
using RoA.Content.VisualEffects;
using RoA.Common.VisualEffects;

using Terraria.ID;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Druid.Claws;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class ClawsSlash : NatureProjectile {
    private Player Owner => Main.player[Projectile.owner];

    private (Color, Color) SlashColors => Owner.GetModPlayer<ClawsHandler>().SlashColors;
    private Color FirstSlashColor => SlashColors.Item1;
    private Color SecondSlashColor => SlashColors.Item2;

    private bool CanFunction => Projectile.localAI[0] >= Projectile.ai[1] * 0.5f;

    protected override void SafeSetDefaults() {
        Projectile.width = Projectile.height = 0;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.ownerHitCheck = true;
        Projectile.ownerHitCheckDistance = 300f;
        Projectile.usesOwnerMeleeHitCD = true;
        Projectile.stopsDealingDamageAfterPenetrateHits = true;
        Projectile.noEnchantmentVisuals = true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => Projectile.ApplyFlaskEffects(target);

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        float angle = MathHelper.PiOver2;
        Vector2 offset = new(0.2f);
        Vector2 velocity = 1.5f * offset;
        Vector2 position = Main.rand.NextVector2Circular(4f, 4f) * offset;
        Color color = Color.Lerp(FirstSlashColor, SecondSlashColor, Main.rand.NextFloat())/*Lighting.GetColor(target.Center.ToTileCoordinates()).MultiplyRGB(Color.Lerp(FirstSlashColor, SecondSlashColor, Main.rand.NextFloat()))*/;
        color.A = 50;
        position = target.Center + target.velocity + position + Main.rand.NextVector2Circular(target.width / 3f, target.height / 3f);
        velocity = angle.ToRotationVector2() * velocity * 0.5f;
        int layer = VisualEffectLayer.ABOVENPCS;
        VisualEffectSystem.New<ClawsSlashHit>(layer).
            Setup(position,
                  velocity,
                  color);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new VisualEffectSpawnPacket(VisualEffectSpawnPacket.VisualEffectPacketType.ClawsHit, Owner, layer, position, velocity, color, 1f, 0f));
        }
    }

    public override bool? CanDamage() => CanFunction;

    public override void CutTiles() {
        DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
        Utils.PlotTileLine(Projectile.Center + (Projectile.rotation - 0.7853982f).ToRotationVector2() * 55f * Projectile.scale, Projectile.Center + (Projectile.rotation + 0.7853982f).ToRotationVector2() * 55f * Projectile.scale, 55f * Projectile.scale, DelegateMethods.CutTiles);
    }

    public override bool? CanCutTiles() => CanFunction;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        float coneLength = 80f * Projectile.scale;
        float num1 = 0.5105088f * Projectile.ai[0];
        float maximumAngle = 0.3926991f;
        float coneRotation = Projectile.rotation + num1;
        return targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center, coneLength, coneRotation, maximumAngle);
    }

    public override bool PreDraw(ref Color lightColor) {
        lightColor *= 2f;
        lightColor.A = 100;
        Vector2 position = Projectile.Center - Main.screenPosition;
        Asset<Texture2D> asset = ModContent.Request<Texture2D>(Texture);
        Rectangle r = asset.Frame(verticalFrames: 2);
        Vector2 origin = r.Size() / 2f;
        float scale = Projectile.scale * 1.1f;
        SpriteEffects effects = Projectile.ai[0] >= 0.0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
        //SpriteEffects effects = Projectile.ai[0] >= 0.0 ? ((player.gravDir == 1f) ? SpriteEffects.None : SpriteEffects.FlipVertically) : ((player.gravDir == 1f) ? SpriteEffects.FlipVertically : SpriteEffects.None);
        float num1 = (Projectile.localAI[0] + 0.5f) / (Projectile.ai[1] + Projectile.ai[1] * 0.5f);
        float num2 = Utils.Remap(num1, 0.0f, 0.6f, 0.0f, 1f) * Utils.Remap(num1, 0.6f, 1f, 1f, 0.0f);
        float num3 = 0.975f;
        float num4 = Utils.Remap((Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * 1.5f).ToVector3().Length() / (float)Math.Sqrt(3.0), 0.6f, 1f, 0.4f, 1f) * Projectile.Opacity;
        Color color1 = FirstSlashColor.MultiplyRGB(lightColor);
        Color color2 = SecondSlashColor.MultiplyRGB(lightColor);
        float num12 = MathHelper.Clamp(Projectile.timeLeft / 2, 0f, 5f);
        if (CanFunction) {
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color1 * num4 * num2, Projectile.rotation + (float)(Projectile.ai[0] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);
            Color shineColor = new(255, 200, 150);
            Color color3 = lightColor * num2 * 0.5f;
            //color3.A = (byte)(color3.A * (1.0 - (double)num4));
            Color color4 = color3 * num4 * 0.5f;
            color4.G = (byte)(color4.G * (double)num4);
            color4.B = (byte)(color4.R * (0.25 + (double)num4 * 0.75));
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color4 * 0.15f, Projectile.rotation + Projectile.ai[0] * 0.01f, origin, scale, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), shineColor * num4 * num2 * 0.3f, Projectile.rotation, origin, scale, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color2 * num4 * num2 * 0.5f, Projectile.rotation, origin, scale * num3, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), lightColor * 0.6f * num2, Projectile.rotation + Projectile.ai[0] * 0.01f, origin, scale, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), lightColor * 0.5f * num2, Projectile.rotation + Projectile.ai[0] * -0.05f, origin, scale * 0.8f, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(asset.Frame(verticalFrames: 2, frameY: 1)), lightColor * 0.4f * num2, Projectile.rotation + Projectile.ai[0] * -0.1f, origin, scale * 0.6f, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color4 * 0.15f, Projectile.rotation + Projectile.ai[0] * 0.01f, origin, scale, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color2 * num4 * num2 * 0.5f, Projectile.rotation, origin, scale * num3, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color1 * num4 * num2, Projectile.rotation + (float)(Projectile.ai[0] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);
            for (int i = 0; i < 5; i++) {
                spriteBatch.Draw(asset.Value, position + Utils.ToRotationVector2((float)(Projectile.timeLeft * 0.1 + i * Math.PI / 5.0)) * num12, new Rectangle?(r), color1 * num4 * num2 * 0.25f, Projectile.rotation + (float)(Projectile.ai[0] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);
            }
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color4 * 0.15f * 0.15f, Projectile.rotation + Projectile.ai[0] * 0.01f, origin, scale, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color2 * 0.15f * num4 * num2 * 0.5f, Projectile.rotation, origin, scale * num3, effects, 0.0f);
            spriteBatch.Draw(asset.Value, position, new Rectangle?(r), color1 * 0.15f * num4 * num2, Projectile.rotation + (float)(Projectile.ai[0] * 0.785398185253143 * -1.0 * (1.0 - (double)num1)), origin, scale, effects, 0.0f);
            spriteBatch.EndBlendState();
        }
        return false;
    }

    public override void AI() {
        Player player = Owner;

        if (player.inventory[player.selectedItem].ModItem is not BaseClawsItem) {
            Projectile.Kill();

            return;
        }

        Projectile.localAI[0] += 1f;
        float fromValue = Projectile.localAI[0] / Projectile.ai[1];
        float num1 = Projectile.ai[0];
        float num2 = 0.2f;
        float num3 = 1f;

        float rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver4 / 2f * Math.Sign(Projectile.velocity.X);
        Projectile.rotation = (float)(MathHelper.Pi * (double)num1 * (double)fromValue + (double)rotation + (double)num1 * MathHelper.Pi) + player.fullRotation;

        Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) - Projectile.velocity;
        Projectile.scale = num3 + fromValue * num2;

        float f = Projectile.rotation + (float)((double)Main.rand.NextFloatDirection() * MathHelper.PiOver2 * 0.7);
        float offset = player.gravDir == 1 ? 0f : (-MathHelper.PiOver4 * num1);
        Vector2 rotationVector2 = (f + Projectile.ai[0] * 1.25f * MathHelper.PiOver2).ToRotationVector2();
        if (Projectile.localAI[0] >= Projectile.ai[1] * 0.7f && Projectile.localAI[0] < Projectile.ai[1] + Projectile.ai[1] * 0.2f) {
            Vector2 position = Projectile.Center + (f - offset).ToRotationVector2() * (float)((double)Main.rand.NextFloat() * 80.0 * Projectile.scale + 20.0 * Projectile.scale);
            if (position.Distance(player.Center) > 45f) {
                int type = ModContent.DustType<Slash>();
                Dust dust = Dust.NewDustPerfect(position, type, new Vector2?(rotationVector2 * player.gravDir), 0, Color.Lerp(FirstSlashColor, SecondSlashColor, Main.rand.NextFloat() * 0.3f) * 2f, Main.rand.NextFloat(0.75f, 0.9f) * 1.3f);
                dust.fadeIn = (float)(0.4 + (double)Main.rand.NextFloat() * 0.15);
                dust.noLight = dust.noLightEmittence = true;
                dust.noGravity = true;
            }
        }
        Projectile.scale *= player.GetAdjustedItemScale(player.inventory[player.selectedItem]);
        if (CanFunction) {
            for (float i = -MathHelper.PiOver4; i <= MathHelper.PiOver4; i += MathHelper.PiOver2) {
                Rectangle rectangle = Utils.CenteredRectangle(Projectile.Center + (Projectile.rotation * player.gravDir + i).ToRotationVector2() * 60f * Projectile.scale, new Vector2(55f * Projectile.scale, 55f * Projectile.scale));
                Projectile.EmitEnchantmentVisualsAtForNonMelee(rectangle.TopLeft(), rectangle.Width, rectangle.Height);
            }
        }
        if (Projectile.localAI[0] < (double)(Projectile.ai[1] + Projectile.ai[1] * 0.3f)) {
            return;
        }
        Projectile.Kill();
    }
}
