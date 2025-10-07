using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Slash : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        Color color = dust.color;
        color.A = 50;

        bool flag = dust.customData is float v && v == 1f;
        if (!flag) {
            Point pos = dust.position.ToTileCoordinates();
            float brightness = MathHelper.Clamp(Lighting.Brightness(pos.X, pos.Y), 0f, 1f);
            color *= brightness;
        }

        return color;
    }

    public override bool Update(Dust dust) {
        if (dust.noGravity) {
            dust.velocity *= 0.95f;
            if (dust.fadeIn == 0.0) {
                dust.scale += 1f / 1000f;
            }
        }
        else {
            dust.velocity *= 0.98f;
            dust.scale -= 1f / 1000f;
        }
        if (WorldGen.SolidTile(Framing.GetTileSafely(dust.position)) && dust.fadeIn == 0f && !dust.noGravity) {
            dust.scale *= 0.99f;
            dust.velocity *= 0.9f;
        }
        if (!dust.noLight) {
            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.5f * dust.scale);
        }
        return true;
    }
}
