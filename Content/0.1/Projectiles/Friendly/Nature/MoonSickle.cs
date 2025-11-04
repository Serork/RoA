using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class MoonSickle : NatureProjectile {
    public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 200, 200) * 0.9f * (1f - Projectile.alpha / 255f);

    private float rotationTimer = 3.14f * 2f;
    private float lightIntensity = 0;

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Sacrificial Sickle");
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
    }

    protected override void SafeSetDefaults() {
        int width = 32; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.aiStyle = AIType = -1;

        Projectile.alpha = 255;
        Projectile.light = 0f;
        Projectile.timeLeft = 60;

        Projectile.friendly = true;
        Projectile.tileCollide = false;

        Projectile.penetrate = 3;
    }

    public override bool PreDraw(ref Color lightColor) {
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Vector2 drawOrigin = new(texture.Width * 0.5f, texture.Height * 0.5f);
        float rotation = Projectile.rotation;
        SpriteEffects effects = (SpriteEffects)(Projectile.ai[2] == 1).ToInt();
        for (int k = 0; k < Projectile.oldPos.Length; k++) {
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition;
            Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            Main.EntitySpriteDraw(texture, drawPos, null, color * 0.5f, Projectile.oldRot[k], drawOrigin, Projectile.scale, effects);
        }

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), rotation - MathHelper.PiOver4 * Projectile.ai[2], drawOrigin, Projectile.scale, effects);

        return false;
    }

    public override void SafePostAI() {
        Projectile.tileCollide = Projectile.alpha <= 0;

        if (Projectile.alpha > 0) {
            Projectile.alpha -= 35;
        }
        else {
            Projectile.alpha = 0;
        }

        for (int num28 = Projectile.oldPos.Length - 1; num28 > 0; num28--) {
            Projectile.oldPos[num28] = Projectile.oldPos[num28 - 1];
            Projectile.oldRot[num28] = Projectile.oldRot[num28 - 1];
        }

        Projectile.oldPos[0] = Projectile.Center;
        Projectile.oldRot[0] = Projectile.rotation;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        base.SafeOnSpawn(source);

        Player player = Main.player[Projectile.owner];
        Vector2 pos = player.MountedCenter;
        pos = Utils.Floor(pos) + Vector2.UnitY * player.gfxOffY;
        Projectile.ai[2] = player.direction;
        Projectile.ai[1] = Main.rand.NextFloat(5f, 10f);
        Projectile.Center = pos - Vector2.UnitX * Projectile.ai[1] * 10f * Projectile.ai[2];
        //Projectile.Center -= Vector2.UnitY * player.height * 2f * Projectile.ai[2];
        //Projectile.Center = pos;
        //Projectile.ai[0] = 10f;
        ////Projectile.Center -= Vector2.UnitY * player.height * 2f * Projectile.ai[2];
        //float dist = Vector2.Distance(Projectile.Center, player.GetViableMousePosition()) * 0.04f;
        //float max = Projectile.ai[0] * 0.5f;
        //if (Math.Abs(dist) < max) {
        //    dist = max * dist.GetDirection();
        //}
        //Projectile.ai[1] = MathHelper.Clamp(dist, -Projectile.ai[0], Projectile.ai[0]);
    }

    public override void AI() {
        if (Main.rand.NextBool(2)) {
            int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 30, 30, DustID.AncientLight, 0f, 0f, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].noLight = false;
            Main.dust[dust].velocity.Y -= 1f;
            Main.dust[dust].velocity.X *= 0.1f;
            Main.dust[dust].alpha = Projectile.alpha;
        }

        lightIntensity = 1f - (float)Projectile.alpha / 255;
        Lighting.AddLight(Projectile.Center, 0.4f * lightIntensity, 0.4f * lightIntensity, 0.2f * lightIntensity);

        Player player = Main.player[Projectile.owner];
        Projectile.rotation += 0.5f / rotationTimer * Projectile.ai[2];
        float rot = Projectile.rotation - MathHelper.PiOver2;
        Projectile.velocity = (rot.ToRotationVector2() * Projectile.ai[1]).Floor();
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 20;
        return Projectile.timeLeft < 30;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.netUpdate = true;

        for (int num615 = 0; num615 < 5; num615++) {
            int num616 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.AncientLight, Projectile.velocity.X, Projectile.velocity.Y, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[num616].noGravity = true;
            Dust dust2 = Main.dust[num616];
            dust2.scale *= 1.25f;
            dust2 = Main.dust[num616];
            dust2.velocity *= 0.5f + 0.5f * Main.rand.NextFloat();
        }

        return true;
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "MoonVanish") { PitchVariance = 0.1f }, Projectile.Center);
        for (int i = 0; i < 5; i++) {
            int dust3 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 24, 24, DustID.AncientLight, 0f, 0f, 0, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[dust3].noGravity = true;
            Main.dust[dust3].noLight = false;
            Main.dust[dust3].velocity.Y -= 1f;
            Main.dust[dust3].velocity.X *= 0.1f;
        }
    }
}