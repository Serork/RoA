using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

class LothorPoison2 : ModDust {
    public override string Texture => base.Texture[..^1];

    public override Color? GetAlpha(Dust dust, Color lightColor) {
        Color result = Color.White * 0.9f * (float)(1f - (float)dust.alpha / 255);
        return result;
    }

    public override void OnSpawn(Dust dust) {
        DustHelper.BasicDust(dust);
    }
}

class LothorPoison : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        Color result = lightColor * (float)(1f - (float)dust.alpha / 255);
        return result;
    }

    public override void OnSpawn(Dust dust) {
        DustHelper.BasicDust(dust);
    }
}