using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Cache;
﻿using RoA.Common.World;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class Lothor : ModNPC {
    private Vector2 _drawOffset;
    private float _trailOpacity;
    private Color? _drawColor = null;
    private float _glowMaskOpacity;

    public static Asset<Texture2D> ItsSpriteSheet { get; private set; } = null!;
    public static Asset<Texture2D> GlowMask { get; private set; } = null!;

    public partial void LoadTextures() {
        if (Main.dedServ) {
            return;
        }

        ItsSpriteSheet = ModContent.Request<Texture2D>(Texture + "_Spritesheet");
        GlowMask = ModContent.Request<Texture2D>(Texture + "_Spritesheet_Glow");
    }

    public override void FindFrame(int frameHeight) {
        HandleAnimations();
        GetFrameInfo(out ushort x, out ushort y, out ushort width, out ushort height);
        NPC.frame = new Rectangle(x, y, width, height);
    }

    partial void HandleAnimations();

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Vector2 origin = NPC.frame.Size() / 2f;
        SpriteEffects effects = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Vector2 positionOffset = Vector2.UnitY * _drawOffset + new Vector2(0f, 2f);
        Vector2 offset = positionOffset - screenPos + origin / 2f;
        if (NPC.IsABestiaryIconDummy) {
            spriteBatch.Draw(ItsSpriteSheet.Value, NPC.position + offset - Vector2.UnitY * 6f, NPC.frame, drawColor * NPC.Opacity, NPC.rotation, origin, NPC.scale, effects, 0f);

            return false;
        }

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
        Color color = Lighting.GetColor(NPC.Center.ToTileCoordinates());
        void enrage(ref Color color) {
            if (_shouldEnrage) {
                color = Color.Lerp(Helper.BuffColor(color, 0.3f, 0.3f, 0.3f, 1f), color, 0.6f);
            }
        }
        drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
        enrage(ref drawColor);
        if (_drawColor == null) {
            _drawColor = color;
        }
        _drawColor = Color.Lerp(_drawColor.Value, color, 0.1f);
        if (_isDead) {
            _glowMaskOpacity = 1f;
        }
        enrage(ref color);
        float glowMaskOpacity = _glowMaskOpacity;
        Vector2 altarPosition = AltarHandler.GetAltarPosition().ToWorldCoordinates();
        float minDistance = 300f;
        Vector2 center = NPC.Center;
        float distance = center.Distance(altarPosition + altarPosition.DirectionTo(center) * minDistance) * 2f;
        float altarOpacity = MathUtils.Clamp01(1f - distance / minDistance);
        if (center.Distance(altarPosition) < minDistance) {
            altarOpacity = 1f;
        }
        float lifeProgress = LifeProgress;
        lifeProgress = MathF.Max(lifeProgress, altarOpacity);
        glowMaskOpacity = MathF.Max(glowMaskOpacity, altarOpacity);
        Color glowMaskColor = (_isDead ? Color.Lerp(Color.White, Color.Black.MultiplyRGB(drawColor), MathHelper.Clamp(_deadStateProgress, 0f, 1f)) : Color.White) * glowMaskOpacity;
        glowMaskColor *= 0.95f;
        if (!_isDead) {
            for (int num173 = 1; num173 < length; num173 += 2) {
                _ = ref NPC.oldPos[num173];
                Color color39 = color;
                color39.R = (byte)(1f * (double)(int)color39.R * (double)(length - num173) / length);
                color39.G = (byte)(1f * (double)(int)color39.G * (double)(length - num173) / length);
                color39.B = (byte)(1f * (double)(int)color39.B * (double)(length - num173) / length);
                color39.A = (byte)(1f * (double)(int)color39.A * (double)(length - num173) / length);
                color39 *= MathHelper.Clamp(NPC.velocity.Length(), 0f, 9f) / 9f;
                color39 *= 1f - num173 / length;
                color39 *= _trailOpacity;
                color39 *= 0.8f;
                Rectangle frame7 = NPC.frame;
                int num174 = ItsSpriteSheet.Value.Height / Main.npcFrameCount[Type];
                frame7.Y -= num174 * num173;
                while (frame7.Y < 0) {
                    frame7.Y += num174 * Main.npcFrameCount[Type];
                }
                Vector2 pos = NPC.oldPos[num173];
                spriteBatch.Draw(ItsSpriteSheet.Value,
                    pos + offset,
                    frame7, color39 * NPC.Opacity, NPC.rotation, origin, NPC.scale, effects, 0f);
                spriteBatch.Draw(GlowMask.Value,
                    pos + offset,
                    frame7, color39 * NPC.Opacity, NPC.rotation, origin, NPC.scale, effects, 0f);
            }
        }

        if (ShouldDrawPulseEffect) {
            Microsoft.Xna.Framework.Color color46 = Microsoft.Xna.Framework.Color.White;
            int num275 = 6;
            float num278 = 0f;
            float maxOffset = 15f;
            for (int num293 = 0; num293 < num275; num293++) {
                Microsoft.Xna.Framework.Color value80 = color;
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
                spriteBatch.Draw(ItsSpriteSheet.Value, position33, NPC.frame, value80, NPC.rotation, origin, NPC.scale, effects, 0f);
                value80 = glowMaskColor;
                value80 = Microsoft.Xna.Framework.Color.Lerp(value80, color46, 0f);
                value80 = NPC.GetAlpha(value80);
                value80 = Microsoft.Xna.Framework.Color.Lerp(value80, color46, 0f);
                opacity = 1f - _pulseStrength;
                value80 *= opacity;
                value80 *= 0.7f;
                spriteBatch.Draw(GlowMask.Value, position33, NPC.frame, value80 * NPC.Opacity, NPC.rotation, origin, NPC.scale, effects, 0f);
            }
        }

        spriteBatch.Draw(ItsSpriteSheet.Value, NPC.position + offset, NPC.frame, drawColor * NPC.Opacity, NPC.rotation, origin, NPC.scale, effects, 0f);
        spriteBatch.Draw(GlowMask.Value, NPC.position + offset, NPC.frame, glowMaskColor * Utils.GetLerpValue(0f, 0.25f, NPC.Opacity, true), NPC.rotation, origin, NPC.scale, effects, 0f);

        SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(spriteBatch);
        spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
        for (float i = -MathHelper.Pi; i <= MathHelper.Pi; i += MathHelper.PiOver2) {
            spriteBatch.Draw(GlowMask.Value, NPC.position + offset +
                Utils.RotatedBy(Utils.ToRotationVector2(i), Main.GlobalTimeWrappedHourly * 10.0, new Vector2())
                * Helper.Wave(0f, 3f, 12f, 0.5f) * lifeProgress,
                NPC.frame, (Color.White * 0.95f).MultiplyAlpha(Helper.Wave(0.5f, 0.75f, 12f, 0.5f)) * lifeProgress * NPC.Opacity, NPC.rotation + Main.rand.NextFloatRange(0.05f) * lifeProgress, origin, NPC.scale, effects, 0f);
        }
        spriteBatch.Begin(snapshot, true);

        DrawWreath(spriteBatch);

        return false;
    }

    private Vector2 WreathOffset() {
        float offset = 50f + 20f * Ease.QuintOut(_wreathProgress);
        return (_wreathLookingPosition - NPC.Center).SafeNormalize(Vector2.One) * offset;
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
        sourceRectangle.X = texture.Width / 2;
        Vector2 position = NPC.Center - Main.screenPosition + WreathOffset();
        SpriteEffects spriteEffects = SpriteEffects.FlipHorizontally;
        float rotation = MathHelper.Pi;
        spriteBatch.Draw(texture, position, sourceRectangle, Color.White * 0.95f, rotation, sourceRectangle.Size() / 2f, 1f, spriteEffects, 0);
        sourceRectangle = frame.GetSourceRectangle(texture);
        float progress = _distanceProgress2 == 0f || _distanceProgress2 == -1f ? 1f : _distanceProgress / _distanceProgress2;
        int height = (int)(texture.Height * progress);
        sourceRectangle.Height = height;
        height = (int)(texture.Height * (1f - progress));
        position.Y += height / 2f;
        spriteBatch.Draw(texture, position, sourceRectangle, Color.White * 0.95f, rotation, sourceRectangle.Size() / 2f, 1f, spriteEffects, 0);

        if (!Main.dedServ) {
            Lighting.AddLight(NPC.Center + WreathOffset(), new Vector3(1f, 0.2f, 0.2f) * 1.25f * progress);
        }
    }
}
