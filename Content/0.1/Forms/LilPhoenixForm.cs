using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid.Forms;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature.Forms;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Reflection;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class LilPhoenixForm : BaseForm {
    private static Asset<Texture2D>? _glowMask2;

    public override ushort SetHitboxWidth(Player player) => Player.defaultWidth;
    public override ushort SetHitboxHeight(Player player) => Player.defaultHeight;

    public override SoundStyle? HurtSound => SoundID.NPCHit31;

    public override bool ShouldApplyUpdateJumpHeightLogic => true;

    public override Vector2 SetWreathOffset(Player player) => new(0f, 0f);
    public override Vector2 SetWreathOffset2(Player player) => new(0f, 0f);

    //protected override _color LightingColor {
    //    get {
    //        float num56 = 1f;
    //        return new(num56, num56 * 0.65f, num56 * 0.4f);
    //    }
    //}

    public override float GetMaxSpeedMultiplier(Player player) => 1f;
    public override float GetRunAccelerationMultiplier(Player player) => 1.5f;

    protected override void SafeSetDefaults() {
        MountData.spawnDust = 6;
        MountData.spawnDustNoGravity = true;
        MountData.fallDamage = 0.1f;
        MountData.flightTimeMax = 0;
        MountData.fatigueMax = 0;
        MountData.jumpHeight = 15;
        MountData.jumpSpeed = 6f;
        MountData.totalFrames = 12;
        MountData.constantJump = false;
        MountData.usesHover = false;

        MountData.yOffset = 2;
        MountData.playerHeadOffset = -24;

        if (!Main.dedServ) {
            _glowMask2 = ModContent.Request<Texture2D>(Texture + "_Glow2");
        }
    }

    protected override void SafePostUpdate(Player player) {
        player.GetFormHandler().UsePlayerSpeed = true;

        float rotation = player.velocity.X * 0.1f;
        float fullRotation = (float)Math.PI / 4f * rotation / 2f;
        float maxRotation = 0.075f;
        fullRotation = MathHelper.Clamp(fullRotation, -maxRotation, maxRotation);
        var plr = player.GetFormHandler();
        if (plr.Dashed) {
            player.fullRotation = (float)Math.Atan2((double)player.velocity.Y, (double)player.velocity.X) + (float)Math.PI / 2f;
        }
        else if (plr.IsPreparing) {
            float length = 9f - player.velocity.Length();
            length *= 0.075f;
            player.fullRotation += (0.4f + Utils.Remap(plr._charge * 2f, 0f, 3.5f, 0f, 0.2f)) * length * player.direction;
        }
        else {
            player.fullRotation = IsInAir(player) ? 0f : fullRotation;
        }

        player.fullRotationOrigin = player.getRect().Size() / 2f + new Vector2(0f, 3f);

        ActivateExtraJumps(player);
        UltraAttackHandler(player);
    }

    private void UltraAttackHandler(Player player) {
        var plr = player.GetFormHandler();
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
        if (plr.IsPreparing) {
            flag = true;
        }
        bool flag4 = !flag || !IsInAir(player);
        StrikeNPC(player, !player.wet && WorldGenHelper.CustomSolidCollision(player.position - Vector2.One * 3, player.width + 6, player.height + 6, TileID.Sets.Platforms));
        if (flag4) {
            if (plr._charge3 < BaseFormHandler.MAXPHOENIXCHARGE) {
                if (plr.Dashed) {
                    plr.ClearPhoenixProjectiles();
                }
                plr.Dashed = false;
            }
            plr.WasPreparing = false;
            if (player.eocDash > 0) {
                player.eocDash -= 10;
            }
            else {
                player.armorEffectDrawShadowEOCShield = false;
            }
        }
        void dash() {
            SoundEngine.PlaySound(SoundID.Item74, player.Center);
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 10, player.Center));
            }
            Vector2 vector_ = player.GetViableMousePosition();
            player.controlLeft = player.controlRight = false;
            player.direction = -(player.Center - vector_).X.GetDirection();
            float speed = 5f * plr._charge;
            Vector2 vector = new(vector_.X - player.Center.X, vector_.Y - player.Center.Y);
            float acceleration = Math.Abs(player.velocity.X) + Math.Abs(player.velocity.Y);
            acceleration += 10f - acceleration;
            vector.X -= player.velocity.X * acceleration;
            vector.Y -= player.velocity.Y * acceleration;
            float sqrt = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            sqrt = speed / sqrt;
            player.velocity.X = vector.X * sqrt;
            player.velocity.Y = vector.Y * sqrt;
            plr.Prepared = false;
            plr.Dashed2 = plr.Dashed = true;
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PhoenixDashPacket(player));
            }
            ushort type = (ushort)ModContent.ProjectileType<LilPhoenixTrailFlame>();
            int damage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(35f);
            float knockBack = (int)player.GetTotalKnockback(DruidClass.Nature).ApplyTo(0f);
            for (int i = 0; i < 2; i++) {
                Projectile.NewProjectile(player.GetSource_Misc("phoenixdash"), player.Center, Vector2.Zero, type, damage, knockBack, player.whoAmI, (float)i);
            }

            NetMessage.SendData(13, -1, -1, null, Main.myPlayer);
            //player.GetWreathHandler().ResetGryphonStats(true, 0.25f);
        }
        player.SyncLMB();
        if (!player.HoldingLMB()) {
            if (plr._charge > 0f) {
                if (player.whoAmI == Main.myPlayer) {
                    dash();
                }

                int k = 36;
                for (int i = 0; i < k; i++) {
                    int x = (int)((double)player.Center.X - 2.5);
                    int y = (int)((double)player.Center.Y + 2.0);
                    Vector2 vector3 = (new Vector2((float)player.width / 2f, player.height) * 0.5f).RotatedBy((float)(i - (k / 2 - 1)) * ((float)Math.PI * 2f) / (float)k) + new Vector2((float)x, (float)y);
                    Vector2 vector2 = -(vector3 - new Vector2((float)x, (float)y));
                    int dust = Dust.NewDust(vector3 - player.velocity * 3f + vector2 * 2f * Main.rand.NextFloat() - new Vector2(1f, 2f), 0, 0, 6, vector2.X * 2f, vector2.Y * 2f, 0, default(Color), 3.15f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = -Vector2.Normalize(vector2) * Main.rand.NextFloat(1.5f, 3f) * Main.rand.NextFloat() + player.velocity * 0.75f * Main.rand.NextFloat(0.5f, 1f);
                }
            }
        }
        if (flag && player.HoldingLMB() && !plr.WasPreparing && !plr.Dashed) {
            player.controlJump = false;
            player.controlLeft = player.controlRight = false;
            player.velocity *= 0.7f;
            player.gravity = 0f;
            player.position.X = Helper.Approach(player.position.X, plr.TempPosition.X, 0.5f);
            player.position.Y = Helper.Approach(player.position.Y, plr.TempPosition.Y, 0.5f);
            plr.TempPosition = Vector2.Lerp(plr.TempPosition, player.position, 0.25f);
            plr.IsPreparing = true;
            plr.WasPreparing = false;
            float max = BaseFormHandler.MAXPHOENIXCHARGE;
            if (plr._charge < max) {
                plr._charge += 0.1f;
                plr._charge3 = plr._charge;
                plr._charge2 += 0.35f;
                plr._charge2 = Math.Min(plr._charge2, max);
            }
            else if (!plr.Prepared) {
                int k = 36;
                for (int i = 0; i < k; i++) {
                    int x = (int)((double)player.Center.X - 2.5);
                    int y = (int)((double)player.Center.Y + 2.0);
                    Vector2 vector = (new Vector2((float)player.width / 2f, player.height) * 0.5f).RotatedBy((float)(i - (k / 2 - 1)) * ((float)Math.PI * 2f) / (float)k) + new Vector2((float)x, (float)y);
                    Vector2 vector2 = vector - new Vector2((float)x, (float)y);
                    int dust = Dust.NewDust(vector + vector2 - new Vector2(1f, 2f), 0, 0, 6, vector2.X * 2f, vector2.Y * 2f, 0, default(Color), 3.15f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = -Vector2.Normalize(vector2) * 2f;
                }
                plr.Prepared = true;
            }
            BaseFormDataStorage.ChangeAttackCharge1(player, plr._charge2 / max * 1.25f);
        }
        else {
            if (plr.IsPreparing) {
                plr.IsPreparing = false;
                plr.WasPreparing = true;
            }
            plr.TempPosition = player.position + player.velocity * 8f;
            if (plr._charge > 0f) {
                player.eocDash = (int)(plr._charge * 15f);
                player.armorEffectDrawShadowEOCShield = true;
            }
            plr._charge = plr._charge2 = 0f;
            plr.Prepared = false;
        }
    }

    private void StrikeNPC(Player player, bool flag4) {
        var plr = player.GetFormHandler();
        void explosion(int i = -1) {
            if (plr.Dashed2 && Main.netMode != NetmodeID.Server) {
                float value = plr._charge3 / BaseFormHandler.MAXPHOENIXCHARGE;
                if (i != -1) {
                    player.immune = true;
                    player.immuneTime = 20;
                    player.immuneNoBlink = true;
                    int direction = Main.npc[i].direction;
                    if (Main.npc[i].velocity.X < 0f)
                        direction = -1;
                    else if (Main.npc[i].velocity.X > 0f)
                        direction = 1;
                    bool crit = false;
                    if (Main.rand.Next(100) < (4 + player.GetTotalCritChance(DruidClass.Nature)))
                        crit = true;
                    int damage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(50f * value);
                    float knockBack = (int)player.GetTotalKnockback(DruidClass.Nature).ApplyTo(4f * value);
                    if (player.whoAmI == Main.myPlayer)
                        player.ApplyDamageToNPC(Main.npc[i], (int)damage, knockBack, direction, crit, DruidClass.Nature, true);
                }
                if (plr._charge3 >= BaseFormHandler.MAXPHOENIXCHARGE) {
                    player.immune = true;
                    player.immuneTime = 30;
                    player.immuneNoBlink = true;
                    SoundEngine.PlaySound(SoundID.Item14, player.position);
                    int damage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(40f * value);
                    int knockBack = (int)player.GetTotalKnockback(DruidClass.Nature).ApplyTo(4f * value);
                    ushort projType = (ushort)ModContent.ProjectileType<LilPhoenixExplosion>();
                    int proj = Projectile.NewProjectile(player.GetSource_Misc("phoenixexplosion"), player.Center.X, player.Center.Y, 0f, 0f, projType, damage, knockBack, player.whoAmI);
                    if (Main.netMode != NetmodeID.SinglePlayer)
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
                    plr._charge3 = 0f;
                    if (plr.Dashed) {
                        plr.ClearPhoenixProjectiles();
                    }
                    plr.Dashed = false;
                }
                plr.Dashed2 = false;
                //sMain.npc[i].StrikeNPC((int)(CurrentDamage * plr.dashChargeValue), 2f * plr.dashChargeValue, direction, Main.rand.Next(2) == 0 ? true : false, false, false);
            }
        }
        for (int i = 0; i < Main.npc.Length; i++) {
            NPC nPC = Main.npc[i];
            Rectangle rectangle = new((int)((double)player.position.X + (double)player.velocity.X * 0.5 - 4.0), (int)((double)player.position.Y + (double)player.velocity.Y * 0.5 - 4.0), player.width + 8, player.height + 8);
            bool flag3 = !(!nPC.active || nPC.dontTakeDamage || nPC.friendly || (nPC.aiStyle == 112 && !(nPC.ai[2] <= 1f)) || !player.CanNPCBeHitByPlayerOrPlayerProjectile(nPC));
            Rectangle rect = nPC.getRect();
            bool flag5 = false;
            if (rectangle.Intersects(rect) && (nPC.noTileCollide || player.CanHit(nPC))) {
                flag5 = true;
            }
            if (!flag5) {
                flag3 = false;
            }
            if (flag3) {
                explosion(i);
            }
        }
        if (flag4) {
            explosion();
        }
    }

    protected override void OnJump(Player player) {
        var plr = player.GetFormHandler();
        SoundEngine.PlaySound(SoundID.Item45, player.position);
        BaseFormDataStorage.ChangeAttackCharge1(player, 1f);
        int damage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(50f);
        float knockBack = (int)player.GetTotalKnockback(DruidClass.Nature).ApplyTo(2f);
        ushort projType = (ushort)ModContent.ProjectileType<LilPhoenixFlames>();
        if (player.whoAmI == Main.myPlayer) {
            for (int i = 0; i < 2; i++) {
                int proj = Projectile.NewProjectile(player.GetSource_Misc("phoenixjump"),
                    player.Center + new Vector2(14f * i * (i == 1 ? -1f : 1f), -4f) + Vector2.UnitX * player.direction * 6f + (player.direction == -1 ? new Vector2(8f, 0f) : Vector2.Zero),
                    Vector2.Zero, projType, damage, knockBack, player.whoAmI);
                //if (Main.netMode != NetmodeID.SinglePlayer)
                //    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
            }
        }
        if (plr.Dashed) {
            plr.ResetPhoenixDash();
        }
        plr.Dashed2 = false;
    }

    protected override void SetJumpSettings(Player player, ref int jumpHeight, ref float jumpSpeed, ref float jumpSpeedBoost, ref float extraFall,
        ref bool hasDoubleJump, ref int jumpOnFirst, ref int jumpOnSecond) {
        jumpSpeed = jumpSpeed * 1.6f;
        jumpHeight = jumpHeight / 3;
        hasDoubleJump = true;
        jumpOnFirst = (int)((double)jumpHeight * 2f);
        jumpOnSecond = (int)((double)jumpHeight * 2.5f);
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        int maxFrame = 4;
        float walkingFrameFrequiency = 24f;
        var plr = player.GetFormHandler();
        if (plr.Dashed) {
            frame = 11;
            frameCounter = 0f;
        }
        else if (plr.IsPreparing) {
            frame = 10;
            frameCounter = 0f;
        }
        else if (IsInAir(player)) {
            if (plr.JustJumpedForAnimation) {
                if (!plr.JustJumpedForAnimation2) {
                    plr.JustJumpedForAnimation2 = true;
                    frameCounter = 4f;
                }
                if (++frameCounter >= 4.0) {
                    int maxMovingFrame = 9;
                    if (frame < maxMovingFrame)
                        frame++;
                    else {
                        frame = 5;
                        plr.JustJumpedForAnimation = false;
                        plr.JustJumpedForAnimation2 = false;
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
            item.shader = drawPlayer.cBody;
            playerDrawData.Add(item);
        }
        if (_glowMask2?.IsLoaded == true) {
            float value = MathHelper.Clamp(Math.Max(drawPlayer.GetModPlayer<BaseFormDataStorage>()._attackCharge2, drawPlayer.GetModPlayer<BaseFormDataStorage>()._attackCharge), 0f, 1f);
            DrawData item = new(_glowMask2.Value, drawPosition, frame, Color.White * 0.85f * ((float)(int)drawColor.A / 255f) * value, rotation, drawOrigin, drawScale, spriteEffects);
            item.shader = drawPlayer.cBody;
            playerDrawData.Add(item);
        }
    }

    protected override void SafeSetMount(Player player, ref bool skipDust) {
        player.GetFormHandler().ResetPhoenixDash(true);
        Vector2 center = player.Center + player.velocity;
        for (int i = 0; i < 32; i++) {
            Point size = new(40, 55);
            int dust = Dust.NewDust(center - size.ToVector2() / 2f + new Vector2(-3f, 10f), size.X, size.Y, MountData.spawnDust, 0, Main.rand.NextFloat(-3f, -0.5f), 0, default(Color), Main.rand.NextFloat(0.6f, 2.4f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].fadeIn = 1f;
            Main.dust[dust].velocity.X *= 0.1f;
            Main.dust[dust].velocity.Y -= 0.5f;
            Main.dust[dust].velocity.Y *= 2.5f;
        }
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "BirdCall") { Pitch = 0.9f, PitchVariance = 0.1f, Volume = 2f }, player.Center);
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Screech") { Pitch = -0.7f, PitchVariance = 0.1f, Volume = 0.8f }, player.Center);
        skipDust = true;
    }

    protected override void SafeDismountMount(Player player, ref bool skipDust) {
        player.GetFormHandler().ResetPhoenixDash(true);
        Vector2 center = player.Center + player.velocity;
        for (int i = 0; i < 56; i++) {
            Point size = new(40, 55);
            int dust = Dust.NewDust(center - size.ToVector2() / 2f + new Vector2(-3f, -5f), size.X, size.Y, MountData.spawnDust, 0, Main.rand.NextFloat(-3f, -0.5f), 0, default(Color), Main.rand.NextFloat(0.6f, 2.4f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].fadeIn = 1f;
            Main.dust[dust].velocity.X *= 0.1f;
            Main.dust[dust].velocity.Y += 0.5f;
            Main.dust[dust].velocity.Y *= 1.5f;
        }
        skipDust = true;
    }
}