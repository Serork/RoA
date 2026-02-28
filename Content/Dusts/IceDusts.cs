using Microsoft.Xna.Framework;

using RoA.Common.Dusts;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Ice_Normal : ModDust {
    public override string Texture => DustLoader.GetDust(ModContent.DustType<Ice>()).Texture;

    public override void OnSpawn(Dust dust) {
        dust.alpha = 50;
    }

    public override bool Update(Dust dust) {
        dust.BasicDust();

        if (dust.customData != null && dust.customData is Player) {
            Player player9 = (Player)dust.customData;
            dust.position += player9.position - player9.oldPosition;
        }

        return false;
    }
}

sealed class Ice : ModDust, IDrawDustPreProjectiles {
    public override Color? GetAlpha(Dust dust, Color lightColor) => lightColor * (1f - dust.alpha / 255f);

    public override void SetStaticDefaults() => UpdateType = DustID.Ice;

    void IDrawDustPreProjectiles.DrawPreProjectiles(Dust dust) {
        dust.QuickDraw(Texture2D.Value);
    }

    public override bool PreDraw(Dust dust) => false;
}

sealed class Ice2 : ModDust, IDrawDustPreProjectiles {
    public override Color? GetAlpha(Dust dust, Color lightColor) => lightColor * (1f - dust.alpha / 255f);

    public override void SetStaticDefaults() => UpdateType = DustID.BubbleBurst_Blue;

    void IDrawDustPreProjectiles.DrawPreProjectiles(Dust dust) {
        dust.QuickDraw(Texture2D.Value);
    }

    public override bool PreDraw(Dust dust) => false;
}