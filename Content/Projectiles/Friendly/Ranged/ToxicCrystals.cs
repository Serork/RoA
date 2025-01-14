using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

class ToxicCrystal2 : ToxicCrystal1 { }

class ToxicCrystal3 : ToxicCrystal1 { }

class ToxicCrystal1 : ModProjectile {
    public override void SetDefaults() {
        int width = 12; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.timeLeft = 360;
        Projectile.aiStyle = 14;
        Projectile.DamageType = DamageClass.Ranged;

        Projectile.penetrate = 3;
        Projectile.friendly = true;
        Projectile.alpha = 100;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = 8;
        height = 8;

        return true;
    }

    public override void AI() {
        if (Projectile.velocity.Y < 0.25 && Projectile.velocity.Y > 0.15) {
            Projectile.velocity.X = Projectile.velocity.X * 0.8f;
        }
        Projectile.rotation = -Projectile.velocity.X * 0.05f;
        int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.JungleSpore, 0f, 0f, 100, default, 1f);
        Main.dust[dust].scale += Main.rand.Next(40) * 0.01f;
        Main.dust[dust].noGravity = true;
        Dust dust2 = Main.dust[dust];
        dust2.position.X -= 2f;
        Dust dust3 = Main.dust[dust];
        dust3.position.Y += 2f;
        Dust _dust4 = Main.dust[dust];
        _dust4.velocity.Y -= 2f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        float num2 = (float)Main.rand.Next(75, 150) * 0.01f;
        target.AddBuff(BuffID.Poisoned, (int)(60f * num2 * 2f), true);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        float num2 = (float)Main.rand.Next(75, 150) * 0.01f;
        target.AddBuff(BuffID.Poisoned, (int)(60f * num2 * 2f), true);
    }
}