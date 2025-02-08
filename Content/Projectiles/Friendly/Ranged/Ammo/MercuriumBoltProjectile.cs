﻿using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged.Ammo;

sealed class MercuriumBoltProjectile : ModProjectile {
    public override void SetDefaults() {
        Projectile.arrow = true;
        Projectile.width = 14;
        Projectile.height = 32;
        Projectile.aiStyle = 1;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.extraUpdates = 1;
        Projectile.timeLeft = 1200;
    }

    public override void AI() {
        if (Projectile.shimmerWet) {
            Projectile.velocity.Y -= 0.4f;
        }

        Projectile.velocity.Y += 0.2f;

        int num67 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.ToxicFumes>(), 0f, 0f);
        Main.dust[num67].noGravity = true;
        Main.dust[num67].fadeIn = 1.5f;
        Main.dust[num67].velocity *= 0.25f;
        Main.dust[num67].velocity += Projectile.velocity * 0.25f;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), 600);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), 600, quiet: false);
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        for (int num671 = 0; num671 < 10; num671++) {
            int num672 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.ToxicFumes>(), 0f, 0f);
            Main.dust[num672].noGravity = true;
            Main.dust[num672].fadeIn = 1.5f;
            Dust dust2 = Main.dust[num672];
            dust2.velocity *= 0.75f;
        }
    }
}