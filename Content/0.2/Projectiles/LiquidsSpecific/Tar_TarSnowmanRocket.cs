using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.LiquidsSpecific;

sealed class TarSnowmanRocket : ModProjectile {
    public override void SetDefaults() {
        Projectile.CloneDefaults(810);
        AIType = 810;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.Kill();
        return true;
    }

    public override void OnKill(int timeLeft) {
        TarRocket.TarLiquidExplosiveKill(Projectile);
    }
}
