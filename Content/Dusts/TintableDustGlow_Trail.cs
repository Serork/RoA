using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class TintableDustGlow_Trail : ModDust {
    public override string Texture => DustLoader.GetDust(ModContent.DustType<TintableDustGlow>()).Texture;

    public override bool Update(Dust dust) {
        dust.BasicDust(!dust.noGravity);

        float num78 = (float)(int)dust.color.R / 255f;
        float num79 = (float)(int)dust.color.G / 255f;
        float num80 = (float)(int)dust.color.B / 255f;
        num78 *= dust.scale * 1.07f * num78;
        num79 *= dust.scale * 1.07f * num79;
        num80 *= dust.scale * 1.07f * num80;
        dust.rotation += 0.1f * dust.scale;

        dust.fadeIn -= 0.1f;
        if (dust.customData is null) {
            dust.fadeIn = 0f;
        }
        if (dust.fadeIn <= 0f) {
            if (dust.alpha < 255) {
                dust.scale += 0.09f;
                if (dust.scale >= 1f) {
                    dust.scale = 1f;
                    dust.alpha = 255;
                }
            }
            else {
                if ((double)dust.scale < 0.8)
                    dust.scale -= 0.01f;

                if ((double)dust.scale < 0.5)
                    dust.scale -= 0.01f;
            }
        }

        if (!dust.noLightEmittence)
            Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), num78, num79, num80);

        if (dust.customData != null && dust.customData is Player) {
            Player player7 = (Player)dust.customData;
            dust.position += player7.position - player7.oldPosition;
        }

        return false;
    }

    public override Color? GetAlpha(Dust dust, Color lightColor) {
        if (dust.customData != null && dust.customData is float) {
            return dust.color;
        }

        float num = (float)(255 - dust.alpha) / 255f;
        num = (num + 9f) / 10f;
        lightColor = dust.color;
        float num6 = (int)((float)(int)lightColor.R * num);
        float num5 = (int)((float)(int)lightColor.G * num);
        float num4 = (int)((float)(int)lightColor.B * num);
        int num8 = lightColor.A - dust.alpha;
        if (num8 < 0)
            num8 = 0;

        if (num8 > 255)
            num8 = 255;

        return new Color(num6, num5, num4, num8);
    }

    public override bool PreDraw(Dust dust) {
        if (dust.customData != null && dust.customData is float) {
            dust.QuickDraw(Texture2D.Value);

            return false;
        }

        float num4 = Math.Abs(dust.velocity.X) + Math.Abs(dust.velocity.Y);
        num4 *= 10f;
        if (num4 > 10f)
            num4 = 10f;

        SpriteBatch spriteBatch = Main.spriteBatch;
        for (int m = 0; (float)m < num4; m++) {
            Vector2 velocity4 = dust.velocity;
            Vector2 vector4 = dust.position - velocity4 * m;
            float scale5 = dust.scale * (1f - (float)m / 10f);
            Microsoft.Xna.Framework.Color color4 = Lighting.GetColor((int)((double)dust.position.X + 4.0) / 16, (int)((double)dust.position.Y + 4.0) / 16);
            color4 = dust.GetAlpha(color4) * (1f - (float)m / num4) * 0.75f;
            spriteBatch.Draw(Texture2D.Value, vector4 - Main.screenPosition, dust.frame, color4, dust.rotation, new Vector2(5f), scale5, SpriteEffects.None, 0f);
            color4 = dust.GetColor(color4) * 0.75f;
            spriteBatch.Draw(Texture2D.Value, vector4 - Main.screenPosition, dust.frame, color4, dust.rotation, new Vector2(5f), scale5, SpriteEffects.None, 0f);
        }

        return true;
    }
}
