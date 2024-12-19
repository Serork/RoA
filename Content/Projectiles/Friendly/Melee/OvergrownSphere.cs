using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Forms;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

sealed class OvergrownSphere : ModProjectile {
	private bool _changeAlpha;
	private int _collisionRegistered;

	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Overgrown Sphere");
		//ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
		//ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
	}

	public override void SetDefaults() {
		int width = 20; int height = width;
		Projectile.Size = new Vector2(width, height);

		Projectile.DamageType = DamageClass.Melee;

		Projectile.aiStyle = -1;

		Projectile.friendly = false;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;

		Projectile.penetrate = 1;
		Projectile.alpha = 255;
	}

    public override void OnSpawn(IEntitySource source) {
        int num = 10;
        if (num != Projectile.oldPos.Length) {
            Array.Resize(ref Projectile.oldPos, num);
            Array.Resize(ref Projectile.oldRot, num);
            Array.Resize(ref Projectile.oldSpriteDirection, num);
        }

        for (int i = 0; i < Projectile.oldPos.Length; i++) {
            Projectile.oldPos[i].X = 0f;
            Projectile.oldPos[i].Y = 0f;
            Projectile.oldRot[i] = 0f;
            Projectile.oldSpriteDirection[i] = 0;
        }
    }

    public override void PostAI() {
        for (int num29 = Projectile.oldPos.Length - 1; num29 > 0; num29--) {
            Projectile.oldPos[num29] = Projectile.oldPos[num29 - 1];
            Projectile.oldRot[num29] = Projectile.oldRot[num29 - 1];
            Projectile.oldSpriteDirection[num29] = Projectile.oldSpriteDirection[num29 - 1];
        }
        Projectile.oldPos[0] = Projectile.position;
        Projectile.oldRot[0] = Projectile.rotation;
        Projectile.oldSpriteDirection[0] = Projectile.spriteDirection;
        float amount = 0.65f;
        int num30 = 1;
        for (int num31 = 0; num31 < num30; num31++) {
            for (int num32 = Projectile.oldPos.Length - 1; num32 > 0; num32--) {
                if (!(Projectile.oldPos[num32] == Vector2.Zero)) {
                    if (Projectile.oldPos[num32].Distance(Projectile.oldPos[num32 - 1]) > 2f)
                        Projectile.oldPos[num32] = Vector2.Lerp(Projectile.oldPos[num32], Projectile.oldPos[num32 - 1], amount);

                    Projectile.oldRot[num32] = (Projectile.oldPos[num32 - 1] - Projectile.oldPos[num32]).SafeNormalize(Vector2.Zero).ToRotation();
                }
            }
        }
    }

    public override void AI() {
		Player player = Main.player[Projectile.owner];

		double deg = (double)(Projectile.ai[1] + Projectile.ai[0] * 240) / 2;
		double rad = deg * (Math.PI / 180);
		double dist = 70;
		Projectile.position.X = player.MountedCenter.X - (int)(Math.Cos(rad) * dist) - player.width / 2;
		Projectile.position.Y = player.MountedCenter.Y - (int)(Math.Sin(rad) * dist) - player.height / 2 + 4 + player.gfxOffY;
		Projectile.ai[1] += 2f;
		Projectile.rotation = Projectile.velocity.ToRotation();
					
		if (_collisionRegistered != 0) _collisionRegistered--;

		ushort spearType = (ushort)ModContent.ProjectileType<OvergrownSpear>();
		ushort boltType = (ushort)ModContent.ProjectileType<OvergrownBolt>();
		for (var i = 0; i < 200; i++) {
			var proj = Main.projectile[i];
			if (proj.active && proj.type == spearType && proj.owner == player.whoAmI) {
				var rect = proj.getRect();
				var rec2 = Projectile.getRect();
				if (rect.Intersects(rec2) && _collisionRegistered == 0) {
					SoundEngine.PlaySound(SoundID.NPCDeath55, Projectile.Center);
					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, proj.velocity, boltType, Projectile.damage, Projectile.knockBack, Projectile.owner);
					float dustCount = 12f;
					int dustCount2 = 0;
					while (dustCount2 < dustCount) {
						Vector2 vector = Vector2.UnitX * 0f;
						vector += -Vector2.UnitY.RotatedBy(dustCount2 * (7f / dustCount), default) * new Vector2(1.5f, 1.5f);
						vector = vector.RotatedBy(Projectile.velocity.ToRotation(), default);
						int dust = Dust.NewDust(Projectile.Center, 0, 0, 107, 0f, 0f, 40, default(Color), 1f);
						Main.dust[dust].noGravity = true;
						Main.dust[dust].position = Projectile.Center + vector;
						Main.dust[dust].velocity = Vector2.Normalize(Projectile.Center - Main.dust[dust].position) * 2f + Projectile.velocity * 2f;
						int max = dustCount2;
						dustCount2 = max + 1;
					}
					_collisionRegistered = 20;
				}
			}
		}
		if (Projectile.Opacity < 1f && !_changeAlpha) Projectile.Opacity += 0.025f;
		else _changeAlpha = true;

		int type = ModContent.ItemType<Items.Weapons.Melee.OvergrownSpear>();
		if (player.HeldItem.type != type || player.inventory[player.selectedItem].type != type || !player.active || player.dead || player.GetModPlayer<BaseFormHandler>().IsInDruidicForm) {
			if (Projectile.Opacity > 0f) Projectile.Opacity -= 0.025f;
			else Projectile.Kill();
		}
	}

	public override bool PreDraw(ref Color lightColor) {
		SpriteBatch spriteBatch = Main.spriteBatch;
		Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
		Rectangle frameRect = new Rectangle(0, 0, texture.Width, texture.Height);
		Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
		for (int k = 0; k < Projectile.oldPos.Length; k++) {
			Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
			Color color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
			spriteBatch.Draw(texture, drawPos, frameRect, color * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
		}
		spriteBatch.Draw(texture, Projectile.position - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY), frameRect, Projectile.GetAlpha(lightColor) * Projectile.Opacity, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
		return false;
	}

	public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.5f);

	public override bool? CanCutTiles()	 => false;
	
	public override bool? CanDamage() => false;
}