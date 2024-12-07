using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Druidic.Forms;

sealed class CorruptionInsect : FormProjectile {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Corruption Swarmer");
		Main.projFrames[Projectile.type] = 4;
	}

	protected override void SafeSetDefaults() {
		Projectile.CloneDefaults(ProjectileID.Bee);

		int width = 12; int height = width;
		Projectile.Size = new Vector2(width, height);

		AIType = ProjectileID.Bee;

		Projectile.scale = 1f;
		Projectile.penetrate = 1;
		Projectile.timeLeft = 600;
		Projectile.alpha = 250;

		Projectile.friendly = true;
		Projectile.hostile = false;
	}

	public override void AI() {
		if (Projectile.alpha > 0) Projectile.alpha -= 10;

		Projectile.velocity *= 0.98f;

		Projectile.frameCounter++;
		if (Projectile.frameCounter > 4) {
			Projectile.frame++;
			Projectile.frameCounter = 0;
		}
		if (Projectile.frame >= 4) Projectile.frame = 0;
	}

	public override void OnKill(int timeLeft) {
		for (int count = 0; count < 9; count++) {
			int dust = Dust.NewDust(Projectile.position, 10, 10, DustID.CursedTorch, 0, 0, 0, default, 0.8f);
			Main.dust[dust].noGravity = true;
			Main.dust[dust].fadeIn = 0.6f;
			Main.dust[dust].velocity *= 0.4f;
		}
	}
}