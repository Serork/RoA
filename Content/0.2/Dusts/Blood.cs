using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Blood : ModDust {
    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        if (Collision.SolidCollision(dust.position - Vector2.One * 2, 4, 4)) {
            dust.scale *= 0.9f;
            dust.velocity *= 0.25f;
        }

        if (dust.scale < 0.75f) {
            dust.scale *= 0.9f;
        }

        dust.scale -= 0.04f;

        return false;
    }
}
