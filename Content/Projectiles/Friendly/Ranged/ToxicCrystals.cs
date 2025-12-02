using Microsoft.Xna.Framework;

// using RoA.Content.Buffs;
using RoA.Content.Dusts;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

class ToxicCrystal2 : ToxicCrystal1 { }

class ToxicCrystal3 : ToxicCrystal1 { }

class ToxicCrystal1 : ModProjectile {
    public override Color? GetAlpha(Color lightColor) => lightColor * Projectile.Opacity;

    public override void SetDefaults() {
        int width = 12; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.timeLeft = 360;
        Projectile.aiStyle = 14;
        Projectile.DamageType = DamageClass.Ranged;

        Projectile.penetrate = 3;
        Projectile.friendly = true;
        Projectile.alpha = 100;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = 8;
        height = 8;

        return true;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.velocity = Vector2.Zero;
        return false;
    }

    public override void AI() {
        Projectile.rotation = Helper.VelocityAngle(Projectile.velocity) + MathHelper.Pi;

        Projectile.Opacity = 0.7f;
        if (Projectile.velocity.Y < 0.25 && Projectile.velocity.Y > 0.15) {
            Projectile.velocity.X = Projectile.velocity.X * 0.8f;
        }
        //Projectile._rotation = -Projectile.velocity.X * 0.05f;
        int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<LothorPoison>(), 0f, 0f, 100, default, 1f);
        Main.dust[dust].scale += Main.rand.Next(40) * 0.01f;
        Main.dust[dust].noGravity = true;
        Main.dust[dust].velocity.X *= 0.5f;
        Dust dust2 = Main.dust[dust];
        dust2.position.X -= 2f;
        Dust dust3 = Main.dust[dust];
        dust3.position.Y += 2f;
        Dust dust4 = Main.dust[dust];
        dust4.velocity.Y -= 2f;


        if (Projectile.timeLeft > 360 - 1) {
            return;
        }

        if (Main.rand.NextBool()) {
            float num3 = 0f;
            //float y = 0f;
            Vector2 vector6 = Projectile.position;
            Vector2 vector7 = Projectile.oldPosition;
            vector7.Y -= num3 / 2f;
            vector6.Y -= num3 / 2f;
            int num5 = (int)Vector2.Distance(vector6, vector7) / 3 + 1;
            if (Vector2.Distance(vector6, vector7) % 3f != 0f)
                num5++;

            for (float num6 = 1f; num6 <= (float)num5; num6 += 1f) {
                Dust obj = Main.dust[Dust.NewDust(Projectile.position, 0, 0, ModContent.DustType<LothorPoison>(), Alpha: 100, Scale: 1f)];
                obj.position = Vector2.Lerp(vector7, vector6, num6 / (float)num5) + new Vector2(Projectile.width, Projectile.height) / 2f;
                obj.noGravity = true;
                obj.velocity *= 0.1f;
                obj.velocity += Projectile.velocity * 0.5f;
                Dust obj2 = obj;
                obj2.position.X -= 2f;
                Dust obj3 = obj;
                obj3.position.Y += 2f;
                Dust obj4 = obj;
                obj4.velocity.Y -= 2f;
            }
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        float num2 = (float)Main.rand.Next(90, 180) * 0.01f;
        target.AddBuff(BuffID.Poisoned, (int)(60f * num2 * 2f), true);
        // target.AddBuff(ModContent.BuffType<ToxicFumes>(), (int)(60f * num2 * 2f), true);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        float num2 = (float)Main.rand.Next(90, 180) * 0.01f;
        target.AddBuff(BuffID.Poisoned, (int)(60f * num2 * 2f), true);
        // target.AddBuff(ModContent.BuffType<ToxicFumes>(), (int)(60f * num2 * 2f), true);
    }
}