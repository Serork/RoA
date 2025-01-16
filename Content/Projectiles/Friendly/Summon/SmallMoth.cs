using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Summon;

sealed class SmallMoth : ModProjectile {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Small Moth");
        Main.projFrames[Projectile.type] = 4;
    }

    public override void SetDefaults() {
        Projectile.CloneDefaults(ProjectileID.Bee);

        int width = 14; int height = width;
        Projectile.Size = new Vector2(width, height);

        AIType = ProjectileID.Bee;

        Projectile.scale = 1f;
        Projectile.penetrate = 1;

        Projectile.minion = true;

        Projectile.minionSlots = 0f;
        Projectile.tileCollide = false;

        Projectile.timeLeft = 300;

        Projectile.DamageType = DamageClass.Summon;

        Projectile.friendly = true;
    }

    public override void AI() {
        if (Projectile.timeLeft == 220) {
            Projectile.tileCollide = true;
        }

        if (Projectile.timeLeft % 2 == 0) {
            if (Projectile.direction < 0) {
                int dust = Dust.NewDust(new Vector2(Projectile.position.X - 2, Projectile.position.Y + 6), 1, 1, 6, 0f, 0f, 0, new Color(), Main.rand.NextFloat(0.85f, 1.1f) * 0.75f);
                Main.dust[dust].velocity *= 0.25f;
                Main.dust[dust].noGravity = true;
            }
            else {
                int dust = Dust.NewDust(new Vector2(Projectile.position.X - 6 + Projectile.width, Projectile.position.Y + 4), 1, 1, 6, 0f, 0f, 0, new Color(), Main.rand.NextFloat(0.85f, 1.1f) * 0.75f);
                Main.dust[dust].velocity *= 0.25f;
                Main.dust[dust].noGravity = true;
            }
        }

        Projectile.frameCounter++;
        if (Projectile.frameCounter > 2) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }
        if (Projectile.frame >= 4) Projectile.frame = 0;
    }
}