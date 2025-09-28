using Microsoft.Xna.Framework;

using RoA.Common.Druid.Forms;
using RoA.Common.Players;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class HallowedGryphonHandler : ModPlayer, IDoubleTap {
    public bool JustStartedDoingLoopAttack;
    public bool LoopAttackIsDone;
    public Vector2 SavedVelocity;
    public ushort CanDoLoopAttackTimer;
    public float AttackFactor;
    public byte AttackCount;
    public float MoveSpeedBuffTime;

    public bool CanDoLoopAttack {
        get => CanDoLoopAttackTimer <= 0;
        set => CanDoLoopAttackTimer = (ushort)(value ? 0 : 30);
    }

    public bool IncreasedMoveSpeed => MoveSpeedBuffTime > 0f;
    public bool CanIncreaseMoveSpeed => MoveSpeedBuffTime == 0f;

    public override void PostUpdate() {
        if (!Player.GetModPlayer<BaseFormHandler>().IsConsideredAs<HallowedGryphon>()) {
            return;
        }
        if (LoopAttackIsDone && CanDoLoopAttackTimer > 0) {
            CanDoLoopAttackTimer--;
            if (CanDoLoopAttackTimer <= 0) {
                LoopAttackIsDone = false;
            }
        }
        if (IncreasedMoveSpeed) {
            MoveSpeedBuffTime--;
            if (CanIncreaseMoveSpeed) {
                MoveSpeedBuffTime = -480f;
            }
        }
        else if (!CanIncreaseMoveSpeed) {
            MoveSpeedBuffTime++;
        }
    }

    public override void PostUpdateRunSpeeds() {
    }

    public void Reset() {
        JustStartedDoingLoopAttack = LoopAttackIsDone = false;
        SavedVelocity = Vector2.Zero;
        CanDoLoopAttackTimer = 0;
        AttackFactor = 0f;
        AttackCount = 0;
        MoveSpeedBuffTime = 0f;
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {
        bool flag = direction == IDoubleTap.TapDirection.Right | direction == IDoubleTap.TapDirection.Left;
        if (!flag) {
            return;
        }
        if (!player.GetModPlayer<BaseFormHandler>().IsConsideredAs<HallowedGryphon>()) {
            return;
        }

        player.GetModPlayer<HallowedGryphonHandler>().GainMoveSpeedBuff();
    }

    public void GainMoveSpeedBuff() {
        if (!CanIncreaseMoveSpeed) {
            return;
        }
        MoveSpeedBuffTime = 120f;
    }
}
