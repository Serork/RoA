using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class Brightstone : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(ref Color lightColor) => false;

    public override void SetDefaults() {
        int width = 35; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.light = 0.1f;
        Projectile.hide = true;
        Projectile.timeLeft = 180;
    }

    public override void AI() {
        if (Projectile.ai[1] < 1f) {
            Projectile.ai[1] += TimeSystem.LogicDeltaTime * 5f;
        }

        int num113 = ModContent.DustType<BrightstoneDust>();
        if (Main.rand.Next(12 + (int)(11 * (1f - Projectile.ai[1]))) == 0) {
            int num114 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, num113, 0f, 0f, 200, default, 0.7f);
            Dust dust2 = Main.dust[num114];
            dust2.velocity *= 0.3f + Main.rand.NextFloatRange(0.1f) * Main.rand.NextFloat();
            dust2.noGravity = true;
        }
        if (Main.rand.NextBool(10 + (int)(9 * (1f - Projectile.ai[1])))) {
            int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 30, 30, DustID.AncientLight, 0f, 0f, 100, new Color(180, 165, 5), Main.rand.NextFloat(0.8f, 1.2f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].noLight = false;
            Main.dust[dust].velocity.Y *= 0.1f;
            Main.dust[dust].velocity.X *= 0.1f;
            Main.dust[dust].alpha = Projectile.alpha;
        }

        Lighting.AddLight(Projectile.Center, new Color(238, 225, 111).ToVector3() * 0.6f * Projectile.ai[1]);
    }

    public override bool? CanCutTiles() => false;

    public override bool? CanDamage() => false;
}