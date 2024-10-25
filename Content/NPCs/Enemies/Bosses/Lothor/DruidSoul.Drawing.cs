using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Data;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class DruidSoul : RoANPC {
    private readonly Color _color = new(241, 53, 84, 200);

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
        Color color = drawColor.MultiplyRGB(_color);
        DrawChain(spriteBatch);
        DrawTextureUnderCustomSoulEffect(spriteBatch, (Texture2D)ModContent.Request<Texture2D>(Texture), color);
        return false;
    }

    private void DrawChain(SpriteBatch spriteBatch) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture + "_Chain").Value;
        Rectangle? sourceRectangle = null;
        Vector2 origin = (sourceRectangle.HasValue ? (sourceRectangle.Value.Size() / 2f) : (texture.Size() / 2f));
        Vector2 altarCoords = (AltarHandler.GetAltarPosition().ToWorldCoordinates() + Vector2.UnitY * 10f);
        Vector2 npcCenter = (NPC.Center + Vector2.UnitY * 10f).MoveTowards(altarCoords, 10f);
        SimpleCurve curve = new(npcCenter, altarCoords, Vector2.Zero);
        Vector2 velocity = NPC.velocity + _velocity + _velocity2 + _velocity3;
        float max = MAXDISTANCETOALTAR * 0.8f;
        float mult = MathHelper.Clamp((altarCoords.Distance(npcCenter) + velocity.Length() * 0.1f) / max, 0f, 1f);
        float width = 100f * mult;
        curve.Control = (curve.Begin + curve.End) / 2f + new Vector2(0f, width);
        Vector2 start = curve.Begin, end = curve.End;
        Vector2 dist = end - start;
        float length = dist.Length();
        length = Math.Max(200f, length);
        int amount = (int)(length / texture.Width * NPC.scale);
        for (int index = 0; index < amount; ++index) {
            Vector2 point = curve.GetPoint((float)index / amount);
            float rotation = (point - start).SafeNormalize(Vector2.Zero).ToRotation() + (float)Math.PI / 2f;
            Color color = Lighting.GetColor(start.ToTileCoordinates()).MultiplyRGB(_color) * NPC.Opacity;
            float min = amount / 2;
            if (index > min) {
                float progress2 = (index - min) / (float)(amount - min);
                color *= progress2;
            }
            color *= 1.5f;
            for (double i = -Math.PI; i <= Math.PI; i += Math.PI / 2.0) {
                color = color.MultiplyAlpha(NPC.Opacity).MultiplyAlpha((float)i);
                spriteBatch.Draw(texture, start + ((float)i).ToRotationVector2().RotatedBy(Main.GlobalTimeWrappedHourly * 2.0, new Vector2()) * Helper.Wave(0f, 3f, speed: 12f) - Main.screenPosition, sourceRectangle, color, rotation, origin, Helper.Wave(NPC.scale + 0.05f, NPC.scale + 0.15f, 1f, 0f) * 0.9f, SpriteEffects.None, 0f);
                spriteBatch.BeginBlendState(BlendState.NonPremultiplied, SamplerState.PointClamp);
                spriteBatch.Draw(texture, start + ((float)i).ToRotationVector2().RotatedBy(Main.GlobalTimeWrappedHourly * 2.0, new Vector2()) * Helper.Wave(0f, 3f, speed: 12f) - Main.screenPosition, sourceRectangle, color, rotation, origin, Helper.Wave(NPC.scale + 0.05f, NPC.scale + 0.15f, 1f, 0f) * 0.9f, SpriteEffects.None, 0f);
                spriteBatch.EndBlendState();
            }
            start = point;
        }
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