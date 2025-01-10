using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class Lothor : ModNPC {
    private Vector2 _drawOffset;
    private float _trailOpacity;

    private Texture2D ItsSpriteSheet => (Texture2D)ModContent.Request<Texture2D>(Texture + "_Spritesheet");

    public override void FindFrame(int frameHeight) {
        HandleAnimations();
        GetFrameInfo(out ushort x, out ushort y, out ushort width, out ushort height);
        NPC.frame = new Rectangle(x, y, width, height);
    }

    partial void HandleAnimations();

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Vector2 origin = NPC.frame.Size() / 2f;
        Vector2 positionOffset = Vector2.UnitY * _drawOffset;
        SpriteEffects effects = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Vector2 offset = positionOffset - screenPos + origin / 2f;
        if (IsDashing) {
            if (_trailOpacity < 1f) {
                _trailOpacity += 0.1f;
            }
        }
        else {
            if (_trailOpacity > 0f) {
                _trailOpacity -= 0.05f;
            }
        }
        int length = NPC.oldPos.Length - 2;
        for (int num173 = 1; num173 < length; num173 += 2) {
            _ = ref NPC.oldPos[num173];
            Color color39 = drawColor;
            color39.R = (byte)(1f * (double)(int)color39.R * (double)(length - num173) / length);
            color39.G = (byte)(1f * (double)(int)color39.G * (double)(length - num173) / length);
            color39.B = (byte)(1f * (double)(int)color39.B * (double)(length - num173) / length);
            color39.A = (byte)(1f * (double)(int)color39.A * (double)(length - num173) / length);
            color39 *= MathHelper.Clamp(NPC.velocity.Length(), 0f, 9f) / 9f;
            color39 *= 1f - num173 / length;
            color39 *= _trailOpacity;
            color39 *= 0.8f;
            Rectangle frame7 = NPC.frame;
            int num174 = ItsSpriteSheet.Height / Main.npcFrameCount[Type];
            frame7.Y -= num174 * num173;
            while (frame7.Y < 0) {
                frame7.Y += num174 * Main.npcFrameCount[Type];
            }
            Vector2 pos = NPC.oldPos[num173];
            spriteBatch.Draw(ItsSpriteSheet,
                pos + offset, 
                frame7, color39, NPC.rotation, origin, NPC.scale, effects, 0f);
        }
        spriteBatch.Draw(ItsSpriteSheet, NPC.position + offset, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);

        return false;
    }
}
