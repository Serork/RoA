using Microsoft.Xna.Framework;

using RoA.Core;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class AmuletOfLifeWisps : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(ref Color lightColor) => false;

    private float radius = 190f;

    //public override void SetStaticDefaults()	
    //	=> DisplayName.SetDefault("Healing Wisps");


    public override void SetDefaults() {
        int width = 22; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.aiStyle = -1;

        Projectile.tileCollide = false;
        Projectile.timeLeft = 600;

        Projectile.friendly = true;

        Projectile.DamageType = DamageClass.Default;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];

        int dust = Dust.NewDust(Projectile.position, 8, 8, DustID.CrystalPulse, Projectile.velocity.X * 0.4f, 0, 50, default(Color), 0.9f);
        Main.dust[dust].noGravity = true;
        Main.dust[dust].velocity.Y = 0;
        Main.dust[dust].alpha++;

        float tick = Projectile.ai[0]++;
        float degrees = MathHelper.ToDegrees(tick);
        Vector2 playerPos = player.Center - new Vector2(player.width / 2f, 0f) + new Vector2(4f, -4f);
        Projectile.position.X = playerPos.X + (float)Math.Cos(degrees / 3600) * radius;
        Projectile.position.Y = playerPos.Y + (float)Math.Sin(degrees / 3600) * radius;
        radius -= 0.5f;
        if (radius <= 0 || player.getRect().Intersects(Projectile.getRect())) {
            player.statLife += 15;
            player.HealEffect(15);
            Projectile.Kill();
        }
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.75f, Volume = 0.75f }, Projectile.Center);
        for (int i = 0; i < 8; i++)
            Dust.NewDust(Projectile.position, 8, 8, DustID.CrystalPulse, Main.rand.Next(-4, 4) * 0.9f, Main.rand.Next(-4, 4) * 0.9f, 50, default(Color), 0.9f);
    }

    public override bool? CanDamage() => false;

    public override bool? CanCutTiles() => false;
}
