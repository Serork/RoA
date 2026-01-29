using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core.Utility;

using System;
using System.ComponentModel;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;
sealed class DistilleryOfDeathGustDust : ModDust {
    public override void OnSpawn(Dust dust) {
        dust.velocity.Y = (float)Main.rand.Next(-10, 6) * 0.1f;
        dust.velocity.X *= 0.3f;
        dust.scale *= 0.7f;
        dust.position -= Vector2.One;
    }

    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        if (dust.customData != null && dust.customData is int) {
            if ((int)dust.customData == 0) {
                if (Collision.SolidCollision(dust.position - Vector2.One * 5f, 10, 10) && dust.fadeIn == 0f) {
                    dust.scale *= 0.9f;
                    dust.velocity *= 0.25f;
                }
            }
            else if ((int)dust.customData == 1) {
                dust.scale *= 0.98f;
                dust.velocity.Y *= 0.98f;
                if (Collision.SolidCollision(dust.position - Vector2.One * 5f, 10, 10) && dust.fadeIn == 0f) {
                    dust.scale *= 0.9f;
                    dust.velocity *= 0.25f;
                }
            }
        }

        //if (!dust.noLight && !dust.noLightEmittence) {
        //    float num56 = dust.scale * 1.4f;
        //    if (num56 > 0.6f)
        //        num56 = 0.6f;

        //    Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), num56, num56 * 0.65f, num56 * 0.4f);
        //}

        return false;
    }

    public override bool PreDraw(Dust dust) {
        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = DustLoader.GetDust(dust.type).Texture2D.Value;
        Vector2 position = dust.position - Main.screenPosition;
        Rectangle clip = dust.frame;
        Vector2 origin = clip.Centered();
        SpriteEffects flip = SpriteEffects.None;
        Color lightColor = Lighting.GetColor(dust.position.ToTileCoordinates());
        Color baseColor = DistilleryOfDeathGust.GetColorPerType((DistilleryOfDeathGust.GustType)(float)dust.customData).MultiplyRGB(lightColor);
        float rotation = dust.rotation;
        float scale = 1f * MathF.Max(0.75f, dust.scale);
        float opacity = 0.5f * Utils.GetLerpValue(0.25f, 1f, dust.scale, true);
        for (float num11 = 0f; num11 < 1f; num11 += 1f / 3f) {
            float num12 = (TimeSystem.TimeForVisualEffects) % 2f / 1f;
            Color color = Main.hslToRgb((num12 + num11) % 1f, 1f, 0.5f).MultiplyRGB(baseColor);
            color.A = 0;
            color *= 0.5f;
            for (int j = 0; j < 2; j++) {
                for (int k = 0; k < 2; k++) {
                    Vector2 drawPosition = position + ((num12 + num11) * ((float)Math.PI * 2f)).ToRotationVector2() * 2f;
                    batch.Draw(texture, drawPosition, clip, Color.Lerp(baseColor, color, 0.5f) * opacity, rotation, origin, scale, flip, 0f);
                }
            }
        }
        baseColor.A = 100;
        baseColor *= 0.75f;
        batch.Draw(texture, position, clip, baseColor * opacity, rotation, origin, scale, flip, 0f);

        return false;
    }
}
