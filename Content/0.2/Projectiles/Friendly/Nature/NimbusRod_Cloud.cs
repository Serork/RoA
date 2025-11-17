using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class RainCloudMoving : NatureProjectile {
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

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 8;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool? CanDamage() => false;

    public override void OnKill(int timeLeft) {
        if (Projectile.owner == Main.myPlayer) {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, 0f, 0f, ModContent.ProjectileType<RainCloudRaining>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
}

sealed class RainCloudRaining : NatureProjectile {
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
            if (Projectile.ai[0] > 8f) {
                Projectile.ai[0] = 0f;
                if (Projectile.owner == Main.myPlayer) {
                    num352 += Main.rand.Next(-14, 15);
                    if (!AttachedNatureWeapon.IsEmpty()) {
                        Projectile.damage = NatureWeaponHandler.GetNatureDamage(AttachedNatureWeapon, Main.player[Projectile.owner]);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), num352, num353, 0f, 5f, ModContent.ProjectileType<Rain>(), Projectile.damage, 0f, Projectile.owner);
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

        if (num354 > 2) {
            Main.projectile[num355].netUpdate = true;
            Main.projectile[num355].ai[1] = 18000f;
        }
    }
}

sealed class Rain : NatureProjectile {
    protected override void SafeSetDefaults() {
        Projectile.ignoreWater = true;
        Projectile.width = 4;
        Projectile.height = 40;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = 5;
        Projectile.timeLeft = 300;
        Projectile.scale = 1.1f;
        //Projectile.magic = true;
        Projectile.extraUpdates = 1;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 10;

        ShouldApplyAttachedNatureWeaponCurrentDamage = false;
    }

    public override void AI() {
        int num359 = (int)(Projectile.Center.X / 16f);
        int num360 = (int)((Projectile.position.Y + (float)Projectile.height) / 16f);
        if (WorldGen.InWorld(num359, num360) && Main.tile[num359, num360] != null && Main.tile[num359, num360].LiquidAmount == byte.MaxValue && Main.tile[num359, num360].LiquidType == LiquidID.Shimmer && Projectile.velocity.Y > 0f) {
            Projectile.velocity.Y *= -1f;
            Projectile.netUpdate = true;
        }

        if (Projectile.type == 239)
            Projectile.alpha = 50;
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.velocity.Y > 0f) {
            int num502 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + (float)Projectile.height - 2f), 2, 2, 154);
            Main.dust[num502].position.X -= 2f;
            Main.dust[num502].alpha = 38;
            Dust dust2 = Main.dust[num502];
            dust2.velocity *= 0.1f;
            dust2 = Main.dust[num502];
            dust2.velocity += -Projectile.oldVelocity * 0.25f;
            Main.dust[num502].scale = 0.95f;
        }
    }
}