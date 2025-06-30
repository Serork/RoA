using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods.Hardmode;

sealed class Woodpecker : ModNPC {
    private static byte FRAMECOUNT => 15;

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(FRAMECOUNT);
    }

    public override void SetDefaults() {
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo() { });
    }

    public override void AI() {

    }
}
