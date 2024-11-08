using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class Lothor : ModNPC {
    public sealed override void SetStaticDefaults() => Main.npcFrameCount[Type] = 1;

    public override void SetDefaults() {
        NPC.damage = 46;
        NPC.lifeMax = 6000;
        NPC.defense = 8;

        NPC.Size = Vector2.One * 72f;

        NPC.aiStyle = NPC.ModNPC.AIType = -1;
    }

    public override void Unload() => UnloadAnimations();

    partial void UnloadAnimations();
}
