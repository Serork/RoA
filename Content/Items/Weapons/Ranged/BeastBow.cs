using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged;

sealed class BeastBow : ModItem {
	public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

	public override void SetDefaults() {
		int width = 34; int height = 56;
		Item.Size = new Vector2(width, height);

		Item.useStyle = ItemUseStyleID.Shoot; 
		Item.useTime = Item.useAnimation = 25; 
		Item.autoReuse = false;

		Item.noUseGraphic = true;

		Item.DamageType = DamageClass.Ranged; 
		Item.damage = 11; 
		Item.knockBack = 2.15f;

		Item.value = Item.sellPrice(gold: 1, silver: 20);
		Item.rare = ItemRarityID.Green; 
		Item.UseSound = SoundID.Item5;

		Item.shoot = ProjectileID.WoodenArrowFriendly;
		Item.useAmmo = AmmoID.Arrow;
		Item.shootSpeed = 8f;
	}

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[ModContent.ProjectileType<BeastProj>()] < 1) {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<BeastProj>(), 0, 0f, player.whoAmI, 0f, 0f, player.itemTimeMax);
        }

        return false;
    }
}

sealed class BeastProj : ModProjectile  {
	public override string Texture => ModContent.GetInstance<BeastBow>().Texture;

	public override void SetDefaults() {
		int width = 34; int height = 56;
		Projectile.Size = new Vector2(width, height);

		Projectile.DamageType = DamageClass.Ranged;

		Projectile.friendly = true;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.extraUpdates = 1;

		Projectile.netImportant = true;

        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

	public override bool? CanCutTiles() => false;

	public override bool? CanDamage() => false;

	public override bool ShouldUpdatePosition()  => false;

	public override bool PreDraw(ref Color lightColor) {
		Player _player = Main.player[Projectile.owner];
		Texture2D _texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
		Vector2 _origin = new(_texture.Width * 0.5f, _texture.Height * 0.5f);
		float _rotOffset = 0.785f * _player.direction;
		if (_player.gravDir == -1f)
			_rotOffset -= 1.57f * _player.direction;
		int x = -(int)_origin.X;
		Vector2 _offset = new(_origin.X + x, 0);
		SpriteEffects effects = _player.direction != 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
        if (_player.gravDir == -1f) {
            if (_player.direction == 1)
                effects = SpriteEffects.FlipVertically;
            else if (_player.direction == -1)
                effects = SpriteEffects.None;
        }
        Main.spriteBatch.Draw(_texture, Projectile.Center - Main.screenPosition/* + new Vector2(0f, Projectile.gfxOffY + 4f)*/, _texture.Bounds, lightColor, Projectile.rotation + _rotOffset - ((float)Math.PI / 2f + (float)Math.PI * 1.8f) * _player.direction * _player.gravDir + 0.45f * _player.direction * _player.gravDir, _origin, Projectile.scale, effects, 0);
		return false;
	}

	private float _rotation;
	private float _attackTimer;
	private float _rotation2;

	public override void SendExtraAI(BinaryWriter writer) {
		writer.Write(_rotation);
		writer.Write(_attackTimer);
		writer.Write(_rotation2);
	}

	public override void ReceiveExtraAI(BinaryReader reader) {
		_rotation = reader.ReadSingle();
		_attackTimer = reader.ReadSingle();
		_rotation2 = reader.ReadSingle();
    }

	public override void AI() {
		Player player = Main.player[Projectile.owner];
		player.heldProj = Projectile.whoAmI;
		Projectile.timeLeft = player.itemAnimation;
		Projectile.damage = 0;
		float offset = (player.direction != 1 ? 3f : 4f) * -player.direction;
		if (Main.myPlayer == Projectile.owner) {
			if (player.noItems || player.CCed || player.itemAnimation <= 0 || player.itemTime <= 0) {
				Projectile.Kill();
				return;
			}
			Vector2 center = player.RotatedRelativePoint(player.MountedCenter, true);
			Vector2 mouseWorld = Main.MouseWorld;
			Vector2 position = Vector2.Subtract(mouseWorld, center);
			position.Normalize();
			if (!Utils.HasNaNs(position)) 
				Projectile.velocity = position;
			player.ChangeDir(Projectile.direction);
            _rotation2 = (float)Math.Atan2(mouseWorld.Y - center.Y, mouseWorld.X - center.X) + _rotation;
			Projectile.netUpdate = true;
		}
		Projectile.rotation = _rotation2;
        Projectile.ai[1] += 0.0045f * player.gravDir;
		double rads = 1.0 * player.direction - (double)(Projectile.ai[1] - 0.5f) * player.direction * (3.14 + 2.0) + (float)Math.PI;
		if (player.gravDir == -1f) {
			rads -= MathHelper.PiOver4 * player.direction;
        }
		float x = (float)Math.Cos(rads);
		float y = (float)Math.Sin(rads);
		Vector2 vector = Projectile.velocity;
		Vector2 velocity = new Vector2();
		velocity.X += vector.X * x - vector.Y * y;
		velocity.Y += vector.X * y + vector.Y * x;
		Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, true) + Projectile.velocity * 20f + new Vector2(offset * player.direction, 2f * player.direction * player.gravDir).RotatedBy(Projectile.velocity.ToRotation());
		Vector2 velocity2 = Projectile.Center + velocity * (float)(31.4 + 31.4 * Projectile.ai[1]);
		Projectile.direction = Projectile.spriteDirection = player.direction;
		++_attackTimer;
		if (_attackTimer >= Projectile.ai[2] / 3f) {
			_rotation -= 0.075f * player.direction * player.gravDir;
            _attackTimer = 0f;
			Projectile.netUpdate = true;
			SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);
			int projToShoot = -1;
			Item? array = null;
			bool num = false;
			bool flag2 = true;
			for (int num2 = 54; num2 < 58; num2++) {
				if (player.inventory[num2].ammo != player.HeldItem.useAmmo) {
					continue;
				}
				else {
					array = player.inventory[num2];
					if (player.inventory[num2].stack > 0) {
						num = true;
						if (player.inventory[num2].maxStack != 1 && Main.rand.NextBool(3)) {
							player.inventory[num2].stack -= 1;
						}

					}
					else {
						continue;
					}
					flag2 = false;
					break;
				}
			}
			if (!num) {
				flag2 = true;

				for (int minValue = 0; minValue < 54; minValue++) {
					if (player.inventory[minValue].ammo != player.HeldItem.useAmmo) {
						continue;
					}
					else {
						array = player.inventory[minValue];
						if (player.inventory[minValue].stack > 0) {
							if (player.inventory[minValue].maxStack != 1 && Main.rand.NextBool(3)) {
								player.inventory[minValue].stack -= 1;
							}
						}
						else {
							continue;
						}
						flag2 = false;
						break;
					}
				}
			}
			if (flag2 || array == null)
				Projectile.Kill();
			projToShoot = array.shoot;
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity * 4f, Helper.VelocityToPoint(Projectile.Center, velocity2, 7f), projToShoot, 7, 2.15f, Projectile.owner);
		}
    }
}
