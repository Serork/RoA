using Terraria.ModLoader;

namespace RoA.Content.NPCs;

abstract class RoANPC : ModNPC {
    public ref float CurrentFrame => ref NPC.localAI[0];

    public ref float StateTimer => ref NPC.ai[0];
    public ref float State => ref NPC.ai[1];

    public bool Attack {
        get => NPC.ai[2] == 1f;
        set => NPC.ai[2] = value ? 1f : 0f;
    }

    protected void ChangeFrame((int, int) frameInfo) => NPC.frame.Y = frameInfo.Item1 * frameInfo.Item2;

    protected void ChangeState(int stateID, bool changeFrame = true, bool keepState = true) {
        if (NPC.frameCounter != 0.0 && changeFrame) {
            NPC.frameCounter = 0.0;
        }
        State = stateID;
        if (!keepState) {
            StateTimer = 0f;
        }
        NPC.netUpdate = true;
    }
}
