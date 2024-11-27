using RoA.Core;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class Bee : NatureProjectile {
    public override string Texture => ResourceManager.ProjectileTextures + nameof(Bee);

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 4;
    }

    protected override void SafeSetDefaults() {
        Projectile.CloneDefaults(ProjectileID.Bee);
        Projectile.friendly = true;
        Projectile.hostile = false;
    }

    public override void PostAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter >= 3) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }

        if (Projectile.frame >= 3)
            Projectile.frame = 0;
    }
}
