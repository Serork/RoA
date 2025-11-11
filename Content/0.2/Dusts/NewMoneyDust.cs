using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class NewMoneyDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        int a = lightColor.A;
        lightColor = Color.Lerp(lightColor, Color.Lerp(Color.White, NewMoneyBullet.BulletColor, 0.75f), 0.5f);
        return new Color(lightColor.R, lightColor.G, lightColor.B, a) * MathHelper.Min(dust.scale, 1f);
    }

    public override void SetStaticDefaults() => UpdateType = DustID.SilverFlame;
}
