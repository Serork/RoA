using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class Lothor : ModNPC {
    private Texture2D ItsSpriteSheet => (Texture2D)ModContent.Request<Texture2D>(Texture + "_Spritesheet");

    public override void FindFrame(int frameHeight) {
        HandleAnimations();
        GetFrameInfo(out ushort x, out ushort y, out ushort width, out ushort height);
        NPC.frame = new Rectangle(x, y, width, height);
    }

    partial void HandleAnimations();

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Vector2 origin = NPC.frame.Size() / 2f;
        Vector2 positionOffset = Vector2.UnitY * DrawOffsetY;
        spriteBatch.Draw(ItsSpriteSheet, NPC.position + positionOffset - screenPos + origin / 2f, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, default, 0f);

        return false;
    }
}
