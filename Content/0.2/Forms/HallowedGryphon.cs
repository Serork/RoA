using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.Forms;

sealed class HallowedGryphon : BaseForm {
    protected override Color GlowColor(Player player, Color drawColor, float progress) => WreathHandler.GetArmorGlowColor1(player, drawColor, progress);

    public override ushort SetHitboxWidth(Player player) => (ushort)(Player.defaultWidth * 2.5f);
    public override ushort SetHitboxHeight(Player player) => (ushort)(Player.defaultHeight * 1f);

    public override Vector2 SetWreathOffset(Player player) => new(0f, 0f);
    public override Vector2 SetWreathOffset2(Player player) => new(0f, 0f);

    protected override void SafeSetDefaults() {
        MountData.totalFrames = 11;
        MountData.fallDamage = 0f;
        MountData.flightTimeMax = 125;
       
        MountData.yOffset = -3;
        MountData.playerHeadOffset = -14;
    }

    protected override void SafePostUpdate(Player player) {
        player.GetModPlayer<BaseFormHandler>().UsePlayerSpeed = true;
        bool flag = player.mount.FlyTime > 0;
        if (IsInAir(player)) {
            bool flag2 = !flag && player.controlJump;
            player.accRunSpeed *= 3f;
            player.runAcceleration *= 1.5f;
            if (flag2) {
                player.accRunSpeed *= 0.9f;
                player.accRunSpeed *= 0.9f;
            }
        }
        else {
            player.runAcceleration *= 1.25f;
            player.maxRunSpeed *= 1.25f;
            player.mount.ResetFlightTime(player.velocity.X);
            player.wingTime = player.wingTimeMax;
        }

        if (player.controlJump && player.velocity.Y < 0f && flag) {
            ref Vector2 velocity = ref player.velocity;
            ref float jumpSpeed = ref Player.jumpSpeed;
            float num2 = 0.85f;
            float num5 = 0.1f;
            float num4 = 1f;
            if (velocity.Y > 0f) {
                velocity.Y -= num2;
            }
            else if (velocity.Y > (0f - jumpSpeed) * num4) {
                velocity.Y -= num5;
            }
        }

        Player.jumpHeight = 1;
        Player.jumpSpeed = 4.5f;
        player.gravity *= 0.75f;
        if (player.controlJump && !player.controlDown) {
            player.velocity.Y = Math.Min(5f, player.velocity.Y);
        }
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        float walkingFrameFrequiency = 14f;
        int minMovingFrame = 1;
        int maxMovingFrame = minMovingFrame + 5;
        if (IsInAir(player)) {
            int minFlyingFrame = maxMovingFrame + 1, maxFlyingFrame = minFlyingFrame + 3;
            float flightFrameFrequency = 14f;
            float speedY = Math.Abs(player.velocity.Y);
            frameCounter += Utils.Clamp(speedY, 3f, 5f) * (player.controlJump && player.velocity.Y > 0f ? 1f : 0.5f);
            float frequency = flightFrameFrequency;
            while (frameCounter > frequency) {
                frameCounter -= frequency;
                frame++;
            }
            if (frame < minFlyingFrame || frame > maxFlyingFrame) {
                frame = minFlyingFrame;
            }
            if (player.velocity.Y > 2.5f) {
                if (player.controlJump) {
                    frame = maxFlyingFrame - 1;
                }
                else {
                    frame = maxFlyingFrame - 2;
                }
            }
        }
        else if (player.velocity.X != 0f) {
            if (frame >= maxMovingFrame) {
                frame = minMovingFrame;
            }
            frameCounter += Math.Abs(player.velocity.X) * 0.9f;
            if (frameCounter >= walkingFrameFrequiency) {
                if (frame < maxMovingFrame) {
                    frame++;
                }
                else {
                    frame = minMovingFrame;
                }
                frameCounter = 0f;
            }
        }
        else {
            frame = 0;
        }

        return false;
    }

    protected override void SafeSetMount(Player player, ref bool skipDust) {
        for (int i = 0; i < 20; i++) {
            Vector2 spawnPos = player.Center + new Vector2(40, 0).RotatedBy(i * Math.PI * 2 / 24f);
            Vector2 direction = (player.Center - spawnPos) * 0.5f;
            int dust = Dust.NewDust(spawnPos, 0, 0, DustID.TintableDustLighted, direction.X, direction.Y, newColor: Color.Yellow);
            Main.dust[dust].velocity *= 0.2f + Main.rand.NextFloatRange(0.15f);
            Main.dust[dust].velocity = new Vector2(-Main.dust[dust].velocity.Y, Main.dust[dust].velocity.X * 2f);
            Main.dust[dust].velocity.Y *= Main.rand.NextFloat(0.15f, 0.5f);
            Main.dust[dust].fadeIn = 1.8f - Main.rand.NextFloat(0.4f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].scale *= 0.75f + Main.rand.NextFloat(0.25f);
        }
        skipDust = true;
    }

    protected override void SafeDismountMount(Player player, ref bool skipDust) {
        for (int i = 0; i < 20; i++) {
            Vector2 spawnPos = player.Center + new Vector2(20, 0).RotatedBy(i * Math.PI * 2 / 24f) - new Vector2(4f, 0f);
            Vector2 direction = (player.Center - spawnPos) * 0.5f;
            int dust = Dust.NewDust(spawnPos, 0, 0, DustID.TintableDustLighted, direction.X, direction.Y, newColor: Color.Yellow);
            Main.dust[dust].velocity *= 0.3f + Main.rand.NextFloatRange(0.15f);
            Main.dust[dust].velocity = new Vector2(-Main.dust[dust].velocity.Y, Main.dust[dust].velocity.X * 2f) * -1f;
            Main.dust[dust].velocity.Y *= Main.rand.NextFloat(0.15f, 0.5f);
            Main.dust[dust].fadeIn = 1.8f - Main.rand.NextFloat(0.4f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].scale *= 0.75f + Main.rand.NextFloat(0.25f);
        }
        skipDust = true;
    }
}
