using Microsoft.Xna.Framework;

using RoA.Common.Dusts;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Ice : ModDust, IDrawDustPreProjectiles {
    public override Color? GetAlpha(Dust dust, Color lightColor) => lightColor * (1f - dust.alpha / 255f);

    public override void OnSpawn(Dust dust) => UpdateType = DustID.Ice;

    void IDrawDustPreProjectiles.DrawPreProjectiles(Dust dust) {
        dust.QuickDraw(Texture2D.Value);
    }

    public override bool PreDraw(Dust dust) => false;
}

sealed class Ice2 : ModDust, IDrawDustPreProjectiles {
    public override Color? GetAlpha(Dust dust, Color lightColor) => lightColor * (1f - dust.alpha / 255f);

    public override void OnSpawn(Dust dust) => UpdateType = DustID.BubbleBurst_Blue;

    void IDrawDustPreProjectiles.DrawPreProjectiles(Dust dust) {
        dust.QuickDraw(Texture2D.Value);
    }

    public override bool PreDraw(Dust dust) => false;
}