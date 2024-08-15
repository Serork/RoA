using RoA.Core.Utility;
using System.Collections.Generic;
using System.IO;
using System;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria;
using System.Linq;

namespace RoA.Content.World.Generations;

sealed class TulipGenerationSystem : ModSystem {
    public readonly struct TulipTileData(ModTile instance, int[] anchorValidTiles, byte index, byte styleX, bool onSurface, byte amount, int[] anchorValidWalls) {
        public readonly ModTile Instance = instance;
        public readonly int[] AnchorValidTiles = anchorValidTiles;
        public readonly int[] AnchorValidWalls = anchorValidWalls;
        public readonly byte Index = index;
        public readonly byte StyleX = styleX;
        public readonly bool OnSurface = onSurface;
        public readonly byte Amount = amount;
    }

    private static byte _nextId;

    private static Dictionary<byte, TulipTileData> _tulipGenerationInfo;
    private static Dictionary<byte, bool> _generatedTulips;
    private static Dictionary<byte, byte> _tulipsAmountToGenerate;

    public override void Load() {
        _tulipGenerationInfo = [];
        _generatedTulips = [];
        _tulipsAmountToGenerate = [];
    }

    public override void Unload() {
        _tulipGenerationInfo = null;
        _generatedTulips = null;
        _tulipsAmountToGenerate = null;
    }

    public static TulipTileData GetInfo<T>(T instance) where T : ModTile => _tulipGenerationInfo.Values.Single(tulipData => tulipData.Instance == instance);
    public static bool Generated<T>(T instance) where T : ModTile => _generatedTulips[GetInfo(instance).Index];
    public static void ResetState<T>(T instance) where T : ModTile {
        TulipTileData info = GetInfo(instance);
        byte index = info.Index;
        _generatedTulips[index] = false;
        _tulipsAmountToGenerate[index]++;
    }

    public static void Register<T>(T instance, int[] anchorValidTiles, byte styleX, bool onSurface, byte amount, int[] anchorValidWalls) where T : ModTile {
        if (_tulipGenerationInfo.ContainsKey(_nextId)) {
            return;
        }
        _tulipGenerationInfo.Add(_nextId, new TulipTileData(instance, anchorValidTiles, _nextId, styleX, onSurface, amount, anchorValidWalls));
        _generatedTulips.Add(_nextId, false);
        _tulipsAmountToGenerate.Add(_nextId, amount);
        _nextId++;
    }

    public override void OnWorldLoad() => ResetFlags();
    public override void OnWorldUnload() => ResetFlags();

    private static void ResetFlags() {
        foreach (KeyValuePair<byte, TulipTileData> keyValuePair in _tulipGenerationInfo) {
            _generatedTulips[keyValuePair.Key] = false;
        }
    }

    public override void SaveWorldData(TagCompound tag) {
        foreach (KeyValuePair<byte, TulipTileData> keyValuePair in _tulipGenerationInfo) {
            if (_generatedTulips[keyValuePair.Key]) {
                tag[keyValuePair.Value.Instance.Name] = true;
            }
        }
    }

    public override void LoadWorldData(TagCompound tag) {
        foreach (KeyValuePair<byte, TulipTileData> keyValuePair in _tulipGenerationInfo) {
            _generatedTulips[keyValuePair.Key] = tag.ContainsKey(keyValuePair.Value.Instance.Name);
        }
    }

    public override void NetSend(BinaryWriter writer) {
        BitsByte flags = new();
        int nextId = 0;
        foreach (KeyValuePair<byte, TulipTileData> keyValuePair in _tulipGenerationInfo) {
            flags[nextId++] = _generatedTulips[keyValuePair.Key];
        }
        writer.Write(flags);
    }

    public override void NetReceive(BinaryReader reader) {
        BitsByte flags = reader.ReadByte();
        int nextId = 0;
        foreach (KeyValuePair<byte, TulipTileData> keyValuePair in _tulipGenerationInfo) {
            _generatedTulips[keyValuePair.Key] = flags[nextId++];
        }
    }

