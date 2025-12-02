using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

class GrimDruidDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        Color color = new Color(200, 200, 200, 0);
        if (dust.customData is float value && value == 1f) {
            float progress = 1f - dust.alpha / 255f;
            color *= Utils.GetLerpValue(0f, 0.1f, progress, true);
        }
        return color
        * Utils.GetLerpValue(0f, 0.25f, 1f - dust.alpha / 255f, true);
    }

    public override void OnSpawn(Dust dust) {
        //UpdateType = DustID.Clentaminator_Red;
    }

    public override bool Update(Dust dust) {
        float num86 = dust.scale * 0.1f;
        if (num86 > 1f)
            num86 = 1f;

        if (!dust.noGravity) {
            dust.velocity.Y *= 0.99f;
            dust.velocity.X *= 0.99f;
        }
        else {
            dust.velocity.Y *= 0.5f;
            dust.velocity.X *= 0.5f;
        }
        dust.position += dust.velocity;
        dust.scale -= 0.01f;
        if (dust.scale < 0.1f)
            dust.active = false;

        if (dust.alpha > 0) {
            dust.alpha -= 10;
        }

        float value = 1f - dust.alpha / 255f;
        Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), num86 * 1.2f * value, num86 * 0.5f * value, num86 * 0.4f * value);

        return false;
    }
}

sealed class ArchdruidDust : GrimDruidDust { }