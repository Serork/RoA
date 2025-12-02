using Microsoft.Xna.Framework;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Bubble : ModDust {
    private static byte FRAMEWIDTH => 16;
    private static byte FRAMEHEIGHT => 16;
    private static byte FRAMECOUNT => 3;

    public override void OnSpawn(Dust dust) {
        dust.velocity *= 0.1f;
        dust.noGravity = true;
        dust.noLight = true;

        dust.frame = new Rectangle(0, FRAMEHEIGHT * Main.rand.Next(FRAMECOUNT), FRAMEWIDTH, FRAMEHEIGHT);
    }

    public override bool Update(Dust dust) {
        dust.position += dust.velocity;
        dust.rotation += dust.velocity.X * 0.15f;
        dust.scale *= 0.99f;
        if (dust.scale < 0.3f) {
            dust.active = false;
        }
        dust.velocity.Y -= 0.1f;
        dust.velocity.Y = Math.Max(-5f, dust.velocity.Y);

        return false;
    }
}
