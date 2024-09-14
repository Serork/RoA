using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core;

using System.IO;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies;

sealed class ArchVileSpike : ModProjectile {
	public override string Texture => ResourceManager.EnemyProjectileTextures + nameof(VileSpike);

    private const int MAX_TIMELEFT = 270;
    
	private bool _spawnedNext;

	public override void SendExtraAI(BinaryWriter writer) => writer.Write(_spawnedNext);
	public override void ReceiveExtraAI(BinaryReader reader) => _spawnedNext = reader.ReadBoolean();

	public override void SetDefaults () {
		int width = 30; int height = 32;
		Projectile.Size = new Vector2(width, height);

		Projectile.CloneDefaults(ProjectileID.VilethornBase);

		Projectile.aiStyle = AIType = -1;

		Projectile.penetrate = -1;
		Projectile.timeLeft = MAX_TIMELEFT;

		Projectile.alpha = byte.MaxValue;

        Projectile.friendly = false;
        Projectile.hostile = true;
    }

	public override bool? CanDamage() => Projectile.alpha == 0;

    public override bool ShouldUpdatePosition() => false;

    public override void AI () {
		float height = 6f;
		float height2 = 4f;
		Projectile.velocity = Vector2.Zero;
		Projectile.ai[0] += 2f;
		if (Projectile.ai[1] == 0f && Projectile.ai[0] < 55f) {
			if (Main.netMode != NetmodeID.Server) {
                int dust = Dust.NewDust(new Vector2(Projectile.Center.X + Main.rand.Next(-32, 32), Projectile.Center.Y + 12f), 8, 8, ModContent.DustType<ArchdruidDust>(), 0f, Main.rand.NextFloat(-2.5f, -0.5f), 255, Scale: 0.9f + Main.rand.NextFloat(0f, 0.4f));
                Main.dust[dust].velocity *= 0.25f;
            }
		}
		if (Projectile.ai[0] < 30f) {
			if (Projectile.ai[1] == 0f) {
				Projectile.timeLeft = MAX_TIMELEFT;
				return;
			}
		}
        if (Projectile.alpha != 0) {
			Projectile.alpha = 0;
		}
		if (Projectile.ai[0] % height2 == 0 && !_spawnedNext) {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int projectile = Projectile.NewProjectile(Projectile.GetSource_FromAI(), new Vector2(Projectile.Center.X, Projectile.Center.Y - 32), Vector2.Zero, ModContent.ProjectileType<ArchVileSpike>(), Projectile.damage, Projectile.knockBack, 0, 0f);
				Main.projectile[projectile].ai[1] = Projectile.ai[1] + 1f;
				Main.projectile[projectile].netUpdate = true;
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile);
            }
			_spawnedNext = true;
			Projectile.netUpdate = true;
		}
		if (Projectile.ai[1] >= height) {
			Projectile.ai[1] = height + 1;
			Projectile.Kill();
            Projectile.netUpdate = true;
		}
	}

    public override void OnKill(int timeLeft) {
		if (Projectile.ai[1] < 6f) {
			return;
		}

        if (Main.netMode != NetmodeID.MultiplayerClient) {
            int projectile = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ArchVileSpikeTip>(), Projectile.damage, Projectile.knockBack);
            Main.projectile[projectile].netUpdate = true;
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile);
        }
    }

    //public override void Kill(int timeLeft) {
    //	if (Main.netMode == NetmodeID.Server) {
    //		return;
    //	}
    //	SoundEngine.PlaySound(SoundID.Dig, new Vector2(Projectile.position.X, Projectile.position.Y));
    //	for (int i = 0; i < 4; i++) {
    //		int dust = Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), 10, 10, 1, 0, 0, 0, new Color(75, 60, 55), 0.4f + Main.rand.NextFloat(0, 1f));
    //		Main.dust [dust].velocity *= 0.3f;
    //	}
    //	for (int i = Main.rand.Next(3); i < 2; i++) {
    //		Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Vector2.Zero, ModContent.Find<ModGore>(nameof(RiseofAges) + "/VileSpikeGore").Type, 1f);
    //	}
    //}
}

sealed class ArchVileSpikeTip : ModProjectile {
    public override string Texture => ResourceManager.EnemyProjectileTextures + nameof(VileSpikeTip);

    public override bool ShouldUpdatePosition() => false;

    public override void SetDefaults() {
		int width = 30; int height = 32;
		Projectile.Size = new Vector2(width, height);

		Projectile.CloneDefaults(ProjectileID.VilethornTip);

		Projectile.friendly = false;
		Projectile.hostile = true;

		Projectile.aiStyle = AIType = -1;

		Projectile.penetrate = -1;
		Projectile.timeLeft = 270;

		Projectile.alpha = 0;
	}

	public override void AI() => Projectile.velocity = Vector2.Zero;

	//public override void Kill(int timeLeft) {
	//	if (Main.netMode == NetmodeID.Server) {
	//		return;
	//	}
	//	SoundEngine.PlaySound(SoundID.Dig, new Vector2(Projectile.position.X, Projectile.position.Y));
	//	Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Vector2.Zero, ModContent.Find<ModGore>(nameof(RiseofAges) + "/VileSpikeGore").Type, 1f);
	//}
}
