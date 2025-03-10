using System.Collections.Generic;

using Terraria.ModLoader;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
sealed class NPCKillCondition : ModCondition {
    public short type;

    public NPCKillCondition(short npcId)
        : base(GetUniqueName(npcId)) {
        type = npcId;
    }

    public override void Register() {
        if (!AchievementLoader.npcKilled.ContainsKey(type)) {
            AchievementLoader.npcKilled[type] = new List<NPCKillCondition>();
        }
        AchievementLoader.npcKilled[type].Add(this);
    }

    public void NPCKilled(short type) {
        if (this.type == type) {
            SetComplete();
        }
    }

    private static string GetUniqueName(short id) {
        ModNPC npc = NPCLoader.GetNPC(id);
        if (npc == null) {
            return "NPCKilled_" + id;
        }
        return "NPCKilled_" + npc.Name;
    }
}
