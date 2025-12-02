using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Dusts;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

public class BrightstoneDust : ModDust, IDrawDustPrePlayer {
    void IDrawDustPrePlayer.DrawPrePlayer(Dust dust) {
        Main.EntitySpriteDraw(DustLoader.GetDust(dust.type).Texture2D.Value, dust.position - Main.screenPosition, dust.frame, dust.GetAlpha(dust.color), dust.rotation, dust.frame.Size() / 2f, dust.scale, 0, 0);
    }

    public override bool PreDraw(Dust dust) => false;

    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.White * 0.9f;

    public override bool Update(Dust dust) {
        dust.velocity *= 0.98f;

        float num94 = dust.scale * 0.8f;

        if (num94 > 1f)
            num94 = 1f;

        Lighting.AddLight(dust.position, new Color(238, 225, 111).ToVector3() * 0.6f * num94);

        return true;
    }
}