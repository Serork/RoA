using Microsoft.Xna.Framework;

using RoA.Core;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

sealed class LilPhoenixExplosion : FormProjectile {
    public override string Texture => ResourceManager.EmptyTexture;
    public override bool PreDraw(ref Color lightColor) => false;

    protected override void SafeSetDefaults() {
        Projectile.width = 150;
        Projectile.height = 150;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 10;
        Projectile.tileCollide = false;
    }

    public override void AI() {
        if (Projectile.timeLeft < 6) {
            return;
        }

        for (int i = 0; i < 10; i++) {
            int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 31, 0f, 0f, 100, default(Color), 2f);
            Main.dust[dustIndex].position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) / 2f;
            Main.dust[dustIndex].velocity *= 1.4f;
        }
        for (int i = 0; i < 10; i++) {
            int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, 0f, 0f, 100, default(Color), 3f);
            Main.dust[dustIndex].position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) / 2f;
            Main.dust[dustIndex].noGravity = true;
            Main.dust[dustIndex].velocity *= 5f;
            dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, 0f, 0f, 100, default(Color), 2f);
            Main.dust[dustIndex].position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) / 2f;
            Main.dust[dustIndex].velocity *= 3f;
        }
        IEntitySource source = Projectile.GetSource_FromAI();

        if (!Main.dedServ) {
            int goreIndex = Gore.NewGore(source, new Vector2(Projectile.position.X + (float)(Projectile.width / 2) - 24f, Projectile.position.Y + (float)(Projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
            //Main.gore[goreIndex].scale = 1.5f;
            Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
            Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - Main.rand.NextFloat(-1.5f, 1.5f);
            //goreIndex = Gore.NewGore(source, new Vector2(Projectile.position.X + (float)(Projectile.width / 2) - 24f, Projectile.position.Y + (float)(Projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
            Main.gore[goreIndex].scale = 1.5f;
            Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
            Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - Main.rand.NextFloat(-1.5f, 1.5f);
        }
    }
}
