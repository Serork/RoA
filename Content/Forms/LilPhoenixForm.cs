using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Druidic.Forms;
using RoA.Core.Utility;
using RoA.Utilities;

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
        internal Vector2 _tempPosition;
        internal bool _isPreparing, _wasPreparing, _prepared;
        internal float _charge, _charge2;
        internal bool _dashed;

        internal void ResetDash() {
            _dashed = false;
            _wasPreparing = true;
            _prepared = true;
        }

        public override void ResetEffects() {
            if (!Player.GetModPlayer<BaseFormHandler>().IsInDruidicForm) {
                ResetDash();
            }
        }
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
        MountData.jumpHeight = 15;
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
        LilPhoenixFormHandler plr = player.GetModPlayer<LilPhoenixFormHandler>();
        if (plr._dashed) {
            player.fullRotation = (float)Math.Atan2((double)player.velocity.Y, (double)player.velocity.X) + (float)Math.PI / 2f;
        }
        else if (plr._isPreparing) {
            float length = 8f - player.velocity.Length();
            length *= 0.075f;
            player.fullRotation += 0.5f * length * player.direction;
            player.fullRotationOrigin = new Vector2(9f, 5f);
        }
        else {
            player.fullRotation = IsInAir(player) ? 0f : fullRotation;
            player.fullRotationOrigin = new Vector2(player.width / 2 + 4f * player.direction, player.height / 2);
        }

        ExtraJumpsHandler(player);
        UltraAttackHandler(player);
    }

    private void UltraAttackHandler(Player player) {
        LilPhoenixFormHandler plr = player.GetModPlayer<LilPhoenixFormHandler>();
        int testY = (int)player.Center.Y / 16;
        int value = 5;
        bool flag = true;
        testY = Math.Min(Main.maxTilesY - value, testY);
        for (int i = testY; i < testY + value; i++) {
            Tile tile = Main.tile[(int)player.Center.X / 16, i];
            if (tile.HasTile && (Main.tileSolid[(int)tile.TileType] || tile.LiquidType != 0)) {
                flag = false;
                break;
            }
        }
        if (plr._isPreparing) {
            flag = true;
        }
        if (!flag && !IsInAir(player)) {
            plr._wasPreparing = false;
            plr._dashed = false;
        }
        void dash() {
            SoundEngine.PlaySound(SoundID.Item74, player.position);
            player.direction = player.position.X < Main.MouseWorld.X ? 1 : -1;
            float speed = 5f * plr._charge;
            Vector2 vector_ = player.GetViableMousePosition();
            Vector2 vector = new(vector_.X - player.Center.X, vector_.Y - player.Center.Y);
            float acceleration = Math.Abs(player.velocity.X) + Math.Abs(player.velocity.Y);
            acceleration += 10f - acceleration;
            vector.X -= player.velocity.X * acceleration;
            vector.Y -= player.velocity.Y * acceleration;
            float sqrt = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            sqrt = speed / sqrt;
            player.velocity.X = vector.X * sqrt;
            player.velocity.Y = vector.Y * sqrt;
            plr._dashed = true;
        }
        bool flag2 = Main.mouseLeft && !Main.mouseText && player.whoAmI == Main.myPlayer;
        if (plr._isPreparing) {
            flag2 = Main.mouseLeft && player.whoAmI == Main.myPlayer;
        }
        if (!flag2) {
            if (plr._charge > 0f) {
                plr._prepared = false;
                dash();
            }
        }
        if (flag && flag2 && !plr._wasPreparing && !plr._dashed) {
            player.controlJump = false;
            player.controlLeft = player.controlRight = false;
            player.velocity *= 0.7f;
            player.gravity = 0f;
            player.position.X = Helper.Approach(player.position.X, plr._tempPosition.X, 0.5f);
            player.position.Y = Helper.Approach(player.position.Y, plr._tempPosition.Y, 0.5f);
            plr._tempPosition = Vector2.Lerp(plr._tempPosition, player.position, 0.25f);
            plr._isPreparing = true;
            plr._wasPreparing = false;
            float max = 3.5f;
            if (plr._charge < max) {
                plr._charge += 0.1f;
                plr._charge2 += 0.35f;
                plr._charge2 = Math.Min(plr._charge2, max);
            }
            else if (!plr._prepared) {
                if (Main.netMode != NetmodeID.Server) {
                    int k = 36;
                    for (int i = 0; i < k; i++) {
                        int x = (int)((double)player.position.X - 3.0 + (double)player.width / 2.0);
                        int y = (int)((double)player.position.Y - 8.0 + (double)player.height / 2.0);
                        Vector2 vector = (new Vector2((float)player.width / 2f, player.height) * 0.8f).RotatedBy((float)(i - (k / 2 - 1)) * ((float)Math.PI * 2f) / (float)k) + new Vector2((float)x, (float)y);
                        Vector2 vector2 = vector - new Vector2((float)x, (float)y);
                        int dust = Dust.NewDust(vector + vector2 - new Vector2(1f, 2f), 0, 0, 6, vector2.X * 2f, vector2.Y * 2f, 0, default(Color), 3.15f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity = -Vector2.Normalize(vector2) * 2f;
                    }
                }
                plr._prepared = true;
            }
            AttackCharge = plr._charge2 / max * 1.25f;
        }
        else {
            if (plr._isPreparing) {
                plr._isPreparing = false;
                plr._wasPreparing = true;
            }
            plr._tempPosition = player.position + player.velocity * 8f;
            plr._charge = plr._charge2 = 0f;
            plr._prepared = false;
        }
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
            int damage = (int)player.GetTotalDamage(DruidClass.NatureDamage).ApplyTo(40f);
            float knockBack = (int)player.GetTotalKnockback(DruidClass.NatureDamage).ApplyTo(2f);
            ushort projType = (ushort)ModContent.ProjectileType<LilPhoenixFlames>();
            for (int i = 0; i < 2; i++) {
                int proj = Projectile.NewProjectile(player.GetSource_Misc("phoenixjump"), 
                    player.Center + new Vector2(14f * i * (i == 1 ? -1f : 1f), -4f) + Vector2.UnitX * player.direction * 6f + (player.direction == -1 ? new Vector2(8f, 0f) : Vector2.Zero),
                    Vector2.Zero, projType, damage, knockBack, player.whoAmI);
                if (Main.netMode != NetmodeID.SinglePlayer)
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
            }
            if (plr._dashed) {
                plr.ResetDash();
            }
        }

        if (player.controlJump && !plr._isPreparing) {
            if (plr._phoenixJump > 0) {
                if (player.velocity.Y == 0f) {
                    plr._phoenixJump = 0;
                }
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
        else {
            plr._phoenixJump = 0;
        }
        if (plr._phoenixJumpsCD > 0) {
            plr._phoenixJumpsCD--;
        }
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        int maxFrame = 4;
        float walkingFrameFrequiency = 24f;
        LilPhoenixFormHandler plr = player.GetModPlayer<LilPhoenixFormHandler>();
        if (plr._dashed) {
            frame = 11;
            frameCounter = 0f;
        }
        else if (plr._isPreparing) {
            frame = 10;
            frameCounter = 0f;
        }
        else if (IsInAir(player)) {
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