using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class PoisonBubble_Large : ModProjectile {
    public override void SetStaticDefaults() {
        Main.projFrames[Projectile.type] = 4;
    }

    public override void SetDefaults() {
        Projectile.aiStyle = -1;
        Projectile.width = 10;
        Projectile.height = 18;
        Projectile.scale = 1f;
        Projectile.hostile = true;
    }

    public override void AI() {
        base.AI();
    }
}
