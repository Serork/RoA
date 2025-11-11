using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BloodCloudRaining : NatureProjectile {
    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 6;
    }

    protected override void SafeSetDefaults() {
        Projectile.netImportant = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.width = 54;
        Projectile.height = 28;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 18000;

        ShouldApplyAttachedNatureWeaponCurrentDamage = false;
    }

    public override void AI() {
        bool flag18 = true;
        int num352 = (int)Projectile.Center.X;
        int num353 = (int)(Projectile.position.Y + (float)Projectile.height);
        if (Collision.SolidTiles(new Vector2(num352, num353), 2, 20))
            flag18 = false;

        Projectile.frameCounter++;
        if (Projectile.frameCounter > 8) {
            Projectile.frameCounter = 0;
            Projectile.frame++;
            if ((!flag18 && Projectile.frame > 2) || Projectile.frame > 5)
                Projectile.frame = 0;
        }

        Projectile.ai[1] += 1f;
        if (Projectile.ai[1] >= 18000f) {
            Projectile.alpha += 5;
            if (Projectile.alpha > 255) {
                Projectile.alpha = 255;
                Projectile.Kill();
            }
        }
        else if (flag18) {
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] > 10f) {
                Projectile.ai[0] = 0f;
                if (Projectile.owner == Main.myPlayer) {
                    num352 += Main.rand.Next(-14, 15);
                    if (AttachedNatureWeapon is not null) {
                        Projectile.damage = NatureWeaponHandler.GetNatureDamage(AttachedNatureWeapon, Main.player[Projectile.owner]);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), num352, num353, 0f, 5f, ModContent.ProjectileType<BloodRain>(), Projectile.damage, 0f, Projectile.owner);
                    }
                }
            }
        }

        Projectile.localAI[0] += 1f;
        if (!(Projectile.localAI[0] >= 10f))
            return;

        Projectile.localAI[0] = 0f;
        int num354 = 0;
        int num355 = 0;
        float num356 = 0f;
        int num357 = Projectile.type;
        for (int num358 = 0; num358 < 1000; num358++) {
            if (Main.projectile[num358].active && Main.projectile[num358].owner == Projectile.owner && Main.projectile[num358].type == num357 && Main.projectile[num358].ai[1] < 18000f) {
                num354++;
                if (Main.projectile[num358].ai[1] > num356) {
                    num355 = num358;
                    num356 = Main.projectile[num358].ai[1];
                }
            }
        }

        if (num354 > 1) {
            Main.projectile[num355].netUpdate = true;
            Main.projectile[num355].ai[1] = 18000f;
        }
    }
}
