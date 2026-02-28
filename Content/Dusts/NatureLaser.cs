using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

public class NatureLaser : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.White * 0.9f;

    public override bool Update(Dust dust) {
        Lighting.AddLight(dust.position, 73f / 255f, 170f / 255f, 104f / 255f);

        DustHelper.BasicDust(dust);

        if (dust.customData != null && dust.customData is Player) {
            Player player9 = (Player)dust.customData;
            dust.position += player9.position - player9.oldPosition;
        }

        return false;
    }
}