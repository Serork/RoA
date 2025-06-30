using Terraria;

namespace RoA.Core.Utility.Extensions;

static class NPCExtensions {
    public static void SetFrameCount(this NPC npc, byte frameCount) => Main.npcFrameCount[npc.type] = frameCount;

    public readonly struct NPCHitInfo(ushort lifeMax = 5, ushort damage = 0, ushort defense = 0, float knockBackResist = 1f) {
        public readonly ushort LifeMax = lifeMax;
        public readonly ushort Damage = damage;
        public readonly ushort Defense = defense;
        public readonly float KnockBackResist = knockBackResist;
    }

    public static void DefaultToEnemy(this NPC npc, in NPCHitInfo hitInfo) {
        npc.friendly = false;

        npc.lifeMax = hitInfo.LifeMax;
        npc.damage = hitInfo.Damage;
        npc.defense = hitInfo.Defense;
        npc.knockBackResist = hitInfo.KnockBackResist;
    }
}
