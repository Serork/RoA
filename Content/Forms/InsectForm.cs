using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Utilities;

using System;

using Terraria;

namespace RoA.Content.Forms;

abstract class InsectForm : BaseForm {
    protected virtual float InsectDustScale { get; } = 1f;

    protected sealed override void SafeSetDefaults() {
        MountData.heightBoost = -16;
        MountData.fallDamage = 0;
        MountData.runSpeed = 5f;
        MountData.dashSpeed = 5f;
        MountData.flightTimeMax = 100;
        MountData.jumpHeight = 50;
        MountData.acceleration = 0.2f;
        MountData.jumpSpeed = 4f;
        MountData.totalFrames = 6;
        MountData.yOffset = 2;

        SafeSetDefaults2();
    }

    protected virtual void SafeSetDefaults2() { }

    protected override void SafeUpdateEffects(Player player) {
        float rotation = player.velocity.X * (IsFlying(player) ? 0.2f : 0.15f);
        float fullRotation = (float)Math.PI / 4f * rotation / 2f;
        float maxRotation = 0.2f;
        fullRotation = MathHelper.Clamp(fullRotation, -maxRotation, maxRotation);
        player.fullRotation = fullRotation;
        if (!IsFlying(player)) {
            player.velocity.X *= 0.9f;
        }
        player.velocity.Y = Math.Min(5f, player.velocity.Y);
        player.fullRotationOrigin = new Vector2(player.width / 2 + 4f * player.direction, player.height / 2 - 10f);
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        int maxFrame = 5;
        float flightFrameFrequency = 4f, walkingFrameFrequiency = 20f;
        if (IsFlying(player)) {
            frameCounter += Math.Abs(player.velocity.Y) * (player.velocity.Y < 0f ? 0.5f : 0.25f);
            while (frameCounter > flightFrameFrequency) {
                frameCounter -= flightFrameFrequency;
                frame++;
            }
            if (frame > maxFrame) {
                frame = 0;
            }
        }
        else if (player.velocity.X != 0f) {
            frameCounter += Math.Abs(player.velocity.X) * 1.5f;
            while (frameCounter > walkingFrameFrequiency) {
                frameCounter -= walkingFrameFrequiency;
                frame++;
            }
            if (frame > maxFrame) {
                frame = 0;
            }
        }
        else {
            frameCounter = 0f;
            frame = 0;
        }

        return false;
    }

    protected sealed override void SafeSetMount(Player player, ref bool skipDust) {
        for (int i = 0; i < 16; i++) {
            Vector2 position = player.Center + new Vector2(0, -6) + new Vector2(30, 0).RotatedBy(i * Math.PI * 2 / 16f) - new Vector2(8f, 4f);
            int dust = Dust.NewDust(position, 0, 0, MountData.spawnDust, 0, 0, 0, default(Color), InsectDustScale);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].fadeIn = 1.25f;
            Main.dust[dust].velocity += Helper.VelocityToPoint(position, player.Center, 2f);
        }
        skipDust = true;
    }

    public sealed override void Dismount(Player player, ref bool skipDust) {
        for (int i = 0; i < 28; i++) {
            Vector2 position = player.Center + new Vector2(2, -6) + new Vector2(20, 0).RotatedBy(i * Math.PI * 2 / 16f) - new Vector2(0f, -12f);
            Vector2 direction = (player.Center - position) * 0.8f;
            int dust = Dust.NewDust(position, 0, 0, MountData.spawnDust, direction.X, direction.Y, 0, default(Color), InsectDustScale * 2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.4f;
        }
        skipDust = true;
    }
}