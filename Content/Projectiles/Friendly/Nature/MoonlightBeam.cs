using Microsoft.Xna.Framework;

using RoA.Core;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class MoonlightBeam : NatureProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Moonlight Beam");
    }

    protected override void SafeSetDefaults() {
        int width = 5; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = 5;
        Projectile.timeLeft = 5 * 60;

        Projectile.aiStyle = -1;

        Projectile.alpha = 255;

        Projectile.tileCollide = true;
        Projectile.friendly = true;

        Projectile.extraUpdates = 100;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox.Inflate(5, 5);
    }

    public override void AI() {
        Lighting.AddLight(Projectile.Center, 0.2f, 0.2f, 0.1f);
        if (Projectile.velocity.X != Projectile.velocity.X) {
            Projectile.position.X = Projectile.position.X + Projectile.velocity.X;
            Projectile.velocity.X = -Projectile.velocity.X;
        }
        if (Projectile.velocity.Y != Projectile.velocity.Y) {
            Projectile.position.Y = Projectile.position.Y + Projectile.velocity.Y;
            Projectile.velocity.Y = -Projectile.velocity.Y;
        }
        Projectile.ai[0]++;
        if (Projectile.ai[0] > 3f) {
            for (int k = 0; k < 1; k++) {
                Projectile.position -= Projectile.velocity * (k * 0.25f);
                int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 1, 1, DustID.AncientLight, 0f, 0f, 0, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].noLight = false;
                Main.dust[dust].velocity.Y -= 1f;
                Main.dust[dust].velocity.X *= 0.1f;
                int dust2 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 1, 1, DustID.AncientLight, 0f, 0f, 0, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.2f));
                Main.dust[dust2].noGravity = true;
                Main.dust[dust2].noLight = false;
                Main.dust[dust2].velocity *= 0.1f;
            }
            return;
        }
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 16; i++) {
            int dust3 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 4, 4, DustID.AncientLight, 0, 0, 0, new Color(180, 165, 5), Main.rand.NextFloat(0.7f, 1.4f));
            Main.dust[dust3].noGravity = true;
            Main.dust[dust3].noLight = false;
            Main.dust[dust3].velocity.Y -= 1f;
        }
    }
}
