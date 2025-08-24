using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class ToxicFumes : ModDust {
    public override void OnSpawn(Dust dust) {
        int width = 24; int height = 24;
        int frame = Main.rand.Next(0, 3);
        dust.frame = new Rectangle(0, frame * height, width, height);

        dust.noGravity = true;
        dust.noLight = false;
        dust.scale *= 1f;
    }

    public override Color? GetAlpha(Dust dust, Color lightColor)
        => dust.color;

    public override bool Update(Dust dust) {
        Helper.ApplyWindPhysics(dust.position, ref dust.velocity);

        float opacity = Utils.GetLerpValue(0f, 0.2f, 1f - (float)dust.alpha / 255, true);
        dust.color = Lighting.GetColor((int)(dust.position.X / 16), (int)(dust.position.Y / 16)).MultiplyRGB(new Color(106, 140, 34, 100)) * 0.1f * opacity;
        dust.position += dust.velocity * 0.12f;
        dust.scale *= 0.99f;
        dust.velocity *= 0.97f;
        dust.alpha += 3;
        if (dust.alpha >= 250) dust.active = false;
        return false;
    }
}