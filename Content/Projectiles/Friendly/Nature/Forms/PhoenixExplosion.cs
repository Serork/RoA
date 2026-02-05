using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Defaults;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

sealed class PhoenixExplosion : FormProjectile {
    public override string Texture => ResourceManager.EmptyTexture;
    public override bool PreDraw(ref Color lightColor) => false;

    protected override void SafeSetDefaults() {
        Projectile.width = 50;
        Projectile.height = 50;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 10;
        Projectile.tileCollide = false;
    }

    public override void AI() {
        if (Projectile.localAI[2] == 0f) {
            Projectile.localAI[2] = 1f;

            Vector2 previousSize = Projectile.Size;
            Projectile.SetSizeValues((int)(50 * Projectile.ai[1]));
            Projectile.position -= (Projectile.Size - previousSize) / 2f;
        }

        if (Projectile.timeLeft < 9) {
            return;
        }

        for (int i = 0; i < 10; i++) {
            int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 31, 0f, 0f, 100, default(Color), 2f);
            Main.dust[dustIndex].position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) / 2f;
            Main.dust[dustIndex].velocity *= 1.4f;

            Main.dust[dustIndex].velocity += Projectile.velocity;
        }
        for (int i = 0; i < 10; i++) {
            int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, 0f, 0f, 100, default(Color), 3f);
            Main.dust[dustIndex].position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) / 2f;
            Main.dust[dustIndex].noGravity = true;
            Main.dust[dustIndex].velocity *= 5f;

            Main.dust[dustIndex].velocity += Projectile.velocity;

            dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, 0f, 0f, 100, default(Color), 2f);
            Main.dust[dustIndex].position = Projectile.Center + Main.rand.NextVector2Circular(Projectile.width, Projectile.height) / 2f;
            Main.dust[dustIndex].velocity *= 3f;

            Main.dust[dustIndex].velocity += Projectile.velocity;
        }
        //IEntitySource source = Projectile.GetSource_FromAI();

        //if (!Main.dedServ) {
        //    int goreIndex = Gore.NewGore(source, new Vector2(Projectile.position.X + (float)(Projectile.width / 2) - 24f, Projectile.position.Y + (float)(Projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
        //    //Main.gore[goreIndex].scale = 1.5f;
        //    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1.5f;
        //    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - Main.rand.NextFloat(-1.5f, 1.5f);
        //    goreIndex = Gore.NewGore(source, new Vector2(Projectile.position.X + (float)(Projectile.width / 2) - 24f, Projectile.position.Y + (float)(Projectile.height / 2) - 24f), default(Vector2), Main.rand.Next(61, 64), 1f);
        //    Main.gore[goreIndex].scale = 1.5f;
        //    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1.5f;
        //    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - Main.rand.NextFloat(-1.5f, 1.5f);

        //    Main.gore[goreIndex].velocity += Projectile.velocity;
        //}
    }
}
