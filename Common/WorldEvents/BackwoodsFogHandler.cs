using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Tiles.Ambient;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common.WorldEvents;

sealed class BackwoodsFogHandler : ModSystem {
    private sealed class ActivateFog : ModCommand {
        public override CommandType Type => CommandType.World;
        public override string Command => "togglebackwoodsfog";
        public override string Usage => "/togglebackwoodsfog";

        public override void Action(CommandCaller caller, string input, string[] args) => ToggleBackwoodsFog(false);
    }

    public static bool IsFogActive { get; private set; } = false;

    public override void OnWorldLoad() => Reset();
    public override void OnWorldUnload() => Reset();

    public override void SaveWorldData(TagCompound tag) {
        tag[nameof(IsFogActive)] = IsFogActive;
    }

    public override void LoadWorldData(TagCompound tag) {
        IsFogActive = tag.GetBool(nameof(IsFogActive));
    }

    private static void Reset() {
        IsFogActive = false;
    }

    private static void ToggleBackwoodsFog(bool naturally = true) {
        if (!IsFogActive) {
            if ((naturally && Main.rand.NextChance(0.33)) || !naturally) {
                string message = Language.GetText("Mods.RoA.World.BackwoodsFog").ToString();
                Helper.NewMessage($"{message}...", Helper.EventMessageColor);
                IsFogActive = true;
            }
        }
        else {
            IsFogActive = false;
        }
        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendData(MessageID.WorldData);
        }
    }

    public override void PostUpdateNPCs() {
        if (Main.dayTime) {
            if (Main.time < 1 || (Main.IsFastForwardingTime() && Main.time < 61)) {
                ToggleBackwoodsFog();
            }
        }
    }

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
