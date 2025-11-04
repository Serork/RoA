using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

public class NatureLaser : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.White * 0.9f;

    public override void OnSpawn(Dust dust) => UpdateType = 70;

    public override bool PreDraw(Dust dust) {
        if (!Main.dedServ) {
            Lighting.AddLight(dust.position, 73f / 255f, 170f / 255f, 104f / 255f);
        }

        return base.PreDraw(dust);
    }
}