using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Forms;

sealed class FlederForm : BaseForm {
    private static bool IsFlying(Player player) {
        bool onTile = Math.Abs(player.velocity.Y) < 1.25f && WorldGenHelper.SolidTile((int)player.Center.X / 16, (int)(player.Bottom.Y + 10f) / 16);
        return !player.sliding && !onTile && player.gfxOffY == 0f;
    }

    protected override float GetMaxSpeedMultiplier(Player player) => 1f;
    protected override float GetRunAccelerationMultiplier(Player player) => 1.5f;

    protected override void SafeSetDefaults() {
        MountData.totalFrames = 8;
        MountData.spawnDust = 59;
        MountData.heightBoost = -14;
        MountData.fallDamage = 0f;
        MountData.flightTimeMax = 60;
        MountData.fatigueMax = 40;
        MountData.jumpHeight = 1;
        MountData.jumpSpeed = 4f;

        if (!Main.dedServ) {
            MountData.textureWidth = MountData.backTexture.Width();
            MountData.textureHeight = MountData.backTexture.Height();
        }
    }

    protected override void SafeUpdateEffects(Player player) {
        float rotation = player.velocity.X * (IsFlying(player) ? 0.2f : 0.15f);
        float fullRotation = (float)Math.PI / 4f * rotation / 2f;
        float maxRotation = 0.3f;
        fullRotation = MathHelper.Clamp(fullRotation, -maxRotation, maxRotation);
        bool flag = IsFlying(player);
        if (flag) {
            float maxFlightSpeedX = 3f;
            if (player.velocity.X > maxFlightSpeedX) {
                player.velocity.X -= player.runAcceleration;
            }
            if (player.velocity.X < -maxFlightSpeedX) {
                player.velocity.X += player.runAcceleration;
            }
        }
        player.fullRotation = fullRotation;
        player.gravity *= 0.75f;
        player.velocity.Y = Math.Min(5f, player.velocity.Y);
        player.fullRotationOrigin = new Vector2(player.width / 2 + 4f * player.direction, player.height / 2 - 6f);
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        int minFrame = 4, maxFrame = 7;
        float flightFrameFrequency = 14f, walkingFrameFrequiency = 16f;
        if (IsFlying(player)) {
            if (player.velocity.Y < 0f) {
                frameCounter += Math.Abs(player.velocity.Y) * 0.5f;
                float frequency = flightFrameFrequency;
                while (frameCounter > frequency) {
                    frameCounter -= frequency;
                    frame++;
                }
                if (frame < minFrame || frame > maxFrame) {
                    frame = minFrame;
                }
            }
            else if (player.velocity.Y > 0f) {
                frame = maxFrame;
            }
        }
        else if (player.velocity.X != 0f) {
            frameCounter += Math.Abs(player.velocity.X) * 1.5f;
            float frequency = walkingFrameFrequiency;
            while (frameCounter > frequency) {
                frameCounter -= frequency;
                frame++;
            }
            if (frame > 3) {
                frame = 0;
            }
        }
        else {
            frameCounter = 0f;
            frame = 0;
        }

        return false;
    }

    protected override void SafeSetMount(Player player, ref bool skipDust) {
        for (int i = 0; i < 24; i++) {
            Vector2 spawnPos = player.Center + new Vector2(30f, 0).RotatedBy(i * Math.PI * 2 / 24f) - new Vector2(-6f, 4f);
            Vector2 direction = (player.Center - spawnPos) * 0.5f;
            int dust = Dust.NewDust(spawnPos, 0, 0, MountData.spawnDust, direction.X, direction.Y);
            Main.dust[dust].velocity *= 0.95f;
            Main.dust[dust].fadeIn = 1.8f;
            Main.dust[dust].noGravity = true;
        }
        skipDust = true;
    }

    public override void Dismount(Player player, ref bool skipDust) {
        for (int i = 0; i < 24; i++) {
            Vector2 spawnPos = player.Center - new Vector2(30f, 0).RotatedBy(i * Math.PI * 2 / 24f) - new Vector2(-6f, 4f);
            Vector2 direction = (player.Center - spawnPos) * 0.5f;
            int dust = Dust.NewDust(spawnPos, 0, 0, MountData.spawnDust, direction.X, direction.Y);
            Main.dust[dust].velocity *= 0.95f;
            Main.dust[dust].fadeIn = 1.8f;
            Main.dust[dust].noGravity = true;
        }
        skipDust = true;
    }
}