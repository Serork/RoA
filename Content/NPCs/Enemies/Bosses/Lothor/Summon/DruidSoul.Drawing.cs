using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
using RoA.Common.WorldEvents;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor.Summon;

sealed partial class DruidSoul : RoANPC {
    private static Asset<Texture2D> _chainTexture = null!,
                                    _eyeTexture = null!;

    private readonly Color _color = new(241, 53, 84, 200), _color2 = new(114, 216, 102, 200);

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;

        NPCID.Sets.TrailCacheLength[Type] = 6;
        NPCID.Sets.TrailingMode[Type] = 1;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Hide = true
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

        if (Main.dedServ) {
            _chainTexture = ModContent.Request<Texture2D>(Texture + "_Chain");
            _eyeTexture = ModContent.Request<Texture2D>(Texture + "_Eye");
        }
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
        DrawChain(spriteBatch, drawColor.MultiplyRGB(_color), drawColor.MultiplyRGB(_color2));
        Color color = drawColor.MultiplyRGB(_color);
        DrawTextureUnderCustomSoulEffect(spriteBatch, NPC.GetTexture(), color);
        return false;
    }

    private void DrawChain(SpriteBatch spriteBatch, Color drawColor1, Color drawColor2) {
        if (NPC.Opacity <= 0f) {
            return;
        }
        Vector2 altarPos = AltarHandler.GetAltarPosition().ToWorldCoordinates();
        Vector2 altarCoords = altarPos + Vector2.UnitY * 5f;
        if (Vector2.Distance(NPC.Center, altarCoords) > 500f) {
            return;
        }
        Texture2D texture = _chainTexture.Value;
        Rectangle? sourceRectangle = null;
        Vector2 origin = (sourceRectangle.HasValue ? (sourceRectangle.Value.Size() / 2f) : (texture.Size() / 2f));
        Vector2 from = NPC.Center + new Vector2(0f, 4f);
        Vector2 npcCenter = (from + Vector2.UnitY * 8f).MoveTowards(altarCoords, 10f);
        Player player = Main.player[NPC.target];
        Vector2 playerCenter = (player.Center + Vector2.UnitY * 10f).MoveTowards(altarCoords, 10f);
        float max = MAXDISTANCETOALTAR * 1.05f;
        Vector2 altarCoords2 = altarCoords.MoveTowards(playerCenter, 20f);
        float dist = npcCenter.Distance(altarCoords),
              dist2 = Math.Max(Math.Abs(playerCenter.X - altarCoords2.X), Math.Abs(npcCenter.X - altarCoords2.X)) /*playerCenter*//*npcCenter.Distance(altarCoords2)*/;
        float lerpValue = 0.1f;
        NPC.localAI[1] = MathHelper.Lerp(NPC.localAI[1], Utils.GetLerpValue(max * 1.025f, max * 1.225f, dist, true), lerpValue);
        float opacity = 1f - NPC.localAI[1];
        float minDist = 60f;
        float value = 1f;
        if (Collision.CanHit(player.Center, 0, 0, altarPos, 0, 0) && Math.Abs(player.Center.X - altarPos.X) < 130f) {
            value = Utils.GetLerpValue(minDist, 100f, dist2, true);
        }
        NPC.localAI[2] = MathHelper.Lerp(NPC.localAI[2], value, lerpValue * 1.35f);
        opacity *= NPC.localAI[2];
        if (dist2 < NPC.localAI[2]) {
            opacity *= 0f;
        }
        opacity = Helper.EaseInOut2(opacity);
        Color drawColor = drawColor1;
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
            Color color = drawColor /*Lighting.GetColor(start.ToTileCoordinates()).MultiplyRGB(_color)*/;
            int max2 = amount - Math.Clamp((int)(amount * 0.35f), amount < 8 ? 3 : 4, 6);
            if (k > max2) {
                float progress2 = 1f - (k - max2) / (float)(amount - max2);
                progress2 = Ease.SineIn(progress2);
                color = Color.Lerp(color, drawColor2, 1f - progress2);
            }
            max2 = amount - 4;
            if (k > max2) {
                float progress2 = 1f - (k - max2) / (float)(amount - max2);
                color *= progress2;
            }
            int amount2 = 1;
            if (k < amount2) {
                float progress2 = k / (float)amount2;
                color *= progress2;
            }
            max2 = amount - 4;
            if (k > max2) {
                float progress2 = 1f - (k - max2) / (float)(amount - max2);
                color *= progress2;
            }
            color *= NPC.Opacity;
            color *= opacity;
            color *= 0.9f;
            color *= 1.5f;
            for (double i = -Math.PI; i <= Math.PI; i += Math.PI / 2.0) {
                color = color.MultiplyAlpha(NPC.Opacity).MultiplyAlpha((float)i);
                spriteBatch.Draw(texture, start + ((float)i).ToRotationVector2().RotatedBy(Main.GlobalTimeWrappedHourly * 2.0, new Vector2()) * Helper.Wave(0f, 3f, speed: 12f) - Main.screenPosition, sourceRectangle, color, rotation, origin, Helper.Wave(NPC.scale + 0.05f, NPC.scale + 0.15f, 1f, 0f) * 0.9f, SpriteEffects.None, 0f);
            }
            SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(spriteBatch);
            spriteBatch.Begin(snapshot with { blendState = BlendState.NonPremultiplied, samplerState = SamplerState.PointClamp }, true);
            for (double i = -Math.PI; i <= Math.PI; i += Math.PI / 2.0) {
                color = color.MultiplyAlpha(NPC.Opacity).MultiplyAlpha((float)i);
                spriteBatch.Draw(texture, start + ((float)i).ToRotationVector2().RotatedBy(Main.GlobalTimeWrappedHourly * 2.0, new Vector2()) * Helper.Wave(0f, 3f, speed: 12f) - Main.screenPosition, sourceRectangle, color, rotation, origin, Helper.Wave(NPC.scale + 0.05f, NPC.scale + 0.15f, 1f, 0f) * 0.9f, SpriteEffects.None, 0f);
            }
            spriteBatch.Begin(snapshot, true);
            start += v * texture.Width;
        }
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(spriteBatch);
        spriteBatch.Begin(snapshot with { blendState = BlendState.NonPremultiplied, samplerState = SamplerState.PointClamp }, true);
        DrawTextureUnderCustomSoulEffect(spriteBatch, NPC.GetTexture(), drawColor);
        spriteBatch.Begin(snapshot, true);
        DrawTextureUnderCustomSoulEffect(spriteBatch, _eyeTexture.Value, drawColor);
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