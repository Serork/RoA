using Microsoft.Xna.Framework;

using RoA.Content.Buffs;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

sealed class WaterStream : ModProjectile {
    private bool IsMain => Projectile.ai[1] == -5f;

    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(ref Color lightColor) => false;

    public override void SetDefaults() {
        Projectile.localNPCHitCooldown = 500;
        Projectile.usesLocalNPCImmunity = true;

        int width = 20; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.DamageType = DamageClass.Magic;
        Projectile.friendly = true;

        Projectile.aiStyle = 1;

        Projectile.alpha = 75;

        Projectile.penetrate = -1;

        Projectile.timeLeft = int.MaxValue;
        Projectile.extraUpdates = 2;

        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;

        AIType = ProjectileID.Bullet;
    }

    public override bool PreAI() {
        if (Collision.LavaCollision(Projectile.position, Projectile.width, Projectile.height) && Projectile.ai[1] != -5f) {
            Projectile.Kill();
        }

        return base.PreAI();
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 2;

        return true;
    }

    public override void AI() {
        Projectile.velocity *= IsMain ? 0.985f : 0.95f;
        if (IsMain) {
            Projectile.tileCollide = false;

            return;
        }
        else {
            Projectile.tileCollide = Projectile.timeLeft < 40;
        }

        if (Projectile.owner == Main.myPlayer) {
            int byUUID = Projectile.GetByUUID(Projectile.owner, (int)Projectile.ai[1]);
            if (Main.projectile.IndexInRange(byUUID)) {
                Projectile parent = Main.projectile[byUUID];
                if (!parent.active) {
                    Projectile.Kill();
                }

                if (++Projectile.ai[2] <= Projectile.alpha) {
                    Projectile.timeLeft = 70;
                    parent.timeLeft = Projectile.timeLeft;
                    parent.netUpdate = true;
                    Projectile.netUpdate = true;
                }
                else if (Projectile.timeLeft <= 55) {
                    Vector2 movement = parent.position - Projectile.position;
                    Vector2 speed = movement * (25f / movement.Length());
                    Projectile.velocity += (speed - Projectile.velocity) / 30f;
                    if (Vector2.Distance(parent.Center, Projectile.Center) <= 5f) {
                        parent.Kill();
                        Projectile.Kill();
                        parent.netUpdate = true;
                    }
                }
            }
            else {
                Projectile.Kill();
            }
        }

        for (int k = 0; k < 3; k++) {
            int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.DungeonWater, 0.0f, 0.0f, 100, new Color(), 1.2f);
            Dust dust2 = Main.dust[dust];
            dust2.noGravity = true;
            dust2.velocity *= 0.3f;
            dust2.velocity += Projectile.velocity * 0.5f;
            dust2.position.X -= Projectile.velocity.X / 3f * k;
            dust2.position.Y -= Projectile.velocity.Y / 3f * k;
        }
        if (Main.rand.NextBool(1, 5)) {
            int dust = Dust.NewDust(new Vector2(Projectile.position.X + 6.0f, Projectile.position.Y + 6.0f), Projectile.width - 12, Projectile.height - 12, DustID.DungeonWater, 0.0f, 0.0f, 100, new Color(), 0.72f);
            Main.dust[dust].velocity *= 0.5f;
            Main.dust[dust].velocity += Projectile.velocity * 0.5f;
        }

        Projectile.netUpdate = true;
    }

    public override bool? CanDamage() => Projectile.ai[1] != -5f;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        int buff = BuffID.OnFire;
        //int buff = BuffID.Wet;
        if (target.FindBuff(buff, out int buffIndex)) {
            target.DelBuff(buffIndex);
        }
        if (!Projectile.lavaWet && !target.lavaWet) {
            target.AddBuff(ModContent.BuffType<Deceleration>(), Main.rand.Next(30, 91), false);
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        int buff = BuffID.OnFire;
        //int buff = BuffID.Wet;
        if (target.FindBuff(buff, out int buffIndex)) {
            target.DelBuff(buffIndex);
        }
        if (!Projectile.lavaWet && !target.lavaWet) {
            target.AddBuff(ModContent.BuffType<Deceleration>(), Main.rand.Next(30, 91), false);
        }
    }

    public override void OnKill(int timeLeft) => SoundEngine.PlaySound(SoundID.Item21, Projectile.position);
}