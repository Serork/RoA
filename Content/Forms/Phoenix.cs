using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Forms;

sealed class Phoenix : BaseForm {
    private static byte FRAMECOUNT => 14;

    protected override void SafeSetDefaults() {
        MountData.totalFrames = FRAMECOUNT;
        MountData.fallDamage = 0f;

        MountData.xOffset = -8;
        MountData.yOffset = -6;

        MountData.playerHeadOffset = -14;
    }

    protected override void SafePostUpdate(Player player) {
        player.GetFormHandler().UsePlayerSpeed = false;
        player.GetFormHandler().UsePlayerHorizontals = false;

        float num = 2f;
        float num2 = 0.1f;
        float num3 = 0.8f;
        if (player.controlUp || player.controlJump) {
            if (player.velocity.Y > 0f)
                player.velocity.Y *= num3;

            player.velocity.Y -= num2;
            if (player.velocity.Y < 0f - num)
                player.velocity.Y = 0f - num;
        }
        if (player.controlDown) {
            if (player.velocity.Y < 0f)
                player.velocity.Y *= num3;

            player.velocity.Y += num2;
            if (player.velocity.Y > num)
                player.velocity.Y = num;
        }
        if (player.controlUp || player.controlJump || player.controlDown) {
        }
        else if ((double)player.velocity.Y < -0.1 || (double)player.velocity.Y > 0.1) {
            player.velocity.Y *= num3;
        }
        else {
            player.velocity.Y = 0f;
        }

        if (player.controlLeft) {
            if (player.velocity.X > 0f)
                player.velocity.X *= num3;

            player.velocity.X -= num2;
            if (player.velocity.X < 0f - num)
                player.velocity.X = 0f - num;
        }
        if (player.controlRight) {
            if (player.velocity.X < 0f)
                player.velocity.X *= num3;

            player.velocity.X += num2;
            if (player.velocity.X > num)
                player.velocity.X = num;
        }
        if (player.controlLeft || player.controlRight) {

        }
        else if ((double)player.velocity.X < -0.1 || (double)player.velocity.X > 0.1) {
            player.velocity.X *= num3;
        }
        else {
            player.velocity.X = 0f;
        }

        if (player.velocity.X < 0f)
            player.direction = -1;
        else if (player.velocity.X > 0f)
            player.direction = 1;

        player.gravity = 0f;

        player.fullRotation = 0f;
        player.fullRotationOrigin = player.getRect().Centered();
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        if (++frameCounter > 6) {
            frameCounter = 0;
            frame++;
            if (frame >= 7) {
                frame = 0;
            }
        }

        return false;
    }

    protected override void SafeSetMount(Player player, ref bool skipDust) {
        
    }

    protected override void SafeDismountMount(Player player, ref bool skipDust) {
        
    }

    protected override void DrawGlowMask(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
        drawPosition = Utils.Floor(drawPosition);
        if (glowTexture != null) {
            DrawData item = new(glowTexture, drawPosition, frame, Color.White * 0.9f * ((float)(int)drawColor.A / 255f), rotation, drawOrigin, drawScale, spriteEffects);
            item.shader = drawPlayer.cBody;
            playerDrawData.Add(item);

            for (float num6 = 0f; num6 < 4f; num6 += 1f) {
                float num3 = ((float)(TimeSystem.TimeForVisualEffects * 60f + drawPlayer.whoAmI * 10) / 40f * ((float)Math.PI * 2f)).ToRotationVector2().X * 3f;
                Color color2 = new Color(80, 70, 40, 0) * (num3 / 8f + 0.5f) * 0.8f * WreathHandler.GetWreathChargeProgress_ForArmorGlow(drawPlayer);
                Vector2 position = item.position + (num6 * ((float)Math.PI / 2f)).ToRotationVector2() * num3;
                DrawData item2 = item;
                item2.position = position;
                item2.color = color2;
                item2.shader = drawPlayer.cBody;
                playerDrawData.Add(item2);
            }
        }
    }
}
