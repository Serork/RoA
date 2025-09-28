using Microsoft.Xna.Framework;

using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Forms;

sealed class HallowedGryphonHandler : ModPlayer {
    public bool JustStartedDoingLoopAttack;
    public bool LoopAttackIsDone;
    public Vector2 SavedVelocity;
    public ushort CanDoLoopAttackTimer;
    public float AttackFactor;
    public byte AttackCount;

    public bool CanDoLoopAttack {
        get => CanDoLoopAttackTimer <= 0;
        set => CanDoLoopAttackTimer = (ushort)(value ? 0 : 30);
    }

    public override void PostUpdate() {
        if (LoopAttackIsDone && CanDoLoopAttackTimer > 0) {
            CanDoLoopAttackTimer--;
            if (CanDoLoopAttackTimer <= 0) {
                LoopAttackIsDone = false;
            }
        }
    }

    public void Reset() {
        JustStartedDoingLoopAttack = LoopAttackIsDone = false;
        SavedVelocity = Vector2.Zero;
        CanDoLoopAttackTimer = 0;
        AttackFactor = 0f;
        AttackCount = 0;
    }
}