    public override void PostUpdateWorld() {
        double worldUpdateRate = WorldGen.GetWorldUpdateRate();
        if (worldUpdateRate == 0.0) {
            return;
        }

        UnifiedRandom genRand = WorldGen.genRand;
        for (int k = 0; k < _nextId; k++) {
            foreach (KeyValuePair<byte, TulipTileData> keyValuePair in _tulipGenerationInfo) {
                if (_generatedTulips[keyValuePair.Key]) {
                    continue;
                }

                if (_tulipsAmountToGenerate[keyValuePair.Key] > 0) {
                    bool onSurface = keyValuePair.Value.OnSurface;
                    int i = genRand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance);
                    int j = onSurface ? genRand.Next(WorldGenHelper.SafeFloatingIslandY, (int)Main.worldSurface - 1) : genRand.Next((int)Main.worldSurface - 1, (int)Main.maxTilesY / 2 + Main.maxTilesY / 3 + 1);

                    int num = i - 1;
                    int num2 = i + 2;
                    int num3 = j - 1;
                    int num4 = j + 2;
                    if (num < 10)
                        num = 10;

                    if (num2 > Main.maxTilesX - 10)
                        num2 = Main.maxTilesX - 10;

                    if (num3 < 10)
                        num3 = 10;

                    if (num4 > Main.maxTilesY - 10)
                        num4 = Main.maxTilesY - 10;

                    Tile tile = WorldGenHelper.GetTileSafely(i, j);
                    if (!tile.HasTile) {
                        continue;
                    }

                    if (TryToPlace(i, j, keyValuePair.Value)) {
                        _tulipsAmountToGenerate[keyValuePair.Key]--;
                    }
                }
                else {
                    _generatedTulips[keyValuePair.Key] = true;
                }
            }
        }
    }

    private static bool TryToPlace(int i, int j, TulipTileData tulipTileData) {
        if (!Main.rand.NextBool(30)) {
            return false;
        }

        UnifiedRandom genRand = WorldGen.genRand;
        int num2 = genRand.Next(Math.Max(WorldGen.beachDistance, i - 10), Math.Min(Main.maxTilesX - WorldGen.beachDistance, i + 10));
        int num3 = tulipTileData.OnSurface ? WorldGenHelper.GetFirstTileY(num2) - 1 : genRand.Next(Math.Max(10, j - 10), Math.Min(Main.maxTilesY - 10, j + 10));
        if (HasValidGroundOnSpot(num2, num3, tulipTileData) && NoNearbyTulips(num2, num3, tulipTileData) &&
            WorldGen.PlaceTile(num2, num3, tulipTileData.Instance.Type, mute: true, style: tulipTileData.StyleX)) {
            Main.LocalPlayer.position = new Microsoft.Xna.Framework.Vector2(num2, num3).ToWorldCoordinates();

            if (Main.netMode == NetmodeID.Server && Main.tile[num2, num3] != null && Main.tile[num2, num3].HasTile)
                NetMessage.SendTileSquare(-1, num2, num3);

            return true;
        }

        return false;
    }

    private static bool HasValidGroundOnSpot(int x, int y, TulipTileData tulipTileData) {
        if (!WorldGen.InWorld(x, y, 2))
            return false;

        Tile tile = Main.tile[x, y - 1];
        if (tile.HasTile)
            return false;

        tile = Main.tile[x, y + 1];
        if (!tile.HasTile)
            return false;

        ushort type = tile.TileType;
        ushort wallType = tile.WallType;
        if (type < 0 || type >= TileID.Count)
            return false;

        int count = tulipTileData.AnchorValidTiles.Length;
        foreach (int anchorValidTileType in tulipTileData.AnchorValidTiles) {
            if (type != anchorValidTileType) {
                count--;
                continue;
            }
        }
        if (count <= 0) {
            return false;
        }
        if (tulipTileData.AnchorValidWalls != null) {
            int count2 = tulipTileData.AnchorValidWalls.Length;
            foreach (int anchorValidWallType in tulipTileData.AnchorValidWalls) {
                if (wallType != anchorValidWallType) {
                    count2--;
                    continue;
                }
            }
            if (count2 <= 0) {
                return false;
            }
        }

        return WorldGen.SolidTileAllowBottomSlope(x, y + 1);
    }

    private static bool NoNearbyTulips(int i, int j, TulipTileData tulipTileData) {
        int num = Utils.Clamp(i - 120, 10, Main.maxTilesX - 1 - 10);
        int num2 = Utils.Clamp(i + 120, 10, Main.maxTilesX - 1 - 10);
        int num3 = Utils.Clamp(j - 120, 10, Main.maxTilesY - 1 - 10);
        int num4 = Utils.Clamp(j + 120, 10, Main.maxTilesY - 1 - 10);
        for (int k = num; k <= num2; k++) {
            for (int l = num3; l <= num4; l++) {
                Tile tile = Main.tile[k, l];
                if (tile.HasTile && tile.TileType == tulipTileData.Instance.Type)
                    return false;
            }
        }

        return true;
    }
}
