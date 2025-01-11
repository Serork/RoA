using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Utilities;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
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

        if (ShouldDrawPulseEffect) {
            Microsoft.Xna.Framework.Color color46 = Microsoft.Xna.Framework.Color.White;
            int num275 = 6;
            float num278 = 0f;
            float maxOffset = 15f;
            for (int num293 = 0; num293 < num275; num293++) {
                Microsoft.Xna.Framework.Color value80 = drawColor;
                value80 = Microsoft.Xna.Framework.Color.Lerp(value80, color46, 0f);
                value80 = NPC.GetAlpha(value80);
                value80 = Microsoft.Xna.Framework.Color.Lerp(value80, color46, 0f);
                float opacity = 1f - _pulseStrength;
                value80 *= opacity;
                value80 *= 0.7f;
                //value80 *= (1f - _trailOpacity);
                Vector2 position33 = NPC.position + offset + ((float)num293 / (float)num275 * ((float)Math.PI * 2f) + NPC.rotation + num278).ToRotationVector2() * maxOffset * _pulseStrength;
                position33 -= new Vector2(NPC.frame.Width, NPC.frame.Height / Main.npcFrameCount[Type]) * NPC.scale / 2f;
                position33 += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                spriteBatch.Draw(ItsSpriteSheet, position33, NPC.frame, value80, NPC.rotation, origin, NPC.scale, effects, 0f);
            }
        }

        spriteBatch.Draw(ItsSpriteSheet, NPC.position + offset, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);

        DrawWreath(spriteBatch);

        return false;
    }

    private void DrawWreath(SpriteBatch spriteBatch) {
        bool flag = false;
        if (ShouldDrawWreath) {
            flag = true;
        }
        if (!flag) {
            return;
        }

        Texture2D texture = ModContent.Request<Texture2D>(Texture + "_Wreath").Value;
        SpriteFrame frame = new(2, 1);
        Rectangle sourceRectangle = frame.GetSourceRectangle(texture);
        Vector2 position = NPC.Center - Main.screenPosition + new Vector2(75f * (Target.Center.X - NPC.Center.X).GetDirection(), -15f);
        spriteBatch.Draw(texture, position, sourceRectangle, Color.White, 0f, sourceRectangle.Size() / 2f, 1f, default, 0);
    }
}
