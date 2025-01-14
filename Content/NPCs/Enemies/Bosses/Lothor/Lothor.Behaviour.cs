﻿using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Enemies.Lothor;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class Lothor : ModNPC {
    private const double FLIGHTFRAMERATE = 6.0;
    private const int SPITCOUNT = 4;

    private sealed class TossedPlayerStateHandler : ModPlayer {
        public float Tossed;

        public override void PostUpdateMiscEffects() {
            if (Tossed <= 0f) {
                return;
            }

            Player.velocity.Y -= Tossed;
            Tossed = 0f;
        }
    }

    private sealed class ScreamProjectileHandler : GlobalProjectile {
        private Vector2 _storedVelocity;
        private float _storedRotation;
        private float _effectTimer;
        private bool _wasAffected;

        public bool ShouldEffectBeActive => NPC.CountNPCS(ModContent.NPCType<Lothor>()) > 0;

        public void ApplyEffect() {
            _wasAffected = true;
            _effectTimer = 10f;
        }

        public override bool InstancePerEntity => true;

        public override bool PreAI(Projectile projectile) {
            if (ShouldEffectBeActive) {
                if (!_wasAffected) {
                    _storedVelocity = projectile.velocity;
                    _storedRotation = projectile.rotation;
                }
            }

            return base.PreAI(projectile);
        }

        public override void PostAI(Projectile projectile) {
            if (!ShouldEffectBeActive) {
                return;
            }

            if (!_wasAffected) {
                return;
            }

            projectile.velocity = _storedVelocity * 0.001f;
            projectile.rotation = _storedRotation;
            if (_effectTimer > 0f) {
                _effectTimer -= 1f;
            }
            else {
                _wasAffected = false;
                projectile.velocity = _storedVelocity;
            }
        }
    }

    private const float AIRDASHLENGTH = 200f;

    private enum LothorAIState : byte {
        Fall,
        Idle,
        Jump,
        Flight,
        AirDash,
        ClawsAttack,
        SpittingAttack,
        Scream,
        FlightAttackPreparation,
        ChargingWreath,
        WreathAttack
    }

    private LothorAIState _previousState;
    private float _dashStrength;
    private bool _spawned;
    private bool _stompSpawned;
    private float _canChangeDirectionTimer;
    private float _pulseStrength;
    private List<LothorAIState> _previousAttacks;
    private float _beforeAttackDelay, _beforeAttackTimerVisual, _beforeAttackTimerVisualMax;
    private bool _frameChosen;
    private int _tempDirection;
    private Vector2 _tempPosition, _wreathLookingPosition;
    private bool _applyFlightAttackAnimation, _flightAttackAnimationDone, _drawWreath;
    private int _spitCount;
    private bool _shouldWreathAttack;
    private float _wreathProgress;
    private bool _wreathCharged;
    private float _tempVelocity;
    private float _yOffsetProgressBeforeLanding;
    private int _attackTime;
    private float _distanceProgress, _distanceProgress2;
    private bool _shouldSpawnPipistrelles;
    private bool _isDead;

    public float LifeProgress => 1f - NPC.life / (float)NPC.lifeMax;

    private LothorAIState CurrentAIState { get => (LothorAIState)NPC.ai[3]; set => NPC.ai[3] = (byte)value; }
    private Player Target { get; set; }
    private List<LothorAIState> Attacks => [LothorAIState.ClawsAttack, LothorAIState.SpittingAttack, LothorAIState.Scream, LothorAIState.FlightAttackPreparation, LothorAIState.WreathAttack];
    private List<LothorAIState> FlightStates => [LothorAIState.Flight, LothorAIState.AirDash, LothorAIState.FlightAttackPreparation, LothorAIState.ChargingWreath, LothorAIState.WreathAttack];
    private List<LothorAIState> AirAttacks => [LothorAIState.AirDash, LothorAIState.FlightAttackPreparation];

    private ref float DashCount => ref NPC.ai[2];
    private ref float FallStrengthIfClose => ref NPC.localAI[0];
    private ref float DashTimer => ref NPC.ai[0];
    private ref float DashDelay => ref NPC.ai[1];
    private ref float NoCollisionTimer => ref NPC.localAI[2];
    private ref float AirDashTimer => ref NPC.localAI[2];
    private ref float StillInJumpBeforeFlightTimer => ref NPC.localAI[1];

    private ref float BeforeAttackTimer => ref NPC.localAI[1];

    private ref float ClawsTimer => ref NPC.ai[0];
    private ref float ClawsAttackTime => ref NPC.ai[1];
    private ref float SpittingTimer => ref NPC.ai[0];
    private ref float SpittingAttackTime => ref NPC.ai[1];
    private ref float FlightAttackTimer => ref NPC.ai[0];
    private ref float FlightAttackTimeMax => ref NPC.ai[1];
    private ref float WreathTimer => ref NPC.ai[0];
    private ref float WreathAttackTime => ref NPC.ai[1];
    private ref float ScreamTimer => ref NPC.ai[0];
    private ref float ScreamAttackTime => ref NPC.ai[1];

    private float PreparationProgress => Helper.EaseInOut3(DashTimer / DashDelay * 1.25f);

    private bool DoingFirstDash => DashCount == 0;
    private bool IsAboutToGoToChangeMainState => DashCount > GetJumpCountToEncourageFlightState();
    private bool BeforeDoingLastJump => DashCount > GetJumpCountToEncourageFlightState() - 1;

    private bool IsFlying => FlightStates.Contains(CurrentAIState);

    private bool IsAboutToLand => IsFlying && BeforeDoingLastJump;

    private bool ShouldDrawPulseEffect => !Attacks.Contains(CurrentAIState) || (Attacks.Contains(CurrentAIState) && _beforeAttackTimerVisual > 0f);

    private float MinDelayBeforeAttack => DashDelay * 0.2f;
    private float MinToChargeFlightAttack => FlightAttackTimeMax * 0.35f;
    private float MinDelayToStartScreaming => ScreamAttackTime * 0.35f;

    private bool JustDidAirDash => NPC.velocity.Length() > 4.5f && DashTimer < DashDelay * 0.15f;
    private bool IsDashing => _previousState != LothorAIState.WreathAttack && StillInJumpBeforeFlightTimer <= 0f && ((CurrentAIState == LothorAIState.Fall && _previousState != LothorAIState.Flight && Math.Abs(NPC.velocity.X) > 5f) ||
        (CurrentAIState == LothorAIState.AirDash && NPC.velocity.Length() > 3.5f) ||
        CurrentAIState == LothorAIState.WreathAttack ||
        (CurrentAIState == LothorAIState.FlightAttackPreparation && NPC.velocity.Length() > 10f) ||
        (CurrentAIState == LothorAIState.Flight && JustDidAirDash));

    private bool ShouldDrawWreath => _drawWreath;

    private int GetJumpCountToEncourageFlightState() => 3;

    public override void HitEffect(NPC.HitInfo hit) {
        if (NPC.life > 0) {
            return;
        }

        if (!_isDead) {
            NPC.life = 10;
            NPC.dontTakeDamage = NPC.immortal = true;

            _isDead = true;
        }
    }

    public override void AI() {
        Init();

        HandleActiveState();

        SetRotation();
        SetOthers();

        UpdateTrailInfo();
        UpdatePulseVisuals();
        UpdateWreath();
        UpdateGlowing();

        if (NPC.Opacity < 1f) {
            NPC.Opacity += 0.025f;
        }
        else {
            NPC.Opacity = 1f;
        }
    }

    private void UpdateWreath() {
        if (!_drawWreath) {
            _wreathProgress = 0f;
            return;
        }

        if (_wreathProgress < 1f) {
            _wreathProgress += 0.15f;
        }
        _wreathProgress = Math.Min(1f, _wreathProgress);
    }

    private void UpdatePulseVisuals() {
        if (Attacks.Contains(CurrentAIState)) {
            float delay = _beforeAttackTimerVisual;
            float num282 = _beforeAttackTimerVisualMax;
            float num283 = delay / num282;
            _pulseStrength = num283;

            if (_beforeAttackTimerVisual > 0f) {
                _beforeAttackTimerVisual--;
            }

            return;
        }

        int pulseCount = 3;
        float min = MinDelayBeforeAttack;
        if (DashTimer > min) {
            float delay = DashTimer - min;
            float num282 = (DashDelay - min) / (float)pulseCount;
            float num283 = delay % num282 / num282;
            _pulseStrength = num283;
        }
    }

    private void ChangeDirectionTo(int direction) {
        if (--_canChangeDirectionTimer > 0f) {
            return;
        }

        NPC.direction = direction;

        _canChangeDirectionTimer = 10f;
    }

    private void LookAtPlayer() => ChangeDirectionTo(-Math.Sign(NPC.Center.X - Target.Center.X));
    private void VelocityLook() => ChangeDirectionTo(NPC.velocity.X.GetDirection());

    private void Init() {
        if (_spawned) {
            return;
        }

        PlayRoarSound();

        CurrentAIState = LothorAIState.Jump;
        _previousState = default;

        NPC.Opacity = 0f;

        _previousAttacks = [];

        int trailLength = 10;
        if (trailLength != NPC.oldPos.Length) {
            Array.Resize(ref NPC.oldPos, trailLength);
            Array.Resize(ref NPC.oldRot, trailLength);
        }
        for (int j = 0; j < NPC.oldPos.Length; j++) {
            NPC.oldRot[j] = 0f;
            NPC.oldPos[j].X = 0f;
            NPC.oldPos[j].Y = 0f;
        }

        _spawned = true;
    }

    private void UpdateTrailInfo() {
        for (int num6 = NPC.oldPos.Length - 1; num6 > 0; num6--) {
            NPC.oldPos[num6] = NPC.oldPos[num6 - 1];
            NPC.oldRot[num6] = NPC.oldRot[num6 - 1];
        }

        NPC.oldPos[0] = NPC.position + NPC.netOffset;
        NPC.oldRot[0] = NPC.rotation;
    }

    private void SetRotation() {
        ref float rotation = ref NPC.localAI[3];
        if (!IsFlying) {
            rotation = 0f;
        }
        else {
            float xVelocity = NPC.velocity.X * 0.085f;
            float maxRotation = 0.3f;
            float to = Math.Clamp(xVelocity, -maxRotation, maxRotation);
            if (CurrentAIState == LothorAIState.Flight && StillInJumpBeforeFlightTimer <= 0f) {
                rotation = to;
            }
            else if (StillInJumpBeforeFlightTimer > 0f || AirAttacks.Contains(CurrentAIState)) {
                rotation = Utils.AngleLerp(rotation, to, CurrentAIState == LothorAIState.FlightAttackPreparation ? 0.3f : Math.Max(0.1f, Math.Abs(to) * 0.4f));
            }
            else {
                rotation = to;
            }
        }
        NPC.rotation = rotation;
    }

    private void SetOthers() {
        if (IsFlying) {
            NPC.noTileCollide = true;
            NPC.noGravity = true;

            return;
        }

        NPC.noGravity = false;
    }

    public override void OnSpawn(IEntitySource source) {
        CurrentAIState = LothorAIState.Idle;
        DashDelay = GetAttackDelay();
        NPC.TargetClosest();
    }

    private void ApplyFlyingAnimation() {
        _currentColumn = SpriteSheetColumn.Flight;
        if (NPC.frameCounter <= 0.0) {
            CurrentFrame = 20;
            //NPC.frameCounter += NPC.velocity.Length() / 4f;
            NPC.frameCounter += 1.0;
        }
        else {
            double flightFrameRate = FLIGHTFRAMERATE;
            if (++NPC.frameCounter > 1.0 + flightFrameRate) {
                CurrentFrame++;
                if (CurrentFrame == 22) {
                    SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "Wingslap") { Volume = 1.5f }, NPC.Center);
                }
                NPC.frameCounter = 1.0;
            }
        }
        if (CurrentFrame > 25) {
            CurrentFrame = 20;
        }
    }

    partial void HandleAnimations() {
        NPC.spriteDirection = -NPC.direction;
        _drawOffset.Y = -6f;
        switch (CurrentAIState) {
            case LothorAIState.Fall:
                _currentColumn = SpriteSheetColumn.Stand;
                CurrentFrame = (byte)(NPC.velocity.Y != 0f ? 19 : 0);
                break;
            case LothorAIState.Idle:
                _currentColumn = SpriteSheetColumn.Stand;
                CurrentFrame = (byte)MathHelper.Lerp(16, 19, PreparationProgress);
                if (CurrentFrame > 18) {
                    CurrentFrame = 18;
                }
                break;
            case LothorAIState.Jump:
                _currentColumn = SpriteSheetColumn.Stand;
                CurrentFrame = 19;
                break;
            case LothorAIState.Flight:
            case LothorAIState.AirDash:
                if (StillInJumpBeforeFlightTimer <= 0f || _previousState == LothorAIState.WreathAttack) {
                    _drawOffset.Y = 16f;
                    ApplyFlyingAnimation();
                }
                break;
            case LothorAIState.ClawsAttack:
                if (BeforeAttackTimer <= 0f) {
                    _currentColumn = SpriteSheetColumn.Stand;
                    byte minFrame = 2;
                    byte maxFrames = 8;
                    float min = ClawsAttackTime * 0.15f;
                    bool flag = ClawsTimer < min;
                    if (CurrentFrame < minFrame || CurrentFrame >= maxFrames || flag) {
                        CurrentFrame = minFrame;
                    }
                    if (!flag) {
                        CurrentFrame = (byte)MathHelper.Lerp(minFrame, maxFrames, (ClawsTimer - min) / (ClawsAttackTime - min));
                        if (CurrentFrame > maxFrames) {
                            CurrentFrame = maxFrames;
                        }
                    }
                }
                break;
            case LothorAIState.SpittingAttack:
                if (BeforeAttackTimer <= 0f) {
                    _currentColumn = SpriteSheetColumn.Stand;
                    byte minFrame = 9;
                    byte maxFrames = 16;
                    float min = SpittingAttackTime * 0.15f;
                    bool flag = SpittingTimer < min;
                    if (CurrentFrame < minFrame || CurrentFrame >= maxFrames || flag) {
                        CurrentFrame = minFrame;
                    }
                    if (!flag) {
                        CurrentFrame = (byte)MathHelper.Lerp(minFrame, maxFrames, (SpittingTimer - min) / (SpittingAttackTime - min));
                        if (CurrentFrame > maxFrames) {
                            CurrentFrame = maxFrames;
                        }
                    }
                }
                break;
            case LothorAIState.Scream:
                if (BeforeAttackTimer <= 0f) {
                    _currentColumn = SpriteSheetColumn.Stand;
                    byte minFrame = 9;
                    byte maxFrames = 12;
                    float min = ScreamAttackTime * 0.15f;
                    bool flag = ScreamTimer < min;
                    if (CurrentFrame < minFrame || CurrentFrame >= maxFrames || flag) {
                        CurrentFrame = minFrame;
                    }
                    if (!flag) {
                        if (ScreamTimer < MinDelayToStartScreaming * 0.75f) {
                            CurrentFrame = (byte)(minFrame + 1);
                        }
                        else if (ScreamTimer < MinDelayToStartScreaming) {
                            CurrentFrame = (byte)(minFrame + 2);
                        }
                        else {
                            CurrentFrame = (byte)(minFrame + 3);
                        }
                    }
                }
                break;
            case LothorAIState.FlightAttackPreparation:
                _drawOffset.Y = 16f;

                _currentColumn = SpriteSheetColumn.Flight;

                byte neededFrame = 23;
                if (_applyFlightAttackAnimation && !_flightAttackAnimationDone) {
                    if (NPC.frameCounter <= 0.0) {
                        CurrentFrame = neededFrame;
                    }
                    else {
                        double secondFrameRate = FLIGHTFRAMERATE;
                        if (NPC.frameCounter < secondFrameRate) {
                            CurrentFrame = 25;
                        }
                        else if (NPC.frameCounter < secondFrameRate + 12.0) {
                            CurrentFrame = 20;
                            void createDusts() {
                                for (int k = 0; k < 20; k++) {
                                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<RedLineDust>());
                                    dust.scale = 1.4f + Main.rand.NextFloatRange(0.2f);
                                    dust.velocity = (NPC.rotation + MathHelper.Pi).ToRotationVector2().RotatedBy(Main.rand.NextFloatDirection() * ((float)Math.PI / 12f)) * (0.5f + Main.rand.NextFloat() * 2.5f) * 15f;
                                    dust.velocity += new Vector2((1f + Main.rand.NextFloat()) * NPC.direction, 0f);
                                    dust.velocity.X *= -NPC.direction;
                                    dust.velocity *= 0.25f;
                                    dust.noLight = true;
                                    dust.noGravity = true;
                                }
                            }
                            if (_shouldWreathAttack) {
                                if (!_drawWreath) {
                                    SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "WreathSpawn"), NPC.Center);
                                    createDusts();
                                    _drawWreath = true;
                                }
                            }
                            else {
                                if (!_shouldSpawnPipistrelles) {
                                    SoundEngine.PlaySound(new(ResourceManager.NPCSounds + "LothorScream2"), NPC.Center);
                                    createDusts();
                                    _shouldSpawnPipistrelles = true;
                                }
                            }
                        }
                        else {
                            _flightAttackAnimationDone = true;
                            _applyFlightAttackAnimation = false;
                        }
                    }
                    NPC.frameCounter += 1.0;
                }
                else if (FlightAttackTimer != 0f && BeforeAttackTimer > 0f) {
                    if (_frameChosen) {
                        CurrentFrame = neededFrame;
                    }
                    else {
                        ApplyFlyingAnimation();
                        if (CurrentFrame == neededFrame) {
                            _frameChosen = true;
                        }
                    }
                }
                else {
                    ApplyFlyingAnimation();
                }
                break;
            case LothorAIState.ChargingWreath:
            case LothorAIState.WreathAttack:
                _drawOffset.Y = 16f;
                ApplyFlyingAnimation();
                break;
        }
    }

    private void Glow() {
        _glowMaskOpacity = 1.5f;
    }

    private void Glow2() {
        if (_glowMaskOpacity < 1.5f) {
            _glowMaskOpacity += 0.1f;
        }
    }

    private void UpdateGlowing() {
        if (_glowMaskOpacity > 0f) {
            _glowMaskOpacity -= TimeSystem.LogicDeltaTime;
        }

        float value = MathHelper.Clamp(Math.Max(_glowMaskOpacity, LifeProgress), 0f, 1f);
        Lighting.AddLight(NPC.Top + Vector2.UnitY * NPC.height * 0.1f, new Vector3(1f, 0.2f, 0.2f) * value * 0.75f);
    }

    private void HandleActiveState() {
        if (GetTargetPlayer(out Player target)) {
            Target = target;
            switch (CurrentAIState) {
                case LothorAIState.Fall:
                    FallState();
                    break;
                case LothorAIState.Idle:
                    IdleState();
                    break;
                case LothorAIState.Jump:
                    JumpState();
                    break;
                case LothorAIState.Flight:
                    FlightState();
                    break;
                case LothorAIState.AirDash:
                    AirDashState();
                    break;
                case LothorAIState.ClawsAttack:
                    ClawsAttack();
                    break;
                case LothorAIState.SpittingAttack:
                    SpittingAttack();
                    break;
                case LothorAIState.Scream:
                    HandleScream();
                    break;
                case LothorAIState.FlightAttackPreparation:
                    FlightAttackPreparation();
                    break;
                case LothorAIState.ChargingWreath:
                    ChargingWreath();
                    break;
                case LothorAIState.WreathAttack:
                    WreathAttack();
                    break;
            }
        }
    }

    private void HandleScream() {
        WhenIdle();

        NPC.knockBackResist = 0f;

        CurrentAIState = LothorAIState.Scream;

        if (BeforeAttackTimer <= 0f) {
            ScreamTimer += 1f;
        }
        else {
            BeforeAttackTimer--;
        }

        if (ScreamTimer >= MinDelayToStartScreaming && ++_attackTime > 8) {
            for (int i = 0; i < NPC.maxBuffs; i++) {
                if (NPC.buffType[i] != 0) {
                    NPC.DelBuff(i);
                }
            }

            Glow();

            _attackTime = 0;
            bool firstTime = _previousState != CurrentAIState;

            float maxDist = 800f;
            foreach (Player player in Main.ActivePlayers) {
                if (player.dead) {
                    continue;
                }
                float dist = NPC.Distance(player.Center);
                if (dist <= maxDist) {
                    Vector2 velocity = player.Center - NPC.Center;
                    velocity.Normalize();
                    float maxSpeed = 10f;
                    player.velocity += velocity * Math.Max((maxDist - dist) / 100f, maxSpeed) * 0.15f;
                    player.velocity *= 1f - Math.Abs(dist) * 0.0035f;
                }
            }

            foreach (NPC npc in Main.ActiveNPCs) {
                if (npc.whoAmI == NPC.whoAmI) {
                    continue;
                }

                float dist = NPC.Distance(npc.Center);
                if (dist <= maxDist) {
                    Vector2 velocity = npc.Center - NPC.Center;
                    velocity.Normalize();
                    npc.velocity += velocity * Math.Max((maxDist - dist) / 100f, 10f);
                }
            }

            foreach (Projectile projectile in Main.ActiveProjectiles) {
                if (projectile.friendly && NPC.Distance(projectile.Center) <= maxDist) {
                    projectile.GetGlobalProjectile<ScreamProjectileHandler>().ApplyEffect();
                    projectile.damage -= projectile.damage / 5;
                }
            }

            if (firstTime) {
                PlayRoarSound();
            }
            string tag = "Lothor Scream";
            float strength = firstTime ? 20f : 5f;
            PunchCameraModifier punchCameraModifier = new PunchCameraModifier(NPC.Center, MathHelper.TwoPi.ToRotationVector2(), strength, firstTime ? 10f : 12.5f, 25, 1000f, tag);
            Main.instance.CameraModifiers.Add(punchCameraModifier);
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                Vector2 center = new Vector2(NPC.Center.X + NPC.width / 2 * NPC.direction, NPC.position.Y + NPC.height / 4);
                ushort projType = (ushort)ModContent.ProjectileType<LothorScream>();
                int projectile = Projectile.NewProjectile(NPC.GetSource_FromAI(), center + new Vector2(-6f * NPC.direction, -6f), Vector2.Zero, projType, 0, 0f, Main.myPlayer, firstTime ? 1f : 0f, NPC.target);
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile);
            }
            _previousState = CurrentAIState;
        }

        if (ScreamTimer >= ScreamAttackTime) {
            ScreamTimer = 0f;
            _previousState = CurrentAIState;

            BeforeAttackTimer = 0f;

            //ChooseAttack(LothorAIState.Scream);

            DashDelay = GetAttackDelay();
            CurrentAIState = LothorAIState.Idle;

            _previousAttacks.Add(_previousState);
        }
    }

    private void WreathAttack() {
        LookAtPlayer();
        if (WreathTimer < WreathAttackTime / 2f) {
            float dashStrength = 15f;
            _dashStrength = Helper.Approach(_dashStrength, 1f, 0.05f);
            if (NPC.velocity.Length() < dashStrength) {
                NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.DirectionTo(_tempPosition) * dashStrength, _dashStrength);
                NPC.velocity += NPC.DirectionTo(_tempPosition) * _dashStrength * dashStrength * 0.35f;
            }
            NPC.velocity += NPC.DirectionTo(_tempPosition) * _dashStrength * dashStrength * 0.25f;
            WreathTimer += WreathAttackTime * (0.075f - 0.035f * LifeProgress);
        }
        else {
            void updatePositionToMove(float x = 300f, float y = -100f) => _tempPosition = new Vector2(Target.Center.X + _tempDirection * x, Target.Center.Y + y);
            if (_tempDirection == 0) {
                _tempDirection = (Target.Center - NPC.Center).X.GetDirection();
                updatePositionToMove();
            }
            _distanceProgress = Vector2.Distance(NPC.Center, _tempPosition);
            if (_distanceProgress2 == -1f) {
                _distanceProgress2 = _distanceProgress;
            }
            void spawnSpike() {
                Glow();

                SoundEngine.PlaySound(SoundID.Item65 with { PitchVariance = 0.1f }, NPC.Center);
                int dustType = ModContent.DustType<RedLineDust>();
                Vector2 spawnPosition = NPC.Center + WreathOffset();
                for (int i = 0; i < 32; i++) {
                    int dust = Dust.NewDust(spawnPosition, 2, 2, dustType,
                                            (float)Math.Cos(MathHelper.Pi / 16f * i) * 20f,
                                            (float)Math.Sin(MathHelper.Pi / 16f * i) * 20f, 0, default, 1.3f + Main.rand.NextFloatRange(0.2f));
                    Main.dust[dust].noGravity = true;
                }

                for (int i = 0; i < 10; i++) {
                    int dust = Dust.NewDust(spawnPosition, 20, 20, dustType, (float)(NPC.velocity.X * 0.25), (float)(NPC.velocity.Y * 0.25));
                    if (Main.rand.NextBool(3)) {
                        Main.dust[dust].fadeIn = (float)(0.75 + (double)Main.rand.Next(-10, 11) * 0.01f);
                        Main.dust[dust].scale = (float)(0.25 + (double)Main.rand.Next(-10, 11) * 0.05f);
                    }
                    else {
                        Main.dust[dust].scale = (float)(1.2 + (double)Main.rand.Next(-10, 11) * 0.05f);
                    }
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.125f;
                }
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    Vector2 velocity = Helper.VelocityToPoint(NPC.Center, Target.Center, 2f);
                    int type = ModContent.ProjectileType<LothorSpike>();
                    int damage = NPC.damage;
                    float knockBack = 0f;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition.X, spawnPosition.Y, velocity.X, velocity.Y, type, damage, knockBack, Main.myPlayer);
                }
            }
            float speed = 8f + 2f * LifeProgress;
            Vector2 desiredVelocity = NPC.DirectionTo(_tempPosition) * speed;
            float acceleration = 0.2f + 0.5f * LifeProgress;
            NPC.SimpleFlyMovement(desiredVelocity, acceleration);
            float min = 10f;
            if (Vector2.Distance(_tempPosition, NPC.Center) < min) {
                _previousState = CurrentAIState;
                GoToFlightState(false, false);
                StillInJumpBeforeFlightTimer = GetAttackDelay() * 0.2f;
                _tempDirection = 0;
                _tempPosition = Vector2.Zero;
                _dashStrength = 0f;
                ResetExtraDrawInfo();
            }

            int attackRate = (int)(20 - 5 * LifeProgress);
            if (++_attackTime > attackRate) {
                _attackTime = 0;
                spawnSpike();
            }
        }

        if (WreathTimer < WreathAttackTime) {
            WreathTimer += 1f;
        }
    }

    private void SpawnPipistrelles() {
        int type = ModContent.NPCType<Pipistrelle>();
        if (_shouldSpawnPipistrelles && NPC.CountNPCS(type) < 1) {
            Vector2 positionToSpawn = NPC.Center + Vector2.UnitY * NPC.height / 2f;
            NPC.NewNPCDirect(NPC.GetSource_Death(), positionToSpawn, type, ai0: NPC.whoAmI, ai3: -1f);
            NPC.NewNPCDirect(NPC.GetSource_Death(), positionToSpawn, type, ai0: NPC.whoAmI, ai3: 1f);
        }
    }

    private void FlightAttackPreparation() {
        LookAtPlayer();

        _wreathLookingPosition = Target.Center;

        void updatePositionToMove(float x = 300f, float y = -50f) => _tempPosition = Target.Center + new Vector2(_tempDirection * x, y);
        float speed = 7f;
        if (_tempDirection == 0) {
            _tempDirection = (Target.Center - NPC.Center).X.GetDirection();
            updatePositionToMove();
        }
        if (NPC.velocity.Length() > speed) {
            float inertia = speed * 2f;
            NPC.velocity *= (float)Math.Pow(0.99, inertia * 2.0 / inertia);
        }
        Vector2 desiredVelocity = NPC.DirectionTo(_tempPosition) * speed;
        if (FlightAttackTimer == 0f) {
            float acceleration = 0.2f;
            NPC.SimpleFlyMovement(desiredVelocity, acceleration);
            float min = 5f;
            if (Vector2.Distance(_tempPosition, NPC.Center) < min) {
                FlightAttackTimer = 1f;
                SetTimeBeforeAttack(MinToChargeFlightAttack, true);
            }
        }
        else {
            Glow2();

            NPC.velocity *= 0.925f;
            if (!_shouldWreathAttack) {
                CreateCircleDusts();
            }
            if (_frameChosen) {
                bool flag = BeforeAttackTimer > 0f;
                if (flag) {
                    BeforeAttackTimer--;
                }
                SpawnPipistrelles();
                if (FlightAttackTimer >= MinToChargeFlightAttack && _flightAttackAnimationDone) {
                    if (_shouldWreathAttack) {
                        ChooseAttack(LothorAIState.WreathAttack);
                        Glow();
                    }
                    else {
                        Glow();
                        _previousState = LothorAIState.WreathAttack;
                        GoToFlightState(false, false);
                        StillInJumpBeforeFlightTimer = GetAttackDelay() * 0.1f;
                        _shouldSpawnPipistrelles = false;
                        AirDashTimer = GetExtraAirDashDelay(false);
                    }
                }
                else {
                    if (!flag) {
                        FlightAttackTimer++;
                        if (!_applyFlightAttackAnimation && !_flightAttackAnimationDone) {
                            NPC.frameCounter = 0.0;
                            _applyFlightAttackAnimation = true;
                        }
                    }
                }
            }
        }
    }

    private void CreateCircleDusts() {
        int type = ModContent.DustType<RedLineDust>();
        for (int i = 0; i < 4; i++) {
            if (Main.rand.NextBool(2)) {
                Vector2 spinningpoint = Vector2.UnitX.RotatedBy((double)Main.rand.NextFloat() * MathHelper.TwoPi);
                Vector2 center = NPC.Center + new Vector2(NPC.direction == 1 ? 3f : -3f, 0f) + NPC.velocity + spinningpoint * (NPC.width * NPC.scale);
                Vector2 rotationPoint = spinningpoint.RotatedBy(0.785) * NPC.direction;
                Vector2 position = center + rotationPoint * 5f;
                int dust = Dust.NewDust(position, 0, 0, type);
                Main.dust[dust].position = position;
                Main.dust[dust].noGravity = true;
                Main.dust[dust].fadeIn = Main.rand.NextFloat() * 1.2f * NPC.scale;
                Main.dust[dust].velocity = rotationPoint * NPC.scale * -2f;
                Main.dust[dust].scale = Main.rand.NextFloat() * Main.rand.NextFloat(1f, 1.25f) * 2f;
                Main.dust[dust].scale *= NPC.scale;
                Main.dust[dust].velocity += NPC.velocity * 1.25f;
                Main.dust[dust].position += Main.dust[dust].velocity * -5f;
            }
        }
    }

    private void SpittingAttack() {
        WhenIdle();

        NPC.knockBackResist = 0f;

        CurrentAIState = LothorAIState.SpittingAttack;

        int count = 6;
        int minFrame = 9;
        int maxFrames = 16;
        float min = SpittingAttackTime * 0.15f;
        bool flag = SpittingTimer < min;
        float current = SpittingTimer - min;
        float max = SpittingAttackTime - min;
        int usedFrame = (byte)MathHelper.Lerp(minFrame, maxFrames, current / max);
        if (usedFrame > maxFrames) {
            usedFrame = maxFrames;
        }
        usedFrame -= minFrame;
        float rate = max / count;
        bool flag2 = current < (rate * count - 1) - min && _spitCount > 0;
        bool flag3 = SpittingTimer < min;
        bool flag4 = true/*_spitCount > 1*/;
        if (flag4) {
            _tempPosition = Target.Center + Target.velocity;
        }
        if (BeforeAttackTimer <= 0f) {
            SpittingTimer += 1f;
        }
        else {
            BeforeAttackTimer--;
        }
        if (!flag) {
            if (current > 1f & (int)current % (int)rate == 0 && flag2) {
                Glow();

                _spitCount--;
                SoundEngine.PlaySound(SoundID.Item111, NPC.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    int damage = NPC.damage / 3;
                    float knockBack = 0.2f;
                    ushort type = (ushort)ModContent.ProjectileType<LothorAngleAttack>();
                    Vector2 position = _tempPosition;
                    float between = 1.75f;
                    float range = 22.5f;
                    float variation = (usedFrame * 8f) / range * Math.Abs((position - NPC.Center).Length()) / between * NPC.direction;
                    float lengthY = Math.Abs((position - NPC.Center).Length() / 7.5f);
                    float lengthX = -variation + Math.Abs((int)((position - NPC.Center).Length() / 3)) * NPC.direction;
                    float maxY = 135f;
                    if (lengthY > maxY) {
                        lengthY = maxY;
                    }
                    int whoAmI = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.One, type, damage, knockBack, Main.myPlayer, NPC.whoAmI,
                        position.X + lengthX,
                        position.Y - lengthY);

                    Main.projectile[whoAmI].As<LothorAngleAttack>().UsedBossFrame = usedFrame;
                }
            }
        }

        if (SpittingTimer >= SpittingAttackTime) {
            SpittingTimer = 0f;
            _previousState = CurrentAIState;

            BeforeAttackTimer = 0f;

            //ChooseAttack(LothorAIState.SpittingAttack);

            DashDelay = GetAttackDelay();
            CurrentAIState = LothorAIState.Idle;

            _previousAttacks.Add(_previousState);
        }
    }

    private void WhenIdle(bool flag = true) {
        NPC.noTileCollide = false;

        FallStrengthIfClose = 0f;

        if (flag) {
            LookAtPlayer();
        }
        NPC.velocity.X *= 0.85f + Math.Min(0.05f, Math.Abs(NPC.velocity.X) * 0.025f);
    }

    private void ClawsAttack() {
        WhenIdle();

        CurrentAIState = LothorAIState.ClawsAttack;

        NPC.knockBackResist = 0f;
        if (ClawsTimer == 1f) {
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                ushort projType = (ushort)ModContent.ProjectileType<LothorClawsSlash>();
                int projectile = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, projType, NPC.damage / 3, 2f, Main.myPlayer, NPC.whoAmI, ClawsAttackTime * 0.6f);
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile);
            }
            string name = "LothorSwipe";
            if (LifeProgress > 0.66f) {
                name = "LothorSwipe3";
            }
            else if (LifeProgress > 0.33f) {
                name = "LothorSwipe2";
            }
            SoundStyle swipeSound = new(ResourceManager.NPCSounds + name) { PitchVariance = 0.1f };
            SoundEngine.PlaySound(swipeSound, NPC.Center);
        }
        if (ClawsTimer < ClawsAttackTime / 2f) {
            Glow2();
        }
        if (BeforeAttackTimer <= 0f) {
            ClawsTimer += 1f;
        }
        else {
            BeforeAttackTimer--;
            //LookAtPlayer();
        }
        if (ClawsTimer >= ClawsAttackTime) {
            ClawsTimer = 0f;
            _previousState = CurrentAIState;
            //ClawsAttackTime = GetClawsAttackDelay();

            DashDelay = GetAttackDelay();
            CurrentAIState = LothorAIState.Idle;

            BeforeAttackTimer = 0f;

            _previousAttacks.Add(_previousState);
        }
    }

    private void AirDashState() {
        NPC.knockBackResist = 0f;
        float dashStrength = 15f;
        _dashStrength = Helper.Approach(_dashStrength, 1f, 0.05f);
        if (_dashStrength < 0.25f && NPC.Distance(Target.Center) > 50f) {
            _tempPosition = GetPositionBehindTarget();
        }
        if (NPC.velocity.Length() < dashStrength) {
            NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.DirectionTo(_tempPosition) * dashStrength, _dashStrength);
            NPC.velocity += NPC.DirectionTo(_tempPosition) * _dashStrength * dashStrength * 0.35f;
        }
        AirDashTimer++;
        NPC.velocity += NPC.DirectionTo(_tempPosition) * _dashStrength * dashStrength * 0.25f;
        float distance = Vector2.Distance(NPC.Center, _tempPosition);
        float minDistance = AIRDASHLENGTH;
        if (AirDashTimer > 10f) {
            if (distance < minDistance || (Vector2.Distance(NPC.Center, Target.Center) > minDistance * 2f && NPC.velocity.Length() > dashStrength * 0.75f) || NPC.velocity.Length() > dashStrength * 1.25f) {
                _dashStrength = 0f;
                if (Main.rand.NextChance(DashCount / (GetJumpCountToEncourageFlightState() + 0))) {
                    _shouldWreathAttack = !_shouldWreathAttack;
                    ChooseAttack(LothorAIState.FlightAttackPreparation);

                    return;
                }
                
                GoToFlightState(false, false);
                _yOffsetProgressBeforeLanding = 0f;
            }
        }

        LookAtPlayer();
    }

    private void PrepareAirDash(bool shouldReset = true) {
        if (JustDidAirDash) {
            NPC.knockBackResist = 0f;
        }
        else {
            SetKnockBackResist();
        }
        DashTimer += 1f;
        if (DashTimer >= DashDelay && shouldReset) {
            _yOffsetProgressBeforeLanding = 0f;
            ResetDashVariables();
            CurrentAIState = LothorAIState.AirDash;

            Glow();

            AirDashTimer = 0f;

            PlayRoarSound();

            DashCount++;
        }
    }

    private Vector2 GetPositionBehindTarget() {
        Vector2 playerCenter = Target.Center;
        return playerCenter + GetVelocityDirectionForFlightState();
    }

    private Vector2 GetBetweenForFlightState() {
        Vector2 npcCenter = NPC.Center;
        Vector2 playerCenter = Target.Center;
        if (_yOffsetProgressBeforeLanding < 1f) {
            _yOffsetProgressBeforeLanding += 0.01f;
        }
        if (IsAboutToLand && CurrentAIState == LothorAIState.Flight) {
            playerCenter = Target.Center - Vector2.UnitY * (AIRDASHLENGTH - AIRDASHLENGTH * 0.5f * _yOffsetProgressBeforeLanding);
        }
        Vector2 dif = playerCenter - npcCenter;
        return dif;
    }

    private Vector2 GetVelocityDirectionForFlightState(float length = -1f) {
        Vector2 dif = GetBetweenForFlightState();
        Vector2 dif2 = dif.SafeNormalize(Vector2.One) * (length != -1f ? length : AIRDASHLENGTH);
        if (IsAboutToLand && CurrentAIState == LothorAIState.Flight) {
            dif2 = Vector2.Zero;
        }
        return dif2;
    }

    private void GoToIdleState() {
        AirDashTimer = 0f;
        DashCount = 0;
        _previousState = CurrentAIState;
        CurrentAIState = LothorAIState.Fall;
        ResetDashVariables();
        NPC.TargetClosest();
        DashDelay = GetAttackDelay();
        _yOffsetProgressBeforeLanding = 0f;
    }

    private void ChargingWreath() {
        LookAtPlayer();

        if (StillInJumpBeforeFlightTimer > 0f) {
            StillInJumpBeforeFlightTimer--;
            return;
        }

        Vector2 dif = GetBetweenForFlightState();
        if (WreathTimer >= WreathAttackTime) {
            ChooseAttack(LothorAIState.WreathAttack);
            Glow();
            return;
        }
        else {
            WreathTimer += 1f;
        }

        Vector2 dif2 = GetVelocityDirectionForFlightState(AIRDASHLENGTH * 0.75f);
        Vector2 velocity = dif - dif2;
        float speed = 15f;
        velocity = Vector2.Normalize(velocity) * speed;
        float inertia = 30f;
        float absDistance = dif.Length();
        if (NPC.Bottom.Y > Target.Top.Y) {
            NPC.velocity.Y -= 0.1f;
        }
        if (Math.Abs(NPC.Center.X - Target.Center.X) < AIRDASHLENGTH) {
            NPC.velocity.X += 0.1f * -NPC.direction;
        }
        float edge = 30f * Utils.Remap(NPC.velocity.Length(), 0f, 10f, 1f, 2f);
        if (absDistance > AIRDASHLENGTH - edge && absDistance < AIRDASHLENGTH + edge && absDistance != AIRDASHLENGTH) {
            NPC.velocity *= (float)Math.Pow(0.97, inertia * 2.0 / inertia);
        }
        else {
            NPC.velocity.X = (NPC.velocity.X * (inertia - 1f) + velocity.X) / (float)inertia;
            NPC.velocity.Y = (NPC.velocity.Y * (inertia - 1f) + velocity.Y) / (float)inertia;
        }
    }

    private void FlightState() {
        if (StillInJumpBeforeFlightTimer > 0f) {
            StillInJumpBeforeFlightTimer--;
            if (_previousState == LothorAIState.WreathAttack) {
                NPC.velocity *= 0.925f;
            }
            AirDashTimer++;
            LookAtPlayer();
            return;
        }
        Vector2 dif = GetBetweenForFlightState();
        Vector2 dif2 = GetVelocityDirectionForFlightState();
        Vector2 velocity = dif - dif2;
        float speed = 10f;
        bool flag5 = IsAboutToLand;
        velocity = Vector2.Normalize(velocity) * speed;
        float inertia = 30f;
        float absDistance = dif.Length();
        float edge = 50f * Utils.Remap(NPC.velocity.Length(), 0f, 10f, 1f, 2f);
        bool flag = _previousState == LothorAIState.AirDash && !JustDidAirDash;
        if (flag) {
            LookAtPlayer();
        }
        if (NPC.Bottom.Y > Target.Top.Y) {
            NPC.velocity.Y -= 0.1f;
        }
        bool flag2 = NPC.velocity.Length() < 5f && BeforeDoingLastJump;
        bool flag3 = Collision.SolidCollision(NPC.position - Vector2.One * 4, NPC.width + 2, NPC.height + 2);
        bool flag4 = DashTimer > DashDelay;
        if (StillInJumpBeforeFlightTimer <= 0f && ++AirDashTimer >= 0f) {
            if (flag5) {
                DashTimer += 1f;
            }
            if (!flag5) {
                PrepareAirDash(!flag2);
            }
            else if (flag4) {
                GoToIdleState();
            }
        }
        bool flag6 = flag5 && absDistance < AIRDASHLENGTH / 6f;
        if (flag5 && flag4 && absDistance < AIRDASHLENGTH / 2f) {
            GoToIdleState();
        }
        if (flag6 || (!flag5 && absDistance > AIRDASHLENGTH - edge && absDistance < AIRDASHLENGTH + edge && absDistance != AIRDASHLENGTH)) {
            NPC.velocity *= (float)Math.Pow(0.97, inertia * 2.0 / inertia);
            if (!flag) {
                LookAtPlayer();
            }
            if (flag4 && flag2 && !flag3) {
                GoToIdleState();
            }
        }
        else {
            if (!flag) {
                if (NPC.velocity.Length() > 7.5f || JustDidAirDash) {
                    VelocityLook();
                }
                else {
                    LookAtPlayer();
                }
            }
            NPC.velocity.X = (NPC.velocity.X * (inertia - 1f) + velocity.X) / (float)inertia;
            NPC.velocity.Y = (NPC.velocity.Y * (inertia - 1f) + velocity.Y) / (float)inertia;
        }
    }

    private void JumpState() {
        NPC.knockBackResist = 0f;
        NPC.noTileCollide = true;
        void doJump() {
            float dashStrength = GetDashStrength();
            float yStrength = Math.Abs(NPC.Center.Y - Target.Center.Y) / 16f;
            float maxSpeedY = dashStrength * 6.5f;
            yStrength = MathHelper.Clamp(yStrength, -maxSpeedY, maxSpeedY); 
            if (IsAboutToGoToChangeMainState) {
                yStrength = maxSpeedY * 0.25f * Math.Sign(yStrength);
            }
            float jumpHeightY = yStrength;
            float jumpY = jumpHeightY * -0.5f;
            float jumpY2 = 8f;
            float dist = Vector2.Distance(NPC.Center, Target.Center);
            float minDist = 800f;
            if (!IsAboutToGoToChangeMainState && dist < minDist) {
                float value = MathHelper.Clamp(dist / minDist, 0.8f, 1f);
                jumpY2 *= value;
            }
            if (_previousState == default) {
                jumpY2 = 0f;
                jumpY = 0f;
                jumpHeightY = 0f;
            }
            if ((int)(NPC.Center.Y - Target.Center.Y) / 16 > -10) {
                NPC.velocity.Y = -jumpY2 + jumpY;
            }
            else {
                if (!Collision.CanHit(NPC.position, NPC.width, NPC.height, Target.position, Target.width, Target.height)) {
                    NPC.noTileCollide = true;
                    NPC.velocity.Y = -(jumpHeightY * -0.5f);
                }
                else {
                    NPC.velocity.Y = -jumpY2 + jumpY;
                }
            }
            float speed = 5f;
            float maxSpeedX = dashStrength * 6.5f;
            float xStrength = Math.Abs(NPC.Center.X - Target.Center.X) / 16f;
            float jumpHeightX = MathHelper.Clamp(xStrength, 0f, maxSpeedX);
            if (IsAboutToGoToChangeMainState) {
                jumpHeightX = maxSpeedX * 1f;
            }
            if (jumpHeightX < 10f) {
                NPC.velocity.X += speed * dashStrength * 0.5f * NPC.direction;
            }
            else {
                NPC.velocity.X = jumpHeightX * -0.5f * -(float)NPC.direction;
            }
            NoCollisionTimer = 10f;
        }
        doJump();
        CurrentAIState = LothorAIState.Fall;
        _previousState = CurrentAIState;
    }

    private float GetExtraAirDashDelay(bool flag = false) {
        return -GetAttackDelay(true) * 0.5f * (StillInJumpBeforeFlightTimer > 0f ? 0.5f : 1f);
    }

    private void GoToFlightState(bool flag = true, bool resetDashCount = true) {
        if (resetDashCount) {
            DashCount = 0;
        }
        CurrentAIState = LothorAIState.Flight;
        ResetDashVariables();
        if (flag) {
            StillInJumpBeforeFlightTimer = GetAttackDelay() * 0.1f;
        }
        else if (_previousState != LothorAIState.WreathAttack && _shouldWreathAttack) {
            _previousState = LothorAIState.AirDash;
        }
        AirDashTimer = GetExtraAirDashDelay(flag);
        NPC.TargetClosest();
        DashDelay = GetAttackDelay();
    }

    private void FallState() {
        if (NoCollisionTimer > 0f) {
            NoCollisionTimer--;
            NPC.noTileCollide = true;
        }
        NPC.knockBackResist = 0f;
        if (IsAboutToGoToChangeMainState) {
            LookAtPlayer();
        }
        else {
            if ((NPC.direction == 1 && NPC.position.X > Target.position.X || (NPC.direction != 1 && NPC.position.X < Target.position.X)) && NPC.Distance(Target.position) > 65f) {
                LookAtPlayer();
            }
        }
        float dashStrength = GetDashStrength();
        if (NPC.velocity.Y != 0f) {
            if (NPC.velocity.Y > 1f && NPC.velocity.Length() > 5f && IsAboutToGoToChangeMainState) {
                GoToFlightState();
                _shouldWreathAttack = !_shouldWreathAttack;
                if (Main.rand.NextBool()) {
                    _shouldWreathAttack = !_shouldWreathAttack;
                }
                _previousAttacks.Clear();
                return;
            }
            if (NoCollisionTimer <= 0f) {
                NPC.noTileCollide = NPC.velocity.Y < 0f && NPC.velocity.Length() > dashStrength && Math.Abs(NPC.velocity.X) > dashStrength / 2f;
            }
            if (NPC.velocity.X > 3f && NPC.Center.X > Target.Center.X || NPC.velocity.X < -3f && NPC.Center.X < Target.Center.X) {
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, 0f, dashStrength / 100f);
            }
            if (NPC.direction == 1 && NPC.velocity.X < 3f || NPC.direction == -1 && NPC.velocity.X > -3f) {
                if (NPC.direction == -1 && NPC.velocity.X < 0.1f || NPC.direction == 1 && NPC.velocity.X > -0.1f) {
                    NPC.velocity.X += 0.2f * NPC.direction;
                }
                else {
                    NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, 0f, dashStrength / 75f);
                }
            }
            Vector2 distance = Target.Center - NPC.Center;
            int maxTilesDistance = 15;
            bool closeRange = distance.Length() < 16 * maxTilesDistance;
            if (NPC.Center.Y < Target.Center.Y && closeRange) {
                float fallAcceleration = 0.01f;
                float fallVelocityY = 0.9f;
                if (FallStrengthIfClose < fallVelocityY) {
                    FallStrengthIfClose += fallAcceleration;
                }
                NPC.velocity.Y += FallStrengthIfClose;
            }
        }
        else {
            SpawnStomp();
            if (Math.Abs(NPC.velocity.Y) <= 0.25f) {
                FallStrengthIfClose = 0f;
                NPC.TargetClosest();
                CurrentAIState = LothorAIState.Idle;
                _stompSpawned = false;
                DashDelay = GetAttackDelay();
            }
        }
    }

    private float ResetDashVariables() => DashDelay = DashTimer = 0f;

    private float GetDashStrength() {
        return 4f;
    }

    public override bool? CanFallThroughPlatforms() {
        if (Target.dead) {
            return true;
        }
        else {
            return Target.position.Y > NPC.position.Y + NPC.height;
        }
    }

    private float GetAttackDelay(bool flag = false) {
        bool flag2 = BeforeDoingLastJump && (CurrentAIState == LothorAIState.Flight || CurrentAIState == LothorAIState.Idle);
        float delay = /* IsFlying ? */flag2 && !flag ? 125f : 100f/* : BeforeDoingLastJump ? 100f : 75f*/;
        //if (IsFlying && DoingFirstDash) {
        //    delay -= delay / 4;
        //}
        delay += delay * 0.333f;
        float scalableValue = MathHelper.Lerp(0.75f, 0.5f, LifeProgress);
        delay *= scalableValue;
        return delay;
    }

    private void TossAPlayer(Player target, float strength) {
        if (!target.active || target.dead) {
            return;
        }

        TossedPlayerStateHandler handler = target.GetModPlayer<TossedPlayerStateHandler>();
        handler.Tossed += strength;
    }

    private void SpawnStomp() {
        if (_stompSpawned) {
            return;
        }

        if (NPC.velocity.Y == 0f && !_stompSpawned) {
            _stompSpawned = true;
        }

        if (Main.netMode != NetmodeID.Server) {
            SoundEngine.PlaySound(SoundID.DD2_OgreGroundPound, NPC.Center);
        }

        float strength = 7.5f/*Main.expertMode ? 7.5f : 5f*/;
        int distInTiles = 20/*Main.expertMode ? 20 : 15*/;
        foreach (Player target in Main.player) {
            bool targetStands = target.velocity.Y == 0f || target.sliding;
            if (Vector2.Distance(target.Center, NPC.Center) < distInTiles * 16f && targetStands) {
                TossAPlayer(target, strength);
            }
        }

        string tag = "Lothor Stomp";
        PunchCameraModifier punchCameraModifier = new(NPC.Center, MathHelper.PiOver2.ToRotationVector2(), strength, 10f, 20, 1000f, tag);
        Main.instance.CameraModifiers.Add(punchCameraModifier);

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        float size = strength * 5f;
        int type = ModContent.ProjectileType<LothorStomp>();
        int damage = NPC.damage / 2;
        float knockback = 2f;
        int projectile = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + NPC.velocity, Vector2.Zero, type, damage, knockback, Main.myPlayer, 0f, size);
        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile);
    }

    private void IdleState() {
        if (!Attacks.Contains(_previousState)) {
            _previousState = LothorAIState.Idle;
        }

        WhenIdle();

        PrepareJump();
    }

    private int GetDoneAttackCount(LothorAIState attackState) => _previousAttacks.Count(x => x == attackState);

    private void SetKnockBackResist() => NPC.knockBackResist = MathHelper.Lerp(0.5f, 0f, PreparationProgress * (IsFlying ? 2f : 1f));

    private float GetClawsAttackDelay() => GetAttackDelay(true) * 0.6f;
    private float GetSpittingAttackDelay() => GetAttackDelay(true) * 0.8f;
    private float GetWreathAttackDelay() => GetAttackDelay(true) * 0.25f;
    private float GetScreamAttackDelay() => GetAttackDelay(true) * 0.8f;

    private void SetTimeBeforeAttack(float time, bool flag = false) {
        _beforeAttackDelay = time;
        _beforeAttackTimerVisual = _beforeAttackTimerVisualMax = _beforeAttackDelay * 5f;
        if (!flag) {
            BeforeAttackTimer = _beforeAttackDelay;
        }
        else {
            BeforeAttackTimer = _beforeAttackTimerVisualMax;
        }
    }

    private void ChooseAttack(LothorAIState lothorAIState) {
        NPC.TargetClosest(false);
        switch (lothorAIState) {
            case LothorAIState.ClawsAttack:
                ClawsAttackTime = GetClawsAttackDelay();
                CurrentAIState = LothorAIState.ClawsAttack;
                SetTimeBeforeAttack(ClawsAttackTime * 0.1f);
                ClawsTimer = 0f;
                break;
            case LothorAIState.SpittingAttack:
                SpittingAttackTime = GetSpittingAttackDelay();
                CurrentAIState = LothorAIState.SpittingAttack;
                SetTimeBeforeAttack(SpittingAttackTime * 0.1f);
                SpittingTimer = 0f;
                _spitCount = SPITCOUNT;
                _tempPosition = Vector2.Zero;
                break;
            case LothorAIState.Scream:
                _attackTime = 0;
                ScreamAttackTime = GetScreamAttackDelay();
                CurrentAIState = LothorAIState.Scream;
                SetTimeBeforeAttack(ScreamAttackTime * 0.1f);
                ScreamTimer = 0f;
                break;
            case LothorAIState.FlightAttackPreparation:
                ResetDashVariables();
                _tempPosition = Vector2.Zero;
                _tempDirection = 0;
                _previousState = CurrentAIState;
                CurrentAIState = LothorAIState.FlightAttackPreparation;
                FlightAttackTimeMax = GetWreathAttackDelay();
                ResetExtraDrawInfo();
                break;
            case LothorAIState.ChargingWreath:
                _tempPosition = Vector2.Zero;
                _tempDirection = 0;
                ResetDashVariables();
                CurrentAIState = LothorAIState.ChargingWreath;
                WreathAttackTime = GetAttackDelay();
                break;
            case LothorAIState.WreathAttack:
                _tempDirection = 0;
                ResetDashVariables();
                CurrentAIState = LothorAIState.WreathAttack;
                WreathAttackTime = GetAttackDelay();
                _dashStrength = 0f;
                _tempPosition = NPC.Center - new Vector2(0f * NPC.direction, 150f);
                PlayRoarSound();
                break;
        }
    }

    private void ResetExtraDrawInfo() {
        _applyFlightAttackAnimation = false;
        _flightAttackAnimationDone = false;
        _frameChosen = false;
        if (_drawWreath) {
            SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "WreathKill"), NPC.Center);
        }
        _drawWreath = false;
        _wreathLookingPosition = NPC.Center;
        _distanceProgress = _distanceProgress2 = -1f;
    }

    private void PrepareJump() {
        SetKnockBackResist();
        if (Math.Abs(NPC.velocity.Y) <= 0.25f) {
            DashTimer += 1f;
        }
        bool flag = Collision.CanHit(Target, NPC);
        float distance = Vector2.Distance(Target.Center, NPC.Center);
        bool flag2 = distance < 250f;
        bool flag4 = DashTimer > MinDelayBeforeAttack * 0.5f && DashTimer < MinDelayBeforeAttack * 1.25f;
        if (flag4) {
            if (_previousState != LothorAIState.Scream && GetDoneAttackCount(LothorAIState.SpittingAttack) < 2 && DashTimer % 5f == 0f && Main.rand.NextBool(5)) {
                ChooseAttack(LothorAIState.Scream);
                return;
            }
            if (_previousState != LothorAIState.ClawsAttack && _previousState != LothorAIState.SpittingAttack && _previousState != LothorAIState.Scream && flag2 &&
                flag) {
                ChooseAttack(LothorAIState.ClawsAttack);
                return;
            }
            bool flag3 = distance > 400f && distance < 900f;
            if (flag3 &&
                flag/* && GetDoneAttackCount(LothorAIState.SpittingAttack) < 2 */) {
                if (_previousState != LothorAIState.SpittingAttack) {
                    ChooseAttack(LothorAIState.SpittingAttack);
                }
                return;
            }
        }
        if (DashTimer >= DashDelay) {
            ResetDashVariables();
            CurrentAIState = LothorAIState.Jump;
            _previousState = CurrentAIState;

            Glow();

            PlayRoarSound();

            DashCount++;
        }
    }

    private void PlayRoarSound() {
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "LothorRoar") { Volume = 1f, PitchVariance = 0.1f }, NPC.Center);
    }

    private bool GetTargetPlayer(out Player player) {
        if (NPC.target < 0 || NPC.target == 255) {
            NPC.TargetClosest();
        }
        player = Main.player[NPC.target];
        if (player.dead || !player.active) {
            NPC.TargetClosest(false);
            player = Main.player[NPC.target];
            if (player.dead || !player.active) {
                return false;
            }
        }
        return true;
    }
}
