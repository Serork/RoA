using Microsoft.Xna.Framework;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Miscellaneous;

sealed partial class PettyGoblin : ModNPC {
    private const int FRAMES_COUNT = 16;
    private const int RUNNING_FRAMESCOUNT = 14;
    private const int STARTRUNNING_FRAME = 2;

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = NPC.direction;

        if (NPC.velocity.Y != 0f) {
            NPC.frame.Y = frameHeight;
            return;
        }
        else if (NPC.velocity.X == 0f) {
            NPC.frame.Y = 0;
            return;
        }

        float speedX = Math.Abs(NPC.velocity.X);
        if (NPC.frameCounter < RUNNING_FRAMESCOUNT * RUNNING_FRAMESCOUNT) {
            NPC.frameCounter += 0.2 * (double)MathHelper.Clamp(speedX, 0f, 3f);
        }
        else {
            NPC.frameCounter = 0.0;
        }
        int currentFrame = STARTRUNNING_FRAME + (int)(NPC.frameCounter % RUNNING_FRAMESCOUNT);
        NPC.frame.Y = currentFrame * frameHeight;
    }
}