using Microsoft.Xna.Framework;

using Terraria;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class DruidSoul : RoANPC {
    private const float MAXDISTANCETOALTAR = 300f;

    public override void SetDefaults() {
        NPC.lifeMax = 10;

        int width = 28; int height = 50;
        NPC.Size = new Vector2(width, height);

        NPC.noTileCollide = NPC.friendly = true;

        NPC.friendly = true;
        NPC.noGravity = true;

        NPC.immortal = NPC.dontTakeDamage = true;

        NPC.aiStyle = AIType = -1;
    }
}