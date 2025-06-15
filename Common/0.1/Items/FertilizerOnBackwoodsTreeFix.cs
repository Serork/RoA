using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Trees;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class FertilizerOnBackwoodsTreeFix : GlobalProjectile {
    public override void AI(Projectile projectile) {
        if (projectile.type != 1019) {
            return;
        }

        bool flag3 = true;
        projectile.velocity *= 0.95f;
        if (projectile.ai[0] == 180f)
            projectile.Kill();

        if (projectile.ai[1] == 0f) {
            projectile.ai[1] = 1f;
            int num89 = 10 + projectile.type;
            int num90 = 30;

            num89 = 0;
            num90 = 40;

            for (int num91 = 0; num91 < num90; num91++) {
                Dust dust7 = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, num89, projectile.velocity.X, projectile.velocity.Y, 50)];
                if (flag3) {
                    dust7.noGravity = num91 % 3 != 0;
                    if (!dust7.noGravity) {
                        Dust dust2 = dust7;
                        dust2.scale *= 1.25f;
                        dust2 = dust7;
                        dust2.velocity /= 2f;
                        dust7.velocity.Y -= 2.2f;
                    }
                    else {
                        Dust dust2 = dust7;
                        dust2.scale *= 1.75f;
                        dust2 = dust7;
                        dust2.velocity += projectile.velocity * 0.65f;
                    }
                }
            }
        }

        bool flag4 = Main.myPlayer == projectile.owner;
        if (flag3)
            flag4 = Main.netMode != 1;

        if (flag4 && flag3) {
            int num92 = (int)(projectile.position.X / 16f) - 1;
            int num93 = (int)((projectile.position.X + projectile.width) / 16f) + 2;
            int num94 = (int)(projectile.position.Y / 16f) - 1;
            int num95 = (int)((projectile.position.Y + projectile.height) / 16f) + 2;
            if (num92 < 0)
                num92 = 0;

            if (num93 > Main.maxTilesX)
                num93 = Main.maxTilesX;

            if (num94 < 0)
                num94 = 0;

            if (num95 > Main.maxTilesY)
                num95 = Main.maxTilesY;

            Vector2 vector15 = default;
            for (int num96 = num92; num96 < num93; num96++) {
                for (int num97 = num94; num97 < num95; num97++) {
                    vector15.X = num96 * 16;
                    vector15.Y = num97 * 16;
                    if (!(projectile.position.X + projectile.width > vector15.X) || !(projectile.position.X < vector15.X + 16f) || !(projectile.position.Y + projectile.height > vector15.Y) || !(projectile.position.Y < vector15.Y + 16f) || !Main.tile[num96, num97].HasTile)
                        continue;

                    if (!flag3)
                        continue;

                    Tile tile = Main.tile[num96, num97];
                    if ((tile.TileType == ModContent.TileType<PrimordialSapling>() || tile.TileType == TileID.Saplings) && TileID.Sets.CommonSapling[tile.TileType]) {
                        if (Main.remixWorld && num97 >= (int)Main.worldSurface - 1 && num97 < Main.maxTilesY - 20)
                            AttemptToGrowTreeFromSapling(num96, num97, underground: false);

                        AttemptToGrowTreeFromSapling(num96, num97, num97 > (int)Main.worldSurface - 1);
                    }
                }
            }
        }

        if (flag3 && projectile.velocity.Length() < 0.5f)
            projectile.Kill();
    }

    private static bool AttemptToGrowTreeFromSapling(int x, int y, bool underground) {
        if (Main.netMode == 1)
            return false;

        if (!WorldGen.InWorld(x, y, 2))
            return false;

        Tile tile = Main.tile[x, y];
        if (tile == null || !tile.HasTile)
            return false;

        bool flag = false;
        int num = 0;
        int num2 = -1;
        if (tile.TileType == ModContent.TileType<PrimordialSapling>()) {
            PrimordialSapling.GrowTree(x, y);
        }
        if (tile.TileType == TileID.Saplings) {
            WorldGen.GrowTree(x, y);
        }

        return false;
    }
}
