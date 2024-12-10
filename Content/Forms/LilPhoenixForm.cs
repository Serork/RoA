using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class LilPhoenixForm : BaseForm {
    protected override Color LightingColor {
        get {
            float num56 = 1f;
            return new(num56, num56 * 0.65f, num56 * 0.4f) ;
        }
    }

    private class LilPhoenixFormHandler : ModPlayer {
        internal bool _phoenixJumped, _phoenixJumped2;
        internal bool _phoenixJustJumped, _phoenixJustJumpedForAnimation, _phoenixJustJumpedForAnimation2;
        internal int _phoenixJumpsCD;
        internal int _phoenixJump;
    }

    protected override float GetMaxSpeedMultiplier(Player player) => 1f;
    protected override float GetRunAccelerationMultiplier(Player player) => 1.5f;

    protected override void SafeSetDefaults() {
        MountData.spawnDust = 6;
        MountData.spawnDustNoGravity = true;
        MountData.heightBoost = -18;
        MountData.fallDamage = 0.1f;
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

        ExtraJumpsHandler(player);
    }

    private void ExtraJumpsHandler(Player player) {
        LilPhoenixFormHandler plr = player.GetModPlayer<LilPhoenixFormHandler>();
        float jumpSpeed = 8.01f;
        int jumpHeight = 5;
        void jump() {
            player.velocity.Y = -jumpSpeed * player.gravDir;
            plr._phoenixJump = (int)((double)jumpHeight * 2f);
            plr._phoenixJumpsCD = 30;
            SoundEngine.PlaySound(SoundID.Item45, player.position);
            plr._phoenixJustJumped = plr._phoenixJustJumpedForAnimation = true;
            AttackCharge = 1f;
        }
        if (player.controlJump) {
            if (plr._phoenixJump > 0) {
                if (player.velocity.Y == 0f)
                    plr._phoenixJump = 0;
                else {
                    player.velocity.Y = -jumpSpeed * player.gravDir;
                    plr._phoenixJump--;
                }
            }
            else {
                if ((player.sliding || player.velocity.Y == 0f || plr._phoenixJumped || plr._phoenixJumped2) && player.releaseJump && plr._phoenixJumpsCD == 0) {
                    bool justJumped = false;
                    bool justJumped2 = false;
                    if (plr._phoenixJumped) {
                        justJumped = true;
                        plr._phoenixJumped = false;
                    }
                    else if (plr._phoenixJumped2) {
                        justJumped2 = true;
                        plr._phoenixJumped2 = false;
                    }
                    if (player.velocity.Y == 0f || player.sliding) {
                        plr._phoenixJumped = true;
                        plr._phoenixJumped2 = true;
                    }
                    if (player.velocity.Y == 0f || player.sliding) {
                        player.velocity.Y = -jumpSpeed * player.gravDir;
                        plr._phoenixJump = (int)((double)jumpHeight * 2.5f);
                        plr._phoenixJumpsCD = 30;
                    }
                    else {
                        if (justJumped) {
                            jump();
                        }
                        else if (justJumped2) {
                            jump();
                        }
                    }
                }
            }
            player.releaseJump = false;
        }
        else
            plr._phoenixJump = 0;
        if (plr._phoenixJumpsCD > 0)
            plr._phoenixJumpsCD--;
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        int maxFrame = 4;
        float walkingFrameFrequiency = 24f;
        if (IsInAir(player)) {
            LilPhoenixFormHandler plr = player.GetModPlayer<LilPhoenixFormHandler>();
            if (plr._phoenixJustJumpedForAnimation) {
                if (!plr._phoenixJustJumpedForAnimation2) {
                    plr._phoenixJustJumpedForAnimation2 = true;
                    frameCounter = 4f;
                }
                if (++frameCounter >= 4.0) {
                    int maxMovingFrame = 9;
                    if (frame < maxMovingFrame)
                        frame++;
                    else {
                        frame = 5;
                        plr._phoenixJustJumpedForAnimation = false;
                        plr._phoenixJustJumpedForAnimation2 = false;
                    }
                    frameCounter = 0f;
                }
            }
            else {
                frame = 5;
                frameCounter = 0f;
            }
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
            float value = Math.Max(MathHelper.Clamp(_attackCharge, 0f, 1f), wreathHandler.ActualProgress4);
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