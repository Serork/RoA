using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Items;
using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Players;
using RoA.Content.NPCs.Enemies.Backwoods;
using RoA.Content.Projectiles.Friendly.Nature.Forms;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class FlederForm : BaseForm {
    private static byte FRAMECOUNT => 10;

    public override ushort SetHitboxWidth(Player player) => (ushort)(Player.defaultWidth * 1.5f);
    public override ushort SetHitboxHeight(Player player) => (ushort)(Player.defaultHeight * 1f);

    public override SoundStyle? HurtSound => SoundID.NPCHit27;

    public override Vector2 SetWreathOffset(Player player) => new(0f, 0f);
    public override Vector2 SetWreathOffset2(Player player) => new(0f, 0f);

    protected override Vector2 GetLightingPos(Player player) => player.Center;
    //protected override _color LightingColor => new(79, 124, 211);

    public override float GetMaxSpeedMultiplier(Player player) => 1f;
    public override float GetRunAccelerationMultiplier(Player player) => 1.25f;

    protected override void SafeSetDefaults() {
        MountData.totalFrames = FRAMECOUNT;
        MountData.spawnDust = 59;
        MountData.fallDamage = 0f;
        MountData.flightTimeMax = 60;
        MountData.fatigueMax = 40;

        MountData.yOffset = 1;
        MountData.playerHeadOffset = -20;
    }

    protected override void SafePostUpdate(Player player) {
        player.GetFormHandler().UsePlayerSpeed = true;

        bool flag = IsInAir(player);

        float yOffset = flag ? -3 : 1;
        player.GetFormHandler().AttackFactor = Helper.Approach(player.GetFormHandler().AttackFactor, yOffset, 5f);

        MountData.yOffset = (int)player.GetFormHandler().AttackFactor;

        if (!flag) {
            player.GetFormHandler().JustJumped = false;
            player.GetFormHandler().JustJumpedForAnimation = false;
        }

        player.npcTypeNoAggro[ModContent.NPCType<Fleder>()] = true;
        player.npcTypeNoAggro[ModContent.NPCType<BabyFleder>()] = true;
        player.statDefense -= 6;

        float velocityX = player.velocity.X;
        float maxVelocityX = flag ? 2.5f : 1.75f;
        velocityX = MathHelper.Clamp(velocityX, -maxVelocityX, maxVelocityX);
        float rotation = velocityX * (IsInAir(player) ? 0.2f : 0.15f);
        float fullRotation = (float)Math.PI / 4f * rotation / 2f;
        if (flag) {
            float maxFlightSpeedX = Math.Min(3.5f, player.maxRunSpeed * 0.75f);
            float acceleration = player.runAcceleration / 2f;
            if (player.velocity.X > maxFlightSpeedX) {
                player.velocity.X -= acceleration;
            }
            if (player.velocity.X < -maxFlightSpeedX) {
                player.velocity.X += acceleration;
            }
        }
        if (player.GetFormHandler().ActiveDash) {
            player.fullRotation = Utils.AngleLerp(player.fullRotation, fullRotation, 0.25f);
        }
        else {
            player.fullRotation = fullRotation;
        }
        //float maxRotation = flag ? 0.3f : 0.2f;
        //player.fullRotation = MathHelper.Clamp(player.fullRotation, -maxRotation, maxRotation);
        Player.jumpHeight = 1;
        Player.jumpSpeed = 4f;
        player.gravity *= 0.75f;
        player.velocity.Y = Math.Min(5f, player.velocity.Y);
        player.fullRotationOrigin = new Vector2(player.width / 2 + 4f * player.direction, player.height / 2 - 6f);

        SpecialAttackHandler(player);
    }

    protected override void SafeLoad() {
        MouseVariables.OnHoldingLMBEvent += MouseVariables_OnHoldingLMBEvent;
    }

    private void MouseVariables_OnHoldingLMBEvent(Player player) {
        if (!player.GetFormHandler().IsConsideredAs<FlederForm>()) {
            return;
        }

        bool flag = player.whoAmI != Main.myPlayer;
        if (flag)
            return;

        OnHoldingLMB(player);
    }

    private static void OnHoldingLMB(Player player) {
        ref int shootCounter = ref player.GetFormHandler().ShootCounter;

        shootCounter++;

        bool flag = shootCounter >= 100;
        int num20 = 1;
        if (shootCounter >= 40f)
            num20++;
        if (shootCounter >= 70f)
            num20++;
        if (flag)
            num20++;

        Vector2 vector11 = player.Center + Vector2.UnitY * (IsInAir(player) ? 7f : 12f);
        for (int k = 0; k < num20; k++) {
            if (Main.rand.NextChance(0.9f - 0.1f * (num20 - 1))) {
                int num23 = 59;
                float num24 = 0.4f;
                if (k % 2 == 1) {
                    num24 = 0.65f;
                }
                num24 *= 2.25f;

                Vector2 vector12 = vector11 + ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * (12f - (float)(num20 * 2));
                int num25 = Dust.NewDust(vector12 - Vector2.One * 20f, 40, 40, num23, player.velocity.X / 2f, player.velocity.Y / 2f);
                if (Vector2.Distance(Main.dust[num25].position, vector11) > 16f) {
                    Main.dust[num25].active = false;
                    continue;
                }
                Main.dust[num25].velocity = Vector2.Normalize(vector11 - vector12) * 1.5f * (10f - (float)num20 * 2f) / 10f;
                Main.dust[num25].noGravity = true;
                Main.dust[num25].scale = num24;
            }
        }
    }

    private void SpecialAttackHandler(Player player) {
        ref int shootCounter = ref player.GetFormHandler().ShootCounter;

        bool flag = player.whoAmI != Main.myPlayer;
        if (flag && player.HoldingLMB()) {
            OnHoldingLMB(player);
        }

        if (shootCounter == 40 || shootCounter == 70 || shootCounter == 100) {
            if (player.whoAmI == Main.myPlayer) {
                SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
            }
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 13, player.Center));
            }
        }

        if (flag || Main.mouseText)
            return;
        if (player.GetFormHandler().DashDelay > BaseFormHandler.CD - 5) {
            BaseFormDataStorage.ChangeAttackCharge1(player, 1.5f);
        }

        player.SyncLMB();

        //if (player.controlUseItem && Main.mouseLeft) {
        //    if (!holdingLmb) {
        //        if (Main.netMode == NetmodeID.MultiplayerClient) {
        //            MultiplayerSystem.SendPacket(new SyncLMBPacket(player, true));
        //        }
        //    }
        //    holdingLmb = true;
        //    OnHoldingLMB();
        //}
        //else {
        //    if (holdingLmb) {
        //        if (Main.netMode == NetmodeID.MultiplayerClient) {
        //            MultiplayerSystem.SendPacket(new SyncLMBPacket(player, false));
        //        }
        //    }
        //    holdingLmb = false;
        //}
        string context = "flederformattack";
        int baseDamage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(30);
        float baseKnockback = player.GetTotalKnockback(DruidClass.Nature).ApplyTo(3f);
        if (Main.mouseLeftRelease) {
            if (shootCounter >= 40 && shootCounter < 70) {
                BaseFormDataStorage.ChangeAttackCharge1(player, 1.5f);
                BaseFormHandler.SpawnFlederDusts(player, 1);
                Vector2 Velocity = Helper.VelocityToPoint(player.Center, Main.rand.RandomPointInArea(new Vector2(player.Center.X, player.Center.Y + 100), new Vector2(player.Center.X, player.Center.Y + 100)), 4);
                Projectile.NewProjectile(player.GetSource_Misc(context), player.Center.X, player.Center.Y, Velocity.X, Velocity.Y + 3, ModContent.ProjectileType<FlederBomb>(), baseDamage, baseKnockback, player.whoAmI, 0f, 0f);
                player.velocity.Y = 0f;
                player.velocity.Y -= 5f;

                if (player.whoAmI == Main.myPlayer) {
                    SoundEngine.PlaySound(SoundID.Item7 with { Pitch = 0.3f, PitchVariance = 0.1f, Volume = 1f }, player.Bottom);
                }
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 15, player.Bottom));
                }

                NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.myPlayer);
            }
            if (shootCounter >= 70 && shootCounter < 100) {
                BaseFormDataStorage.ChangeAttackCharge1(player, 1.5f);
                BaseFormHandler.SpawnFlederDusts(player, 2);
                Vector2 Velocity = Helper.VelocityToPoint(player.Center, Main.rand.RandomPointInArea(new Vector2(player.Center.X, player.Center.Y + 100), new Vector2(player.Center.X, player.Center.Y + 100)), 4);
                Projectile.NewProjectile(player.GetSource_Misc(context), player.Center.X, player.Center.Y, Velocity.X, Velocity.Y + 5, ModContent.ProjectileType<FlederBomb>(), baseDamage * 2, baseKnockback * 1.2f, player.whoAmI, 1f, 0f);
                player.velocity.Y = 0f;
                player.velocity.Y -= 7.5f;

                NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.myPlayer);

                if (player.whoAmI == Main.myPlayer) {
                    SoundEngine.PlaySound(SoundID.Item7 with { Pitch = 0.3f, PitchVariance = 0.1f, Volume = 1f }, player.Bottom);
                }
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 15, player.Bottom));
                }
            }
            if (shootCounter >= 100) {
                BaseFormDataStorage.ChangeAttackCharge1(player, 1.5f);
                BaseFormHandler.SpawnFlederDusts(player);
                Vector2 Velocity = Helper.VelocityToPoint(player.Center, Main.rand.RandomPointInArea(new Vector2(player.Center.X, player.Center.Y + 100), new Vector2(player.Center.X, player.Center.Y + 100)), 4);
                Projectile.NewProjectile(player.GetSource_Misc(context), player.Center.X, player.Center.Y, Velocity.X, Velocity.Y + 8, ModContent.ProjectileType<FlederBomb>(), baseDamage * 3, baseKnockback * 1.4f, player.whoAmI, 2f, 0f);
                player.velocity.Y = 0f;
                player.velocity.Y -= 10f;

                NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.myPlayer);

                if (player.whoAmI == Main.myPlayer) {
                    SoundEngine.PlaySound(SoundID.Item7 with { Pitch = 0.3f, PitchVariance = 0.1f, Volume = 1f }, player.Bottom);
                }
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 15, player.Bottom));
                }
            }
            shootCounter = 0;
        }
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        int flightAnimationStartFrame = 6, flightAnimationEndFrame = 9;
        float flightFrameFrequency = 14f, walkingFrameFrequiency = 20f;

        byte runAnimationEndFrame = 5,
             runAnimationStartFrame = 0;

        byte swingAnimationStartFrame = 3,
             swingAnimationEndFrame = 5;

        float flightAnimationCounterSpeed = Math.Abs(player.velocity.Y) * 0.5f;

        void playFlapSound(bool reset = false) {
            if (reset) {
                player.flapSound = false;
                return;
            }
            if (!player.flapSound) {
                SoundEngine.PlaySound(SoundID.Item32, player.Center);
            }
            player.flapSound = true;
        }

        if (IsInAir(player)) {
            if (!player.GetFormHandler().JustJumped) {
                if (!player.GetFormHandler().JustJumpedForAnimation) {

                    frame = swingAnimationStartFrame;
                    frameCounter = 0;

                    player.GetFormHandler().JustJumpedForAnimation = true;
                }
                else {
                    frameCounter += flightAnimationCounterSpeed * 1.5f;
                    float frequency = flightFrameFrequency;
                    while (frameCounter > frequency) {
                        frameCounter -= frequency;
                        frame++;
                        if (frame == swingAnimationEndFrame) {
                            player.GetFormHandler().JustJumped = true;
                        }
                    }
                }
            }
            else {
                if (player.velocity.Y < 0f) {
                    frameCounter += flightAnimationCounterSpeed;
                    float frequency = flightFrameFrequency;
                    while (frameCounter > frequency) {
                        frameCounter -= frequency;
                        frame++;
                    }

                    if (frame == flightAnimationEndFrame - 1) {
                        playFlapSound();
                    }
                    else {
                        playFlapSound(true);
                    }

                    bool end = frame > flightAnimationEndFrame;
                    if (frame < flightAnimationStartFrame || end) {
                        frame = flightAnimationStartFrame;
                    }
                }
                else if (player.velocity.Y > 0f) {
                    frame = flightAnimationEndFrame;
                }
            }
        }
        else if (player.velocity.X != 0f) {
            frameCounter += Math.Abs(player.velocity.X) * 1.5f;
            float frequency = walkingFrameFrequiency;
            while (frameCounter > frequency) {
                frameCounter -= frequency;
                frame++;
            }
            if (frame > runAnimationEndFrame) {
                frame = runAnimationStartFrame;
            }
        }
        else {
            frameCounter = 0f;
            frame = 0;
        }

        return false;
    }

    protected override void SafeSetMount(Player player, ref bool skipDust) {
        Vector2 center = player.Center + player.velocity;
        for (int i = 0; i < 24; i++) {
            Vector2 spawnPos = center + new Vector2(player.width, 0).RotatedBy(i * Math.PI * 2 / 24f * player.direction) - new Vector2(-6f, 4f);
            Vector2 direction = (center - spawnPos) * 0.5f;
            int dust = Dust.NewDust(spawnPos, 0, 0, MountData.spawnDust, direction.X, direction.Y);
            Main.dust[dust].velocity *= 0.95f;
            Main.dust[dust].fadeIn = 1.8f;
            Main.dust[dust].noGravity = true;
        }
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "PipistrelleScream1") { Pitch = -0.3f, PitchVariance = 0.1f, Volume = 0.8f }, player.Center);
        skipDust = true;

        player.GetFormHandler().ResetFlederStats();
    }

    protected override void SafeDismountMount(Player player, ref bool skipDust) {
        Vector2 center = player.Center + player.velocity;
        for (int i = 0; i < 24; i++) {
            Vector2 spawnPos = center - new Vector2(player.width, 0).RotatedBy(i * Math.PI * 2 / 24f * player.direction) - new Vector2(-6f, 4f);
            Vector2 direction = (center - spawnPos) * 0.5f;
            int dust = Dust.NewDust(spawnPos, 0, 0, MountData.spawnDust, direction.X, direction.Y);
            Main.dust[dust].velocity *= 0.95f;
            Main.dust[dust].fadeIn = 1.8f;
            Main.dust[dust].noGravity = true;
        }
        skipDust = true;

        player.GetFormHandler().ResetFlederStats();
    }
}