using Microsoft.Xna.Framework;

using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class Lothor : ModNPC {
    private enum LothorAIState : byte {
        Fall,
        Idle,
        Jump,
        Flight
    }

    private LothorAIState CurrentAIState { get => (LothorAIState)NPC.ai[3]; set => NPC.ai[3] = (byte)value; }
    private Player Target { get; set; }

    private ref float JumpCount => ref NPC.ai[2];
    private ref float FallFallStrengthIfClose => ref NPC.ai[0];
    private ref float DashTimer => ref NPC.ai[0];
    private ref float DashDelay => ref NPC.ai[1];
    private ref float NoCollisionTimer => ref NPC.localAI[2];
    private ref float InertiaTimer => ref NPC.ai[0];

    private float PreparationProgress => Helper.EaseInOut3(DashTimer / DashDelay * 1.25f);
    private bool IsAboutToGoToFlightState => JumpCount > GetJumpCountToEncourageFlightState();
    private bool IsAboutToGoToFlightState2 => JumpCount > GetJumpCountToEncourageFlightState() - 1;
    private bool IsFlying => CurrentAIState == LothorAIState.Flight;

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
            rotation = Math.Clamp(xVelocity, -maxRotation, maxRotation);
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
            }
        }
    }

    private void FlightState() {
        if (--InertiaTimer > 0f) {
            NPC.direction = NPC.velocity.X.GetDirection();
            return;
        }

        Vector2 npcCenter = NPC.Center;
        Vector2 playerCenter = Target.Center;
        Vector2 dif = playerCenter - npcCenter;
        float distance = 200f;
        Vector2 velocity = dif - dif.SafeNormalize(Vector2.One) * distance;
        float speed = 6.225f;
        velocity = Vector2.Normalize(velocity) * speed;
        float inertia = 30f;
        float absDistance = Math.Abs(dif.Length());
        float edge = 15f * Utils.Remap(NPC.velocity.Length(), 0f, 10f, 1f, 2f);
        if (absDistance > distance - edge && absDistance < distance + edge && absDistance != distance) {
            NPC.velocity *= (float)Math.Pow(0.97, inertia * 2.0 / inertia);
            NPC.LookAtPlayer(Target);
        }
        else {
            if (NPC.velocity.Length() > 5f) {
                NPC.direction = NPC.velocity.X.GetDirection();
            }
            else {
                NPC.LookAtPlayer(Target);
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

            // noTileCollide timer
            NoCollisionTimer = 10f;
        }
        doJump();
        CurrentAIState = LothorAIState.Fall;
    }

    private void FallState() {
        NPC.knockBackResist = 0f;
        if ((NPC.direction == 1 && NPC.position.X > Target.position.X || (NPC.direction != 1 && NPC.position.X < Target.position.X)) && NPC.Distance(Target.position) > 65f) {
            NPC.LookAtPlayer(Target);
        }
        float dashStrength = GetDashStrength();
        if (NPC.velocity.Y != 0f) {
            if (NPC.velocity.Y > 1f && IsAboutToGoToFlightState) {
                JumpCount = 0;
                CurrentAIState = LothorAIState.Flight;
                ResetDashVariables();
                InertiaTimer = 10f;
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
                if (FallFallStrengthIfClose < fallVelocityY) {
                    FallFallStrengthIfClose += fallAcceleration;
                }
                NPC.velocity.Y += FallFallStrengthIfClose;
            }
        }
        else {
            SpawnStomp();

            NPC.velocity.X *= 0.85f + Math.Min(0.05f, Math.Abs(NPC.velocity.X) * 0.025f);
            if (Math.Abs(NPC.velocity.X) < 0.025f) {
                FallFallStrengthIfClose = 0f;
                NPC.TargetClosest(true);
                CurrentAIState = LothorAIState.Idle;
                float dashDelay = GetDashDelay();
                DashDelay = dashDelay + dashDelay * 0.333f;
                DashDelay /= 2f;
            }
        }

        if (--NoCollisionTimer > 0f) {
            NPC.noTileCollide = true;
        }
    }

    private float ResetDashVariables() => DashDelay = DashTimer = 0f;

    private float GetDashStrength() {
        return 3f;
    }

    private float GetDashDelay() {
        return IsAboutToGoToFlightState2 ? 150f : 100f;
    }

    private void SpawnStomp() {

    }

    private void IdleState() {
        NPC.knockBackResist = MathHelper.Lerp(0.5f, 0f, PreparationProgress);
        NPC.noTileCollide = false;

        NPC.LookAtPlayer(Target);
        NPC.velocity.X *= 0.875f;

        HandleBeforeMakingJump();
    }

    private void HandleBeforeMakingJump() {
        DashTimer += 1f - 0.35f * MathHelper.Clamp(NPC.Distance(Target.Center) / 600f, 0f, 1f);
        if (DashTimer >= DashDelay) {
            ResetDashVariables();
            CurrentAIState = LothorAIState.Jump;

            JumpCount++;
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
