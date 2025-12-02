using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class CoralBubble : ModDust {
    public override void OnSpawn(Dust dust) {
        dust.noGravity = true;
        dust.noLight = true;
        dust.frame = Texture2D.Value.Bounds;

        dust.velocity.Y = MathF.Abs(dust.velocity.Y) * -1f;

        dust.alpha = Main.rand.Next(100, 225);
    }

    public override bool Update(Dust dust) {
        dust.position += dust.velocity;
        dust.rotation += dust.velocity.X * 0.15f;
        dust.scale *= 0.99f;
        if (dust.scale < 0.3f) {
            dust.active = false;
        }
        dust.velocity.X *= 0.9f;
        dust.velocity.Y -= 0.1f;
        dust.velocity.Y = Math.Max(-5f, dust.velocity.Y);

        return false;
    }
}
