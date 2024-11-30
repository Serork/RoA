using Terraria.ID;
using Terraria;
using Microsoft.Xna.Framework;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class MoonSickle : NatureProjectile {
    private float rotationTimer = 3.14f;
    private float lightIntensity = 0;

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Moon Sickle");
    }

    protected override void SafeSetDefaults() {
        int width = 14; int height = 18;
        Projectile.Size = new Vector2(width, height);

        Projectile.CloneDefaults(ProjectileID.IceSickle);
        Projectile.aiStyle = 0;

        Projectile.alpha = 0;
        Projectile.light = 0f;

        Projectile.friendly = true;
    }

    public override void AI() {
        if (Main.rand.Next(Projectile.alpha) == 0) {
            int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 30, 30, DustID.AncientLight, 0f, 0f, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].noLight = false;
            Main.dust[dust].velocity.Y -= 1f;
            Main.dust[dust].velocity.X *= 0.1f;
            Main.dust[dust].alpha = Projectile.alpha;
        }
        lightIntensity = 1f - (float)Projectile.alpha / 255;
        Lighting.AddLight(Projectile.Center, 0.4f * lightIntensity, 0.4f * lightIntensity, 0.2f * lightIntensity);
        Projectile.rotation += 1 / rotationTimer;
        rotationTimer += 0.01f;
        Projectile.velocity *= 0.9f;
        Projectile.alpha += 2;
        if (Projectile.alpha == 250) Projectile.Kill();
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 20;
        return true;
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.alpha < 200) {
            for (int i = 0; i < 10; i++) {
                int dust3 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 24, 24, DustID.AncientLight, 0f, 0f, 0, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
                Main.dust[dust3].noGravity = true;
                Main.dust[dust3].noLight = false;
                Main.dust[dust3].velocity.Y -= 1f;
                Main.dust[dust3].velocity.X *= 0.1f;
            }
        }
    }
}