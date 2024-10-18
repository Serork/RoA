using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class DruidSoul : RoANPC {
    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;

        NPCID.Sets.TrailCacheLength[Type] = 6;
        NPCID.Sets.TrailingMode[Type] = 1;
    }

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = -NPC.direction;
        double maxCounter = 6.0;
        if (++NPC.frameCounter >= maxCounter * (Main.npcFrameCount[Type] - 1)) {
            NPC.frameCounter = 0.0;
        }
        int currentFrame = (int)(NPC.frameCounter / maxCounter);
        NPC.frame.Y = currentFrame * frameHeight;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        DrawTextureUnderCustomSoulEffect(spriteBatch, (Texture2D)ModContent.Request<Texture2D>(Texture), drawColor.MultiplyRGB(new(241, 53, 84, 200)));
        return false;
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        spriteBatch.BeginBlendState(BlendState.NonPremultiplied, SamplerState.PointClamp);
        DrawTextureUnderCustomSoulEffect(spriteBatch, (Texture2D)ModContent.Request<Texture2D>(Texture), drawColor);
        spriteBatch.EndBlendState();
        DrawTextureUnderCustomSoulEffect(spriteBatch, ModContent.Request<Texture2D>(Texture + "_Eye").Value, drawColor);
    }

    private void DrawTextureUnderCustomSoulEffect(SpriteBatch spriteBatch, Texture2D texture, Color drawColor) {
        Color color = drawColor * NPC.Opacity;
        for (int index = 0; index < NPC.oldPos.Length; index++) {
            float factor = (NPC.oldPos.Length - (float)index) / NPC.oldPos.Length;
            for (double i = -Math.PI; i <= Math.PI; i += Math.PI / 2.0) {
                Vector2 position = NPC.oldPos[index] + NPC.Size / 2 + ((float)i).ToRotationVector2().RotatedBy(Main.GlobalTimeWrappedHourly * 2.0, new Vector2()) * Helper.Wave(0f, 3f, speed: 12f) - Main.screenPosition;
                Color color2 = color.MultiplyAlpha(NPC.Opacity).MultiplyAlpha((float)i / NPC.oldPos.Length) * factor;
                SpriteEffects effect = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                spriteBatch.Draw(texture, position, NPC.frame, color2 * (NPC.Opacity + 0.5f), NPC.rotation, NPC.Size / 2, Helper.Wave(NPC.scale + 0.05f, NPC.scale + 0.15f, 1f, 0f) * 0.9f * factor, effect, 0f);
            }
        }
    }
}