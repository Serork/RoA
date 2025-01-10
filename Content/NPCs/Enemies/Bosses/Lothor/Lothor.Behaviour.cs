using Microsoft.Xna.Framework;

using RoA.Common.WorldEvents;
using RoA.Content.Projectiles.Enemies.Lothor;
using RoA.Core;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class Lothor : ModNPC {
    private sealed class TossedStateHandler : ModPlayer {
        public float Tossed;

        public override void PostUpdateMiscEffects() {
            if (Tossed <= 0f) {
                return;
            }

            Player.velocity.Y -= Tossed;
            Tossed = 0f;
        }
    }

    private const float AIRDASHLENGTH = 250f;

    private enum LothorAIState : byte {
        Fall,
        Idle,
        Jump,
        Flight,
        AirDash,
        ClawsAttack
    }

    private Vector2 _playerTempPosition;
    private LothorAIState _previousState;
    private float _dashStrength;
    private bool _spawned;
    private bool _stompSpawned;
    private float _canChangeDirectionTimer;
    private float _pulseStrength;

    private LothorAIState CurrentAIState { get => (LothorAIState)NPC.ai[3]; set => NPC.ai[3] = (byte)value; }
    private Player Target { get; set; }

    private ref float DashCount => ref NPC.ai[2];
    private ref float FallStrengthIfClose => ref NPC.localAI[0];
    private ref float DashTimer => ref NPC.ai[0];
    private ref float DashDelay => ref NPC.ai[1];
    private ref float NoCollisionTimer => ref NPC.localAI[2];
    private ref float AirDashTimer => ref NPC.localAI[2];
    private ref float StillInJumpBeforeFlightTimer => ref NPC.localAI[1];

    private ref float ClawsTimer => ref NPC.ai[0];
    private ref float ClawsAttackTime => ref NPC.ai[1];

    private float PreparationProgress => Helper.EaseInOut3(DashTimer / DashDelay * 1.25f);

    private bool DoingFirstDash => DashCount == 0;
    private bool IsAboutToGoToChangeMainState => DashCount > GetJumpCountToEncourageFlightState();
    private bool BeforeDoingLastJump => DashCount > GetJumpCountToEncourageFlightState() - 1;

    private bool IsFlying => CurrentAIState == LothorAIState.Flight || CurrentAIState == LothorAIState.AirDash;

    private bool IsAboutToLand => IsFlying && BeforeDoingLastJump;

    private bool ShouldDrawPulseEffect => CurrentAIState != LothorAIState.ClawsAttack;

    private bool JustDidAirDash => NPC.velocity.Length() > 4.5f && DashTimer < DashDelay * 0.25f;
    private bool IsDashing => StillInJumpBeforeFlightTimer <= 0f && ((CurrentAIState == LothorAIState.Fall && _previousState != LothorAIState.Flight && Math.Abs(NPC.velocity.X) > 5f) || 
        (CurrentAIState == LothorAIState.AirDash && NPC.velocity.Length() > 3.5f) || 
        (CurrentAIState == LothorAIState.Flight && JustDidAirDash));

    private int GetJumpCountToEncourageFlightState() => 3;

    public override void AI() {
        Init();

        HandleActiveState();

        SetRotation();
        SetOthers();

        UpdateTrailInfo();
        UpdatePulseVisuals();
    }

    private void UpdatePulseVisuals() {
        int pulseCount = 3;
        float num282 = DashDelay / (float)pulseCount;
        float num283 = DashTimer % num282 / num282;
        _pulseStrength = num283;
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
            if (StillInJumpBeforeFlightTimer > 0f || CurrentAIState == LothorAIState.AirDash) {
                rotation = Utils.AngleLerp(rotation, to, Math.Abs(to) * 0.4f);
            }
            else {
                rotation = Math.Clamp(xVelocity, -maxRotation, maxRotation);
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
                if (StillInJumpBeforeFlightTimer <= 0f) {
                    _drawOffset.Y = 16f;
                    _currentColumn = SpriteSheetColumn.Flight;
                    if (NPC.frameCounter <= 0.0) {
                        CurrentFrame = 20;
                        NPC.frameCounter += NPC.velocity.Length() / 4f;
                        NPC.frameCounter += 1.0;
                    }
                    else {
                        double flightFrameRate = 6.0;
                        if (++NPC.frameCounter > 1.0 + flightFrameRate) {
                            CurrentFrame++;
                            NPC.frameCounter = 1.0;
                        }
                    }
                }
                if (CurrentFrame > 25) {
                    CurrentFrame = 20;
                }
                break;
            case LothorAIState.ClawsAttack:
                _currentColumn = SpriteSheetColumn.Stand;
                byte minFrame = 2;
                byte maxFrames = 8;
                float min = ClawsAttackTime * 0.15f;
                bool flag = ClawsTimer < min;
                if (CurrentFrame < 2 || CurrentFrame >= maxFrames || flag) {
                    CurrentFrame = 2;
                }
                if (!flag) {
                    CurrentFrame = (byte)MathHelper.Lerp(minFrame, maxFrames, (ClawsTimer - min) / (ClawsAttackTime - min));
                    if (CurrentFrame > maxFrames) {
                        CurrentFrame = maxFrames;
                    }
                }
                break;
        }
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
            }
        }
    }

    private void WhenIdle() {
        NPC.noTileCollide = false;

        FallStrengthIfClose = 0f;

        LookAtPlayer();
        NPC.velocity.X *= 0.875f;
    }

    private void ClawsAttack() {
        WhenIdle();

        NPC.knockBackResist = 0f;
        ClawsTimer += 1f;
        if (ClawsTimer >= ClawsAttackTime) {
            ClawsTimer = 0f;
            DashDelay = GetAttackDelay();
            _previousState = CurrentAIState;
            CurrentAIState = LothorAIState.Idle;
        }
    }

    private void AirDashState() {
        NPC.knockBackResist = 0f;
        float dashStrength = 15f;
        _dashStrength = Helper.Approach(_dashStrength, 1f, 0.05f);
        if (_dashStrength < 0.25f && NPC.Distance(Target.Center) > 50f) {
            _playerTempPosition = GetPositionBehindTarget();
        }
        if (NPC.velocity.Length() < dashStrength) {
            NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.DirectionTo(_playerTempPosition) * dashStrength, _dashStrength);
            NPC.velocity += NPC.DirectionTo(_playerTempPosition) * _dashStrength * dashStrength * 0.35f;
        }
        AirDashTimer++;
        NPC.velocity += NPC.DirectionTo(_playerTempPosition) * _dashStrength * dashStrength * 0.25f;
        float distance = Vector2.Distance(NPC.Center, _playerTempPosition);
        float minDistance = AIRDASHLENGTH;
        if (AirDashTimer > 10f) {
            if (distance < minDistance || (Vector2.Distance(NPC.Center, Target.Center) > minDistance * 2f && NPC.velocity.Length() > dashStrength * 0.75f) || NPC.velocity.Length() > dashStrength * 1.25f) {
                GoToFlightState(false, false);
                _dashStrength = 0f;
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
            ResetDashVariables();
            CurrentAIState = LothorAIState.AirDash;

            AirDashTimer = 0f;

            PlayRoarSound();

            DashCount++;
        }
    }

    private Vector2 GetPositionBehindTarget() {
        Vector2 playerCenter = Target.Center;
        return playerCenter + GetVelocityDirection();
    }

    private Vector2 GetBetween() {
        Vector2 npcCenter = NPC.Center;
        Vector2 playerCenter = Target.Center;
        if (IsAboutToLand) {
            playerCenter = Target.Center - Vector2.UnitY * (AIRDASHLENGTH - AIRDASHLENGTH * 0.75f * (1f - DashTimer / DashDelay));
        }
        Vector2 dif = playerCenter - npcCenter;
        return dif;
    }

    private Vector2 GetVelocityDirection() {
        Vector2 dif = GetBetween();
        Vector2 dif2 = dif.SafeNormalize(Vector2.One) * AIRDASHLENGTH;
        if (IsAboutToLand) {
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
    }

    private void FlightState() {
        if (StillInJumpBeforeFlightTimer > 0f) {
            StillInJumpBeforeFlightTimer--;
            AirDashTimer++;
            LookAtPlayer();
            return;
        }
        Vector2 dif = GetBetween();
        Vector2 dif2 = GetVelocityDirection();
        Vector2 velocity = dif - dif2;
        float speed = 10f;
        bool flag5 = IsAboutToLand;
        velocity = Vector2.Normalize(velocity) * speed;
        float inertia = 30f;
        float absDistance = dif.Length();
        float edge = 15f * Utils.Remap(NPC.velocity.Length(), 0f, 10f, 1f, 2f);
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
                float value = MathHelper.Clamp(dist / minDist, 0.4f, 1f);
                jumpY2 *= value;
                NPC.noTileCollide = false;
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
    }

    private float GetExtraAirDashDelay(bool flag = false) {
        return -GetAttackDelay(true) * 0.5f;
    }

    private void GoToFlightState(bool flag = true, bool resetDashCount = true) {
        if (resetDashCount) {
            DashCount = 0;
        }
        CurrentAIState = LothorAIState.Flight;
        ResetDashVariables();
        if (flag) {
            StillInJumpBeforeFlightTimer = 10f;
        }
        else {
            _previousState = LothorAIState.AirDash;
        }
        AirDashTimer = GetExtraAirDashDelay(flag);
        NPC.TargetClosest();
        DashDelay = GetAttackDelay();
    }

    private void FallState() {
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
                return;
            }
            NPC.noTileCollide = NPC.velocity.Y < 0f && NPC.velocity.Length() > dashStrength && Math.Abs(NPC.velocity.X) > dashStrength / 2f;
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
            NPC.velocity.X *= 0.85f + Math.Min(0.05f, Math.Abs(NPC.velocity.X) * 0.025f);
            if (Math.Abs(NPC.velocity.X) < 0.025f) {
                FallStrengthIfClose = 0f;
                NPC.TargetClosest();
                CurrentAIState = LothorAIState.Idle;
                _stompSpawned = false;
                DashDelay = GetAttackDelay();
            }
        }

        if (--NoCollisionTimer > 0f) {
            NPC.noTileCollide = true;
        }
    }

    private float ResetDashVariables() => DashDelay = DashTimer = 0f;

    private float GetDashStrength() {
        return 4f;
    }

    private float GetAttackDelay(bool flag = false) {
        float delay = /* IsFlying ? */BeforeDoingLastJump && !flag ? 125f : 100f/* : BeforeDoingLastJump ? 100f : 75f*/;
        //if (IsFlying && DoingFirstDash) {
        //    delay -= delay / 4;
        //}
        delay += delay * 0.333f;
        delay *= 0.75f;
        return delay;
    }

    private void TossAPlayer(Player target, float strength) {
        if (!target.active || target.dead) {
            return;
        }

        TossedStateHandler handler = target.GetModPlayer<TossedStateHandler>();
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
        if (_previousState != LothorAIState.ClawsAttack) {
            _previousState = LothorAIState.Idle;
        }

        WhenIdle();

        PrepareJump();
    }

    private void SetKnockBackResist() => NPC.knockBackResist = MathHelper.Lerp(0.5f, 0f, PreparationProgress * (IsFlying ? 2f : 1f));

    private void PrepareJump() {
        SetKnockBackResist();
        if (NPC.velocity.Y == 0f) {
            DashTimer += 1f;
            if (_previousState != LothorAIState.ClawsAttack && Vector2.Distance(Target.Center, NPC.Center) < 200f &&
                Collision.CanHit(Target, NPC)) {
                ClawsAttackTime = GetAttackDelay(true) * 0.8f;
                CurrentAIState = LothorAIState.ClawsAttack;
            }
            else if (DashTimer >= DashDelay) {
                ResetDashVariables();
                CurrentAIState = LothorAIState.Jump;
                _previousState = CurrentAIState;

                PlayRoarSound();

                DashCount++;
            }
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
