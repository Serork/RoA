using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class SunSigil : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        return dust.color.MultiplyRGB(new Color(230, 230, 230, 85)) with { A = 50 };
    }

    public override bool Update(Dust dust) {
        if (!dust.noLight && !dust.noLightEmittence) {
            float num22 = dust.scale * 0.3f;
            if (num22 > 1f)
                num22 = 1f;

            Lighting.AddLight(dust.position, new Vector3(0.4f, 0.6f, 0.7f) * num22);
        }

        if (dust.noGravity) {
            dust.velocity *= 0.93f;
            if (dust.fadeIn == 0f)
                dust.scale += 0.0025f;
        }

        dust.velocity *= new Vector2(0.97f, 0.99f);
        dust.scale -= 0.0025f;
        if (dust.customData != null && dust.customData is Player) {
            Player player4 = (Player)dust.customData;
            dust.position += player4.position - player4.oldPosition;
        }

        dust.velocity *= 0.95f;

        dust.BasicDust();

        return false;
    }

    public override bool PreDraw(Dust dust) {
        Main.EntitySpriteDraw(ResourceManager.Bloom, dust.position - Main.screenPosition, null,
                (dust.GetAlpha(Lighting.GetColor(dust.position.ToTileCoordinates())) with { A = 0 }) * 0.375f, dust.rotation, ResourceManager.Bloom.Size() / 2f, dust.scale * 0.125f, 0, 0);

        dust.QuickDraw(Texture2D.Value);

        return false;
    }
}
