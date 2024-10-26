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
        DrawChain(spriteBatch, color);
        DrawTextureUnderCustomSoulEffect(spriteBatch, (Texture2D)ModContent.Request<Texture2D>(Texture), color);
        return false;
    }

    private void DrawChain(SpriteBatch spriteBatch, Color drawColor) {
        Texture2D texture = ModContent.Request<Texture2D>(Texture + "_Chain").Value;
        Rectangle? sourceRectangle = null;
        Vector2 origin = (sourceRectangle.HasValue ? (sourceRectangle.Value.Size() / 2f) : (texture.Size() / 2f));
        Vector2 altarPos = AltarHandler.GetAltarPosition().ToWorldCoordinates();
        Vector2 altarCoords = altarPos + Vector2.UnitY * 5f;
        Vector2 from = NPC.Center/* + Vector2.UnitX * 4f * NPC.direction*/;
        Vector2 npcCenter = (from + Vector2.UnitY * 8f).MoveTowards(altarCoords, 10f);
        Vector2 playerCenter = (Main.player[NPC.target].Center + Vector2.UnitY * 10f).MoveTowards(altarCoords, 10f);
        float max = MAXDISTANCETOALTAR;
        Vector2 altarCoords2 = altarCoords.MoveTowards(playerCenter, 20f);
        float dist = npcCenter.Distance(altarCoords),
              dist2 = Math.Max(Math.Abs(playerCenter.X - altarCoords2.X), Math.Abs(npcCenter.X - altarCoords2.X)) /*playerCenter*//*npcCenter.Distance(altarCoords2)*/;
        float lerpValue = 0.1f;
        NPC.localAI[1] = MathHelper.Lerp(NPC.localAI[1], Utils.GetLerpValue(max * 1.025f, max * 1.225f, dist, true), lerpValue);
        float opacity = 1f - NPC.localAI[1];
        float minDist = 60f;
        NPC.localAI[2] = MathHelper.Lerp(NPC.localAI[2], Utils.GetLerpValue(minDist, 100f, dist2, true), lerpValue);
        opacity *= NPC.localAI[2];
        if (dist2 < NPC.localAI[2]) {
            opacity *= 0f;
        }
        opacity = Helper.EaseInOut2(opacity);
        drawColor *= opacity;
        drawColor *= 0.85f;
        Vector2 velocity = NPC.velocity + _velocity + _velocity2 + _velocity3;
        float mult = 1f - MathHelper.Clamp((altarCoords.Distance(npcCenter) + velocity.Length() * 4f) / max, 0f, 1f);
        //npcCenter = (from + Vector2.UnitY * 8f * (1f - mult)).MoveTowards(altarCoords, 10f);
        float width = 250f * mult;
        altarCoords = altarPos + Vector2.UnitY * 5f * (1f - mult);
        SimpleCurve curve = new(npcCenter, altarCoords, Vector2.Zero);
        curve.Control = (curve.Begin + curve.End) / 2f + new Vector2(0f, width);
        Vector2 start = curve.Begin, end = curve.End;
        Vector2 between = end - start;
        float length = between.Length();
        //length = Math.Max(200f, length);
        int amount = (int)(length / texture.Width * NPC.scale);
        for (int k = 0; k <= amount; ++k) {
            Vector2 point = curve.GetPoint((float)k / amount);
            Vector2 v = (point - start).SafeNormalize(Vector2.Zero);
            float rotation = v.ToRotation() + (float)Math.PI / 2f;
            Color color = drawColor /*Lighting.GetColor(start.ToTileCoordinates()).MultiplyRGB(_color)*/ * NPC.Opacity;
            float min = amount / 2 + amount / 3;
            int amount2 = 3;
            if (k < amount2) {
                float progress2 = k / (float)amount2 * 1.25f;
                color *= progress2;
            }
            amount2 = 4;
            int max2 = amount - amount2;
            if (k > max2) {
                float progress2 = 1f - (k - max2) / (float)(amount - max2);
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
            start += v * texture.Width;
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