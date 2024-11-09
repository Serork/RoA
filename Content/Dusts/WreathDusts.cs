using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using Terraria.GameContent;
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

    public override bool PreDraw(Dust dust) {
        Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture).Value, dust.position - Main.screenPosition, dust.frame, dust.GetAlpha(dust.color), dust.rotation, dust.frame.Size() / 2f, dust.scale, 0, 0);
        Color color = dust.GetAlpha(dust.color);
        color *= 1.4f;
        color.A = (byte)Math.Clamp(255 - (float)dust.customData * 110f, 180, 255);
        Main.NewText(color.A);
        Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture).Value, dust.position - Main.screenPosition, dust.frame, color, dust.rotation, dust.frame.Size() / 2f, dust.scale, 0, 0);

        return false;
    }
}