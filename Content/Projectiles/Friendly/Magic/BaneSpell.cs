using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

sealed class BaneSpell : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults() {
        int width = 14; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.aiStyle = 1;
        AIType = ProjectileID.Bullet;

        Projectile.friendly = true;
        Projectile.tileCollide = true;
        Projectile.DamageType = DamageClass.Magic;

        Projectile.penetrate = 1;
        Projectile.timeLeft = 180;

        Projectile.extraUpdates = 1;
        Projectile.alpha = 200;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
        modifiers.DisableCrit();
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 6;
        return true;
    }

    public override void AI() {
        if (Projectile.timeLeft == 180) {
            float radius = 30f;
            int dustCount = 0;
            while (dustCount < radius) {
                Vector2 vector = Vector2.UnitX * 0f;
                vector += -Vector2.UnitY.RotatedBy(dustCount * (7f / radius), default) * new Vector2(3.2f, 3f);
                vector = vector.RotatedBy(Projectile.velocity.ToRotation(), default);
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.Shadowflame, 0f, 0f, 0, Color.DarkViolet);
                Main.dust[dust].alpha = Projectile.alpha;
                Main.dust[dust].noGravity = true;
                Main.dust[dust].position = Projectile.Center + vector - Projectile.velocity.SafeNormalize(Vector2.Zero) * 4f;
                Main.dust[dust].velocity = vector.SafeNormalize(Vector2.UnitY) * 1f * Main.rand.NextFloat(0.8f, 1f);
                Main.dust[dust].velocity += Projectile.velocity.SafeNormalize(Vector2.Zero) * 1.5f;
                int dustCountMax = dustCount;
                dustCount = dustCountMax + 1;
            }
        }
        if (Projectile.timeLeft <= 178) {
            Projectile.alpha -= 2;
            if (Main.netMode != NetmodeID.Server) {
                for (var i = 0; i < 3; i++) {
                    var x = Projectile.Center.X - Projectile.velocity.X / 3f * i;
                    var y = Projectile.Center.Y - Projectile.velocity.Y / 3f * i;
                    var dust2 = Dust.NewDust(new Vector2(x, y), 1, 1, DustID.Shadowflame, 0f, 0f, 0, Color.DarkViolet, 1.5f);
                    Main.dust[dust2].alpha = Projectile.alpha;
                    Main.dust[dust2].position.X = x;
                    Main.dust[dust2].position.Y = y;
                    Main.dust[dust2].velocity *= 0f;
                    Main.dust[dust2].noGravity = true;
                }
            }
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        NPC flag = null;
        foreach (NPC npc in Main.ActiveNPCs) {
            if (npc.HasBuff<EssenceDrain>() && npc.GetGlobalNPC<EssenceDrainNPC>().Source == Projectile.owner) {
                npc.ClearBuff(ModContent.BuffType<EssenceDrain>());
                break;
            }
        }
        target.AddBuff(ModContent.BuffType<EssenceDrain>(), 600);
        target.GetGlobalNPC<EssenceDrainNPC>().Source = Projectile.owner;

        SoundEngine.PlaySound(SoundID.NPCDeath55, Projectile.Center);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(ModContent.BuffType<EssenceDrain>(), 600);
        SoundEngine.PlaySound(SoundID.NPCDeath55, Projectile.Center);
    }

    public override void OnKill(int timeLeft) {
        for (int k = 0; k < 20; ++k) {
            int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y) - Projectile.velocity * 2f, Projectile.width, Projectile.height,
                DustID.Shadowflame, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 100, Color.DarkViolet, 1.6f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].alpha = 0;
        }
    }
}