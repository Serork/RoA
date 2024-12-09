using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Common.Players;
using RoA.Content.NPCs.Enemies.Backwoods;
using RoA.Content.Projectiles.Friendly.Druidic.Forms;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class LilPhoenixForm : BaseForm {
    protected override float GetMaxSpeedMultiplier(Player player) => 1f;
    protected override float GetRunAccelerationMultiplier(Player player) => 1.5f;

    protected override void SafeSetDefaults() {
        MountData.spawnDust = 6;
        MountData.spawnDustNoGravity = true;
        MountData.heightBoost = -18;
        MountData.fallDamage = 0;
        MountData.runSpeed = 2.5f;
        MountData.dashSpeed = 1f;
        MountData.flightTimeMax = 0;
        MountData.fatigueMax = 0;
        MountData.jumpHeight = 17;
        MountData.acceleration = 0.2f;
        MountData.jumpSpeed = 4f;
        MountData.blockExtraJumps = false;
        MountData.totalFrames = 12;
        MountData.constantJump = false;
        MountData.usesHover = false;
    }

    protected override void SafeUpdateEffects(Player player) {
        player.GetModPlayer<BaseFormHandler>().UsePlayerSpeed = true;
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        int maxFrame = 4;
        float walkingFrameFrequiency = 24f;
        if (player.velocity.Y != 0f) {
            frame = 5;
        }
        else if (player.velocity.Y == 0f) {
            if (player.velocity.X != 0f) {
                frameCounter += Math.Abs(player.velocity.X) * 1.5f;
                if (frameCounter >= walkingFrameFrequiency) {
                    int maxMovingFrame = maxFrame;
                    if (frame < maxMovingFrame) {
                        frame++;
                    }
                    else {
                        frame = 0;
                    }
                    frameCounter = 0f;
                }
            }
            else {
                frame = 0;
            }
        }

        return false;
    }

    protected override void SafeSetMount(Player player, ref bool skipDust) {
        for (int i = 0; i < 32; i++) {
            int dust = Dust.NewDust(player.position + new Vector2(-12, -20), 40, 55, MountData.spawnDust, 0, Main.rand.NextFloat(-3f, -0.5f), 0, default(Color), Main.rand.NextFloat(0.6f, 2.4f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].fadeIn = 1f;
            Main.dust[dust].velocity.X *= 0.1f;
            Main.dust[dust].velocity.Y -= 0.5f;
            Main.dust[dust].velocity.Y *= 2.5f;
        }
        skipDust = true;
    }

    public override void Dismount(Player player, ref bool skipDust) {
        for (int i = 0; i < 56; i++) {
            int dust = Dust.NewDust(player.position + new Vector2(-12, -30), 40, 55, MountData.spawnDust, 0, Main.rand.NextFloat(-3f, -0.5f), 0, default(Color), Main.rand.NextFloat(0.6f, 2.4f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].fadeIn = 1f;
            Main.dust[dust].velocity.X *= 0.1f;
            Main.dust[dust].velocity.Y += 0.5f;
            Main.dust[dust].velocity.Y *= 1.5f;
        }
        skipDust = true;
    }
}