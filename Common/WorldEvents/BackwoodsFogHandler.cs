using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Tiles.Ambient;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.WorldEvents;

sealed class BackwoodsFogHandler : ModSystem {
    public static bool IsFogActive { get; set; } = false;

    public override void PostUpdatePlayers() {
        if (Main.gameMenu && Main.netMode != NetmodeID.Server) {
            return;
        }

        if (!BackwoodsBiome.IsActiveForFogEffect || !IsFogActive) {
            return;
        }

        Rectangle tileWorkSpace = GetTileWorkSpace();
        int num = tileWorkSpace.X + tileWorkSpace.Width;
        int num2 = tileWorkSpace.Y + tileWorkSpace.Height;
        for (int i = tileWorkSpace.X; i < num; i++) {
            for (int j = tileWorkSpace.Y; j < num2; j++) {
                TrySpawnFog(i, j);
            }
        }
    }

    private Rectangle GetTileWorkSpace() {
        Point point = Main.LocalPlayer.Center.ToTileCoordinates();
        int num = 120;
        int num2 = 45;
        return new Rectangle(point.X - num / 2, point.Y - num2 / 2, num, num2);
    }

    private void TrySpawnFog(int x, int y) {
        Tile tile = WorldGenHelper.GetTileSafely(x, y);
        //if (y >= Main.worldSurface) {
        //    return;
        //}
        if (!tile.HasTile || tile.Slope > 0 || tile.IsHalfBlock || !Main.tileSolid[tile.TileType]) {
            return;
        }
        if (TileID.Sets.Platforms[tile.TileType]) {
            return;
        }
        tile = WorldGenHelper.GetTileSafely(x, y + 1);
        if (!tile.AnyWall() && !WorldGenHelper.GetTileSafely(x, y).AnyWall()) {
            return;
        }
        tile = WorldGenHelper.GetTileSafely(x, y - 1);
        int type = ModContent.TileType<OvergrownAltar>();
        if (!WorldGen.SolidTile(tile) && tile.TileType != type && WorldGenHelper.GetTileSafely(x + 1, y - 1).TileType != type && WorldGenHelper.GetTileSafely(x - 1, y - 1).TileType != type && Main.rand.NextBool(20)) {
            SpawnFloorCloud(x, y);
            if (Main.rand.NextBool(3)) {
                SpawnFloorCloud(x, y - 1);
            }
        }
    }

    private void SpawnFloorCloud(int x, int y) {
        Vector2 position = new Point(x, y - 1).ToWorldCoordinates();
        float num = 16f * Main.rand.NextFloat();
        position.Y -= num;
        float num2 = 0.4f;
        float scale = 0.8f + Main.rand.NextFloat() * 0.2f;
        int dust = Dust.NewDust(position, 5, 5, ModContent.DustType<Fog>(), Scale: scale);
        Main.dust[dust].velocity.X = num2 * Main.WindForVisuals;
    }
}
