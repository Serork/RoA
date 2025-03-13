using Microsoft.Xna.Framework;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged.Ammo;

sealed class GalipotArrowProjectile : ModProjectile {
    public override void SetDefaults() {
        Projectile.arrow = true;
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.aiStyle = 1;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 1200;
    }

    public override void AI() {
        if (Projectile.shimmerWet) {
            Projectile.velocity.Y -= 0.4f;
        }

        if (Main.rand.NextBool(3)) {
            int num67 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.Galipot>(), 0f, 0f, Alpha: 50);
            Main.dust[num67].noGravity = true;
            Main.dust[num67].fadeIn = 1.5f;
            Main.dust[num67].velocity *= 0.25f;
            Main.dust[num67].velocity += Projectile.velocity * 0.25f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<Buffs.SolidifyingSap>(), 300);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(ModContent.BuffType<Buffs.SolidifyingSap>(), 300);
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        for (int num671 = 0; num671 < 6; num671++) {
            int num672 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.Galipot>(), 0f, 0f, Alpha: 50);
            Main.dust[num672].noGravity = true;
            Main.dust[num672].fadeIn = 1.5f;
            Dust dust2 = Main.dust[num672];
        }
    }
}