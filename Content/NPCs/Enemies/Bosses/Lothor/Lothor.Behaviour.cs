using Microsoft.Xna.Framework;

using RoA.Utilities;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class Lothor : ModNPC {
    private const float AIRDASHLENGTH = 200f;

    private enum LothorAIState : byte {
        Fall,
        Idle,
        Jump,
        Flight,
        AirDash
    }

    private Vector2 _playerTempPosition;
    private LothorAIState _previousState;
    private float _dashStrength;

    private LothorAIState CurrentAIState { get => (LothorAIState)NPC.ai[3]; set => NPC.ai[3] = (byte)value; }
    private Player Target { get; set; }

    private ref float DashCount => ref NPC.ai[2];
    private ref float FallStrengthIfClose => ref NPC.localAI[0];
    private ref float DashTimer => ref NPC.ai[0];
    private ref float DashDelay => ref NPC.ai[1];
    private ref float NoCollisionTimer => ref NPC.localAI[2];
    private ref float StillInJumpBeforeFlightTimer => ref NPC.localAI[1];

    private float PreparationProgress => Helper.EaseInOut3(DashTimer / DashDelay * 1.25f);

    private bool DoingFirstDash => DashCount == 0;
    private bool IsAboutToGoToFlightState => DashCount > GetJumpCountToEncourageFlightState();
    private bool BeforeDoingLastJump => DashCount > GetJumpCountToEncourageFlightState() - 1;

    private bool IsFlying => CurrentAIState == LothorAIState.Flight || CurrentAIState == LothorAIState.AirDash;

    private int GetJumpCountToEncourageFlightState() => 3;

    public override void AI() {
        HandleActiveState();

        SetRotation();
        SetOthers();
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
        DashDelay = GetDashDelay();
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
                        NPC.frameCounter++;
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
            }
        }
    }

    private void AirDashState() {
        NPC.knockBackResist = 0f;
        float dashStrength = 12.5f;
        _dashStrength = Helper.Approach(_dashStrength, 1f, 0.0075f);
        if (_dashStrength < 0.05f && NPC.Distance(Target.Center) > 50f) {
            _playerTempPosition = GetPositionBehindTarget();
        }
        if (NPC.velocity.Length() < dashStrength) {
            NPC.velocity = Vector2.SmoothStep(NPC.velocity, NPC.DirectionTo(_playerTempPosition) * dashStrength, _dashStrength);
            NPC.velocity += NPC.DirectionTo(_playerTempPosition) * _dashStrength * dashStrength * 0.35f;
        }
        NPC.velocity += NPC.DirectionTo(_playerTempPosition) * _dashStrength * dashStrength * 0.25f;
        float distance = Vector2.Distance(NPC.Center, _playerTempPosition);
        float minDistance = 200f;
        if (distance < minDistance || (Vector2.Distance(NPC.Center, Target.Center) > minDistance * 2f && NPC.velocity.Length() > dashStrength * 0.75f) || NPC.velocity.Length() > dashStrength * 1.25f) {
            GoToFlightState(false, false);
            _dashStrength = 0f;
        }
        NPC.LookAtPlayer(Target);
    }

    private void PrepareAirDash(bool shouldReset = true) {
        NPC.knockBackResist = MathHelper.Lerp(0.5f, 0f, PreparationProgress);
        DashTimer += 1f;
        if (DashTimer >= DashDelay && shouldReset) {
            ResetDashVariables();
            CurrentAIState = LothorAIState.AirDash;
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
        Vector2 dif = playerCenter - npcCenter;
        return dif;
    }

    private Vector2 GetVelocityDirection() {;
        Vector2 dif = GetBetween();
        Vector2 dif2 = dif.SafeNormalize(Vector2.One) * AIRDASHLENGTH;
        return dif2;
    }

    private void GoToIdleState() {
        DashCount = 0;
        CurrentAIState = LothorAIState.Fall;
        ResetDashVariables();
        NPC.TargetClosest();
        SetDashDelay();
    }

    private void FlightState() {
        if (--StillInJumpBeforeFlightTimer > 0f) {
            NPC.LookAtPlayer(Target);
            return;
        }
        Vector2 dif = GetBetween();
        Vector2 dif2 = GetVelocityDirection();
        Vector2 velocity = dif - dif2;
        float speed = 10f;
        velocity = Vector2.Normalize(velocity) * speed;
        float inertia = 30f;
        float absDistance = Math.Abs(dif.Length());
        float edge = 15f * Utils.Remap(NPC.velocity.Length(), 0f, 10f, 1f, 2f);
        bool flag = _previousState == LothorAIState.AirDash;
        if (flag) {
            NPC.LookAtPlayer(Target);
        }
        if (NPC.Bottom.Y > Target.Top.Y) {
            NPC.velocity.Y -= 0.1f;
        }
        bool flag2 = NPC.velocity.Length() < 5f && BeforeDoingLastJump;
        PrepareAirDash(!flag2);
        bool flag3 = Collision.SolidCollision(NPC.position - Vector2.One * 4, NPC.width + 2, NPC.height + 2);
        bool flag4 = DashTimer > DashDelay;
        if (flag4 && !flag2 && BeforeDoingLastJump && !flag3) {
            if (flag2) {
                GoToIdleState();
            }
        }
        if (absDistance > AIRDASHLENGTH - edge && absDistance < AIRDASHLENGTH + edge && absDistance != AIRDASHLENGTH) {
            NPC.velocity *= (float)Math.Pow(0.97, inertia * 2.0 / inertia);
            if (!flag) {
                NPC.LookAtPlayer(Target);
            }
            if (flag4 && flag2 && !flag3) {
                GoToIdleState();
            }
        }
        else {
            if (!flag) {
                if (NPC.velocity.Length() > 5f) {
                    NPC.direction = NPC.velocity.X.GetDirection();
                }
                else {
                    NPC.LookAtPlayer(Target);
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
            if (IsAboutToGoToFlightState) {
                yStrength = maxSpeedY * 0.25f * Math.Sign(yStrength);
            }
            float jumpHeightY = yStrength;
            float jumpY = jumpHeightY * -0.5f;
            if ((int)(NPC.Center.Y - Target.Center.Y) / 16 > -10) {
                NPC.velocity.Y = -8f + jumpY;
            }
            else {
                if (!Collision.CanHit(NPC.position, NPC.width, NPC.height, Target.position, Target.width, Target.height)) {
                    NPC.noTileCollide = true;
                    NPC.velocity.Y = -(jumpHeightY * -0.5f);
                }
                else {
                    NPC.velocity.Y = -8f + jumpY;
                }
            }
            float speed = 5f;
            float maxSpeedX = dashStrength * 6.5f;
            float xStrength = Math.Abs(NPC.Center.X - Target.Center.X) / 16f;
            float jumpHeightX = MathHelper.Clamp(xStrength, 0f, maxSpeedX);
            if (IsAboutToGoToFlightState) {
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
        NPC.TargetClosest();
        SetDashDelay();
    }

    private void FallState() {
        NPC.knockBackResist = 0f;
        if (IsAboutToGoToFlightState) {
            NPC.LookAtPlayer(Target);
        }
        else {
            if ((NPC.direction == 1 && NPC.position.X > Target.position.X || (NPC.direction != 1 && NPC.position.X < Target.position.X)) && NPC.Distance(Target.position) > 65f) {
                NPC.LookAtPlayer(Target);
            }
        }
        float dashStrength = GetDashStrength();
        if (NPC.velocity.Y != 0f) {
            if (NPC.velocity.Y > 1f && IsAboutToGoToFlightState) {
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
                SetDashDelay();
            }
        }

        if (--NoCollisionTimer > 0f) {
            NPC.noTileCollide = true;
        }
    }

    private void SetDashDelay() {
        float dashDelay = GetDashDelay();
        DashDelay = dashDelay + dashDelay * 0.333f;
        if (!IsFlying) {
            DashDelay /= 2f;
        }
    }

    private float ResetDashVariables() => DashDelay = DashTimer = 0f;

    private float GetDashStrength() {
        return 4f;
    }

    private float GetDashDelay() {
        float delay = /* IsFlying ? */BeforeDoingLastJump ? 125f : 100f/* : BeforeDoingLastJump ? 100f : 75f*/;
        if (IsFlying && DoingFirstDash) {
            delay -= delay / 4;
        }
        return delay;
    }

    private void SpawnStomp() {

    }

    private void IdleState() {
        NPC.noTileCollide = false;

        FallStrengthIfClose = 0f;

        NPC.LookAtPlayer(Target);
        NPC.velocity.X *= 0.875f;

        PrepareJump();
    }

    private void PrepareJump() {
        NPC.knockBackResist = MathHelper.Lerp(0.5f, 0f, PreparationProgress);
        DashTimer += 1f - 0.35f * MathHelper.Clamp(NPC.Distance(Target.Center) / 600f, 0f, 1f);
        if (DashTimer >= DashDelay) {
            ResetDashVariables();
            CurrentAIState = LothorAIState.Jump;

            DashCount++;
        }
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
