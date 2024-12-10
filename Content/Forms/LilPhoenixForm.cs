using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class LilPhoenixForm : BaseForm {
    protected override Color LightingColor {
        get {
            float num56 = 0.45f;
            return new(num56, num56 * 0.65f, num56 * 0.4f);
        }
    }

    protected override float GetMaxSpeedMultiplier(Player player) => 1f;
    protected override float GetRunAccelerationMultiplier(Player player) => 1.5f;

    protected override void SafeSetDefaults() {
        MountData.spawnDust = 6;
        MountData.spawnDustNoGravity = true;
        MountData.heightBoost = -18;
        MountData.fallDamage = 0.25f;
        MountData.flightTimeMax = 0;
        MountData.fatigueMax = 0;
        MountData.jumpHeight = 17;
        MountData.jumpSpeed = 6f;
        MountData.blockExtraJumps = false;
        MountData.totalFrames = 12;
        MountData.constantJump = false;
        MountData.usesHover = false;
        MountData.yOffset = -7;
    }

    protected override void SafeUpdateEffects(Player player) {
        player.GetModPlayer<BaseFormHandler>().UsePlayerSpeed = true;

        float rotation = player.velocity.X * 0.1f;
        float fullRotation = (float)Math.PI / 4f * rotation / 2f;
        float maxRotation = 0.075f;
        fullRotation = MathHelper.Clamp(fullRotation, -maxRotation, maxRotation);
        player.fullRotation = IsInAir(player) ? 0f : fullRotation;
        player.fullRotationOrigin = new Vector2(player.width / 2 + 4f * player.direction, player.height / 2);
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        int maxFrame = 4;
        float walkingFrameFrequiency = 24f;
        if (IsInAir(player)) {
            frame = 5;
        }
        else if (player.velocity.X != 0f) {
            int maxMovingFrame = maxFrame;
            if (frame >= maxMovingFrame) {
                frame = 0;
            }
            frameCounter += Math.Abs(player.velocity.X) * 1f;
            if (frameCounter >= walkingFrameFrequiency) {
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

        return false;
    }

    protected override void DrawGlowMask(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
        if (glowTexture != null) {
            DrawData item = new(glowTexture, drawPosition, frame, Color.White * ((float)(int)drawColor.A / 255f), rotation, drawOrigin, drawScale, spriteEffects);
            playerDrawData.Add(item);
        }
        WreathHandler wreathHandler = drawPlayer.GetModPlayer<WreathHandler>();
        if (glowTexture != null) {
            float value = Math.Max(MathHelper.Clamp(AttackCharge, 0f, 1f), wreathHandler.ActualProgress4);
            DrawData item = new(ModContent.Request<Texture2D>(Texture + "_Glow2").Value, drawPosition, frame, Color.White * ((float)(int)drawColor.A / 255f) * value, rotation, drawOrigin, drawScale, spriteEffects);
            playerDrawData.Add(item);
        }
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