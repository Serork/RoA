using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;
sealed class SnowDust3 : ModDust {
    public override bool Update(Dust dust) {
        dust.BasicDust();

        dust.scale += 0.009f;
        float y = Main.player[Main.myPlayer].velocity.Y;
        if (y > 0f && dust.fadeIn == 0f && dust.velocity.Y < y)
            dust.velocity.Y = MathHelper.Lerp(dust.velocity.Y, y, 0.04f);

        if (!dust.noLight && y > 0f)
            dust.position.Y += Main.player[Main.myPlayer].velocity.Y * 0.2f;

        //if (Collision.SolidCollision(dust.position - Vector2.One * 5f, 10, 10) && dust.fadeIn == 0f) {
        //    dust.scale *= 0.9f;
        //    dust.velocity *= 0.25f;
        //}

        return false;
    }
}
