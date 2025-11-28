using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Common.Items;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature.Forms;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.Forms;

sealed class HallowedGryphon : BaseForm {
    private static byte FRAMECOUNT => 19;
    private static float MOVESPEEDBOOSTMODIFIER => 3f;
    private static float LOOPATTACKSIZEMODIFIER => 0.7f;

    protected override Color GlowColor(Player player, Color drawColor, float progress) => WreathHandler.GetArmorGlowColor1(player, drawColor, progress);

    public override ushort SetHitboxWidth(Player player) => (ushort)(Player.defaultWidth * 2.5f);
    public override ushort SetHitboxHeight(Player player) => (ushort)(Player.defaultHeight * 1f);

    public override Vector2 SetWreathOffset(Player player) => new(0f, 0f);
    public override Vector2 SetWreathOffset2(Player player) => new(0f, 0f);

    //protected override _color LightingColor => _color.Yellow;

    protected override void SafeSetDefaults() {
        MountData.totalFrames = FRAMECOUNT;
        MountData.fallDamage = 0f;
        MountData.flightTimeMax = 125;
       
        MountData.yOffset = -3;
        MountData.playerHeadOffset = -16;
    }

    protected override void SafePostUpdate(Player player) {
        MountData.yOffset = -7;
        MountData.playerHeadOffset = -16;

        player.GetFormHandler().UsePlayerSpeed = true;
        bool flag = player.mount.FlyTime > 0;
        if (IsInAir(player)) {
            bool flag2 = !flag && player.controlJump;
            player.accRunSpeed *= 3f;
            player.runAcceleration *= 1.5f;
            if (flag2) {
                player.accRunSpeed *= 0.9f;
                player.runAcceleration *= 0.9f;
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
        if (player.controlJump && !player.controlDown) {
            player.velocity.Y = Math.Min(5f, player.velocity.Y);
        }

        player.fullRotation = 0f;
        player.fullRotationOrigin = player.getRect().Centered() + Vector2.UnitY * 4f;

        HandleLoopAttack(player);

        BaseFormHandler formHandler = player.GetFormHandler();
        if (!IsInAir(player)) {
            formHandler.JustJumped = false;
            formHandler.JustJumpedForAnimation = false;
        }

        if (!player.GetFormHandler().IncreasedMoveSpeed) {
            return;
        }

        BaseFormDataStorage.ChangeAttackCharge1(player, 1.5f, false);

        float progress = (MOVESPEEDBOOSTMODIFIER - 1f) * GetMoveSpeedFactor(player);
        player.accRunSpeed *= 1f + progress;
        Player.jumpSpeed *= 1f + progress;
        player.runAcceleration *= 1f + progress;
        player.maxRunSpeed *= 1f + progress;
    }

    public static float GetMoveSpeedFactor(Player player) {
        float moveSpeedFactor = player.GetFormHandler().MoveSpeedBuffTime;
        return Utils.GetLerpValue(0f, BaseFormHandler.MOVESPEEDBUFFTIMEINTICKS * 0.05f, moveSpeedFactor, true) * Utils.GetLerpValue(BaseFormHandler.MOVESPEEDBUFFTIMEINTICKS, BaseFormHandler.MOVESPEEDBUFFTIMEINTICKS * 0.95f, moveSpeedFactor, true);
    }

    private int GetLoopAttackTime(Player player) {
        bool increasedMoveSpeed = player.GetFormHandler().IncreasedMoveSpeed;
        int num15 = 185;
        if (increasedMoveSpeed) {
            num15 = (int)(num15 * (1f - ((MOVESPEEDBOOSTMODIFIER - 1f) / 3.425f * GetMoveSpeedFactor(player))));
        }
        num15 = (int)(num15 * LOOPATTACKSIZEMODIFIER);
        return num15;
    }

    private void HandleLoopAttack(Player player) {
        var handler = player.GetFormHandler();
        ref Vector2 velocity = ref player.velocity;
        ref float rotation = ref player.fullRotation;
        ref Vector2 savedVelocity = ref handler.SavedVelocity;
        ref Vector2 position = ref player.position;
        int direction = player.direction;

        ref bool justStartedDoingLoopAttack = ref handler.JustStartedDoingLoopAttack;
        ref bool loopAttackIsDone = ref handler.LoopAttackIsDone;
        ref float attackFactor = ref handler.AttackFactor;
        ref float attackFactor2 = ref handler.AttackFactor2;
        ref byte attackCount = ref handler.AttackCount;

        bool canDoLoopAttack = handler.CanDoLoopAttack;

        float startLoopVelocity = 10f;

        bool increasedMoveSpeed = handler.IncreasedMoveSpeed;
        int num15 = GetLoopAttackTime(player);
        int attackPerCycle = increasedMoveSpeed ? 15 : 10;
        float num19 = (float)Math.PI * 2f / (float)(num15 / 2);
        int perAttack = (int)(num15 / (float)attackPerCycle);

        bool isAttacking = player.HoldingLMB(true);
        if (isAttacking && canDoLoopAttack) {
            if (!justStartedDoingLoopAttack) {
                justStartedDoingLoopAttack = true;
                savedVelocity = Vector2.UnitX * player.direction * startLoopVelocity;
                velocity *= 0f;
                handler.CanDoLoopAttack = false;
                attackFactor2 = perAttack;
                SoundEngine.PlaySound(SoundID.Item77 with { Pitch = 1.2f, Volume = 0.8f }, player.Center);
                SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "GryphonRoar2") with { Pitch = 0.5f, Volume = 1.4f }, player.Center);
            }
        }
        if (!justStartedDoingLoopAttack) {
            return;
        }
        if (loopAttackIsDone) {
            if (handler.JustStartedDoingLoopAttack) {
                velocity = savedVelocity.SafeNormalize() * startLoopVelocity;
                handler.JustStartedDoingLoopAttack = false;
                attackCount = 0;
                attackFactor2 = 0f;

                player.shimmering = false;
            }
            return;
        }

        player.gravity *= 0f;

        player.shimmering = true;
        player.shimmerTransparency = 0f;

        player.itemTime = player.itemAnimation = 1;

        player.controlJump = false;

        WreathHandler.GetWreathStats(player).LockWreathPosition = true;

        float desiredRotation = savedVelocity.ToRotation() - MathHelper.PiOver2;
        if (direction > 0) {
            desiredRotation += MathHelper.PiOver2;
        }
        else {
            desiredRotation -= MathHelper.PiOver2;
        }
        float moveSpeedFactor = player.GetFormHandler().MoveSpeedBuffTime;
        rotation = desiredRotation;
        savedVelocity = savedVelocity.RotatedBy((0f - num19) * (float)direction);
        if (attackFactor2++ > perAttack) {
            BaseFormDataStorage.ChangeAttackCharge1(player, 1.25f, false);
            int baseDamage = (int)player.GetTotalDamage(DruidClass.Nature).ApplyTo(30);
            float baseKnockback = player.GetTotalKnockback(DruidClass.Nature).ApplyTo(3f);
            if (player.IsLocal()) {
                ProjectileUtils.SpawnPlayerOwnedProjectile<HallowedFeather>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_FromThis()) {
                    Damage = baseDamage,
                    KnockBack = baseKnockback,
                    Velocity = player.Center.DirectionTo(player.GetWorldMousePosition())
                });
            }
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.6f, PitchVariance = 0.15f, Volume = 0.3f, MaxInstances = 5 }, player.Center);
            attackFactor2 = 0f;
        }
        if (attackFactor++ > num15 / 2 - 2) {
            SoundEngine.PlaySound(SoundID.Item77 with { Pitch = 1.1f }, player.Center);
            bool doneEnough = attackCount >= 2;

            if (!isAttacking || doneEnough) {
                loopAttackIsDone = true;
                if (doneEnough) {
                    handler.CanDoLoopAttackTimer = 180;
                    attackFactor = 0f;
                    return;
                }
            }
            attackFactor = 0f;
            attackCount++;
            if (!isAttacking) {
                handler.CanDoLoopAttackTimer = (ushort)(60 * attackCount);
            }
        }
        position += savedVelocity * (increasedMoveSpeed ? 1f + (MOVESPEEDBOOSTMODIFIER - 1f) * 0.666f * GetMoveSpeedFactor(player) : 1f);
        velocity *= 0f;
    }

    protected override bool SafeUpdateFrame(Player player, ref float frameCounter, ref int frame) {
        var handler = player.GetFormHandler();

        float walkingFrameFrequiency = 14f;
        int minMovingFrame = 1;
        int maxMovingFrame = minMovingFrame + 8;

        byte swingAnimationStartFrame = (byte)(maxMovingFrame + 1),
             swingAnimationEndFrame = (byte)(swingAnimationStartFrame + 2);

        byte flightAnimationStartFrame = (byte)(swingAnimationEndFrame + 1),
             flightAnimationEndFrame = (byte)(flightAnimationStartFrame + 5);

        byte slowFallFrame1 = (byte)(flightAnimationEndFrame - 2),
             slowFallFrame2 = (byte)(flightAnimationEndFrame - 3);

        float startSwingAnimationFrequency = 10f;
        float flightFrameFrequency = 14f;

        float flightAnimationCounterSpeed = handler.IncreasedMoveSpeed ? 3f : 2f;

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

        BaseFormHandler formHandler = player.GetFormHandler();

        if (handler.IsInLoopAttack) {
            var attackTime = GetLoopAttackTime(player);
            var attackFactor = handler.AttackFactor;
            bool slowFall2 = false;
            //if (!(attackFactor < attackTime * 0.2f || attackFactor > attackTime * 0.4f)) {
            //    if (frame == slowFallFrame2) {
            //        slowFall2 = true;
            //    }
            //}
            if (!slowFall2) {
                frameCounter += flightAnimationCounterSpeed;
                float frequency = flightFrameFrequency;
                while (frameCounter > frequency) {
                    frameCounter -= frequency;
                    frame++;
                    if (frame == flightAnimationEndFrame) {
                        playFlapSound();
                    }
                    else {
                        playFlapSound(true);
                    }
                }
                if (frame < flightAnimationStartFrame || frame > flightAnimationEndFrame) {
                    frame = flightAnimationStartFrame;
                }
            }
        }
        else if (IsInAir(player)) {
            if (!formHandler.JustJumped) {
                if (!formHandler.JustJumpedForAnimation) {
                    formHandler.JustJumpedForAnimation = true;
                    frame = swingAnimationStartFrame;
                    frameCounter = 0;
                }
                else {
                    frameCounter += flightAnimationCounterSpeed;
                    float frequency = startSwingAnimationFrequency;
                    while (frameCounter > frequency) {
                        frameCounter -= frequency;
                        frame++;
                        if (frame == swingAnimationEndFrame) {
                            formHandler.JustJumped = true;
                        }
                    }
                }
            }
            else {
                bool slowFall1 = false,
                     slowFall2 = false;
                if (player.velocity.Y > 2.5f) {
                    if (player.controlJump) {
                        if (frame == slowFallFrame1) {
                            slowFall1 = true;
                        }
                    }
                    else {
                        if (frame == slowFallFrame2) {
                            slowFall1 = true;
                        }
                    }
                }
                if (!slowFall1 && !slowFall2) {
                    float speedY = Math.Abs(player.velocity.Y);
                    frameCounter += Utils.Clamp(speedY, 3f, 5f) * (player.controlJump && player.velocity.Y > 0f ? 1f : 0.5f);
                    float frequency = flightFrameFrequency;
                    while (frameCounter > frequency) {
                        frameCounter -= frequency;
                        frame++;
                        if (frame == flightAnimationEndFrame) {
                            playFlapSound();
                        }
                        else {
                            playFlapSound(true);
                        }
                    }
                }
                if (frame < flightAnimationStartFrame || frame > flightAnimationEndFrame) {
                    frame = flightAnimationStartFrame;
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
		player.GetFormHandler().ResetGryphonStats();
        Vector2 center = player.Center + player.velocity;
        for (int i = 0; i < 20; i++) {
            Vector2 spawnPos = center + new Vector2(40, 0).RotatedBy(i * Math.PI * 2 / 20f);
            Vector2 direction = (center - spawnPos) * 0.5f * -player.direction;
            int dust = Dust.NewDust(spawnPos, 0, 0, DustID.TintableDustLighted, direction.X, direction.Y, newColor: Color.Yellow);
            Main.dust[dust].velocity *= 0.2f + Main.rand.NextFloatRange(0.15f);
            Main.dust[dust].velocity = new Vector2(-Main.dust[dust].velocity.Y, Main.dust[dust].velocity.X * 2f);
            Main.dust[dust].velocity.Y *= Main.rand.NextFloat(0.15f, 0.5f);
            Main.dust[dust].fadeIn = 1.8f - Main.rand.NextFloat(0.4f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].scale *= 0.75f + Main.rand.NextFloat(0.25f);
        }
        skipDust = true;
    }

    protected override void SafeDismountMount(Player player, ref bool skipDust) {
		player.GetFormHandler().ResetGryphonStats();
        Vector2 center = player.Center + player.velocity;
        for (int i = 0; i < 15; i++) {
            Vector2 spawnPos = center + new Vector2(25, 0).RotatedBy(i * Math.PI * 2 / 15f) - new Vector2(4f, 0f);
            Vector2 direction = (center - spawnPos) * 0.5f * -player.direction;
            int dust = Dust.NewDust(spawnPos, 0, 0, DustID.TintableDustLighted, direction.X, direction.Y, newColor: Color.Yellow);
            Main.dust[dust].velocity *= 0.3f + Main.rand.NextFloatRange(0.15f);
            Main.dust[dust].velocity = new Vector2(-Main.dust[dust].velocity.Y, Main.dust[dust].velocity.X * 2f) * -1f;
            Main.dust[dust].velocity.Y *= Main.rand.NextFloat(0.15f, 0.5f);
            Main.dust[dust].fadeIn = 1.8f - Main.rand.NextFloat(0.4f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].scale *= 0.75f + Main.rand.NextFloat(0.25f);
        }
        skipDust = true;
    }
}
