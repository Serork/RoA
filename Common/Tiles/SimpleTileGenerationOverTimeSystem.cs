using RoA.Core.Utility;

using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria;

namespace RoA.Common.Tiles;

sealed class SimpleTileGenerationOverTimeSystem : ModSystem {
    public readonly struct TileGenerationData(ModTile instance, int[] anchorValidTiles, byte index, byte styleX, bool onSurface, bool inDungeon, byte amount, ushort extraChance, int[] anchorValidWalls, byte xSize, byte ySize) {
        public readonly ModTile Instance = instance;
        public readonly int[] AnchorValidTiles = anchorValidTiles;
        public readonly int[] AnchorValidWalls = anchorValidWalls;
        public readonly byte Index = index;
        public readonly byte StyleX = styleX;
        public readonly bool OnSurface = onSurface;
        public readonly bool InDungeon = inDungeon;
        public readonly byte Amount = amount;
        public readonly ushort ExtraChance = extraChance;
        public readonly byte XSize = xSize;
        public readonly byte YSize = ySize;

        public readonly bool Is1x1 => XSize == 1 && YSize == 1;
        public readonly bool Is2x2 => XSize == 2 && YSize == 2;
        public readonly bool Is2x3 => XSize == 2 && YSize == 3;
    }

    private static byte _nextId;
    private static readonly Dictionary<byte, TileGenerationData> _tileGenerationsInfo = [];
    private static readonly Dictionary<byte, bool> _generatedTiles = [];
    private static readonly Dictionary<byte, byte> _tileAmountToGenerate = [];

    public static TileGenerationData GetInfo<T>(T instance) where T : ModTile => _tileGenerationsInfo.Values.Single(tulipData => tulipData.Instance == instance);
    public static bool Generated<T>(T instance) where T : ModTile => _generatedTiles[GetInfo(instance).Index];
    public static void ResetState<T>(T instance) where T : ModTile {
        byte index = GetInfo(instance).Index;
        _generatedTiles[index] = false;
        _tileAmountToGenerate[index]++;
    }

    public static void Register<T>(T instance, int[] anchorValidTiles, byte styleX, bool onSurface, bool inDungeon, byte amount, ushort extraChance, int[] anchorValidWalls, byte xSize, byte ySize) where T : ModTile {
        _tileGenerationsInfo.Add(_nextId, new TileGenerationData(instance, anchorValidTiles, _nextId, styleX, onSurface && !inDungeon, inDungeon, amount, extraChance, anchorValidWalls, xSize, ySize));
        _generatedTiles.Add(_nextId, false);
        _tileAmountToGenerate.Add(_nextId, amount);
        _nextId++;
    }

    public override void ClearWorld() => ResetFlags();

    private static void ResetFlags() {
        foreach (KeyValuePair<byte, TileGenerationData> keyValuePair in _tileGenerationsInfo) {
            _generatedTiles[keyValuePair.Key] = false;
            _tileAmountToGenerate[keyValuePair.Key] = keyValuePair.Value.Amount;
        }
    }

    public override void SaveWorldData(TagCompound tag) {
        foreach (KeyValuePair<byte, TileGenerationData> keyValuePair in _tileGenerationsInfo) {
            if (_generatedTiles[keyValuePair.Key]) {
                tag[keyValuePair.Value.Instance.FullName] = true;
            }
            tag[keyValuePair.Value.Instance.FullName + "amount"] = _tileAmountToGenerate[keyValuePair.Key];
        }
    }

    public override void LoadWorldData(TagCompound tag) {
        foreach (KeyValuePair<byte, TileGenerationData> keyValuePair in _tileGenerationsInfo) {
            _generatedTiles[keyValuePair.Key] = tag.ContainsKey(keyValuePair.Value.Instance.FullName);
            _tileAmountToGenerate[keyValuePair.Key] = tag.GetByte(keyValuePair.Value.Instance.FullName + "amount");
        }
    }

    public override void NetSend(BinaryWriter writer) {
        BitsByte flags = new();
        int nextId = 0;
        foreach (KeyValuePair<byte, TileGenerationData> keyValuePair in _tileGenerationsInfo) {
            flags[nextId++] = _generatedTiles[keyValuePair.Key];
        }
        writer.Write(flags);
        foreach (KeyValuePair<byte, TileGenerationData> keyValuePair in _tileGenerationsInfo) {
            writer.Write(_tileAmountToGenerate[keyValuePair.Key]);
        }
    }

    public override void NetReceive(BinaryReader reader) {
        BitsByte flags = reader.ReadByte();
        int nextId = 0;
        foreach (KeyValuePair<byte, TileGenerationData> keyValuePair in _tileGenerationsInfo) {
            _generatedTiles[keyValuePair.Key] = flags[nextId++];
        }
        foreach (KeyValuePair<byte, TileGenerationData> keyValuePair in _tileGenerationsInfo) {
            _tileAmountToGenerate[keyValuePair.Key] = reader.ReadByte();
        }
    }

