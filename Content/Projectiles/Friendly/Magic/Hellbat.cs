﻿using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

sealed class Hellbat : ModProjectile {
	public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, 0) * (1f - Projectile.alpha / 255f);

	public override void SetStaticDefaults() {
		Main.projFrames[Type] = 5;
	}

	public override void SetDefaults() {
		int width = 44; int height = 40;
		Projectile.Size = new Vector2(width, height);

		Projectile.friendly = true;

		Projectile.DamageType = DamageClass.Magic;
		Projectile.penetrate = 1;

		Projectile.aiStyle = 1;
		AIType = ProjectileID.Bullet;

		Projectile.ignoreWater = true;

		Projectile.extraUpdates = 1;

		Projectile.alpha = 255;
		Projectile.timeLeft = 70;

		Projectile.alpha = 255;
	}

	public override void AI() {
        if (Projectile.alpha < 65 && Main.rand.NextBool(6)) {
            int num179 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Torch, Projectile.velocity.X, Projectile.velocity.Y, 100, default, 1.2f + Main.rand.NextFloat() * 0.5f);
            //Main.dust[num179].noLightEmittence = true;
            Main.dust[num179].noGravity = true;
            Main.dust[num179].velocity *= 0.2f;
        }
        
		if (Projectile.ai[0] == 1f) {
			Projectile.scale += 0.008f;
			if (Projectile.scale >= 1.4f) {
				Projectile.alpha += 25;
			}
		} 
		else if (Projectile.alpha > 0) {
			Projectile.alpha -= 15;
			Projectile.ai[0] = 1f;
		}
		Projectile.rotation = 0f;
		Projectile.spriteDirection = -Projectile.direction;
	}

    public override void PostAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter >= 6) {
            Projectile.frame++; Projectile.frameCounter = 0;
        }
        if (Projectile.frame >= Main.projFrames[Type]) {
            Projectile.frame = 0;
        }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
		width = height = 10;
		return true;
	}

	public override void OnKill(int timeLeft) => SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
		target.immune[Projectile.owner] = 8;
		int buff = ModContent.BuffType<Deceleration>();
		if (target.FindBuff(buff, out int buffIndex)) {
			target.DelBuff(buffIndex);
		}
		if (!Projectile.wet && !target.wet) {
			target.AddBuff(BuffID.OnFire3, Main.rand.Next(20, 50), false);
		}
	}

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
		if (Projectile.wet || target.wet) {
			modifiers.FinalDamage /= 2;
		}
		if (Main.player[Projectile.owner].position.Y > (Main.maxTilesY - 200) * 16) {
			modifiers.FinalDamage *= 1.5f;
		}
	}
}