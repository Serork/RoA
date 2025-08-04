using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BloodCloudMoving : NatureProjectile {
    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 4;
    }

    protected override void SafeSetDefaults() {
        Projectile.netImportant = true;
        Projectile.width = 28;
        Projectile.height = 28;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;

        ShouldApplyAttachedNatureWeaponCurrentDamage = false;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 8;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool? CanDamage() => false;

    public override void AI() {
        float num350 = Projectile.ai[0];
        float num351 = Projectile.ai[1];
        if (num350 != 0f && num351 != 0f) {
            bool flag16 = false;
            bool flag17 = false;
            if (Projectile.velocity.X == 0f || (Projectile.velocity.X < 0f && Projectile.Center.X < num350) || (Projectile.velocity.X > 0f && Projectile.Center.X > num350)) {
                Projectile.velocity.X = 0f;
                flag16 = true;
            }

            if (Projectile.velocity.Y == 0f || (Projectile.velocity.Y < 0f && Projectile.Center.Y < num351) || (Projectile.velocity.Y > 0f && Projectile.Center.Y > num351)) {
                Projectile.velocity.Y = 0f;
                flag17 = true;
            }

            if (Projectile.owner == Main.myPlayer && flag16 && flag17)
                Projectile.Kill();
        }

        Projectile.rotation += Projectile.velocity.X * 0.02f;
        Projectile.frameCounter++;
        if (Projectile.frameCounter > 4) {
            Projectile.frameCounter = 0;
            Projectile.frame++;
            if (Projectile.frame > 3)
                Projectile.frame = 0;
        }
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer) {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, 0f, 0f, ModContent.ProjectileType<BloodCloudRaining>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
}
