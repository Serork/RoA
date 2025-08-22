using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Corruptor : ModDust {
    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        if (dust.customData is bool value) {
            if (value == true) {
                UpdateType = -1;
                if (Collision.SolidCollision(dust.position - Vector2.One * 3, 6, 6)) {
                    dust.scale *= 0.9f;
                    dust.velocity *= 0.25f;
                }
            }
            else {
                UpdateType = DustID.CorruptGibs;
            }
        }

        return false;
    }
}
