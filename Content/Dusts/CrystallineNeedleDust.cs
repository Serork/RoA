using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Dusts;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class CrystallineNeedleDust : ModDust, IDrawDustPrePlayer {
    public override bool Update(Dust dust) {
        dust.BasicDust();

        float num86 = dust.scale * 0.1f;
        if (num86 > 1f)
            num86 = 1f;

        //if (!dust.noGravity) {
        //    dust.velocity.Y *= 0.999f;
        //    dust.velocity.X *= 0.999f;
        //}
        //else {
        //    dust.velocity.Y *= 0.5f;
        //    dust.velocity.X *= 0.5f;
        //}
        dust.position += dust.velocity;
        dust.scale -= 0.0025f;
        if (dust.scale < 0.1f)
            dust.active = false;

        if (dust.alpha > 0) {
            dust.alpha -= 2;
        }

        return false;
    }

    public override bool PreDraw(Dust dust) {
        Color color = Color.White.MultiplyRGB(dust.color) with { A = (byte)(dust.color.A / 2) } * (1f - dust.alpha / 255f);
        Texture2D texture = Texture2D.Value;
        Rectangle sourceRectangle = dust.frame;
        Vector2 origin = sourceRectangle.Centered();
        float scale = dust.scale;
        float rotation = dust.rotation;
        Main.EntitySpriteDraw(texture, dust.position - Main.screenPosition, sourceRectangle, color, rotation, origin, scale, 0, 0);
        float waveOffset = dust.customData is float ? (float)dust.customData : 0f;
        for (float num10 = -0.02f; num10 <= 0.02f; num10 += 0.01f) {
            float num11 = (float)Math.PI * 2f * num10 * 0.5f;
            Vector2 vector2 = dust.position + num11.ToRotationVector2() * 2f * dust.velocity.X.GetDirection();
            float alpha = Helper.Wave(0.5f, 0.75f, 10f, waveOffset + num10 * 200f);
            DrawData item = new(texture, vector2, sourceRectangle,
                color.MultiplyAlpha(alpha) * MathHelper.Lerp(0.5f, 0.625f, 0.5f) * 0.5f, rotation + num11, origin, scale, 0);
            item.Draw(Main.spriteBatch);
        }
        for (float num10 = -0.01f; num10 <= 0.01f; num10 += 0.005f) {
            float num11 = (float)Math.PI * 2f * num10 * 0.5f;
            Vector2 vector2 = dust.position + num11.ToRotationVector2() * 2f * dust.velocity.X.GetDirection();
            float alpha = Helper.Wave(0.5f, 0.75f, 10f, waveOffset + num10 * 200f);
            DrawData item = new(texture, vector2, sourceRectangle,
                color.MultiplyAlpha(alpha) * MathHelper.Lerp(0.5f, 0.625f, 0.5f) * 0.75f, rotation + num11, origin, scale, 0);
            item.Draw(Main.spriteBatch);
        }

        return false;
    }

    void IDrawDustPrePlayer.DrawPrePlayer(Dust dust) {

    }
}
