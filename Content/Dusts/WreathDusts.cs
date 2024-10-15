using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class WreathDust3 : WreathDust { }

sealed class WreathDust2 : WreathDust { }

class WreathDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        //float alpha = Lighting.Brightness((int)dust.position.X / 16, (int)dust.position.Y / 16);
        //alpha = (alpha + 1f) / 2f;
        //Color color = Color.Multiply(Utils.MultiplyRGB(dust.color, lightColor * (float)dust.customData), alpha);
        Color color = new(255, 255, 200, 200);
        return color;
    }

    public override void OnSpawn(Dust dust) => UpdateType = DustID.FireworksRGB;
}