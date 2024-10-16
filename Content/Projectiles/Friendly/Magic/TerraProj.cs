using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

sealed class TerraProj : ModProjectile {
	private float currentCharge, hittingTimer;
	private bool maxCharged, hitting;

    public override void SendExtraAI(BinaryWriter writer) {
		writer.Write(hittingTimer);
        writer.Write(hitting);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
		hittingTimer = reader.ReadSingle();
		hitting = reader.ReadBoolean();
    }

    public override Color? GetAlpha(Color lightColor) => Color.White;

	public override void SetDefaults() {
		int width = 2; int height = width;
		Projectile.Size = new Vector2(width, height);

		Projectile.friendly = true;

		Projectile.penetrate = -1;

		Projectile.tileCollide = false;

        //Projectile.hide = true;

        Projectile.aiStyle = -1;

		hitting = false;
		currentCharge = 0;
		maxCharged = false;

        Projectile.ignoreWater = true;
    }

	public override bool PreDraw(ref Color lightColor) {
		Player _player = Main.player[Projectile.owner];
		bool flag = _player.direction != 1;
		Item _item = _player.HeldItem;
		Texture2D _texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.ItemsWeaponsMagicTextures + "RodOfTheTerra_OnUse");
		Vector2 _origin = new(_texture.Width * 0.5f * (1 - _player.direction), (_player.gravDir == -1f) ? 0 : _texture.Height);
		int x = -(int)_origin.X;
		ItemLoader.HoldoutOrigin(_player, ref _origin);
		Vector2 _offset = new(_origin.X + x, 0);
        float _rotOffset = 0.785f * _player.direction;
        if (_player.gravDir == -1f)
            _rotOffset -= 1.57f * _player.direction;
        SpriteEffects effects = flag ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        if (_player.gravDir == -1f) {
            if (_player.direction == 1) {
                effects = SpriteEffects.FlipVertically;
            }
            else {
                effects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            }
        }
        SpriteBatch _spriteBatch = Main.spriteBatch;
		_spriteBatch.Draw(_texture, _player.itemLocation - Main.screenPosition + _offset + new Vector2(0f, Projectile.gfxOffY + 4f) * _player.gravDir, _texture.Bounds, lightColor, _player.itemRotation + _rotOffset, _origin, _item.scale, effects, 0);
		_texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.ItemsWeaponsMagicTextures + "RodOfTheTerra2_Glow");
		float maxValue = Projectile.ai[1] / 110f;
        float _k = 1f - (maxValue > 0.3f ? 0.3f : 0f) - (maxValue > 0.6f ? 0.3f : 0f) - (maxValue > 0.9f ? 0.3f : 0f);
		float value = Projectile.ai[1] / 45f;
        float _rate = (float)(value % _k);
		//Color _lightColor = Color.Lerp(Color.DarkGreen, Color.White, 1f - (_rate - 0.5f) / 0.5f);
		Color _lightColor = Color.White;
		//if (_lightColor == Color.White && Projectile.ai[1] <= 170f && Projectile.localAI[1] <= 0f)
		//	SoundEngine.PlaySound(SoundID.Item43.WithVolumeScale(0.15f), Projectile.position);
		_spriteBatch.Draw(_texture, _player.itemLocation - Main.screenPosition + _offset + new Vector2(0f, Projectile.gfxOffY + 4f) * _player.gravDir, _texture.Bounds, Color.White, _player.itemRotation + _rotOffset, _origin, _item.scale, effects, 0);
		if (maxCharged)
			DrawLaser(_spriteBatch, (Texture2D)ModContent.Request<Texture2D>(Texture), _player.Center + new Vector2(0f, Projectile.gfxOffY + 4f) * _player.gravDir, Projectile.velocity, 5, Projectile.damage, -1.57f, 1f, 1000f, _lightColor, 78);
		_spriteBatch.BeginBlendState(BlendState.AlphaBlend, SamplerState.AnisotropicClamp);
		if (maxCharged)
			DrawLaser(_spriteBatch, (Texture2D)ModContent.Request<Texture2D>(Texture), _player.Center + new Vector2(0f, Projectile.gfxOffY + 4f) * _player.gravDir, Projectile.velocity, 5, Projectile.damage, -1.57f, 1f, 1000f, _lightColor * 0.3f, 78);
		_spriteBatch.EndBlendState();
		//_texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.ItemsWeaponsMagicTextures + "RodOfTheTerra3_Glow");
		//_spriteBatch.Draw(_texture, _player.itemLocation - Main.screenPosition + _offset + new Vector2(0f, Projectile.gfxOffY + 4f) * _player.gravDir, _texture.Bounds, !maxCharged ? _lightColor : Color.White, _player.itemRotation + _rotOffset, _origin, _item.scale, effects, 0);
		return false;
	}

	private void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default, int transDist = 50) {
		if (Projectile.ai[2] < 5f) {
			return;
		}

		float value = (float)Math.Pow(Utils.GetLerpValue(5f, 25f, Projectile.localAI[1], true), 0.5f) * Utils.GetLerpValue(currentCharge, currentCharge - 40f, Projectile.localAI[1], true);
        float value1 = (float)Math.Pow(value, 0.5f);
        Vector2 drawScale = new(scale * value1, scale);
        for (float i = transDist; i <= Projectile.ai[0]; i += step) {
			Color c = Color.White;
			Vector2 _origin = start + i * unit;
			spriteBatch.Draw(texture, _origin - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), new Color(c.R, c.G, c.B, 200), unit.ToRotation() + rotation, new Vector2(7f, 20f), drawScale, 0, 0);
		}
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
		Player _player = Main.player[Projectile.owner];
		Texture2D _texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.ItemsWeaponsMagicTextures + "RodOfTheTerra_OnUse");
		Vector2 _position = _player.Center + new Vector2(0f, Projectile.gfxOffY + 4f) + new Vector2(-10f, 8f);
        float _ = 0f;
        bool flag = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), _position, _position + Projectile.velocity + Vector2.Normalize(Projectile.velocity) * Projectile.ai[0], 2, ref _);

		//if (flag) {
  //          Projectile.ai[0] = Vector2.Distance(targetHitbox.Center(), Projectile.position) * 0.935f;
  //          hittingTimer = 5f + (int)(Projectile.localAI[1] / 10f);
  //          hitting = true;

  //          Projectile.netUpdate = true;
  //          Projectile.localAI[2] = 1f;
  //      }
		//else {
  //          Projectile.localAI[2] = 0f;
  //      }
  //      if (!flag && hittingTimer <= 0f)
  //          hitting = false;

        return flag;
	}

	public override bool? CanDamage() => maxCharged && Projectile.ai[2] >= 5f && Projectile.localAI[1] <= currentCharge - 10f;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
		if (Projectile.ai[2] < 5f) {
			return;
		}

		//Projectile.damage = Main.player[Projectile.owner].HeldItem.damage / 5 + (int)(float)(Projectile.damage * 0.9f);
		target.immune[Projectile.owner] = 3 + (int)(Projectile.localAI[1] / 10f);
	}

    //public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
    //	damage = Main.player[Projectile.owner].HeldItem.damage + (int)(currentCharge - Projectile.localAI[1]) / 5;
    //}

    public override void AI() {
		if (Projectile.ai[1] < 0f) Projectile.ai[1] = 0f;
		if (hittingTimer > 0f) hittingTimer--;
		else {
			hitting = false;
		}
		if (Projectile.ownerHitCheck) {
			hittingTimer = 5f + (float)(Projectile.localAI[1] / 10f);
			hitting = true;
		}

		Player player = Main.player[Projectile.owner];
        int dir = Projectile.direction;
        player.ChangeDir(dir);
		if (player.gravDir != -1f) {
			player.heldProj = Projectile.whoAmI;
		}

        Vector2 mousePosition = player.GetViableMousePosition();
        Texture2D _texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.ItemsWeaponsMagicTextures + "RodOfTheTerra_OnUse");
		Vector2 _origin = new(_texture.Width * 0.5f * (1 - player.direction), (player.gravDir == -1f) ? 0 : _texture.Height);
		int _x = -(int)_origin.X;
		ItemLoader.HoldoutOrigin(player, ref _origin);
		Vector2 _offset = new(_origin.X + _x, 0);
		Vector2 pos = player.itemLocation;
        //if (player.gravDir == -1f) {
        //    float num = player.position.Y - player.itemLocation.Y;
        //    pos.Y = player.Bottom.Y + num;
        //}
        Vector2 _position = pos + _offset + new Vector2(0f, Projectile.gfxOffY + 4f) * player.gravDir;
		Lighting.AddLight(_position, 73f / 255f * (Projectile.ai[1] / 300f), 170f / 255f * (Projectile.ai[1] / 300f), 104f / 255f * (Projectile.ai[1] / 300f));
        float value = Utils.GetLerpValue(0f, 50f, Projectile.localAI[1], true) * Utils.GetLerpValue(currentCharge, currentCharge - 40f, Projectile.localAI[1], true);
        float value1 = (float)Math.Pow(value, 0.5f);
        if (maxCharged && Projectile.ai[2] >= 5f) {
			if (Projectile.soundDelay <= 0) {
				Projectile.soundDelay = 15;
				Projectile.soundDelay *= 2;

				SoundEngine.PlaySound(SoundID.Item15, Projectile.position);
			}
            Vector2 spinningpoint4 = Vector2.UnitX * 14f;
			float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            spinningpoint4 = spinningpoint4.RotatedBy(rotation - (float)Math.PI / 2f);
            Vector2 position = player.Center + Vector2.Normalize(Projectile.velocity) * 37f;
            Vector2 vector13 = position + spinningpoint4;
            for (int l = 0; l < 2; l++) {
				if (Main.rand.NextBool()) {
					int num26 = ModContent.DustType<NatureLaser>();
					float num27 = 0.35f;
					if (l % 2 == 1) {
						num27 = 0.45f;
					}
					num27 *= 1.5f;
					num27 *= 1.25f * Main.rand.NextFloat(0.75f, 1f);
					num27 *= 1.25f * Main.rand.NextFloat(0.75f, 1f);
					num27 *= 1.01f;

                    float num28 = Main.rand.NextFloatDirection();
					Vector2 vector14 = vector13 + (rotation + num28 * ((float)Math.PI / 4f) * 0.8f - (float)Math.PI / 2f).ToRotationVector2() * 6f;
					int num29 = 18;
					int num30 = Dust.NewDust(vector14 - Vector2.One * (num29 / 2) - new Vector2(4f, -4f * player.gravDir), num29, num29, num26, Projectile.velocity.X / 2f, Projectile.velocity.Y / 2f);
					Main.dust[num30].velocity = (vector14 - vector13).SafeNormalize(Vector2.Zero) * MathHelper.Lerp(1.5f, 9f, Utils.GetLerpValue(1f, 0f, Math.Abs(num28), clamped: true)) * 0.5f * value1;
					Main.dust[num30].noGravity = true;
					Main.dust[num30].scale = num27;
					Main.dust[num30].fadeIn = 0.5f;
				}
            }
			for (int i = 0; i < 2; i++) {
				if (Main.rand.NextBool()) {
					float _spawnX = Projectile.velocity.ToRotation() + (float)((Main.rand.Next(2) == 1 ? -1.0 : 1.0) * 1.82);
					float _spawnY = (float)(Main.rand.NextDouble() * 4.0 + 1.0);
					Vector2 _velocity = new((float)Math.Cos((double)_spawnX) * _spawnY, (float)Math.Sin((double)_spawnX) * _spawnY);
					var _dust = Dust.NewDust(_position + Projectile.ai[0] * Projectile.velocity * 1.01f - new Vector2(4f, 4f), 0, 0, ModContent.DustType<NatureLaser>(), _velocity.X * 0.7f, _velocity.Y * 0.7f, 0, Color.White, 3.3f);
					Lighting.AddLight(_position + Projectile.ai[0] * Projectile.velocity * 1.01f - new Vector2(4f, 4f), 73f / 255f, 170f / 255f, 104f / 255f);
					Main.dust[_dust].scale = 1f;
					Main.dust[_dust].noGravity = true;
					Main.dust[_dust].fadeIn = 0.25f;
					if (Projectile.scale != 1.4) {
						Dust dust = Dust.CloneDust(_dust);
						dust.scale /= 2f;
					}
					_spawnX = Projectile.velocity.ToRotation() + (float)((Main.rand.Next(2) == 1 ? -1.0 : 1.0) * 3.14);
					_spawnY = (float)(Main.rand.NextDouble() * 10.0 + 1.0);
					_velocity = new((float)Math.Cos((double)_spawnX) * _spawnY, (float)Math.Sin((double)_spawnX) * _spawnY);
					_dust = Dust.NewDust(_position + Projectile.ai[0] * Projectile.velocity * 1.01f - new Vector2(4f, 4f), 0, 0, ModContent.DustType<NatureLaser>(), _velocity.X * 0.7f, _velocity.Y * 0.7f, 0, Color.White, 3.3f);
					Main.dust[_dust].scale = 1f;
					Main.dust[_dust].noGravity = true;
					Main.dust[_dust].fadeIn = 0.15f;
					if (Projectile.scale > 1.0) {
						Main.dust[_dust].velocity *= Projectile.scale;
						Main.dust[_dust].scale *= Projectile.scale;
					}
				}
			}
		}
		else if (Projectile.owner == Main.myPlayer) {
			Vector2 _diff = mousePosition - _position;
			_diff.Normalize();
			Projectile.velocity = _diff;
			Projectile.direction = Main.MouseWorld.X > player.position.X ? 1 : -1;
			Projectile.netUpdate = true;
		}

		Projectile.position = _position;
		//Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians((Projectile.ai[1] / 300f) * 0.35f) * (1f - Projectile.localAI[1] / (currentCharge + 0.0001f)));
		Projectile.timeLeft = 2;
		player.itemTime = 2;
		player.itemAnimation = 2;
		player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * dir, Projectile.velocity.X * dir);
		player.itemRotation = MathHelper.WrapAngle(player.itemRotation);

        int _delay = 5 + (int)(Projectile.localAI[1] / 10f);
		if (player.noItems || player.CCed || player.itemAnimation <= 0)
			Projectile.Kill();
		else {
			if (!maxCharged) {
                Vector2 spinningpoint3 = Vector2.UnitX * 18f;
                spinningpoint3 = spinningpoint3.RotatedBy(Projectile.rotation - (float)Math.PI / 2f);
				Vector2 position = player.Center + Vector2.Normalize(Projectile.velocity) * 54f + new Vector2(0f, Projectile.gfxOffY + 4f) * player.gravDir + new Vector2(-10f, 8f);
                Vector2 vector11 = position + spinningpoint3;
				int count = Math.Min(6, (int)(Projectile.ai[1] / 15f));
                for (int k = 0; k < count + 1; k++) {
                    int num23 = ModContent.DustType<NatureLaser>();
                    float num24 = 0.4f;
                    if (k % 2 == 1) {
                        num24 = 0.65f;
                    }
                    num24 *= 1.1f;

                    Vector2 vector12 = vector11 + ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * (12f - (float)(count * 2));
                    int num25 = Dust.NewDust(vector12, 16, 16, num23, 0f, 0f);
                    Main.dust[num25].velocity = Vector2.Normalize(vector11 - vector12) * 1.5f * (10f - (float)count * 2f) / 10f;
                    Main.dust[num25].noGravity = true;
                    Main.dust[num25].scale = num24;
                    //Main.dust[num25].customData = player;
                }
                Projectile.ai[1] += 1f;
				if (Projectile.ai[1] % 10 < 1 && !player.CheckMana(4, true))
					maxCharged = true;
				if (Projectile.ai[1] >= 130f)
					maxCharged = true;
				else if (Projectile.ai[1] >= 100f && Projectile.ai[1] < 105f) {
					if (Projectile.ai[1] >= 103f) {
						if (Projectile.localAI[1] == 0f) {
							Projectile.localAI[1] = 1f;
							SoundEngine.PlaySound(SoundID.Item43, Projectile.position);
						}
                    }
					for (int i = 0; i < 5; i++) {
						float _spawnX = Projectile.velocity.ToRotation() + (float)((Main.rand.Next(2) == 1 ? -1.0 : 1.0) * 0.1f);
						float _spawnY = (float)(Main.rand.NextDouble() * 4.0 + 1.0);
						Vector2 _velocity = new((float)Math.Cos((double)_spawnX) * _spawnY, (float)Math.Sin((double)_spawnX) * _spawnY);
						var _dust = Dust.NewDust(_position + Projectile.velocity * 44f - new Vector2(4f, 4f), 0, 0, ModContent.DustType<NatureLaser>(), _velocity.X * 0.7f, _velocity.Y * 0.7f, 0, Color.White, 2.5f);
						Main.dust[_dust].scale = 1.2f;
						Main.dust[_dust].noGravity = true;
					}
				}
				currentCharge = Projectile.ai[1];
				Projectile.localAI[0] = 1f;
			} 
			else if (Projectile.localAI[0] == 1f) {
				maxCharged = true;
				if (Projectile.ai[1] < 100f) {
					if (Projectile.localAI[1] == 0f) {
						Projectile.localAI[1] = 1f;
						SoundEngine.PlaySound(SoundID.Item43, Projectile.position);
                        int count = 3 + (int)(3 * Projectile.ai[1] / 100f);
                        for (int i = 0; i < count; i++) {
                            float _spawnX = Projectile.velocity.ToRotation() + (float)((Main.rand.Next(2) == 1 ? -1.0 : 1.0) * 0.1f);
                            float _spawnY = (float)(Main.rand.NextDouble() * 4.0 + 1.0);
                            Vector2 _velocity = new((float)Math.Cos((double)_spawnX) * _spawnY, (float)Math.Sin((double)_spawnX) * _spawnY);
                            var _dust = Dust.NewDust(_position + Projectile.velocity * 44f - new Vector2(4f, 4f), 0, 0, ModContent.DustType<NatureLaser>(), _velocity.X * 0.7f, _velocity.Y * 0.7f, 0, Color.White, 2.5f);
                            Main.dust[_dust].scale = 1.2f;
                            Main.dust[_dust].noGravity = true;
                        }
                    }
                }
                Projectile.localAI[1]++;
				if (Projectile.localAI[1] >= currentCharge) {
					//player.channel = false;
					Projectile.Kill();
				}
			}
		}
		if (maxCharged)
			if (Projectile.ai[2] < 10f) {
				Projectile.ai[2]++;
			}
			else {
				for (int i = 78; i < Projectile.ai[0]; i++) {
					if (Main.rand.NextChance(0.005)) {
						Vector2 _velocity = Utils.RotatedBy(Projectile.velocity, 0d, default) * value1;
						Vector2 _position2 = _position + i * Projectile.velocity + (player.direction == 1 ? -new Vector2(4f, 4f) : new Vector2(4f, -4f));
						var _dust = Dust.NewDust(_position2, 0, 0, ModContent.DustType<NatureLaser>(), _velocity.X, _velocity.Y, 0, Color.White, 2.3f);
						Main.dust[_dust].scale = 1f;
						Main.dust[_dust].noGravity = true;
						Lighting.AddLight(_position2, 73f / 255f, 170f / 255f, 104f / 255f);
					}
				}
			}

		//if (!hitting)
		//	for (Projectile.ai[0] = 100f; Projectile.ai[0] <= 2000f; Projectile.ai[0] += 8f) {
		//		Vector2 checkedPos = Projectile.position + Projectile.velocity * Projectile.ai[0] + Projectile.velocity * 24f;
		//		if (WorldGen.SolidOrSlopedTile((int)checkedPos.X / 16, (int)checkedPos.Y / 16)) {
		//			Projectile.ai[0] += 8f;
		//			break;
		//		}
		//	}
	}

    public override void PostAI() {
        if (!hitting && !Projectile.ownerHitCheck && Projectile.localAI[2] == 0f) {
            float num716 = 3;
            Vector2 samplingPoint = Main.player[Projectile.owner].Center + new Vector2(0f, Projectile.gfxOffY + 4f);
            float[] array2 = new float[(int)num716];
            Collision.LaserScan(samplingPoint, Projectile.velocity, 0f, 2000f, array2);
            float num718 = 0f;
            for (int num719 = 0; num719 < array2.Length; num719++) {
                num718 += array2[num719];
            }
            num718 /= num716;
			Projectile.ai[0] = num718;

			Projectile.netUpdate = true;
        }
    }

    public override bool ShouldUpdatePosition() => false;

	public override void CutTiles() {
		if (Projectile.localAI[1] <= 0f || Projectile.ai[2] < 5f)
			return;
		DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
		Vector2 unit = Projectile.velocity;
		Utils.PlotTileLine(Projectile.Center, Projectile.Center + unit * Projectile.ai[0], (Projectile.width + 16) * Projectile.scale, DelegateMethods.CutTiles);
	}
}