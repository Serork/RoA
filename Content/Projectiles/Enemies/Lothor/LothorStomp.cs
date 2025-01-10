using Microsoft.Xna.Framework;

using RoA.Core;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class LothorStomp : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;
    public override bool PreDraw(ref Color lightColor) => false;

    public override void SetDefaults() {
        Projectile.hostile = true;
        Projectile.friendly = false;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.timeLeft = 120;
        Projectile.penetrate = -1;

        Projectile.aiStyle = -1;

        Projectile.alpha = 255;
    }

    public override void AI() {
        float num = Projectile.ai[1];
        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] > 3f) {
            Projectile.Kill();
            return;
        }
        Projectile.velocity = Vector2.Zero;
        Projectile.position = Projectile.Center;
        Projectile.Size = new Vector2(16f, 8f) * MathHelper.Lerp(5f, num, Utils.GetLerpValue(0f, 9f, Projectile.ai[0]));
        Projectile.Center = Projectile.position;
        Point point = Projectile.TopLeft.ToTileCoordinates();
        Point point2 = Projectile.BottomRight.ToTileCoordinates();
        int num2 = point.X / 2 + point2.X / 2;
        int num3 = Projectile.width / 2;
        if ((int)Projectile.ai[0] % 3 != 0) {
            return;
        }
        int num4 = (int)Projectile.ai[0] / 3;
        for (int i = point.X; i <= point2.X; i++) {
            for (int j = point.Y; j <= point2.Y; j++) {
                if (Vector2.Distance(Projectile.Center, new Vector2(i * 16, j * 16)) > num3)
                    continue;
                Tile tileSafely = Framing.GetTileSafely(i, j);
                if (!tileSafely.HasTile || !Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType] || Main.tileFrameImportant[tileSafely.TileType]) {
                    continue;
                }
                Tile tileSafely2 = Framing.GetTileSafely(i, j - 1);
                if (tileSafely2.HasTile && Main.tileSolid[tileSafely2.TileType] && !Main.tileSolidTop[tileSafely2.TileType]) {
                    continue;
                }
                int num5 = WorldGen.KillTile_GetTileDustAmount(fail: true, tileSafely, i, j);
                for (int k = 0; k < num5; k++) {
                    Dust obj = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                    obj.velocity.Y -= 3f + num4 * 1.5f;
                    obj.velocity.Y *= Main.rand.NextFloat();
                    obj.velocity.Y *= 0.75f;
                    obj.scale += num4 * 0.03f;
                }
                if (num4 >= 2) {
                    for (int m = 0; m < num5 - 1; m++) {
                        Dust obj2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                        obj2.velocity.Y -= 1f + num4;
                        obj2.velocity.Y *= Main.rand.NextFloat();
                        obj2.velocity.Y *= 0.75f;
                    }
                }
                if (num4 >= 2) {
                    for (int m = 0; m < num5 - 1; m++) {
                        Dust obj2 = Main.dust[WorldGen.KillTile_MakeTileDust(i, j, tileSafely)];
                        obj2.velocity.Y -= 1f + num4;
                        obj2.velocity.Y *= Main.rand.NextFloat();
                        obj2.velocity.Y *= 0.75f;
                    }
                }
                if (num5 <= 0 || Main.rand.Next(3) == 0) {
                    continue;
                }
            }
        }
    }
}
