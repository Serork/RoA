using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Projectiles.Friendly;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RiseofAges.Content.Projectiles.Friendly.Druid;

sealed class InfectedWave : NatureProjectile {
	public override string Texture => ResourceManager.ProjectileTextures + "EvilBiomeClawsSpecialAttack";

    protected override void SafeSetDefaults() {
        int width = 62; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;

        Projectile.timeLeft = 180;
        Projectile.tileCollide = false;

        Projectile.friendly = true;
        Projectile.Opacity = 0f;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.ignoreWater = true;
        Projectile.localNPCHitCooldown = 100;

		ShouldApplyWreathPoints = false;
    }

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.SourceDamage += 2f;
        modifiers.SetCrit();
    }

    public override void OnSpawn(IEntitySource source) {
        Player player = Main.player[Projectile.owner];
        Vector2 velocity = Vector2.Subtract(Main.MouseWorld, player.RotatedRelativePoint(player.MountedCenter, true));
        velocity.Normalize();
        if (!Utils.HasNaNs(velocity)) {
            Projectile.velocity = velocity;
        }
    }

    public override void AI() {
		Player player = Main.player[Projectile.owner];
		Projectile.Center = player.MountedCenter + Projectile.velocity * 50f;
		Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
		Projectile.rotation = Utils.ToRotation(Projectile.velocity) + 1.57f;
		if (Projectile.Opacity == 0f) {
			Projectile.Opacity = 1f;
		}
		player.heldProj = Projectile.whoAmI;
		if (Main.myPlayer == Projectile.owner) {
			if (player.noItems || player.CCed) {
				Projectile.Kill();
				return;
			}
		}
		//if (Main.rand.Next(5) == 0) {
		//	int dust = Dust.NewDust(Projectile.position + Projectile.velocity * 0.25f, Projectile.width, Projectile.height, ModContent.DustType<CrimsonFoliage>(), -Projectile.velocity.X * 5f * Main.rand.NextFloat(0.9f, 1.2f), -Projectile.velocity.Y * 5f * Main.rand.NextFloat(0.9f, 1.2f), 0, default, 1f);
		//	Main.dust[dust].noGravity = false;
		//	Main.dust[dust].noLight = true;
		//}
		//if (Main.rand.Next(3) == 0) {
		//	int dust = Dust.NewDust(Projectile.position + Projectile.velocity * 0.25f, Projectile.width, Projectile.height, ModContent.DustType<ShadewoodLeaves>(), -Projectile.velocity.X * 5f * Main.rand.NextFloat(0.9f, 1.2f), -Projectile.velocity.Y * 5f * Main.rand.NextFloat(0.9f, 1.2f), 0, default, Main.rand.NextFloat(0.9f, 1.2f));
		//	Main.dust[dust].noGravity = true;
		//	Main.dust[dust].noLight = true;
		//}
		if (Main.rand.Next(3) == 0) {
			int dust = Dust.NewDust(Projectile.position + Projectile.velocity * 0.25f, Projectile.width, Projectile.height, 17, -Projectile.velocity.X * 5f * Main.rand.NextFloat(0.9f, 1.2f), -Projectile.velocity.Y * 5f * Main.rand.NextFloat(0.9f, 1.2f), 0, default, Main.rand.NextFloat(0.9f, 1.2f));
			Main.dust[dust].noGravity = true;
			Main.dust[dust].noLight = true;
		}
		if (Projectile.Opacity > 0.1f) {
			Projectile.Opacity -= 0.085f;
		}
		else {
			Projectile.Kill();
		}
		Projectile.netUpdate = true;
	}

	//public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Infected>(), 180);

	public override bool PreDraw(ref Color lightColor) {
		SpriteBatch spriteBatch = Main.spriteBatch;
		Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
		Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
		Vector2 position = Projectile.Center - Main.screenPosition + Projectile.velocity * 50f;
		Color color = Projectile.GetAlpha(lightColor) * Projectile.Opacity;
		spriteBatch.BeginBlendState(BlendState.NonPremultiplied, SamplerState.PointClamp);
		spriteBatch.Draw(texture, position, null, color, Projectile.rotation, drawOrigin, 1f, SpriteEffects.None, 0f);
		spriteBatch.EndBlendState();
		spriteBatch.BeginBlendState(BlendState.Additive);
		spriteBatch.Draw(texture, position, null, color.MultiplyAlpha((float)(1.0 - (double)(Projectile.Opacity - 0.5f))), Projectile.rotation, drawOrigin, 1f, SpriteEffects.None, 0f);
		spriteBatch.EndBlendState();
		return false;
	}


    public override Color? GetAlpha(Color lightColor) => new Color(66, 54, 112, 255).Bright(2f);
}