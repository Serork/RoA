using Microsoft.Xna.Framework;

using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class PumpkinSeed : NatureProjectile {
    protected override void SafeSetDefaults() {
        int width = 20, height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.ignoreWater = true;
        Projectile.friendly = true;

        Projectile.tileCollide = true;

        Projectile.aiStyle = 1;
        AIType = ProjectileID.Bullet;

        Projectile.Opacity = 0f;

        Projectile.timeLeft = 90;

        DrawOffsetX = -2;
    }

    public override void SafePostAI() {
        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.25f);

        Projectile.velocity *= 0.99f;
        if (Projectile.timeLeft <= 50) {
            Projectile.velocity.Y += 0.4f;
            Projectile.rotation += Projectile.velocity.Y * Projectile.direction;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity);

        return false;
    }

    protected override void SafeOnSpawn(IEntitySource source) => Projectile.scale = Main.rand.NextFloat(0.8f, 1.2f);

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 8;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        SoundEngine.PlaySound(SoundID.Dig with { Pitch = 1.1f, Volume = 0.5f }, Projectile.Center);

        return base.OnTileCollide(oldVelocity);
    }

    public override void OnKill(int timeLeft) {
        if (Main.netMode != NetmodeID.Server) {
            for (int i = 0; i < 5; i++) {
                int dust = Dust.NewDust(Projectile.position, 20, 20, DustID.Water_Desert, Projectile.velocity.X * 0.3f, Projectile.velocity.Y * 0.3f, 0, new Color(250, 220, 120), 1f);
                Main.dust[dust].noGravity = false;
                Main.dust[dust].scale *= 0.8f;
            }
        }
    }
}