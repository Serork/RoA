using RoA.Common.Dusts;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class BulbCaneGlow : ModDust, IDrawDustPreProjectiles {
    void IDrawDustPreProjectiles.DrawPreProjectiles(Dust dust) {
        dust.QuickDraw(Texture2D.Value);
    }

    public override bool PreDraw(Dust dust) => false;

    public override void OnSpawn(Dust dust) {
        dust.velocity.Y = (float)Main.rand.Next(-10, 6) * 0.1f;
        dust.velocity.X *= 0.3f;
        dust.scale *= 0.7f;
    }

    public override bool Update(Dust dust) {
        dust.BasicDust();

        return false;
    }
}
