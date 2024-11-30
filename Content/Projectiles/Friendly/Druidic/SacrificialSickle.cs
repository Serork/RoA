using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace RoA.Content.Projectiles.Friendly.Druidic;

sealed class SacrificialSickle : NatureProjectile {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Sacrificial Sickle");
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    protected override void SafeSetDefaults() {
        int width = 38; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.CloneDefaults(ProjectileID.LightDisc);
        AIType = 106;

        Projectile.penetrate = -1;
        Projectile.scale = 1f;

        Projectile.timeLeft = 200;

        Projectile.tileCollide = false;
    }

    public override void AI() {
        Lighting.AddLight(Projectile.Center, 0.4f, 0.4f, 0.2f);
        Projectile.localAI[2]++;
        if (Projectile.localAI[2] >= 20) {
            int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 1, 1, DustID.AncientLight, 0f, 0f, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].noLight = false;
            Main.dust[dust].velocity.Y = -1f;
            Main.dust[dust].velocity.X *= 0.1f;
            int dust2 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + 24), 1, 1, DustID.AncientLight, 0f, 0f, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[dust2].noGravity = true;
            Main.dust[dust2].noLight = false;
            Main.dust[dust2].velocity.Y = -1f;
            Main.dust[dust2].velocity.X *= 0.1f;
        }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;
        return true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        base.OnHitNPC(target, hit, damageDone);

        Player player = Main.player[Projectile.owner];
        if (Projectile.localAI[2] >= 35) {
            player.HealEffect(3);
            player.statLife += 3;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        if (Projectile.localAI[2] >= 35) {
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++) {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                spriteBatch.Draw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }
        }
        return true;
    }

    public override Color? GetAlpha(Color lightColor)
     => new Color(255, 255, 200, 200);
}

