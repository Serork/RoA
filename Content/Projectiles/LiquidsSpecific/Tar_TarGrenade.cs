using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.LiquidsSpecific;

sealed class TarGrenade : ModProjectile {
    public override void SetDefaults() {
        Projectile.CloneDefaults(800);
        AIType = 800;
        Projectile.timeLeft = 180;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        fallThrough = false;
        return true;
    }

    public override void OnKill(int timeLeft) {
        TarRocket.TarLiquidExplosiveKill(Projectile, true);
    }
}
