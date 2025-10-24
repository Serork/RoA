//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//using RoA.Core.Utility;

//using Terraria.ID;
//using Terraria.ModLoader;

//namespace RoA.Content.NPCs.Enemies.Tar;

//sealed class PerfectMimic : ModNPC {
//    public override void SetDefaults() {
//        NPC.SetSizeValues(30, 60);
//        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

//        NPC.aiStyle = -1;
//    }

//    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
//        return base.PreDraw(spriteBatch, screenPos, drawColor);
//    }

//    public override void AI() {
//        NPC.velocity.X *= 0.8f;
//    }
//}
