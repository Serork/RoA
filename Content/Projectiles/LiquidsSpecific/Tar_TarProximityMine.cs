using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.LiquidsSpecific;

sealed class TarProximityMine : ModProjectile {
    public override void SetDefaults() {
        Projectile.CloneDefaults(801);
        AIType = 801;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        fallThrough = false;
        return true;
    }

    public override void OnKill(int timeLeft) {
        TarRocket.TarLiquidExplosiveKill(Projectile);
    }
}
