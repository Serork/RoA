using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Summon;

sealed class BoneHarpy : ModProjectile {
    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 6;
        Main.projPet[Type] = false;

        ProjectileID.Sets.TrailCacheLength[Type] = 12;
        ProjectileID.Sets.TrailingMode[Type] = 0;

        ProjectileID.Sets.MinionSacrificable[Type] = false;
    }

    public override void SetDefaults() {
        int width = 100; int height = 86;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.minion = false;
        Projectile.DamageType = DamageClass.Summon;

        Projectile.aiStyle = -1;

        Projectile.friendly = true;
        Projectile.tileCollide = false;
    }

    public override void AI() { }

    public override void PostAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter > 4) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }
        if (Projectile.frame >= 6) {
            Projectile.frame = 0;
        }
    }
}
