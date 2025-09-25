using RoA.Content.Tiles.Miscellaneous;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace RoA.Common.Tiles;

[Autoload(false)]
sealed class SimpleTileGenerationOverTimeSystem : ModSystem {
    public readonly struct TileGenerationData(SimpleTileBaseToGenerateOverTime instance, byte index, byte amount) {
        public readonly SimpleTileBaseToGenerateOverTime Instance = instance;
        public readonly byte Index = index;
        public readonly byte Amount = amount;

        public readonly byte XSize => Instance.XSize;
        public readonly byte YSize => Instance.YSize;
        public readonly bool Is1x1 => XSize == 1 && YSize == 1;
        public readonly bool Is2x2 => XSize == 2 && YSize == 2;
        public readonly bool Is2x3 => XSize == 2 && YSize == 3;
    }

    private static byte _nextId;
    private static Dictionary<byte, TileGenerationData> _tileGenerationsInfo = [];
    private static Dictionary<byte, bool> _generatedTiles = [];
    private static Dictionary<byte, byte> _tileAmountToGenerate = [];

    public override void Unload() {
        _tileGenerationsInfo.Clear();
        _tileGenerationsInfo = null;
        _generatedTiles.Clear();
        _generatedTiles = null;
        _tileAmountToGenerate.Clear();
        _tileAmountToGenerate = null;
    }

    public static TileGenerationData GetInfo<T>(T instance) where T : SimpleTileBaseToGenerateOverTime => _tileGenerationsInfo.Values.Single(tulipData => tulipData.Instance == instance);
    public static bool Generated<T>(T instance) where T : SimpleTileBaseToGenerateOverTime => _generatedTiles[GetInfo(instance).Index];
    public static void ResetState<T>(T instance) where T : SimpleTileBaseToGenerateOverTime {
        byte index = GetInfo(instance).Index;
        _generatedTiles[index] = false;
        _tileAmountToGenerate[index]++;
    }

    public static void Register<T>(T instance) where T : SimpleTileBaseToGenerateOverTime {
        _tileGenerationsInfo.Add(_nextId, new TileGenerationData(instance, _nextId, instance.Amount));
        _generatedTiles.Add(_nextId, false);
        _tileAmountToGenerate.Add(_nextId, instance.Amount);
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
                var instance = keyValuePair.Value.Instance;
                bool onSurface = instance.OnSurface && !instance.InUnderground;

                int i = genRand.Next(10, Main.maxTilesX - 10);
                int checkX = (int)(Main.maxTilesX * 0.08f);
                while (i > Main.spawnTileX - checkX && i < Main.spawnTileX + checkX) {
                    i = genRand.Next(10, Main.maxTilesX - 10);
                }

                // stinks
                if (instance.Type == ModContent.TileType<ExoticTulip>()) {
                    if (genRand.NextBool()) {
                        i = genRand.Next(275, 1000);
                    }
                    else {
                        i = genRand.Next(Main.maxTilesX - 1000, Main.maxTilesX - 275);
                    }
                }

                int j = !onSurface ? genRand.Next((int)Main.worldSurface - 1, (int)Main.maxTilesY - 100 + 1) : genRand.Next(WorldGenHelper.SafeFloatingIslandY, (int)Main.worldSurface - 1);
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
        var instance = tileGenerationData.Instance;
        UnifiedRandom genRand = WorldGen.genRand;
        if (!genRand.NextBool(30 + instance.ExtraChance)) {
            return false;
        }

        if (Helper.OnScreenWorld(i, j)) {
            return false;
        }

        i = genRand.Next(Math.Max(10, i - 10), Math.Min(Main.maxTilesX - 10, i + 10));
        ushort tileType = instance.Type;
        bool onSurface = instance.OnSurface && !instance.InUnderground;
        j = onSurface ? WorldGenHelper.GetFirstTileY(i) : j;
        j -= 1;
        byte styleX = instance.StyleX;
        if (!Main.tileAlch[Main.tile[i, j].TileType] && HasValidGroundOnSpot(i, j, tileGenerationData) && NoNearbySame(i, j, tileGenerationData) &&
            (tileGenerationData.Is2x3 ? WorldGenHelper.Place2x3(i, j, tileType, style: styleX, countCut: false) :
             tileGenerationData.Is2x2 ? WorldGenHelper.Place2x2(i, j, tileType, style: styleX, countCut: false) : WorldGen.PlaceTile(i, j, tileType, mute: true, forced: true, style: styleX))) {
            if ((tileGenerationData.Is1x1 && WorldGenHelper.GetTileSafely(i, j).ActiveTile(tileType)) || !tileGenerationData.Is1x1) {
                if (tileGenerationData.Is1x1) {
                    Main.tile[i, j].CopyPaintAndCoating(Main.tile[i, j + 1]);
                }

                //Helper.NewMessage(new Vector2(i, j).ToString(), DrawColor.White);

                //Main.LocalPlayer.position = new Vector2(i, j).ToWorldCoordinates();

                if (Main.netMode == NetmodeID.Server && Main.tile[i, j].HasTile)
                    NetMessage.SendTileSquare(-1, i, j);

                return true;
            }
        }

        return false;
    }

    private static bool HasValidGroundOnSpot(int x, int y, TileGenerationData tileGenerationData) {
        bool checkTile(int i, int j) {
            if (!WorldGen.InWorld(i, j, 2))
                return false;

            Tile main = Main.tile[i, j];
            if (main.AnyLiquid())
                return false;

            Tile tile = Main.tile[i, j + 1];
            if (!tile.HasTile)
                return false;

            ushort type = tile.TileType;
            ushort wallType = main.WallType;
            if (type < 0 || type >= TileID.Count)
                return false;

            var instance = tileGenerationData.Instance;
            int[] anchorValidTiles = instance.AnchorValidTiles;
            int count = anchorValidTiles.Length;
            foreach (int anchorValidTileType in anchorValidTiles) {
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
            Predicate<ushort> conditionForWallToBeValid = instance.ConditionForWallToBeValid;
            if (conditionForWallToBeValid != null) {
                if (!conditionForWallToBeValid(wallType)) {
                    return false;
                }
            }
            int[] anchorValidWalls = instance.AnchorValidWalls;
            if (anchorValidWalls != null) {
                int count2 = anchorValidWalls.Length;
                foreach (int anchorValidWallType in anchorValidWalls) {
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