    public override void PostUpdateWorld() {
        double worldUpdateRate = WorldGen.GetWorldUpdateRate();
        if (worldUpdateRate == 0.0) {
            return;
        }

        UnifiedRandom genRand = WorldGen.genRand;
        foreach (KeyValuePair<byte, TileGenerationData> keyValuePair in _tileGenerationsInfo) {
            if (_generatedTiles[keyValuePair.Key]) {
                continue;
            }

            if (_tileAmountToGenerate[keyValuePair.Key] > 0) {
                bool onSurface = keyValuePair.Value.OnSurface;
                int i = genRand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance);
                int j = !onSurface ? genRand.Next((int)Main.worldSurface - 1, (int)Main.maxTilesY - 100 + 1) : genRand.Next(WorldGenHelper.SafeFloatingIslandY, (int)Main.worldSurface - 1);

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
                    _tileAmountToGenerate[keyValuePair.Key]--;
                }
            }
            else {
                _generatedTiles[keyValuePair.Key] = true;
            }
        }
    }

    private static bool TryToPlace(int i, int j, TileGenerationData tileGenerationData) {
        if (!Main.rand.NextBool(30 + tileGenerationData.ExtraChance)) {
            return false;
        }

        UnifiedRandom genRand = WorldGen.genRand;
        int num2 = genRand.Next(Math.Max(WorldGen.beachDistance, i - 10), Math.Min(Main.maxTilesX - WorldGen.beachDistance, i + 10));
        int num3 = tileGenerationData.OnSurface ? WorldGenHelper.GetFirstTileY(num2) : j;
        num3 -= 1;
        ushort tileType = tileGenerationData.Instance.Type;
        if (HasValidGroundOnSpot(num2, num3, tileGenerationData) && NoNearbySame(num2, num3, tileGenerationData) &&
            (tileGenerationData.Is2x3 ? WorldGenHelper.Place2x3(num2, num3, tileType, style: tileGenerationData.StyleX) : tileGenerationData.Is2x2 ? WorldGenHelper.Place2x2(num2, num3, tileType, style: tileGenerationData.StyleX) : WorldGen.PlaceTile(num2, num3, tileType, mute: true, style: tileGenerationData.StyleX))) {
            if ((tileGenerationData.Is1x1 && WorldGenHelper.GetTileSafely(num2, num3).ActiveTile(tileType)) || !tileGenerationData.Is1x1) {
                Main.LocalPlayer.position = new Microsoft.Xna.Framework.Vector2(num2, num3).ToWorldCoordinates();

                if (Main.netMode == NetmodeID.Server && Main.tile[num2, num3] != null && Main.tile[num2, num3].HasTile)
                    NetMessage.SendTileSquare(-1, num2, num3);

                return true;
            }
        }

        return false;
    }

    private static bool HasValidGroundOnSpot(int x, int y, TileGenerationData tileGenerationData) {
        bool checkTile(int i, int j) {
            if (!WorldGen.InWorld(i, j, 2))
                return false;

            Tile tile = Main.tile[i, j];
            if (tile.AnyLiquid())
                return false;

            tile = Main.tile[i, j + 1];
            if (!tile.HasTile)
                return false;

            ushort type = tile.TileType;
            ushort wallType = tile.WallType;
            if (type < 0 || type >= TileID.Count)
                return false;

            int count = tileGenerationData.AnchorValidTiles.Length;
            foreach (int anchorValidTileType in tileGenerationData.AnchorValidTiles) {
                if (type != anchorValidTileType) {
                    count--;
                    continue;
                }
                else {
                    break;
                }
            }
            if (count <= 0) {
                return false;
            }
            if (tileGenerationData.InDungeon) {
                if (!Main.wallDungeon[wallType]) {
                    return false;
                }
            }
            if (tileGenerationData.AnchorValidWalls != null) {
                int count2 = tileGenerationData.AnchorValidWalls.Length;
                foreach (int anchorValidWallType in tileGenerationData.AnchorValidWalls) {
                    if (wallType != anchorValidWallType) {
                        count2--;
                        continue;
                    }
                    else {
                        break;
                    }
                }
                if (count2 <= 0) {
                    return false;
                }
            }

            return true;
        }

        if (tileGenerationData.XSize > 1) {
            for (int k = 0; k < tileGenerationData.XSize; k++) {
                if (!checkTile(x + k, y)) {
                    return false;
                }
            }
        }
        else {
            if (!checkTile(x, y)) {
                return false;
            }
        }

        return WorldGen.SolidTileAllowBottomSlope(x, y + 1);
    }

    private static bool NoNearbySame(int i, int j, TileGenerationData tulipTileData) {
        int num = Utils.Clamp(i - 120, 10, Main.maxTilesX - 1 - 10);
        int num2 = Utils.Clamp(i + 120, 10, Main.maxTilesX - 1 - 10);
        int num3 = Utils.Clamp(j - 120, 10, Main.maxTilesY - 1 - 10);
        int num4 = Utils.Clamp(j + 120, 10, Main.maxTilesY - 1 - 10);
        for (int k = num; k <= num2; k++) {
            for (int l = num3; l <= num4; l++) {
                Tile tile = Main.tile[k, l];
                if (tile.HasTile && tile.TileType == tulipTileData.Instance.Type) {
                    return false;
                }
            }
        }

        return true;
    }
}
