using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class MushroomFumes : ModDust {
    public override void OnSpawn(Dust dust) {
        int width = 24; int height = 24;
        int frame = Main.rand.Next(0, 3);
        dust.frame = new Rectangle(0, frame * height, width, height);

        dust.noGravity = true;
        dust.noLight = false;
        dust.scale *= 1f;
    }

    public override Color? GetAlpha(Dust dust, Color lightColor) {
        bool hasCustomData = dust.customData is float;
        float opacity = Utils.GetLerpValue(0f, 0.2f, 1f - (float)dust.alpha / 255, true);
        Color color = Lighting.GetColor((int)(dust.position.X / 16), (int)(dust.position.Y / 16)).MultiplyRGB(dust.color) * (hasCustomData ? (float)dust.customData : 0.1f) * opacity;

        Color result = color;
        result *= Utils.GetLerpValue(0, 30, dust.alpha, true) * Utils.GetLerpValue(255, 225, dust.alpha, true);

        return result;
    }

    public override bool Update(Dust dust) {
        Helper.ApplyWindPhysics(dust.position, ref dust.velocity);

        dust.position += dust.velocity * 0.12f;
        dust.scale *= 0.99f;
        dust.velocity *= 0.97f;
        dust.alpha += 3;
        if (dust.alpha >= 250) dust.active = false;

        int direction = (dust.velocity.X + dust.velocity.Y).GetDirection();
        dust.rotation += dust.velocity.Length() * 0.0025f * direction;

        return false;
    }
}