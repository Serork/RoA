using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Dusts;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class WreathDust3 : WreathDust { }

sealed class WreathDust2 : WreathDust { }

class WreathDust : ModDust, IDrawDustPostPlayer {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        Color color = new(255, 255, 200, 200);
        return color;
    }

    public override void OnSpawn(Dust dust) => UpdateType = DustID.FireworksRGB;

    public override bool PreDraw(Dust dust) => false;

    void IDrawDustPostPlayer.DrawPostPlayer(Dust dust) {
        Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture).Value, dust.position - Main.screenPosition, dust.frame, dust.GetAlpha(dust.color), dust.rotation, dust.frame.Size() / 2f, dust.scale, 0, 0);
        Color color = dust.GetAlpha(dust.color);
        color *= 1.4f;
        color.A = (byte)Utils.Remap(Math.Clamp((float)dust.customData * 120f, 175, 255), 175, 255, 255, 175, false);
        Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture).Value, dust.position - Main.screenPosition, dust.frame, color, dust.rotation, dust.frame.Size() / 2f, dust.scale, 0, 0);
    }
}