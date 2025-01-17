using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies;

sealed class PrimordialLeaf : ModProjectile {
	public override void SetStaticDefaults() {
		Main.projFrames[Projectile.type] = 5;
	}

	public override Color? GetAlpha(Color lightColor) => Color.White;

	public override void SetDefaults() {
		Projectile.hostile = true;
		Projectile.width = Projectile.height = 12;
		Projectile.aiStyle = 0;
		Projectile.extraUpdates = 1;
		Projectile.timeLeft = 360;

		DrawOffsetX = -6;
		DrawOriginOffsetY = -2;
	}

	public override void AI() {
		Projectile.rotation = Projectile.velocity.ToRotation();
		if (Main.netMode != NetmodeID.Server) {
			if (Main.rand.NextBool(20) && Projectile.timeLeft < 330) {
				int num1 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 2, 2, ModContent.DustType<GhostLeaf>(), Projectile.velocity.X, Projectile.velocity.Y * 0.02f, 80, default, Main.rand.NextFloat(0.85f, 1.1f));
				Main.dust[num1].velocity *= 0.95f;
			}
			if (Main.rand.NextBool(4)) {
				int num2 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 2, 2, 15, 0f, 0f, 40, default, 1f);
				Main.dust[num2].velocity *= 0.2f;
			}
		}
	}

    public override void PostAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter > 5) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
            if (Projectile.frame >= 5) {
                Projectile.frame = 0;
            }
        }
    }

    public override void OnKill(int timeLeft) => Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 2, 2, ModContent.DustType<GhostLeaf>(), Projectile.velocity.X, Projectile.velocity.Y * 0.02f, 125, default, 1f);
}
