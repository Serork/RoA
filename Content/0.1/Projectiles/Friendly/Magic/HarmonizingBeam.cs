using Microsoft.Xna.Framework;

using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

sealed class HarmonizingBeam : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetDefaults() {
        int width = 16; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.aiStyle = -1;
        AIType = ProjectileID.Bullet;

        Projectile.friendly = true;
        Projectile.tileCollide = true;
        Projectile.DamageType = DamageClass.Magic;

        Projectile.penetrate = 1;
        Projectile.timeLeft = 120;

        Projectile.extraUpdates = 1;
        Projectile.alpha = 255;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 8;
        return true;
    }

    public override void AI() {
        Projectile.position += new Vector2(0f, Projectile.ai[0] * Projectile.ai[2]).RotatedBy(Projectile.velocity.ToRotation());
        if (Projectile.ai[1] != 1f) Projectile.ai[0] -= 1f;
        else Projectile.ai[0] += 1;
        if (Projectile.ai[0] <= -6) Projectile.ai[1] = 1f;
        if (Projectile.ai[0] >= 6) Projectile.ai[1] = 0f;


        if (Projectile.timeLeft > 120 - 1) {
            return;
        }

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
            Dust obj = Main.dust[Dust.NewDust(Projectile.position, 0, 0, 6)];
            obj.position = Vector2.Lerp(vector7, vector6, num6 / (float)num5) + new Vector2(Projectile.width, Projectile.height) / 2f;
            //obj.noGravity = true;
            //obj.velocity *= 0.1f;
            //obj.velocity += Projectile.velocity * 0.5f;
            //obj.noLight = obj.noLightEmittence = true;
            obj.noGravity = true;
            obj.velocity.Y *= 0.25f;
            obj.scale *= 0.9f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        => target.AddBuff(BuffID.OnFire, 180);

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
        => target.AddBuff(BuffID.OnFire, 180);
}