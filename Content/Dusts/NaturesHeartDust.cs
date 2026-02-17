using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class NaturesHeartDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.Lerp(lightColor, Color.White * 0.9f, 0.5f);

    public override void OnSpawn(Dust dust) => UpdateType = 184;

    public override bool PreDraw(Dust dust) {
        if (!Main.dedServ) {
            Lighting.AddLight(dust.position + Vector2.UnitX * 2f,
         (Color.Green * 1f).ToVector3() * 0.5f);
        }

        return true;
    }
}