using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent.Achievements;
using Terraria.Audio;
using RoA.Content.Dusts;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class SlipperyDynamite : ModProjectile {
	private Vector2 memorizeVelocity;
	private int effectCounter;
	private int effectCounterMax = 1;

	public override void SetDefaults() {
		int width = 10; int height = width;
		Projectile.Size = new Vector2(width, height);

		Projectile.aiStyle = 16;
		Projectile.penetrate = -1;

		Projectile.friendly = true;
		Projectile.tileCollide = true;

		Projectile.timeLeft = 180;

		DrawOriginOffsetY = -10;
	}

	public override void AI() {
		if (!Projectile.tileCollide) {
			memorizeVelocity *= 0.965f;
			Projectile.velocity = memorizeVelocity;
			effectCounter++;
            if (Projectile.ai[2] <= 0f) {
                if (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
                    memorizeVelocity = Vector2.Zero;
                    Projectile.tileCollide = true;
                }
            }
            else {
                Projectile.ai[2]--;
            }
            if (effectCounter == effectCounterMax && effectCounterMax < 20) {
				effectCounterMax += 3;
				effectCounter = 0;
                SoundEngine.PlaySound(SoundID.WormDig, Projectile.position);
            }
            if (effectCounter % 4 == 0 && effectCounterMax < 20) {
                int dustDig = Dust.NewDust(Projectile.Center - Vector2.One * 10, 20, 20, ModContent.DustType<Galipot2>(), 0f, 0f, 0, default(Color), 1f);
                Main.dust[dustDig].velocity *= 0.1f;
                Main.dust[dustDig].noGravity = true;
            }
        }

		if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
			Projectile.tileCollide = false;
			Projectile.alpha = 255;
			Projectile.position = Projectile.Center;
			Projectile.width = 250;
			Projectile.height = 250;
			Projectile.Center = Projectile.position;
			Projectile.damage = 250;
			Projectile.knockBack = 10f;
		}
		else {
			if (Main.rand.NextBool()) {
				int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 31, 0f, 0f, 100, default(Color), 1f);
				Main.dust[dustIndex].scale = 0.1f + (float)Main.rand.Next(5) * 0.1f;
				Main.dust[dustIndex].fadeIn = 1.5f + (float)Main.rand.Next(5) * 0.1f;
				Main.dust[dustIndex].noGravity = true;
				Main.dust[dustIndex].position = Projectile.Center + new Vector2(0f, (float)(-(float)Projectile.height / 2)).RotatedBy((double)Projectile.rotation, default(Vector2)) * 1.1f;
				dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, 0f, 0f, 100, default(Color), 1f);
				Main.dust[dustIndex].scale = 1f + (float)Main.rand.Next(5) * 0.1f;
				Main.dust[dustIndex].noGravity = true;
				Main.dust[dustIndex].position = Projectile.Center + new Vector2(0f, (float)(-(float)Projectile.height / 2 - 6)).RotatedBy((double)Projectile.rotation, default(Vector2)) * 1.1f;
			}
		}
		Projectile.ai[0] += 1f;
		Projectile.rotation += Projectile.velocity.X * 0.05f;
		return;
	}

	public override bool OnTileCollide(Vector2 oldVelocity) {
		if (Projectile.ai[1] != 0) return true;
		if (memorizeVelocity == Vector2.Zero)
			memorizeVelocity = oldVelocity * 0.75f;
		Projectile.tileCollide = false;
        Projectile.ai[2] = 10f;
        return false;
	}

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        if (Main.expertMode) {
            if (target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail) modifiers.FinalDamage /= 5;
        }
    }

	public override void OnKill(int timeLeft) {
		SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
		for (int i = 0; i < 50; i++) {
			int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 31, 0f, 0f, 100, default(Color), 2f);
			Main.dust[dustIndex].velocity *= 1.4f;
		}

		for (int i = 0; i < 80; i++) {
			int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, 0f, 0f, 100, default(Color), 3f);
			Main.dust[dustIndex].noGravity = true;
			Main.dust[dustIndex].velocity *= 5f;
			dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, 0f, 0f, 100, default(Color), 2f);
			Main.dust[dustIndex].velocity *= 3f;
		}

		if (!Main.dedServ) {
			for (int g = 0; g < 2; g++) {
				int goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + (Projectile.width / 2) - 24f, Projectile.position.Y + (Projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1.5f;
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
				goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + (Projectile.width / 2) - 24f, Projectile.position.Y + (Projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1.5f;
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1.5f;
				goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + (Projectile.width / 2) - 24f, Projectile.position.Y + (Projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1.5f;
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
				goreIndex = Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.position.X + (Projectile.width / 2) - 24f, Projectile.position.Y + (Projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
				Main.gore[goreIndex].scale = 1.5f;
				Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
				Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1.5f;
			}
		}

		Projectile.position.X = Projectile.position.X + (Projectile.width / 2);
		Projectile.position.Y = Projectile.position.Y + (Projectile.height / 2);
		Projectile.width = 10;
		Projectile.height = 10;
		Projectile.position.X = Projectile.position.X - (Projectile.width / 2);
		Projectile.position.Y = Projectile.position.Y - (Projectile.height / 2);

		int explosionRadius = 7;
		int minTileX = (int)(Projectile.position.X / 16f - explosionRadius);
		int maxTileX = (int)(Projectile.position.X / 16f + explosionRadius);
		int minTileY = (int)(Projectile.position.Y / 16f - explosionRadius);
		int maxTileY = (int)(Projectile.position.Y / 16f + explosionRadius);
		if (minTileX < 0) minTileX = 0;
		if (maxTileX > Main.maxTilesX) maxTileX = Main.maxTilesX;
		if (minTileY < 0) minTileY = 0;
		if (maxTileY > Main.maxTilesY) maxTileY = Main.maxTilesY;

		bool canKillWalls = false;
		for (int x = minTileX; x <= maxTileX; x++) {
			for (int y = minTileY; y <= maxTileY; y++) {
				float diffX = Math.Abs((float)x - Projectile.position.X / 16f);
				float diffY = Math.Abs((float)y - Projectile.position.Y / 16f);
				double distance = Math.Sqrt((diffX * diffX + diffY * diffY));
				if (distance < explosionRadius && Main.tile[x, y] != null && Main.tile[x, y].WallType == 0) {
					canKillWalls = true;
					break;
				}
			}
		}

		AchievementsHelper.CurrentlyMining = true;
		for (int i = minTileX; i <= maxTileX; i++) {
			for (int j = minTileY; j <= maxTileY; j++) {
				float diffX = Math.Abs((float)i - Projectile.position.X / 16f);
				float diffY = Math.Abs((float)j - Projectile.position.Y / 16f);
				double distanceToTile = Math.Sqrt((diffX * diffX + diffY * diffY));
				if (distanceToTile < explosionRadius) {
					bool canKillTile = true;
					if (Main.tile[i, j] != null && Main.tile[i, j].HasTile) {
						canKillTile = true;
						if (Main.tileDungeon[(int)Main.tile[i, j].TileType] || Main.tile[i, j].TileType == 88 || Main.tile[i, j].TileType == 21 || Main.tile[i, j].TileType == 26 || Main.tile[i, j].TileType == 107 || Main.tile[i, j].TileType == 108 || Main.tile[i, j].TileType == 111 || Main.tile[i, j].TileType == 226 || Main.tile[i, j].TileType == 237 || Main.tile[i, j].TileType == 221 || Main.tile[i, j].TileType == 222 || Main.tile[i, j].TileType == 223 || Main.tile[i, j].TileType == 211 || Main.tile[i, j].TileType == 404) {
							canKillTile = false;
						}
						if (!Main.hardMode && Main.tile[i, j].TileType == 58) {
							canKillTile = false;
						}
                        if (!TileLoader.CanExplode(i, j) || !Projectile.CanExplodeTile(i, j)) {
                            canKillTile = false;
						}
						if (canKillTile) {
							WorldGen.KillTile(i, j, false, false, false);
							if (!Main.tile[i, j].HasTile && Main.netMode != NetmodeID.SinglePlayer)
								NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, (float)i, (float)j, 0f, 0, 0, 0);
						}
					}

					if (canKillTile) {
						for (int x = i - 1; x <= i + 1; x++) {
							for (int y = j - 1; y <= j + 1; y++) {
								if (Main.tile[x, y] != null && Main.tile[x, y].WallType > 0 && canKillWalls && WallLoader.CanExplode(x, y, Main.tile[x, y].WallType)) {
									WorldGen.KillWall(x, y, false);
									if (Main.tile[x, y].WallType == 0 && Main.netMode != NetmodeID.SinglePlayer)
										NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 2, (float)x, (float)y, 0f, 0, 0, 0);
								}
							}
						}
					}
				}
			}
		}
		AchievementsHelper.CurrentlyMining = false;
	}
}
