using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.NPCs.Enemies.Backwoods.Hardmode;
using RoA.Core.Data;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed partial class NPCCommon : GlobalNPC {
    public int MenhirEffectAppliedBy;
    public bool IsMenhirEffectActive;
    public bool? DontTakeDamagePrevious;

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (npc.type == ModContent.NPCType<Menhir>()) {
            foreach (NPC checkNPC in Main.ActiveNPCs) {
                if (checkNPC.whoAmI == npc.whoAmI) {
                    continue;
                }
                var handler = checkNPC.GetCommon();
                if (!(handler.IsMenhirEffectActive && handler.MenhirEffectAppliedBy == npc.whoAmI)) {
                    continue;
                }

                DrawChain(spriteBatch, npc, checkNPC, drawColor, screenPos);
            }
        }

        return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        var handler = npc.GetCommon();
        if (!handler.IsMenhirEffectActive) {
            return;
        }
        DrawLock(spriteBatch, Main.npc[handler.MenhirEffectAppliedBy], npc, drawColor, screenPos);
    }

    private void DrawChain(SpriteBatch spriteBatch, NPC source, NPC target, Color drawColor, Vector2 screenPos) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Menhir>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Color drawColor2 = drawColor;
        Texture2D texture = indexedTextureAssets[(byte)Menhir.MenhirRequstedTextureType.Chain].Value,
                  lockTexture = indexedTextureAssets[(byte)Menhir.MenhirRequstedTextureType.Lock].Value;
        Rectangle? sourceRectangle = null;
        Vector2 origin = texture.Bounds.BottomCenter();
        NPC NPC = source;
        Vector2 sourceCenter = target.position + target.Size / 2f;
        Vector2 targetCenter = source.As<Menhir>().ChainCenter + Vector2.UnitY * source.height / 4f - texture.Size() / 2f;
        float mult = MathHelper.Clamp((targetCenter + targetCenter.DirectionTo(sourceCenter) * source.width).Distance(sourceCenter) / 100f, 0f, 1f);
        for (int k2 = 0; k2 < 3; k2++) {
            float width = 50f * mult * Helper.Wave(0.25f, 1f, 5f, source.whoAmI + 10 * k2);
            SimpleCurve curve = new(sourceCenter, targetCenter, Vector2.Zero);
            curve.Control = (curve.Begin + curve.End) / 2f + new Vector2(0f, width);
            Vector2 start = curve.Begin, end = curve.End;
            Vector2 between = end - start;
            float length = between.Length();
            int height = (int)(texture.Height * 0.9f);
            int amount = (int)(length / height);
            int attempts = 0;
            for (int k = 2;; ++k) {
                if (start.Distance(targetCenter) < height || attempts > amount) {
                    break;
                }
                attempts++;
                Vector2 point = curve.GetPoint((float)k / amount);
                Vector2 v = (point - start).SafeNormalize(Vector2.Zero);
                Color color = Color.Lerp(drawColor, Menhir.GlowColor * NPC.Opacity, 0.9f) * 0.75f;

                float rotation = v.ToRotation() + (float)Math.PI / 2f;

                float scale = MathHelper.Lerp(target.scale, source.scale, (float)k / amount);
                for (double i = -Math.PI; i <= Math.PI; i += Math.PI / 2.0) {
                    color = color.MultiplyAlpha(NPC.Opacity).MultiplyAlpha((float)i);
                    spriteBatch.Draw(texture, start + ((float)i).ToRotationVector2().RotatedBy(Main.GlobalTimeWrappedHourly * 2.0, new Vector2()) * Helper.Wave(0f, 3f, speed: 12f) - screenPos, sourceRectangle,
                        color * source.As<Menhir>().GlowOpacityFactor * 0.1f * Helper.Wave(k * 10f, 0.75f, 2f, 5f, k), rotation, origin, 
                        scale * Helper.Wave(scale + 0.05f, scale + 0.15f, 1f, 0f) * 0.9f, SpriteEffects.None, 0f);
                }
                start += v * (height * scale);
            }
        }
    }

    private void DrawLock(SpriteBatch spriteBatch, NPC source, NPC target, Color drawColor, Vector2 screenPos) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Menhir>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        NPC NPC = target;
        Texture2D texture = indexedTextureAssets[(byte)Menhir.MenhirRequstedTextureType.Chain].Value,
                  lockTexture = indexedTextureAssets[(byte)Menhir.MenhirRequstedTextureType.Lock].Value;
        Rectangle clip = lockTexture.Bounds;
        Vector2 origin = clip.Size() / 2f;
        Vector2 position = target.position + target.Size / 2f - screenPos;
        Color color = Color.Lerp(drawColor, Menhir.GlowColor * NPC.Opacity, 0.9f) * 0.85f;
        for (double i = -Math.PI; i <= Math.PI; i += Math.PI / 2.0) {
            color = color.MultiplyAlpha(NPC.Opacity).MultiplyAlpha((float)i);
            spriteBatch.Draw(lockTexture, position + ((float)i).ToRotationVector2().RotatedBy(Main.GlobalTimeWrappedHourly * 2.0, new Vector2()) * Helper.Wave(0f, 3f, speed: 12f), DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color * 0.25f * source.As<Menhir>().GlowOpacityFactor,
                Scale = 1.25f * target.scale * Vector2.One * Helper.Wave(NPC.scale + 0.05f, NPC.scale + 0.15f, 1f, 0f) * 0.9f,
                Rotation = 0f
            }, false);
        }
    }
}
