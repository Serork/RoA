using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class Lothor : ModNPC {
    private enum LothorAIState : byte {
        Idle
    }

    private LothorAIState CurrentAIState { get => (LothorAIState)NPC.ai[0]; set => NPC.ai[0] = (byte)value; }

    public override void AI() {
        HandleActiveState();
    }

    private void HandleActiveState() {
        switch (CurrentAIState) {
            case LothorAIState.Idle:
                IdleState();
                break;
        }
    }

    private void IdleState() {
        PlayAnimation(nameof(ClawsAnimation));
    }
}
