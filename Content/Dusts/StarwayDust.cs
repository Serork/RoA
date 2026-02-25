using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class StarwayDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(Color.White.R, Color.White.G, Color.White.B, 25);

    public override void SetStaticDefaults() {

    }

    public override void OnSpawn(Dust dust) {
        dust.velocity.Y = (float)Main.rand.Next(-10, 6) * 0.1f;
        dust.velocity.X *= 0.3f;
        dust.scale *= 0.7f;
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

        if (!dust.noLight && !dust.noLightEmittence) {
            float num56 = dust.scale * 1.4f;
            if (num56 > 0.6f)
                num56 = 0.6f;

            Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), num56 * 1f, num56 * 0.9f, num56 * 0.13f);
        }

        return false;
    }

    public override bool PreDraw(Dust dust) {
        float opacity = 1f;
        Color baseColor = Color.White;
        baseColor = baseColor.MultiplyAlpha(0.75f);
        baseColor *= opacity;
        Color color = baseColor * Helper.Wave(0.5f, 0.75f, 5f, 0f);
        float scale = dust.GetVisualScale();
        Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, dust.frame, color, dust.GetVisualRotation(), new Vector2(4f, 4f), scale, SpriteEffects.None, 0f);
        float num184 = Helper.Wave(2f, 6f, 1f, 0f);
        for (int num185 = 0; num185 < 4; num185++) {
            Main.spriteBatch.Draw(Texture2D.Value, Vector2.UnitX.RotatedBy((float)num185 * ((float)Math.PI / 4f) - Math.PI) * num184 + dust.position - Main.screenPosition, dust.frame, new Microsoft.Xna.Framework.Color(64, 64, 64, 0) * 0.25f * opacity, dust.GetVisualRotation(), new Vector2(4f, 4f), scale, SpriteEffects.None, 0f);
        }

        return false;
    }
}
