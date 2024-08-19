using Terraria;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static class NPCExtensions {
    public static T As<T>(this NPC npc) where T : ModNPC => npc.ModNPC as T;
}
