using Microsoft.Xna.Framework;

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
        Jump
    }

    private LothorAIState CurrentAIState { get => (LothorAIState)NPC.ai[3]; set => NPC.ai[3] = (byte)value; }
    private Player Target { get; set; }

    private float PreparationProgress => Helper.EaseInOut3(NPC.ai[0] / NPC.ai[1]* 1.25f);

    public override void AI() {
        HandleActiveState();
    }

    public override void OnSpawn(IEntitySource source) {
        CurrentAIState = LothorAIState.Idle;
        NPC.ai[1] = GetDashDelay();
        NPC.TargetClosest();
    }

    partial void HandleAnimations() {
        NPC.spriteDirection = -NPC.direction;
        DrawOffsetY = -6f;
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
            }
        }
    }

    private void JumpState() {
        NPC.knockBackResist = 0f;
        NPC.noTileCollide = true;
        void jump(bool extraY = false) {
            float dashStrength = GetDashStrength();
            float jumpHeightY = (extraY ? 1.1f : 1f) * (int)Math.Abs(NPC.Center.Y - Target.Center.Y) / 16;
            float jump = jumpHeightY * -0.5f;
            if ((int)(NPC.Center.Y - Target.Center.Y) / 16 > -10) {
                NPC.velocity.Y = -8f + jump;
            }
            else {
                if (Collision.CanHit(NPC.position, NPC.width, NPC.height, Target.position, Target.width, Target.height)) {
                    NPC.noTileCollide = true;
                    NPC.velocity.Y = -(jumpHeightY * -0.5f);
                }
                else {
                    NPC.velocity.Y = -8f + jump;
                }
            }
            float speed = 5f;
            int jumpHeightX = (int)MathHelper.Clamp(Math.Abs(NPC.Center.X - Target.Center.X) / 16f, 0f, dashStrength * 6.5f);
            if (jumpHeightX < 10f) {
                NPC.velocity.X += speed * dashStrength * 0.5f * NPC.direction;
            }
            else {
                NPC.velocity.X = jumpHeightX * -0.5f * -(float)NPC.direction;
            }
        }
        jump();
        CurrentAIState = LothorAIState.Fall;
    }

    private void FallState() {
        NPC.knockBackResist = 0f;
        if ((NPC.direction == 1 && NPC.position.X > Target.position.X || (NPC.direction != 1 && NPC.position.X < Target.position.X)) && NPC.Distance(Target.position) > 65f) {
            NPC.LookAtPlayer(Target);
        }
        float dashStrength = GetDashStrength();
        if (NPC.velocity.Y != 0f) {
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
                if (NPC.ai[0] < 0.9f) {
                    NPC.ai[0] += 0.01f;
                }
                NPC.velocity.Y += NPC.ai[0];
            }
        }
        else {
            SpawnStomp();

            NPC.velocity.X *= 0.85f + Math.Min(0.05f, Math.Abs(NPC.velocity.X) * 0.025f);
            if (Math.Abs(NPC.velocity.X) < 0.025f) {
                NPC.ai[0] = 0f;
                NPC.TargetClosest(true);
                CurrentAIState = LothorAIState.Idle;
                float dashDelay = GetDashDelay();
                NPC.ai[1] = dashDelay + dashDelay * 0.333f;
                NPC.ai[1] /= 2f;
            }
        }
    }

    private float GetDashStrength() {
        return 4f;
    }

    private float GetDashDelay() {
        return 100f;
    }

    private void SpawnStomp() {

    }

    private void IdleState() {
        NPC.knockBackResist = MathHelper.Lerp(0.5f, 0f, PreparationProgress);
        NPC.noTileCollide = false;

        NPC.LookAtPlayer(Target);
        NPC.velocity.X *= 0.875f;

        NPC.ai[0] += 1f - 0.35f * MathHelper.Clamp(NPC.Distance(Target.Center) / 600f, 0f, 1f);
        if (NPC.ai[0] >= NPC.ai[1]) {
            NPC.ai[0] = NPC.ai[1] = 0f;
            CurrentAIState = LothorAIState.Jump;
        }
    }

    private bool GetTargetPlayer(out Player player) {
        //if (NPC.target < 0 || NPC.target == 255) {
        //    NPC.TargetClosest();
        //}
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
