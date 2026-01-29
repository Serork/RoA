using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Smoke2 : ModDust {
    public override void OnSpawn(Dust dust) {
        dust.noGravity = true;
        dust.scale *= Main.rand.NextFloat(0.8f, 2f);
        dust.frame = new Rectangle(0, 0, 34, 36);
        dust.rotation = Main.rand.NextFloat(6.28f);
    }

    public override Color? GetAlpha(Dust dust, Color lightColor) {
        return dust.color * 0.5f;
    }

    public override bool PreDraw(Dust dust) {
        Main.EntitySpriteDraw(DustLoader.GetDust(dust.type).Texture2D.Value, dust.position - Main.screenPosition, dust.frame, dust.GetAlpha(dust.color), dust.rotation, dust.frame.Size() / 2f, dust.scale, 0, 0);

        return false;
    }

    public override bool Update(Dust dust) {
        dust.ApplyDustScale();

        dust.velocity *= 0.98f;
        dust.velocity.X *= 0.95f;
        dust.color *= 0.98f;

        if (dust.alpha > 100) {
            dust.scale *= 0.975f;
            dust.alpha += 2;
        }

        dust.position += dust.velocity;
        dust.rotation += 0.04f;

        if (dust.alpha >= 255)
            dust.active = false;

        return false;
    }
}