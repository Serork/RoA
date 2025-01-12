using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class CursedAcorn : ModProjectile {
    public override Color? GetAlpha(Color lightColor) => Color.White;

    public override void SetDefaults() {
        Projectile.CloneDefaults(ProjectileID.Bullet);
        Projectile.width = 10;
        Projectile.height = 18;
        Projectile.penetrate = 2;
        Projectile.alpha = 0;
        Projectile.scale = 1.2f;
        Projectile.friendly = false;
        Projectile.hostile = true;
    }

    public override void AI() {
        int vecX = (int)Projectile.position.X / 16;
        int vecY = (int)Projectile.position.Y / 16 + 1;
        Tile tile = Main.tile[vecX, vecY];
        if (tile != null && tile.HasTile) {
            if (TileID.Sets.Platforms[tile.TileType]) {
                Projectile.Kill();
            }
        }
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center + Vector2.UnitY * 22f, -Vector2.UnitY, ModContent.ProjectileType<LothorSpike>(), 
                Projectile.damage, Projectile.knockBack, Main.myPlayer, ai2: 12.5f);
        }
    }
}
