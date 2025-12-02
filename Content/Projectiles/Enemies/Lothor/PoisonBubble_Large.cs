using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class PoisonBubble_Large : ModProjectile {
    public override Color? GetAlpha(Color lightColor) => (int)Projectile.ai[0] == 1 ? Color.White * 0.9f * Projectile.Opacity : (lightColor * Projectile.Opacity);

    public override void SetStaticDefaults() {
        Main.projFrames[Projectile.type] = 4;
    }

    public override void SetDefaults() {
        Projectile.aiStyle = -1;
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.scale = 1f;
        Projectile.hostile = true;

        Projectile.tileCollide = false;
    }

    public override void AI() {
        Projectile.velocity *= 0.95f;
        if (Projectile.velocity.Length() <= 0.5f) {
            if (++Projectile.localAI[0] >= 4f) {
                Projectile.localAI[0] = 0f;
                Projectile.frame++;
                if (Projectile.frame >= 4) {
                    Projectile.Kill();
                }
            }
        }
    }
}
