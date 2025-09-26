using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;

using System;

using Terraria;

namespace RoA.Content.Forms;

sealed class HallowedGryphon : BaseForm {
    protected override Color GlowColor(Player player, Color drawColor, float progress) => WreathHandler.GetArmorGlowColor1(player, drawColor, progress);

    public override ushort HitboxWidth => (ushort)(Player.defaultWidth * 2.5f);
    public override ushort HitboxHeight => (ushort)(Player.defaultHeight * 1f);

    public override Vector2 WreathOffset => new(0f, 0f);
    public override Vector2 WreathOffset2 => new(0f, 0f);

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
        if (player.controlJump) {
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
        skipDust = true;
    }

    protected override void SafeDismountMount(Player player, ref bool skipDust) {
        skipDust = true;
    }
}
