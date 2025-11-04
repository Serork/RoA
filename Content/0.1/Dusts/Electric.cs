using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Electric : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.White * 0.9f;

    public override void OnSpawn(Dust dust) {
        UpdateType = DustID.Electric;
        dust.noLight = true;
    }

    public override bool PreDraw(Dust dust) {
        if (!Main.dedServ) {
            Lighting.AddLight(dust.position, new Color(99, 200, 204).ToVector3() * 0.625f);
        }

        return base.PreDraw(dust);
    }
}
