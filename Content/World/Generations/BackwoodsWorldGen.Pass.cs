using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Furniture;
using RoA.Content.Tiles.Plants;
using RoA.Content.Tiles.Platforms;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.World.Generations;

sealed class BackwoodsBiomePass(string name, double loadWeight) : GenPass(name, loadWeight) {
    public static readonly ushort[] SandInvalidTileTypesToKill = { TileID.HardenedSand, TileID.Sandstone };
    public static readonly ushort[] SandInvalidWallTypesToKill = { 187, 220, 222, 221, 275, 308, 310, 309, 216, 217, 219, 218, 304, 305, 307, 306, 216, 187, 304, 275 };
    public static readonly ushort[] MidInvalidTileTypesToKill = { TileID.HardenedSand, TileID.Sandstone, TileID.Ebonstone, TileID.Crimstone, TileID.Marble, TileID.Granite };
    public static readonly ushort[] MidInvalidWallTypesToKill = { WallID.EbonstoneEcho, WallID.EbonstoneUnsafe, WallID.CrimstoneEcho, WallID.CrimstoneUnsafe, WallID.GraniteUnsafe, WallID.MarbleUnsafe, WallID.Marble };
    public static readonly ushort[] MidReplaceWallTypes = { WallID.MudUnsafe, WallID.MudWallEcho, WallID.EbonstoneEcho, WallID.EbonstoneUnsafe, WallID.CrimstoneEcho, WallID.CrimstoneUnsafe };
    public static readonly ushort[] SkipBiomeInvalidTileTypeToKill = { TileID.HardenedSand, TileID.Sandstone, TileID.Ebonstone, TileID.Crimstone };
    public static readonly ushort[] SkipBiomeInvalidWallTypeToKill = { WallID.HardenedSand, WallID.Sandstone, WallID.GraniteUnsafe, WallID.MarbleUnsafe, WallID.Marble, WallID.EbonstoneEcho, WallID.EbonstoneUnsafe, WallID.CrimstoneEcho, WallID.CrimstoneUnsafe };
    public static readonly ushort[] SkipBiomeInvalidWallTypeToKill2 = { WallID.HardenedSand, WallID.Sandstone, WallID.GraniteUnsafe, WallID.MarbleUnsafe, WallID.Marble };
    public static readonly ushort[] MidMustKillTileTypes = { TileID.Ebonstone, TileID.Crimstone };
    public static readonly ushort[] MidMustSkipWallTypes = { WallID.Marble, WallID.GraniteUnsafe, WallID.MarbleUnsafe, WallID.Marble };
    public static readonly ushort[] MidMustKillWallTypes = { WallID.EbonstoneEcho, WallID.EbonstoneUnsafe, WallID.CrimstoneEcho, WallID.CrimstoneUnsafe };
    public static readonly ushort[] SandTileTypes = { TileID.Sand, TileID.Crimsand, TileID.Ebonsand };

    private static ushort AltarPlaceholderTileType => TileID.ChlorophyteBrick;
    private static ushort AltarPlaceholderTileType2 => TileID.Adamantite;
    private static ushort CliffPlaceholderTileType => TileID.ChlorophyteBrick;

    private List<ushort> _backwoodsPlants;
    private List<Point> _biomeSurface, _altarTiles;
    private GenerationProgress _progress;
    private Point _positionToPlaceBiome;
    private int _biomeWidth = 100, _biomeHeight = 162;
    private ushort _dirtTileType, _grassTileType, _stoneTileType, _mossTileType, _mossGrowthTileType, _elderwoodTileType, _elderwoodTileType2, _leavesTileType;
    private ushort _dirtWallType, _grassWallType, _elderwoodWallType, _elderwoodWallType2, _leavesWallType;
    private ushort _fallenTreeTileType, _plantsTileType, _bushTileType, _elderWoodChestTileType, _altarTileType, _mintTileType, _vinesTileType;
    private int _lastCliffX;
    private bool _toLeft;

    private int CenterX {
        get => _positionToPlaceBiome.X;
        set {
            _positionToPlaceBiome.X = Math.Clamp(value, GenVars.beachBordersWidth, Main.maxTilesX - GenVars.beachBordersWidth);
        }
    }

    private int CenterY {
        get => _positionToPlaceBiome.Y;
        set {
            _positionToPlaceBiome.Y = Math.Max(value, (int)GenVars.worldSurface - 100);
        }
    }

    private int Top => CenterY - _biomeHeight;
    private int Bottom => CenterY + _biomeHeight;
    private int Left => CenterX - _biomeWidth;
    private int Right => CenterX + _biomeWidth;
    private Point TopLeft => new(Left, Top);
    private Point TopRight => new(Right, Top);
    private int EdgeX => _biomeWidth / 4;
    private int EdgeY => _biomeHeight / 4;

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
        _progress = progress;
        _progress.Message = Language.GetOrRegister("Mods.RoA.WorldGen.Backwoods").Value;

        Step0_Setup();
        Step1_FindPosition();
        Step2_ClearZone();
        Step3_GenerateBase();
        Step4_CleanUp();
        Step5_CleanUp();
        Step6_SpreadGrass();
        Step6_2_SpreadGrass();
        Step11_AddOre();
        Step7_AddStone();
        Step8_AddCaves();
        Step12_AddRoots();
        Step9_SpreadMoss();
        Step17_AddStatues();
        Step10_SpreadMossGrass();
        Step13_GrowBigTrees();
        Step14_ClearRockLayerWalls();

        GenVars.structures.AddProtectedStructure(new Rectangle(Left, Top, _biomeWidth * 2, _biomeHeight * 2), 10);
    }

    private void Step16_PlaceAltar() {
        int x = CenterX;
        int posX = x;

        Platform(posX + 6, WorldGenHelper.GetFirstTileY2(posX, true, true) + 1 - 3);

        Spike(posX - 6, WorldGenHelper.GetFirstTileY2(posX - 6, true, true) - 1, MathHelper.Pi + 1.3f + Main._rand.NextFloat(-0.15f, 0.15f), 4, 3);
        Spike(posX, WorldGenHelper.GetFirstTileY2(posX, true, true) - 2, MathHelper.Pi + 1.2f + Main._rand.NextFloat(-0.15f, 0.15f));
        Spike(posX + 14, WorldGenHelper.GetFirstTileY2(posX + 14, true, true) - 3, MathHelper.Pi + 0.5f + Main._rand.NextFloat(-0.15f, 0.15f), 6);
        Spike(posX + 21, WorldGenHelper.GetFirstTileY2(posX + 21, true, true) - 1, MathHelper.Pi + 0.9f, 4, 3);

        int index = 3;
        while (index-- > 0) {
            foreach (Point pos in _altarTiles) {
                int i = pos.X, j = pos.Y;
                if (WorldGenHelper.ActiveTile(i, j, AltarPlaceholderTileType) &&
                    WorldGenHelper.ActiveTile(i + 1, j, AltarPlaceholderTileType)) {
                    for (int k = 0; k < 3; k++) {
                        for (int i2 = -1; i2 < 1; i2++) {
                            for (int j3 = 1; j3 < 5; j3++) {
                                int j4 = j - j3;
                                WorldGenHelper.GetTileSafely(i + i2, j4).HasTile = false;
                                WorldGenHelper.GetTileSafely(i + i2 + 1, j4).HasTile = false;
                                WorldGenHelper.GetTileSafely(i + i2 + 2, j4).HasTile = false;
                            }
                        }
                    }
                    break;
                }
            }
            foreach (Point pos in _altarTiles) {
                int i = pos.X, j = pos.Y;
                if (WorldGenHelper.ActiveTile(i, j, AltarPlaceholderTileType) &&
                    WorldGenHelper.ActiveTile(i + 1, j, AltarPlaceholderTileType)) {
                    for (int k = 0; k < 3; k++) {
                        for (int i2 = -1; i2 < 1; i2++) {
                            for (int j3 = 1; j3 < 5; j3++) {
                                int j4 = j - j3;
                                WorldGenHelper.GetTileSafely(i + i2, j4).HasTile = false;
                                WorldGenHelper.GetTileSafely(i + i2 + 1, j4).HasTile = false;
                                WorldGenHelper.GetTileSafely(i + i2 + 2, j4).HasTile = false;
                            }
                        }
                    }
                }
            }
            bool placed = false;
            foreach (Point pos in _altarTiles) {
                int i = pos.X, j = pos.Y;
                if (WorldGenHelper.ActiveTile(i, j, AltarPlaceholderTileType) &&
                    WorldGenHelper.ActiveTile(i + 1, j, AltarPlaceholderTileType)) {
                    WorldGenHelper.Place3x2(i + 1, j - 1, _altarTileType, onPlaced: () => {
                        placed = true;
                    });
                }
                if (placed) {
                    break;
                }
            }
        }
        Point checkedPos = _altarTiles.First();
        int tileX = checkedPos.X, tileY = checkedPos.Y;
        for (int i = tileX - 40; i < tileX + 40; i++) {
            for (int j = tileY - 40; j < tileY + 40; j++) {
                Tile tile = Framing.GetTileSafely(i, j);
                if (WorldGenHelper.ActiveTile(i, j, AltarPlaceholderTileType)) {
                    WorldGenHelper.GetTileSafely(i, j).TileType = _grassTileType;
                }
                if (WorldGenHelper.ActiveTile(i, j, AltarPlaceholderTileType2)) {
                    WorldGenHelper.GetTileSafely(i, j).TileType = TileID.Dirt;
                }
                int x2 = i, y2 = j;
                if (Main.tile[x2, y2].ActiveTile(_elderwoodTileType)) {
                    if (!Main.tile[x2 - 1, y2].ActiveTile(_elderwoodTileType) && !Main.tile[x2 + 1, y2].ActiveTile(_elderwoodTileType) && !Main.tile[x2, y2 - 1].ActiveTile(_elderwoodTileType) && !Main.tile[x2, y2 + 1].ActiveTile(_elderwoodTileType) ||
                        !Main.tile[x2 - 1, y2 - 1].ActiveTile(_elderwoodTileType) && !Main.tile[x2 + 1, y2 - 1].ActiveTile(_elderwoodTileType) && !Main.tile[x2 + 1, y2 + 1].ActiveTile(_elderwoodTileType) && !Main.tile[x2 - 1, y2 + 1].ActiveTile(_elderwoodTileType)) {
                        WorldGen.KillTile(i, j);
                    }
                }
                ushort treeBranch = (ushort)ModContent.TileType<TreeBranch>();
                if (tile.ActiveTile(ModContent.TileType<TreeBranch>()) && !WorldGenHelper.GetTileSafely(i - 1, j).ActiveTile(TileID.Trees) && !WorldGenHelper.GetTileSafely(i + 1, j).ActiveTile(TileID.Trees)) {
                    WorldGen.KillTile(i, j);
                }
            }
        }
    }

    private void Platform(int x, int y) {
        //while (!Framing.GetTileSafely(x, y).HasTile || !SolidTile2(x, y)) {
        //    y++;
        //}
        for (int i = x - 15; i < x + 15; i++) {
            for (int j = y - 15; j < y + 5; j++) {
                Tile tile = Framing.GetTileSafely(i, j);
                if (tile.ActiveTile(TileID.Trees) || tile.ActiveTile(ModContent.TileType<TreeBranch>())) {
                    WorldGen.KillTile(i, j);
                }
                if (tile.AnyLiquid()) {
                    tile.LiquidAmount = 0;
                }
            }
        }
        for (int i = x - 2; i < x + 5; i++) {
            for (int j = y - 4; j < y + 2; j++) {
                Tile tile = Framing.GetTileSafely(i, j);
                if (tile.HasTile && (tile.TileType != _grassTileType || tile.TileType != TileID.Dirt || tile.TileType != _dirtTileType || tile.TileType != _stoneTileType || tile.TileType != _elderwoodTileType || tile.TileType != _leavesTileType)) {
                    WorldGen.KillTile(i, j);
                    WorldGenHelper.ReplaceWall(i, j, _leavesWallType);
                }
            }
            for (int j = y + 1; j < y + 4; j++) {
                if (j != y + 1 || i != x && i != x + 1 && i != x + 2) {
                    if (j != y + 3 || j == y + 3 && i > x - 2 && i < x + 4) {
                        for (int i2 = -3; i2 < 4; i2++) {
                            for (int j2 = -1; j2 < 4; j2++) {
                                if (!(i + i2 > x - 2 && i + i2 < x + 4) && _random.NextChance(0.5) && Main.tile[i + i2, j + j2].TileType != _grassTileType && Main.tile[i + i2, j + j2].TileType != _elderwoodTileType && Main.tile[i + i2, j + j2].HasTile) {
                                    WorldGenHelper.ReplaceWall(i + i2, j + j2, _leavesWallType);
                                    WorldGenHelper.ReplaceTile(i + i2, j + j2, _leavesTileType);
                                }
                            }
                        }
                    }
                }
            }
            for (int j = y + 1; j < y + 4; j++) {
                if (j != y + 1 || i != x && i != x + 1 && i != x + 2) {
                    if (j != y + 3 || j == y + 3 && i > x - 2 && i < x + 4) {
                        WorldGenHelper.ReplaceWall(i, j, _leavesWallType);
                        WorldGenHelper.ReplaceTile(i, j, _leavesTileType);
                    }
                }
                if (j > y + 1 && i > x - 1 && i < x + 3) {
                    WorldGen.KillTile(i, j - 2);
                    bool flag = j > y + 2;
                    WorldGenHelper.ReplaceTile(i, j, flag ? AltarPlaceholderTileType2 : AltarPlaceholderTileType);
                    _altarTiles.Add(new Point(i, j));
                    if (!flag) {
                        for (int j2 = 1; j2 < 4; j2++) {
                            if (WorldGenHelper.ActiveTile(i, j - j2)) {
                                WorldGen.KillTile(i, j - j2);
                            }
                        }
                    }
                }
            }
        }
    }

    private void Spike(int x, int y, float a, int distance = 8, int size = 4) {
        float x2 = x;
        float y2 = y;
        int startingSize = size;
        int endingSize = 1;
        float angle = a;
        for (int num3 = 0; num3 < distance; num3++) {
            float num4 = num3 / (float)distance;
            float num5 = MathHelper.Lerp(startingSize, endingSize, num4);
            float angle2 = angle;
            angle += (angle2 - (float)Math.PI / 2f) * 0.1f * (1f - num4);
            for (int i = 0; i < (int)num5; i++) {
                for (int j = 0; j < (int)num5; j++) {
                    int x3 = (int)x2 + i;
                    int y3 = (int)y2 + j;
                    WorldGenHelper.ReplaceTile(x3, y3, _elderwoodTileType);
                    if (Main.tile[x3 - 1, y3].TileType == _elderwoodTileType && Main.tile[x3 + 1, y3].TileType == _elderwoodTileType && Main.tile[x3, y3 - 1].TileType == _elderwoodTileType && Main.tile[x3, y3 + 1].TileType == _elderwoodTileType && Main.tile[x3 - 1, y3 - 1].TileType == _elderwoodTileType && Main.tile[x3 + 1, y3 - 1].TileType == _elderwoodTileType && Main.tile[x3 + 1, y3 + 1].TileType == _elderwoodTileType && Main.tile[x3 - 1, y3 + 1].TileType == _elderwoodTileType) {
                        WorldGenHelper.ReplaceWall(x3, y3, _elderwoodWallType);
                    }
                }
            }
            x2 += (float)Math.Cos(angle);
            y2 += (float)Math.Sin(angle);
        }
    }

    public void BackwoodsLootRooms(GenerationProgress progress, GameConfiguration configuration) {
        progress.Message = Language.GetOrRegister("Mods.RoA.WorldGen.Backwoods3").Value;

        int roomCount = 5 * WorldGenHelper.WorldSize;
        for (int i = 0; i < roomCount; i++) {
            progress.Set((float)(i + 1) / roomCount);

            GenerateLootRoom();
        }
    }

    private void WallBush(int i, int j) {
        int sizeX = _random.Next(8, 17);
        int sizeY = _random.Next(2, 5);
        int sizeY2 = sizeY;
        while (sizeY2 > 0) {
            double progress = sizeX * ((double)sizeY2 / sizeY);
            --sizeY2;
            int x1 = (int)(i - progress * 0.5);
            int x2 = (int)(i + progress * 0.5);
            int y1 = (int)(j - progress * 0.5);
            int y2 = (int)(j + progress * 0.5);
            for (int x = x1; x < x2; x++) {
                for (int y = y1; y < y2; y++) {
                    double min = Math.Abs((double)(x - i)) + Math.Abs((double)(y - j));
                    double max = (double)sizeX * 0.5 * (1.0 + _random.Next(-5, 10) * 0.025);
                    if (min < max && WorldGenHelper.GetTileSafely(x, y).WallType != _leavesWallType && (WorldGenHelper.GetTileSafely(x, y).WallType != _elderwoodWallType && (!WorldGen.SolidTile2(x, y) || WorldGenHelper.ActiveTile(x, y, _grassTileType)))) {
                        WorldGenHelper.ReplaceWall(x, y, _leavesWallType);
                    }
                }
            }
        }
    }

    private void Step14_ClearRockLayerWalls() {
        // adapted vanilla 
        int num1047 = 0;
        for (int num1048 = Left - 10; num1048 < Right + 10; num1048++) {
            for (int num1049 = CenterY - 10; num1049 < Bottom + 10; num1049++) {
                num1047 += _random.Next(-1, 2);
                if (num1047 < 0)
                    num1047 = 0;

                if (num1047 > 10)
                    num1047 = 10;

                if (!(num1049 < Main.worldSurface + 10.0 && !((double)num1049 > Main.worldSurface + (double)num1047))) {
                    if (!SkipBiomeInvalidWallTypeToKill.Contains(Main.tile[num1048, num1049].WallType) && !MidMustSkipWallTypes.Contains(Main.tile[num1048, num1049].WallType) && Main.tile[num1048, num1049].WallType != _grassWallType && Main.tile[num1048, num1049].WallType != _leavesWallType && Main.tile[num1048, num1049].WallType != _elderwoodWallType) {
                        Main.tile[num1048, num1049].WallType = WallID.None;
                    }
                }
            }
        }
    }

    private void Step13_GrowBigTrees() {
        int left = _toLeft ? _lastCliffX + 20 : Left;
        int i = _random.Next(left + 15, left + 30);
        GrowBigTree(i);
        int right = !_toLeft ? _lastCliffX - 20 : Right;
        i = _random.Next(right - 30, right - 15);
        GrowBigTree(i);
    }

    private void BigTreeBranch(int i, int j) {
        Point position = new(i, j);
        Shapes.Circle circle = new(_random.Next(4, 8));
        int size = _random.Next(4, 8);
        GenAction action = Actions.Chain(new Modifiers.Blotches(size, Math.Min(size - 4, _random.Next(4, 8)), 1f), 
                                         new Modifiers.SkipTiles([_elderwoodTileType]),
                                         new SetTileAndWall(_leavesTileType, _leavesWallType),
                                         new Actions.SetFrames(true));
        WorldUtils.Gen(position, circle, action);
        //WorldGen.TileRunner(i, j, _random.Next(sizeX + 6) + _random.Next(2, 5), _random.Next(sizeX + 6) + _random.Next(2, 5), _leavesTileType, true, noYChange: true, overRide: true);
    }

    private class BigTreeBranchRoot : GenShape {
        private double _angle;
        private double _startingSize;
        private double _endingSize;
        private double _distance;
        private ushort _leavesType;
        private ushort _leavesWall;
        private bool _leavesSpawned;

        public BigTreeBranchRoot(double angle, ushort leavesType, ushort leavesWall, double distance = 10.0, double startingSize = 4.0, double endingSize = 1.0) {
            _angle = angle;
            _leavesType = leavesType;
            _leavesWall = leavesWall;
            _distance = distance;
            _startingSize = startingSize;
            _endingSize = endingSize;
        }

        private bool DoRoot(Point origin, GenAction action, double angle, double distance, double startingSize) {
            double num = origin.X;
            double num2 = origin.Y;
            double max = distance * 0.85;
            for (double num3 = 0.0; num3 < max; num3 += 1.0) {
                double num4 = num3 / distance;
                double num5 = Utils.Lerp(startingSize, _endingSize, num4);
                num += Math.Cos(angle);
                num2 += Math.Sin(angle);
                if (num3 >= max - distance * 0.1 && !_leavesSpawned) {
                    _leavesSpawned = true;
                    int size = _random.Next(5, 8);
                    WorldGenHelper.TileWallRunner((int)num, (int)num2, size, size, _leavesType, _leavesWall, true, noYChange: true, overRide: false);
                }
                angle += (double)GenBase._random.NextFloat() - 0.5 + (double)GenBase._random.NextFloat() * (_angle - 1.5707963705062866) * 0.1 * (1.0 - num4);
                angle = angle * 0.4 + 0.45 * Utils.Clamp(angle, _angle - 2.0 * (1.0 - 0.5 * num4), _angle + 2.0 * (1.0 - 0.5 * num4)) + Utils.Lerp(_angle, 1.5707963705062866, num4) * 0.15;
                for (int i = 0; i < (int)num5; i++) {
                    for (int j = 0; j < (int)num5; j++) {
                        if (!UnitApply(action, origin, (int)num + i, (int)num2 + j) && _quitOnFail)
                            return false;
                    }
                }
            }

            return true;
        }

        public override bool Perform(Point origin, GenAction action) => DoRoot(origin, action, _angle, _distance, _startingSize);
    }

    private void GrowBigTree(int i) {
        int j = WorldGenHelper.GetFirstTileY2(i);
        j += 2;
        if (!WorldGenHelper.GetTileSafely(i, j + 1).HasTile) {
            j = WorldGenHelper.GetFirstTileY2(i);
            j += 2;
        }
        int height = _random.Next(40, 50);
        //TreeBranch(x, y2);

        // foliage
        for (int sizeX = 0; sizeX < 9; sizeX++) {
            Shapes.Slime circle = new(Math.Max(4, _random.Next(sizeX + 2) + 2), _random.NextFloat(0.925f, 1.1f) + _random.NextFloat(-0.15f, 0.21f), _random.NextFloat(1.0f, 1.2f) + _random.NextFloat(-0.075f, 0.11f));
            int y2 = j - height + height / 3 + 1;
            WorldUtils.Gen(new Point(i, y2), circle, new SetTileAndWall(_leavesTileType, _leavesWallType));
            BigTreeBranch(i, y2);
        }

        // roots
        for (int sizeX = -_random.Next(2, 4); sizeX <= _random.Next(2, 4) + 1; sizeX++) {
            if (sizeX < -1) {
                continue;
            }

            WorldUtils.Gen(new Point(i - 1, j + 1), new ShapeRoot(sizeX / 3f * 2f + 0.57075f, (int)(_random.Next(12, 18) * 0.7), 3, 1), new SetTileAndWall(_elderwoodTileType, _elderwoodWallType));
        }

        // stem
        for (int testY = j; testY >= j - height; testY--) {
            float progress = 1f - ((j - testY) / (float)height);
            int radius = (int)(3f * progress);
            for (int i2 = -radius; i2 < radius; i2++) {
                WorldGenHelper.TileWallRunner(i + (int)(i2 * progress), testY, _random.Next(3, 6), _random.Next(2, 5), _elderwoodTileType, _elderwoodWallType, true, speedY: _random.Next(1, 3), noYChange: true);
            }
        }

        // branches
        int index = height / _random.Next(18, 30);
        int maxY = j - height / 6 - 3, minY = j - height / 2 + height / 10;
        List<(int, int)> takenYs = [];
        void placeBranch(bool directedRight = true) {
            takenYs = [];
            index = height / _random.Next(18, 30);
            int attempts = 100;
            while (index > 0) {
                if (--attempts <= 0) {
                    break;
                }
                bool place = true;
                int randomY = _random.Next(minY, maxY);
                for (int index2 = 0; index2 < takenYs.Count; index2++) {
                    (int, int) takenY = takenYs[index2];
                    if (randomY >= takenY.Item1 && randomY <= takenY.Item2) {
                        randomY = _random.Next(minY, maxY);
                        if (--attempts <= 0) {
                            place = false;
                        }
                        else {
                            index2--;
                        }
                    }
                }
                if (place) {
                    int y = randomY + _random.Next(-1, 2);
                    WorldUtils.Gen(new Point(i + (directedRight ? 2 : -2), y), new BigTreeBranchRoot((directedRight ? ((float)Math.PI - 2.8f) : 2.8f) + _random.NextFloat(!directedRight ? -0.1075f : -0.1225f, !directedRight ? 0.06f : 0.07f), _leavesTileType, _leavesWallType, _random.Next(7, 11), 3, 1), new SetTileAndWall(_elderwoodTileType, _elderwoodWallType));
                    index--;
                    takenYs.Add((y - 4, y + 3));
                }
            }
        }
        if (_random.Next(10) <= 9) {
            placeBranch(false);
        }
        if (_random.Next(10) <= 9) {
            placeBranch();
        }

        // walls
        for (int i2 = -25; i2 < 25; i2++) {
            for (int j2 = -(height + 10); j2 < 5; j2++) {
                int x2 = i + i2;
                int y2 = j + j2;
                Tile tile = WorldGenHelper.GetTileSafely(x2, y2);
                if (Main.tile[x2 - 1, y2].HasTile && Main.tile[x2 + 1, y2].HasTile && Main.tile[x2, y2 - 1].HasTile && Main.tile[x2, y2 + 1].HasTile && Main.tile[x2 - 1, y2 - 1].HasTile && Main.tile[x2 + 1, y2 - 1].HasTile && Main.tile[x2 + 1, y2 + 1].HasTile && Main.tile[x2 - 1, y2 + 1].HasTile) {
                    //if (tile.ActiveTile(_leavesTileType)) {
                    //    WorldGenHelper.ReplaceWall(x2, y2, _leavesWallType);
                    //}
                    //if (tile.ActiveTile(_elderwoodTileType)) {
                    //    WorldGenHelper.ReplaceWall(x2, y2, _elderwoodWallType);
                    //}
                }
            }
        }
        for (int i2 = -25; i2 < 25; i2++) {
            for (int j2 = -(height + 10); j2 < 5; j2++) {
                int x2 = i + i2;
                int y2 = j + j2;
                Tile tile = WorldGenHelper.GetTileSafely(x2, y2);
                Tile leftTile = WorldGenHelper.GetTileSafely(x2 - 1, y2);
                Tile aboveTile = WorldGenHelper.GetTileSafely(x2, y2 - 1);
                Tile belowTile = WorldGenHelper.GetTileSafely(x2, y2 + 1);
                if (tile.ActiveTile(_elderwoodTileType) && aboveTile.ActiveTile(_elderwoodTileType) && belowTile.ActiveTile(_elderwoodTileType) && leftTile.ActiveTile(_elderwoodTileType) && leftTile.Slope != SlopeType.Solid) {
                    if (tile.WallType == _elderwoodWallType) {
                        WorldGen.KillWall(x2, y2);
                    }
                }
                if (!Main.tile[x2 - 1, y2].ActiveTile(_elderwoodTileType) || !Main.tile[x2 + 1, y2].ActiveTile(_elderwoodTileType) || !Main.tile[x2, y2 - 1].ActiveTile(_elderwoodTileType) || !Main.tile[x2, y2 + 1].ActiveTile(_elderwoodTileType) ||
                    !Main.tile[x2 - 1, y2 - 1].ActiveTile(_elderwoodTileType) || !Main.tile[x2 + 1, y2 - 1].ActiveTile(_elderwoodTileType) || !Main.tile[x2 + 1, y2 + 1].ActiveTile(_elderwoodTileType) || !Main.tile[x2 - 1, y2 + 1].ActiveTile(_elderwoodTileType)) {
                    if (tile.WallType == _elderwoodWallType) {
                        WorldGen.KillWall(x2, y2);
                    }
                }
                if (!Main.tile[x2 - 1, y2].ActiveTile(_leavesTileType) || !Main.tile[x2 + 1, y2].ActiveTile(_leavesTileType) || !Main.tile[x2, y2 - 1].ActiveTile(_leavesTileType) || !Main.tile[x2, y2 + 1].ActiveTile(_leavesTileType) ||
                    !Main.tile[x2 - 1, y2 - 1].ActiveTile(_leavesTileType) || !Main.tile[x2 + 1, y2 - 1].ActiveTile(_leavesTileType) || !Main.tile[x2 + 1, y2 + 1].ActiveTile(_leavesTileType) || !Main.tile[x2 - 1, y2 + 1].ActiveTile(_leavesTileType)) {         
                    if (tile.WallType == _leavesWallType) {
                        WorldGen.KillWall(x2, y2);
                    }
                }
                if (tile.WallType == _dirtWallType) {
                    tile.WallType = WallID.None;
                }
            }
        }

        // entrance
        int startX = i - 1, startY = j - 2;
        int offset = _random.Next(4) <= 2 ? 0 : _random.NextBool() ? 1 : -1;
        bool increasedWidth = _random.NextBool();
        for (int i2 = -2; i2 <= 2; i2++) {
            for (int j2 = -3; j2 <= 0; j2++) {
                WorldGen.KillTile(startX + i2, startY + j2);
                offset += increasedWidth ? 1 : 0;
                if (j2 == -3 && (i2 == 0 + offset || i2 == 1 + offset || (increasedWidth ? i2 == 2 + offset : false))) {
                    WorldGen.KillTile(startX + i2, startY + j2 - 1);
                }
                if (i2 > -2 && i2 < 2) {
                    WorldGenHelper.ReplaceWall(startX + i2, startY + j2, _elderwoodWallType);
                }
            }
        }

        // fallen trees
        bool flag = false;
        for (int i2 = i - 15; i2 < i; i2++) {
            int j2 = WorldGenHelper.GetFirstTileY(i2, _grassTileType);
            WorldGenHelper.Place3x2(i2, j2 - 1, _fallenTreeTileType, styleX: _random.Next(2), onPlaced: () => {
                flag = true;
            });
            if (flag) {
                break;
            }
        }
        flag = false;
        for (int i2 = i + 15; i2 > i; i2--) {
            int j2 = WorldGenHelper.GetFirstTileY(i2, _grassTileType);
            WorldGenHelper.Place3x2(i2, j2 - 1, _fallenTreeTileType, styleX: _random.Next(2), onPlaced: () => {
                flag = true;
            });
            if (flag) {
                break;
            }
        }
    }

    private void Step12_AddRoots() {
        int minY = BackwoodsVars.FirstTileYAtCenter + _biomeHeight / 10;
        for (int i = Left - 25; i < Right + 25; i++) {
            for (int j = minY; j < Bottom + 10; j++) {
                if ((WorldGenHelper.ActiveTile(i, j, _dirtTileType) || WorldGenHelper.ActiveTile(i, j, _stoneTileType)) &&
                    _random.NextChance(0.0025)) {
                    Root(i, j);
                }
            }
        }
    }

    private void Step6_2_SpreadGrass() {
        int minY = BackwoodsVars.FirstTileYAtCenter + _biomeHeight / 10;
        for (int i = Left - 25; i < Right + 25; i++) {
            for (int j = Top - 10; j < (int)GenVars.worldSurfaceHigh; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(_dirtTileType)) {
                    if (_random.NextBool(25)) {
                        tile.TileType = _grassTileType;
                    }
                }
            }
        }
    }

    private class SetTileAndWall : GenAction {
        private ushort _tileType, _wallType;
        private bool _doFraming;
        private bool _doNeighborFraming;

        public SetTileAndWall(ushort tileType, ushort wallType, bool setSelfFrames = true, bool setNeighborFrames = true) {
            _tileType = tileType;
            _wallType = wallType;
            _doFraming = setSelfFrames;
            _doNeighborFraming = setNeighborFrames;
        }

        public override bool Apply(Point origin, int i, int j, params object[] args) {
            Tile tile = WorldGenHelper.GetTileSafely(i, j);
            ushort wall = tile.WallType;
            int wallFrameX = tile.WallFrameX;
            int wallFrameY = tile.WallFrameY;
            tile.Clear(~(TileDataType.Wiring | TileDataType.Actuator));
            tile.TileType = _tileType;
            tile.HasTile = true;

            Tile leftTile = WorldGenHelper.GetTileSafely(i - 1, j);
            Tile rightTile = WorldGenHelper.GetTileSafely(i + 1, j);
            Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
            Tile belowTile = WorldGenHelper.GetTileSafely(i, j + 1);
            if (leftTile.ActiveTile(_tileType) && leftTile.Slope == SlopeType.Solid &&
                rightTile.ActiveTile(_tileType) && rightTile.Slope == SlopeType.Solid &&
                aboveTile.ActiveTile(_tileType) && aboveTile.Slope == SlopeType.Solid &&
                belowTile.ActiveTile(_tileType) && belowTile.Slope == SlopeType.Solid) {
                //tile.WallType = _wallType;
            }
            else {
                tile.WallType = wall;
            }

            tile.WallFrameX = wallFrameX;
            tile.WallFrameY = wallFrameY;

            if (_doFraming) {
                WorldUtils.TileFrame(i, j, _doNeighborFraming);
            }

            return UnitApply(origin, i, j, args);
        }
    }

    private void Root(int i, int j) {
        float angle = (1 + _random.Next(3)) / 3f * 2f + 0.57075f;
        int k = (int)(_random.Next(12, 72) * 0.7);
        int min = (int)(_random.Next(4, 9) * 0.7);
        int max = (int)(_random.Next(1, 3) * 0.7);
        WorldUtils.Gen(new Point(i, j), new ShapeRoot(angle, k, min, max), new SetTileAndWall(_elderwoodTileType, _elderwoodWallType));
    }

    private void Step10_SpreadMossGrass() {
        for (int i = Left - 25; i < Right + 25; i++) {
            for (int j = Top - 10; j < Bottom + 10; j++) {
                if (WorldGenHelper.ActiveTile(i, j, _mossTileType)) {
                    for (int offset = 0; offset < 4; offset++) {
                        int i2 = i, j2 = j;
                        if (offset == 0) {
                            i2--;
                        }
                        if (offset == 1) {
                            i2++;
                        }
                        if (offset == 2) {
                            j2--;
                        }
                        if (offset == 3) {
                            j2++;
                        }
                        if (!WorldGenHelper.ActiveTile(i2, j2)) {
                            SpreadMossGrass(i2, j2);
                        }
                    }
                }
            }
        }
    }

    private void GenerateLootRoom(int posX = 0, int posY = 0) {
        int startX = Left - 1;
        int endX = Right + 1;
        int centerY = BackwoodsVars.FirstTileYAtCenter + EdgeY;
        int minY = centerY;
        int maxY = Bottom + EdgeY;
        int baseX, baseY;
        if (posX == 0) {
            baseX = _random.Next(startX, endX);
        }
        else {
            baseX = posX;
        }
        if (posY == 0) {
            baseY = _random.Next(minY + EdgeY, Bottom - EdgeY);
        }
        else {
            baseY = posY;
        }

        ushort[] skipTileTypes = [TileID.Dirt, _stoneTileType, _mossTileType];
        int attempts = 20;
        while (WorldGenHelper.GetTileSafely(baseX, baseY).HasTile && !skipTileTypes.Contains(WorldGenHelper.GetTileSafely(baseX, baseY).TileType)) {
            baseX = _random.Next(startX, endX);
            baseY = _random.Next(minY + EdgeY, Bottom - EdgeY);
            if (attempts-- <= 0) {
                break;
            }
        }

        float x = (float)baseX, y = (float)baseY;
        ushort placeholderTileType = _elderwoodTileType2,
               placeholderWallType = _elderwoodWallType2;

        // base
        int distance = 30;
        int startSize = 8, endSize = 2;
        float angle = (float)_random.NextFloat(-MathHelper.TwoPi, MathHelper.TwoPi);
        for (int num3 = 0; num3 < distance; num3++) {
            float num4 = num3 / (float)distance;
            float num5 = MathHelper.Lerp(startSize, endSize, num4);
            float angle2 = angle;
            float value = _random.NextFloat() - 0.5f + _random.NextFloat() * (angle2 - (float)Math.PI / 2f) * 0.1f * (1f - num4);
            float maxAngle = 0.15f;
            angle += MathHelper.Clamp(value, -maxAngle, maxAngle);
            for (int i = (int)-num5 / 2; i < (int)num5 / 2; i++) {
                for (int j = (int)-num5 / 2; j < (int)num5 / 2; j++) {
                    WorldGenHelper.ReplaceTile((int)x + i, (int)y + j, placeholderTileType);
                    int x3 = (int)x + i;
                    int y3 = (int)y + j;
                    if (Main.tile[x3 - 1, y3].TileType == placeholderTileType && Main.tile[x3 + 1, y3].TileType == placeholderTileType && Main.tile[x3, y3 - 1].TileType == placeholderTileType && Main.tile[x3, y3 + 1].TileType == placeholderTileType && Main.tile[x3 - 1, y3 - 1].TileType == placeholderTileType && Main.tile[x3 + 1, y3 - 1].TileType == placeholderTileType && Main.tile[x3 + 1, y3 + 1].TileType == placeholderTileType && Main.tile[x3 - 1, y3 + 1].TileType == placeholderTileType) {
                        WorldGenHelper.ReplaceWall(x3, y3, placeholderWallType);
                    }
                }
            }
            x += (float)Math.Cos(angle);
            y += (float)Math.Sin(angle);
        }

        // chest room
        int size = 12;
        WorldGenHelper.TileWallRunner(baseX, baseY, size, size, placeholderTileType, placeholderWallType, true, noYChange: true, overRide: true);

        // clean up
        List<Point> killTiles = [];
        for (int x2 = baseX - distance / 2; x2 < baseX + distance / 2; x2++) {
            for (int y2 = baseY - distance / 2; y2 < baseY + distance / 2; y2++) {
                if (!Main.tile[x2 - 1, y2].ActiveTile(placeholderTileType) || !Main.tile[x2 + 1, y2].ActiveTile(placeholderTileType) || !Main.tile[x2, y2 - 1].ActiveTile(placeholderTileType) || !Main.tile[x2, y2 + 1].ActiveTile(placeholderTileType) ||
                    !Main.tile[x2 - 1, y2 - 1].ActiveTile(placeholderTileType) || !Main.tile[x2 + 1, y2 - 1].ActiveTile(placeholderTileType) || !Main.tile[x2 + 1, y2 + 1].ActiveTile(placeholderTileType) || !Main.tile[x2 - 1, y2 + 1].ActiveTile(placeholderTileType)) {
                    Tile tile = WorldGenHelper.GetTileSafely(x2, y2);
                    if (tile.ActiveWall(placeholderWallType)) {
                        WorldGen.KillWall(x2, y2);
                    }
                }

                bool flag = true;
                for (int i2 = -2; i2 <= 2; i2++) {
                    for (int j2 = -2; j2 <= 2; j2++) {
                        if (Math.Abs(i2) != 1 && Math.Abs(j2) != 1) {
                            if (!Main.tile[x2 + i2, y2 + j2].ActiveTile(placeholderTileType)) {
                                flag = false;
                            }
                        }
                    }
                }
                if (!(!Main.tile[x2 - 1, y2].ActiveTile(placeholderTileType) || !Main.tile[x2 + 1, y2].ActiveTile(placeholderTileType) || !Main.tile[x2, y2 - 1].ActiveTile(placeholderTileType) || !Main.tile[x2, y2 + 1].ActiveTile(placeholderTileType) ||
                      !Main.tile[x2 - 1, y2 - 1].ActiveTile(placeholderTileType) || !Main.tile[x2 + 1, y2 - 1].ActiveTile(placeholderTileType) || !Main.tile[x2 + 1, y2 + 1].ActiveTile(placeholderTileType) || !Main.tile[x2 - 1, y2 + 1].ActiveTile(placeholderTileType)) &&
                      flag) {
                    Tile tile = WorldGenHelper.GetTileSafely(x2, y2);
                    if (tile.ActiveWall(placeholderWallType)) {
                        killTiles.Add(new Point(x2, y2));
                    }
                }
            }
        }
        foreach (Point killPos in killTiles) {
            WorldGen.KillTile(killPos.X, killPos.Y);
        }
        //for (int x2 = baseX - distance; x2 < baseX + distance; x2++) {
        //    for (int y2 = baseY - distance; y2 < baseY + distance; y2++) {
        //        if (WorldGenHelper.ActiveTile(x2, y2, placeholderTileType)) {
        //            WorldGenHelper.ReplaceTile(x2, y2, _elderwoodTileType);
        //        }
        //    }
        //}

        //for (int x2 = baseX - distance; x2 < baseX + distance; x2++) {
        //    for (int y2 = baseY - distance; y2 < baseY + distance; y2++) {
        //        if (WorldGenHelper.ActiveWall(x2, y2, placeholderWallType)) {
        //            WorldGenHelper.ReplaceWall(x2, y2, _elderwoodWallType);
        //        }
        //    }
        //}

        // place chest
        bool chestPlaced = false;
        int killTileCount = killTiles.Count;
        List<Point> killTiles2 = [];
        int attempts2 = 50;
        while (killTileCount > 0 && !chestPlaced) {
            if (attempts2-- <= 0) {
                break;
            }
            Point killPos = killTiles[_random.Next(killTiles.Count)];
            int attempts3 = killTileCount * 2;
            while (killTiles2.Contains(killPos)) {
                killPos = killTiles[_random.Next(killTiles.Count)];
                if (attempts3-- <= 0) {
                    break;
                }
            }
            killTiles2.Add(killPos);
            Tile tile = WorldGenHelper.GetTileSafely(killPos.X, killPos.Y);
            if (tile.ActiveWall(placeholderWallType)) {
                WorldGenHelper.PlaceChest(killPos.X, killPos.Y, _elderWoodChestTileType, onPlaced: (chest) => {
                    chestPlaced = true;
                    int slotId = 0;
                    void addItemInChest(int itemType, int itemStack, double chance = 1.0) {
                        if (_random.NextChance(chance)) {
                            Item item = chest.item[slotId];
                            (bool, Item) hasItemInChest = (false, null);
                            foreach (Item chestItem in chest.item) {
                                if (chestItem.IsEmpty()) {
                                    continue;
                                }
                                if (chestItem.type == itemType) {
                                    hasItemInChest = (true, chestItem);
                                }
                            }
                            if (!hasItemInChest.Item1) {
                                item.SetDefaults(itemType);
                                item.stack = itemStack;
                                slotId++;
                            }
                            else {
                                hasItemInChest.Item2.stack += itemStack;
                            }
                        }
                    }
                    Action[] secondTierAddings = [
                        () => addItemInChest(WorldGen.SavedOreTiers.Gold == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar,
                                             _random.Next(3, 11),
                                             0.75),
                        () => addItemInChest(_random.Next([/*(ushort)ModContent.ItemType<DeathWardPotion>(),*/ ItemID.HealingPotion]),
                                             _random.Next(3, 7),
                                             0.75),
                        () => addItemInChest(_random.Next([ItemID.SwiftnessPotion, ItemID.IronskinPotion, ItemID.ShinePotion, ItemID.NightOwlPotion, ItemID.ArcheryPotion, ItemID.HunterPotion]),
                                             _random.Next(1, 3),
                                             0.75),
                        () => addItemInChest(_random.Next([ItemID.RecallPotion, ItemID.TeleportationPotion]),
                                             _random.Next(1, 3),
                                             0.75),
                        () => addItemInChest(_random.Next([ItemID.Torch, ItemID.SpelunkerGlowstick]),
                                             _random.Next(15, 31),
                                             0.75),
                        () => addItemInChest(_random.Next([ItemID.GoldCoin]),
                                             _random.Next(2, 6),
                                             0.75),
                        () => addItemInChest(_random.Next([ItemID.GoldCoin]),
                                             _random.Next(2, 6),
                                             0.25)
                    ];
                    List<Action> cacheAddings = [];
                    int length = secondTierAddings.Length;
                    int count = length;
                    while (count > 0) {
                        Action add = secondTierAddings[_random.Next(length)];
                        int attempts2 = 100;
                        while (cacheAddings.Contains(add)) {
                            add = secondTierAddings[_random.Next(length)];
                            if (attempts2-- <= 0) {
                                break;
                            }
                        }
                        cacheAddings.Add(add);
                        add();
                        count--;
                    }
                });
            }
            if (chestPlaced) {
                break;
            }
            killTileCount--;
        }

        size = distance;
        WorldGenHelper.SlopeAreaNatural(baseX, baseY, size, placeholderTileType);
    }

    public void BackwoodsCleanup(GenerationProgress progress, GameConfiguration config) {
        for (int i = Left - 50; i < Right + 50; i++) {
            for (int j = Top - 15; j < Bottom + 15; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(_dirtTileType)) {
                    tile.TileType = TileID.Dirt;
                }
            }
        }
    }

    public void BackwoodsOtherPlacements(GenerationProgress progress, GameConfiguration config) {
        progress.Message = Language.GetOrRegister("Mods.RoA.WorldGen.Backwoods2").Value;

        void cleanUp() {
            int num1047 = 0;
            for (int i = Left - 25; i < Right + 25; i++) {
                for (int j = Top - 15; j < Bottom + 15; j++) {
                    Tile tile = WorldGenHelper.GetTileSafely(i, j);
                    if (tile.ActiveTile(_mossTileType)) {
                        Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
                        Tile leftTile = WorldGenHelper.GetTileSafely(i - 1, j);
                        if (aboveTile.ActiveTile(_mossTileType) && (leftTile.ActiveTile(_mossTileType) || leftTile.ActiveTile(_grassTileType))) {
                            tile.TileType = _stoneTileType;
                        }
                        Tile belowTile = WorldGenHelper.GetTileSafely(i, j + 1);
                        if (leftTile.ActiveTile(_mossTileType) && belowTile.Slope != SlopeType.Solid) {
                            tile.TileType = _stoneTileType;
                        }
                    }
                    if (tile.ActiveTile(TileID.Dirt)) {
                        Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
                        if (aboveTile.ActiveTile(TileID.Trees)) {
                            WorldGenHelper.ReplaceTile(i, j, TileID.Dirt);
                            WorldGen.SpreadGrass(i, j, TileID.Dirt, _grassTileType);
                        }
                    }
                    if (tile.ActiveTile(TileID.Vines) || tile.ActiveTile(TileID.VineFlowers)) {
                        tile.TileType = _vinesTileType;
                    }
                    if (tile.ActiveTile(TileID.Grass)) {
                        Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
                        if (aboveTile.HasTile && !Main.tileSolid[aboveTile.TileType]) {
                            WorldGen.KillTile(i, j - 1);
                        }
                        Tile belowTile = WorldGenHelper.GetTileSafely(i, j + 1);
                        if (!belowTile.HasTile) {
                            WorldGen.KillTile(i, j);
                        }
                        else {
                            WorldGenHelper.ReplaceTile(i, j, TileID.Dirt);
                            WorldGen.SpreadGrass(i, j, TileID.Dirt, _grassTileType);
                        }
                    }
                    num1047 += _random.Next(-1, 2);
                    if (num1047 < 0)
                        num1047 = 0;

                    if (num1047 > 5)
                        num1047 = 5;
                    bool edgeLeft = i < Left - 5, edgeRight = i > Right + 5;
                    bool edgeX = edgeLeft || edgeRight;
                    bool flag = (edgeX && _random.NextBool(3)) || (!edgeLeft && !edgeRight);
                    tile = WorldGenHelper.GetTileSafely(i + (edgeX ? num1047 : 0), j);
                    if (flag) {
                        if (tile.WallType == WallID.MudUnsafe) {
                            tile.WallType = _dirtWallType;
                        }
                        ushort[] invalidWalls = [WallID.FlowerUnsafe, WallID.GrassUnsafe, WallID.JungleUnsafe];
                        if (invalidWalls.Contains(tile.WallType)) {
                            tile.WallType = _grassWallType;
                        }
                    }
                }
            }
        }

        progress.Set(0.1f);
        cleanUp();

        progress.Set(0.2f);
        foreach (Point surface in _biomeSurface) {
            for (int j = 3; j > -_biomeHeight / 3; j--) {
                Tile aboveTile = WorldGenHelper.GetTileSafely(surface.X, surface.Y + j - 1);
                Tile belowTile = WorldGenHelper.GetTileSafely(surface.X, surface.Y + j + 1);
                Tile tile = WorldGenHelper.GetTileSafely(surface.X, surface.Y + j);
                if (!tile.ActiveTile(CliffPlaceholderTileType) && !_backwoodsPlants.Contains(tile.TileType) && !tile.ActiveTile(_elderwoodTileType) && !tile.ActiveTile(_leavesTileType)) {
                    if (tile.ActiveTile(TileID.Trees)) {
                        WorldGen.KillTile(surface.X, surface.Y + j);
                    }
                    if (belowTile.HasTile && belowTile.WallType == _dirtWallType && !tile.HasTile) {
                        WorldGenHelper.ReplaceTile(surface.X, surface.Y + j, _grassTileType);
                    }
                    if (j < 0) {
                        if (tile.ActiveTile(TileID.Sand)) {
                            WorldGen.KillTile(surface.X, surface.Y + j);
                        }
                        if (tile.WallType == _dirtWallType) {
                            tile.WallType = WallID.None;
                        }
                    }
                    if (tile.ActiveTile(_grassTileType) && aboveTile.HasTile && !Main.tileSolid[aboveTile.TileType]) {
                        if (!_backwoodsPlants.Contains(aboveTile.TileType)) {
                           WorldGen.KillTile(surface.X, surface.Y + j - 1);
                        }
                    }
                    bool flag = j <= 0 && tile.ActiveTile(_mossTileType);
                    if (MidMustKillTileTypes.Contains(tile.TileType) || flag ||
                        ((j < 0) && ((!belowTile.HasTile && belowTile.WallType != WallID.None) ||
                        (tile.HasTile && tile.WallType == WallID.None && !belowTile.HasTile)))) {
                        WorldGen.KillTile(surface.X, surface.Y + j);
                        if (tile.WallType != _elderwoodWallType) {
                            tile.WallType = WallID.None;
                        }
                    }
                }
            }
        }

        progress.Set(0.3f);
        cleanUp();

        progress.Set(0.4f);
        for (int i = 0; i < 3; i++) {
            GrowTrees();
        }

        progress.Set(0.5f);
        for (int i = Left; i < Right; i++) {
            for (int j = Top - 15; j < Bottom + 15; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(TileID.Grass)) {
                    WorldGenHelper.ReplaceTile(i, j, _grassTileType);
                }
                if (tile.ActiveTile(TileID.Stone)) {
                    WorldGenHelper.ReplaceTile(i, j, _stoneTileType);
                }
            }
        }

        progress.Set(0.6f);
        foreach (Point surface in _biomeSurface) {
            for (int j = -10; j < 4; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(surface.X, surface.Y + j);
                if (MidMustKillWallTypes.Contains(tile.WallType)) {
                    tile.WallType = WallID.None;
                }
                if (WorldGenHelper.IsCloud(surface.X, surface.Y + j)) {
                    break;
                }
            }
        }

        progress.Set(0.7f);
        Step6_SpreadGrass();
        Step9_SpreadMoss();
        Step10_SpreadMossGrass();

        progress.Set(0.8f);
        // destroy non solid above leafs
        for (int i = Left - 25; i < Right + 25; i++) {
            for (int j = Top - 15; j < CenterY - EdgeY; j++) {
                Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
                if (WorldGenHelper.ActiveTile(i, j, _leavesTileType) && !Main.tileSolid[aboveTile.TileType]) {
                    WorldGen.KillTile(i, j - 1);
                    break;
                }
            }
        }

        progress.Set(0.9f);

        // place vines
        for (int i = Left - 50; i <= Right + 50; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < CenterY + EdgeY; j++) {
                Tile tile = Main.tile[i, j];
                if ((tile.TileType == _grassTileType || tile.TileType == _leavesTileType) && !Main.tile[i, j + 1].HasTile) {
                    if (WorldGen.genRand.NextBool(tile.TileType == _grassTileType ? 2 : 3)) {
                        WorldGen.PlaceTile(i, j + 1, _vinesTileType);
                    }
                }
                if (tile.TileType == _vinesTileType) {
                    WorldGenHelper.PlaceVines(i, j, WorldGen.genRand.Next(1, 4), _vinesTileType);
                }
            }
        }

         // place plants
        for (int i = Left - 25; i <= Right + 25; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < CenterY + 20; j++) {
                Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(_grassTileType) && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock && !aboveTile.HasTile) {
                    tile = WorldGenHelper.GetTileSafely(i, j - 1);
                    tile.HasTile = true;
                    tile.TileFrameY = 0;
                    if (_random.NextChance(0.15)) {
                        tile.TileType = _bushTileType;
                        tile.TileFrameX = (short)(34 * _random.Next(4));
                    }
                    else {
                        if (_random.NextBool(10)) {
                            tile.TileType = _mintTileType;
                            tile.TileFrameX = (short)(18 * (_random.Next(4) < 2 ? 2 : _random.NextBool() ? 0 : 1));
                            ModContent.GetInstance<MiracleMintTE>().Place(i, j - 1);
                        }
                        else {
                            tile.TileType = _plantsTileType;
                            tile.TileFrameX = (short)(18 * _random.Next(20));
                        }
                    }
                    break;
                }
            }
        }

        // place bushes
        for (int i = Left - 15; i <= Right + 15; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < CenterY - EdgeY; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(_grassTileType)) {
                    if (!WorldGen.SolidTile2(i, j - 1)) {
                        if (_random.NextChance(0.5)) {
                            WallBush(i, j + 3);
                            i += _random.Next(2, 8);
                        }
                    }
                }
            }
        }

        progress.Set(1f);
        Step16_PlaceAltar();
        //Step15_AddLootRooms();
    }

    private void Step17_AddStatues() {
        int statueCount = 15 + 10 * (WorldGenHelper.WorldSize - 1);
        int minY = BackwoodsVars.FirstTileYAtCenter + EdgeY * 2;
        while (statueCount > 0) {
            int x = _random.Next(Left - 15, Right + 15);
            int y = _random.Next(minY, Bottom + 15);
            Tile tile = Framing.GetTileSafely(x, y);
            if (tile.HasTile && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock && (tile.TileType == _grassTileType || tile.TileType == TileID.Dirt || tile.TileType == _dirtTileType || tile.TileType == _stoneTileType || tile.TileType == _elderwoodTileType || tile.TileType == _leavesTileType || tile.TileType == _mossTileType)) {
                if (WorldGenHelper.Place2x3(x, y - 1, (ushort)ModContent.TileType<DryadStatue>(), _random.Next(6))) {
                    statueCount--;
                    //Console.WriteLine(123);
                }
            }
        }
    }

    private void Step11_AddOre() {
        ushort tier1 = (ushort)WorldGen.SavedOreTiers.Copper,
               tier2 = (ushort)WorldGen.SavedOreTiers.Iron,
               tier3 = (ushort)WorldGen.SavedOreTiers.Silver,
               tier4 = (ushort)WorldGen.SavedOreTiers.Gold;
        double oreAmount = (double)(Main.maxTilesX * Main.maxTilesY / 11);
        for (int copper = 0; copper < (int)(oreAmount * 7E-05); copper++) {
            int i = WorldGen.genRand.Next(Left - 15, Right + 15);
            int j = WorldGen.genRand.Next(Top, Bottom);

            if (Main.tile[i, j] != null && Main.tile[i, j].HasTile && Main.tile[i, j].TileType == _dirtTileType) {
                WorldGen.TileRunner(i, j, WorldGen.genRand.Next(5, 12), WorldGen.genRand.Next(5, 12), tier1, false, 0f, 0f, false, true);
            }
        }

        for (int iron = 0; iron < (int)(oreAmount * 7E-05); iron++) {
            int i = WorldGen.genRand.Next(Left - 15, Right + 15);
            int j = WorldGen.genRand.Next(Top, Bottom);

            if (Main.tile[i, j] != null && Main.tile[i, j].HasTile && Main.tile[i, j].TileType == _dirtTileType) {
                WorldGen.TileRunner(i, j, WorldGen.genRand.Next(4, 10), WorldGen.genRand.Next(4, 10), tier2, false, 0f, 0f, false, true);
            }
        }

        int y2 = BackwoodsVars.FirstTileYAtCenter;
        int minY = y2 + _biomeHeight / 3;
        for (int silver = 0; silver < (int)(oreAmount * 6E-05); silver++) {
            int i = WorldGen.genRand.Next(Left - 15, Right + 15);
            int j = WorldGen.genRand.Next(minY, Bottom);

            if (Main.tile[i, j] != null && Main.tile[i, j].HasTile && Main.tile[i, j].TileType == _dirtTileType) {
                WorldGen.TileRunner(i, j, WorldGen.genRand.Next(3, 8), WorldGen.genRand.Next(3, 8), tier3, false, 0f, 0f, false, true);
            }
        }

        for (int gold = 0; gold < (int)(oreAmount * 5E-05); gold++) {
            int i = WorldGen.genRand.Next(Left - 15, Right + 15);
            int j = WorldGen.genRand.Next(minY + _biomeHeight / 3, Bottom);

            if (Main.tile[i, j] != null && Main.tile[i, j].HasTile && Main.tile[i, j].TileType == _dirtTileType) {
                WorldGen.TileRunner(i, j, WorldGen.genRand.Next(3, 8), WorldGen.genRand.Next(3, 8), tier4, false, 0f, 0f, false, true);
            }
        }
    }

    private void Step9_SpreadMoss() {
        for (int i = Left - 50; i < Right + 50; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY - 25; j < Bottom + EdgeY + 25; j++) {
                if (WorldGenHelper.ActiveTile(i, j, _stoneTileType) &&
                    (!WorldGenHelper.GetTileSafely(i - 1, j).HasTile || !WorldGenHelper.GetTileSafely(i + 1, j).HasTile ||
                    !WorldGenHelper.GetTileSafely(i, j - 1).HasTile || !WorldGenHelper.GetTileSafely(i, j + 1).HasTile)) {
                    SpreadMoss(i, j);
                }
            }
        }
    }

    private void Step5_CleanUp() {
        foreach (Point surface in _biomeSurface) {
            for (int j = -_biomeHeight / 3; j < -2; j++) {
                if (WorldGenHelper.IsCloud(surface.X, surface.Y + j)) {
                    break;
                }
                if (Main.tile[surface.X, surface.Y + j].WallType == _leavesWallType || Main.tile[surface.X, surface.Y + j].WallType == _elderwoodWallType) {
                    break;
                }
                if (Main.tile[surface.X, surface.Y + j].TileType == _elderwoodTileType || Main.tile[surface.X, surface.Y + j].TileType == _leavesWallType) {
                    break;
                }
                ushort[] trees = [TileID.LivingWood, TileID.LeafBlock];
                Tile tile = WorldGenHelper.GetTileSafely(surface.X, surface.Y + j);
                if (tile.ActiveTile(CliffPlaceholderTileType)) {
                    continue;
                }
                if (!trees.Contains(tile.TileType) && WorldGenHelper.ActiveTile(surface.X, surface.Y + j)) {
                    WorldGen.KillTile(surface.X, surface.Y + j);
                }
                if (SandTileTypes.Contains(tile.TileType)) {
                    if (j >= 0) {
                        WorldGenHelper.ReplaceTile(surface.X, surface.Y + j, _dirtTileType);
                    }
                    else {
                        WorldGen.KillTile(surface.X, surface.Y + j);
                    }
                }
                if (tile.WallType != WallID.None) {
                    tile.WallType = WallID.None;
                }
            }
        }

        foreach (Point surface in _biomeSurface) {
            for (int j = -5; j < 2; j++) {
                if (WorldGenHelper.IsCloud(surface.X, surface.Y + j)) {
                    break;
                }
                Tile tile = WorldGenHelper.GetTileSafely(surface.X, surface.Y + j);
                if (SandTileTypes.Contains(tile.TileType) && _random.NextBool(3)) {
                    WorldGenHelper.ReplaceTile(surface.X, surface.Y + j, _dirtTileType);
                }
            }
        }

        foreach (Point surface in _biomeSurface) {
            for (int j = -10; j < 2; j++) {
                if (WorldGenHelper.IsCloud(surface.X, surface.Y + j)) {
                    break;
                }
                Tile tile = WorldGenHelper.GetTileSafely(surface.X, surface.Y + j);
                if (SandTileTypes.Contains(tile.TileType)) {
                    WorldGenHelper.ReplaceTile(surface.X, surface.Y + j, _dirtTileType);
                }
            }
        }

        for (int i = Left - 20; i < Right + 20; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < CenterY + 15; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(CliffPlaceholderTileType)) {
                    WorldGenHelper.ReplaceTile(i, j, _dirtTileType);
                }
            }
        }
    }

    private void Step0_Setup() {
        _backwoodsPlants = [];
        _biomeSurface = [];
        _altarTiles = [];

        _biomeWidth += (int)(_biomeWidth * 1.35f * WorldGenHelper.WorldSize2);
        _biomeHeight += (int)(_biomeHeight * 1.35f * WorldGenHelper.WorldSize2);

        CenterX = GenVars.JungleX;
        CenterY = (int)Main.worldSurface;

        _dirtTileType = (ushort)ModContent.TileType<BackwoodsDirt>();
        _grassTileType = (ushort)ModContent.TileType<BackwoodsGrass>();
        _stoneTileType = (ushort)ModContent.TileType<BackwoodsStone>();
        _mossTileType = (ushort)ModContent.TileType<BackwoodsGreenMoss>();
        _mossGrowthTileType = (ushort)ModContent.TileType<MossGrowth>();
        _elderwoodTileType = (ushort)ModContent.TileType<LivingElderwood>();
        _elderwoodTileType2 = (ushort)ModContent.TileType<LivingElderwood2>();
        _leavesTileType = (ushort)ModContent.TileType<LivingElderwoodlLeaves>();
        _dirtWallType = WallID.DirtUnsafe;
        _grassWallType = (ushort)ModContent.WallType<BackwoodsGrassWall>();
        _elderwoodWallType = (ushort)ModContent.WallType<ElderwoodWall>();
        _elderwoodWallType2 = (ushort)ModContent.WallType<ElderwoodWall2>();
        _leavesWallType = (ushort)ModContent.WallType<LivingBackwoodsLeavesWall>();
        _elderWoodChestTileType = (ushort)ModContent.TileType<ElderwoodChest>();
        _altarTileType = (ushort)ModContent.TileType<OvergrownAltar>();
        _vinesTileType = (ushort)ModContent.TileType<BackwoodsVines>();

        _backwoodsPlants.Add(_fallenTreeTileType = (ushort)ModContent.TileType<FallenTree>());
        _backwoodsPlants.Add(_plantsTileType = (ushort)ModContent.TileType<BackwoodsPlants>());
        _backwoodsPlants.Add(_bushTileType = (ushort)ModContent.TileType<BackwoodsBush>());
        _backwoodsPlants.Add(_mintTileType = (ushort)ModContent.TileType<MiracleMint>());
    }

    private void Step1_FindPosition() {
        int tileCountToCheck = _biomeWidth - _biomeWidth / 3;
        SkipTilesByTileType(TileID.Mud, tileCountToCheck: tileCountToCheck);
        SkipTilesByTileType(TileID.Sand, tileCountToCheck: tileCountToCheck * 2, offsetPositionDirectionOnCheck: -1);

        BackwoodsVars.FirstTileYAtCenter = WorldGenHelper.GetFirstTileY(CenterX);
        CenterY = BackwoodsVars.BackwoodsTileForBackground = WorldGenHelper.GetFirstTileY2(CenterX);
        CenterY += _biomeHeight / 2;
    }

    private void Step2_ClearZone() {
        int num1047 = 0;
        int num1048 = 0;
        for (int i = Left; i < Right; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY - 15; j < Bottom; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (WorldGenHelper.IsCloud(i, j)) {
                    continue;
                }
                if (SkipBiomeInvalidWallTypeToKill2.Contains(tile.WallType)) {
                    continue;
                }
                if (!SandInvalidTileTypesToKill.Contains(tile.TileType) && !SandInvalidWallTypesToKill.Contains(tile.WallType) && !MidInvalidTileTypesToKill.Contains(tile.TileType)) {
                    bool killTile = true;
                    bool replace = false;
                    if (MidInvalidWallTypesToKill.Contains(tile.WallType)) {
                        bool edgeLeft = i < Left + 5, edgeRight = i > Right - 5;
                        num1048 += _random.Next(-1, 2);
                        if (num1048 < 0)
                            num1048 = 0;

                        if (num1048 > 5)
                            num1048 = 5;
                        if (((edgeLeft || edgeRight)) || (!edgeLeft && !edgeRight)) {
                            WorldGenHelper.ReplaceWall(i + num1048, j, _dirtWallType);
                        }
                    }
                    if (SkipBiomeInvalidWallTypeToKill.Contains(tile.WallType)) {
                        continue;
                    }
                    int spreadY = 0;
                    bool killSand = false;
                    if (SandTileTypes.Contains(tile.TileType)) {
                        int topLeftTileX = Left + 20, topRightTileX = Right - 20;
                        bool edgeLeft = i < topLeftTileX + _biomeWidth / 4, edgeRight = i > topRightTileX - _biomeWidth / 4;
                        bool edgeLeft2 = i > topLeftTileX + _biomeWidth / 3 + 2, edgeRight2 = i < topRightTileX - _biomeWidth / 3 - 2;
                        int minY = BackwoodsVars.FirstTileYAtCenter + EdgeY;
                        killTile = false;
                        if (!edgeLeft && !edgeRight) {
                            if (j < minY) {
                                killTile = true;
                                killSand = true;
                                if (!edgeLeft2 && !edgeRight2) {
                                    spreadY += Main.rand.Next(-2, 3);
                                }
                            }
                        }
                        else {
                            if (j > Top - 10) {
                                num1047 += _random.Next(-1, 2);
                                if (num1047 < 0)
                                    num1047 = 0;

                                if (num1047 > 5)
                                    num1047 = 5;
                                replace = killTile = _random.NextBool(4);
                            }
                        }
                    }
                    if (killTile) {
                        int[] replaceWallTypes = [WallID.EbonstoneUnsafe, WallID.CrimstoneUnsafe];
                        if (replaceWallTypes.Contains(tile.WallType)) {
                            tile.WallType = _dirtWallType;
                        }
                        if (replace) {
                            if (tile.WallType != WallID.None) {
                                WorldGenHelper.ReplaceTile(i + num1047, j + spreadY, _dirtTileType);
                            }
                        }
                        else {
                            if ((killSand && _random.NextBool(3)) || !killSand) {
                                WorldGen.KillTile(i, j);
                            }
                        }
                    }
                }
            }
        }
    }

    private void Step3_GenerateBase() {
        int topLeftTileX = TopLeft.X - 2, topLeftTileY = WorldGenHelper.GetFirstTileY(topLeftTileX);
        int topRightTileX = TopRight.X + 2, topRightTileY = WorldGenHelper.GetFirstTileY(topRightTileX);
        int surfaceY = 0;
        int angle = 25;
        int between = Math.Clamp(topRightTileY - topLeftTileY, -angle, angle);
        int leftY = WorldGenHelper.GetFirstTileY(topLeftTileX), rightY = WorldGenHelper.GetFirstTileY(topRightTileX);
        int max = Math.Min(leftY, rightY) == leftY ? topLeftTileX : topRightTileX;
        _toLeft = max == topLeftTileX;
        //for (int k = 0; k < 30; k++) {
        //    WorldGenHelper.ReplaceTile(toLeft ? topLeftTileX - 1 : (topRightTileX + 1), (toLeft ? topLeftTileY : (topRightTileY)) - k, CliffPlaceholderTileType);
        //}
        void setSurfaceY() {
            int getSurfaceOffset() {
                int offset = 0;
                while (_random.Next(0, 8) == 0) {
                    offset += _random.Next(-1, 2);
                }
                return offset;
            }
            surfaceY += getSurfaceOffset();
            if (Math.Abs(surfaceY) > 2) {
                surfaceY -= _random.Next(0, 3) * Math.Sign(surfaceY);
            }
        }
        void generateBase(int i, bool reversedProgress = false) {
            float placementProgress = (i - topLeftTileX) / (float)(topRightTileX - topLeftTileX);
            _progress.Set(reversedProgress ? 1f - placementProgress : placementProgress);
            int y = topLeftTileY + (int)(between * placementProgress), y2 = y + surfaceY;
            _biomeSurface.Add(new Point(i, y2));
            for (int j3 = 0; j3 < -5; j3++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, y2 + j3);
                if (!tile.ActiveTile(_grassTileType)) {
                    WorldGen.KillTile(i, y2 + j3);
                }
            }
            int j = y2;
            int waterYRandomness = 0;
            while (true) {
                bool edgeLeft = i < topLeftTileX + 10, edgeRight = i > topRightTileX - 10;
                bool mid = i > topLeftTileX + _biomeWidth / 3 - 20 && i < topRightTileX - _biomeWidth / 3 + 20;
                bool edgeX = edgeLeft || edgeRight, jungleEdge = GenVars.dungeonSide == 1 ? edgeLeft : edgeRight;
                bool edge = j > y2 + 25 && (edgeX || j > Bottom - 25);
                if (edge) {
                    int randomnessX = (int)(_random.Next(-20, 21) * 0.1f);
                    int strength = Main.rand.Next(15, 40), step = Main.rand.Next(2, 13);
                    WorldGenHelper.ModifiedTileRunnerForBackwoods(i + randomnessX, j, strength, step, _dirtTileType, _dirtWallType, true, 0f, 0f, true, true, true, false);
                }
                else {
                    Tile tile = WorldGenHelper.GetTileSafely(i, j);
                    if (mid || (!mid && !SkipBiomeInvalidWallTypeToKill.Contains(tile.WallType)) || tile.AnyLiquid()) {
                        bool killTile = true, killWater = false;
                        if (MidInvalidTileTypesToKill.Contains(tile.TileType) || MidMustSkipWallTypes.Contains(tile.WallType) ||
                            SandInvalidWallTypesToKill.Contains(tile.WallType) || SandInvalidTileTypesToKill.Contains(tile.TileType)) {
                            killTile = false;
                        }
                        if (tile.AnyLiquid()) {
                            if (j > y2 + 20) {
                                killTile = false;
                            }
                            else {
                                killWater = true;
                                waterYRandomness += _random.Next(-1, 2);
                                if (waterYRandomness < 0)
                                    waterYRandomness = 0;

                                if (waterYRandomness > 5)
                                    waterYRandomness = 5;
                            }
                        }
                        if (SandTileTypes.Contains(tile.TileType)) {
                            if (j > y2 + 5) {
                                killTile = false;
                            }
                            else if (edgeX && _random.NextBool(3)) {
                                killTile = false;
                            }
                        }
                        if (killTile || (mid && MidMustKillTileTypes.Contains(tile.TileType))) {
                            void replaceTile(int? tileType = null, ushort? wallType = null) {
                                WorldGenHelper.ReplaceTile(i, j + (killWater ? waterYRandomness : 0), tileType ?? _dirtTileType);
                                if (tile.AnyWall()) {
                                    tile.WallType = wallType ?? _dirtWallType;
                                }
                            }
                            bool spread = _random.NextBool(3);
                            if (jungleEdge && spread) {
                                replaceTile(TileID.Mud, WallID.MudUnsafe);
                            }
                            else {
                                replaceTile();
                                if (tile.WallType == WallID.MudUnsafe || tile.WallType == WallID.MudWallEcho) {
                                    tile.WallType = _dirtWallType;
                                }
                            }
                        }
                    }
                }
                j++;
                if (j > Bottom) {
                    break;
                }
            }
            setSurfaceY();
        }
        if (_toLeft) {
            for (int i = topRightTileX; i > topLeftTileX; i--) {
                generateBase(i, true);
            }
        }
        else {
            for (int i = topLeftTileX; i < topRightTileX; i++) {
                generateBase(i);
            }
        }
        Point cliffTileCoords = Point.Zero;
        cliffTileCoords.X = _toLeft ? topLeftTileX - 10 : (topRightTileX + 10);
        cliffTileCoords.Y = WorldGenHelper.GetFirstTileY2(cliffTileCoords.X);
        // cliff
        int lastSurfaceY = _biomeSurface.Last().Y;
        //int distance = lastSurfaceY - cliffTileCoords.Y;
        int cliffX = cliffTileCoords.X;
        int startY = cliffTileCoords.Y;
        while (startY < lastSurfaceY) {
            while (_random.Next(0, 6) <= _random.Next(1, 4)) {
                startY++;
            }
            bool flag = Math.Abs(cliffX - cliffTileCoords.X) > 20;
            if ((_random.NextChance(0.75) && flag) || !flag) {
                cliffX -= _toLeft ? -1 : 1;
            }
            bool flag2 = Math.Abs(cliffX - cliffTileCoords.X) > 10;
            int testJ = startY;
            while (true) {
                if (testJ > startY + _biomeHeight / 3) {
                    break;
                }
                bool flag3 = !flag2 && Main.tile[cliffX, testJ].HasTile;
                if (flag3 || flag2) {
                    if (flag3) {
                        WorldGenHelper.ReplaceTile(cliffX, testJ, _random.NextChance(0.75f) ? CliffPlaceholderTileType : TileID.Mud);
                    }
                    else {
                        WorldGenHelper.ReplaceTile(cliffX, testJ, CliffPlaceholderTileType);
                    }
                }
                testJ++;
            }
        }
        _lastCliffX = cliffX;

        foreach (Point surface in _biomeSurface) {
            for (int j = EdgeY; j > -50; j--) {
                Tile tile = WorldGenHelper.GetTileSafely(surface.X, surface.Y + j);
                if (tile.AnyLiquid()) {
                    tile.LiquidAmount = 0;
                }
            }
        }
    }

    private void Step4_CleanUp() {
        for (int i = Left - 35; i <= Right + 35; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < CenterY; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (SkipBiomeInvalidTileTypeToKill.Contains(tile.TileType)) {
                    continue;
                }
                if (SkipBiomeInvalidWallTypeToKill.Contains(tile.WallType)) {
                    continue;
                }
                if (MidReplaceWallTypes.Contains(tile.WallType)) {
                    tile.WallType = _dirtWallType;
                }
                if (MidInvalidWallTypesToKill.Contains(tile.WallType)) {
                    tile.WallType = _dirtWallType;
                }
                if (MidMustKillWallTypes.Contains(tile.WallType)) {
                    tile.WallType = WallID.None;
                }
                if (tile.ActiveTile(CliffPlaceholderTileType) || WorldGenHelper.IsCloud(i, j) || MidInvalidTileTypesToKill.Contains(tile.TileType)) {
                    break;
                }
                if (!SandTileTypes.Contains(tile.TileType) && !tile.ActiveTile(TileID.Mud) && !tile.ActiveTile(TileID.Stone)) {
                    int count = 0;
                    if (WorldGenHelper.ActiveTile(i - 1, j)) {
                        count++;
                    }
                    if (WorldGenHelper.ActiveTile(i + 1, j)) {
                        count++;
                    }
                    if (WorldGenHelper.ActiveTile(i, j - 1)) {
                        count++;
                    }
                    if (WorldGenHelper.ActiveTile(i, j + 1)) {
                        count++;
                    }
                    if (count >= 3) {
                        WorldGenHelper.ReplaceTile(i, j, _dirtTileType);
                    }
                }
                int minY = BackwoodsVars.FirstTileYAtCenter + EdgeY;
                if (j > minY - EdgeY && j < CenterY + EdgeY && tile.WallType == WallID.None) {
                    tile.WallType = _dirtWallType;
                }
            }
        }
    }

    private void Step6_SpreadGrass() {
        double randomnessY = Main.worldSurface + 40.0;
        for (int i = Left - EdgeX; i < Right + EdgeX; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY - 10; j < CenterY; j++) {
                randomnessY += (double)_random.NextFloat(-2f, 3f);
                randomnessY = Math.Clamp(randomnessY, Main.worldSurface + 30.0, Main.worldSurface + 50.0);
                bool spread = false;
                for (int j2 = (int)GenVars.worldSurfaceLow; j2 < randomnessY; j2++) {
                    Tile tile = WorldGenHelper.GetTileSafely(i, j2);
                    if (tile.HasTile) {
                        if (j2 < Main.worldSurface - 1.0 && !spread) {
                            if (tile.TileType == _dirtTileType || tile.TileType == TileID.Dirt) {
                                WorldGen.grassSpread = 0;
                                WorldGen.SpreadGrass(i, j2, _dirtTileType, _grassTileType);
                            }
                        }
                        spread = true;
                    }
                }
            }
        }
    }

    private void Step7_AddStone() {
        int tileCount = (int)(Main.maxTilesX * Main.maxTilesY * 0.0005) * 375;
        int startX = Left - 1;
        int endX = Right + 1;
        int worldSize = Main.maxTilesX / 4200;
        int k = worldSize == 1 ? (int)(_biomeHeight * 0.25) : worldSize == 2 ? (int)(_biomeHeight * 0.2) : (int)(_biomeHeight * 0.15);
        int y2 = BackwoodsVars.FirstTileYAtCenter;
        int minY = (int)Main.worldSurface - (20 * WorldGenHelper.WorldSize);
        int maxY = Bottom + EdgeY;
        int stoneCount = (int)(tileCount * 0.0003f * 0.75f);
        int x;
        for (int i = 0; i < stoneCount; i++) {
            x = _random.Next(startX - 30, endX + 30);
            int y = _random.Next(y2 - EdgeY, minY + 15);
            int sizeX = _random.Next(4, 9);
            int sizeY = _random.Next(5, 18);
            if (WorldGenHelper.ActiveTile(x, y, _dirtTileType)) {
                WorldGen.TileRunner(x, y, sizeX, sizeY, _stoneTileType);
            }
        }
        stoneCount = (int)(tileCount * 0.0002f * 0.9f);
        for (int i = 0; i < stoneCount; i++) {
            x = _random.Next(startX - 30, endX + 30);
            int y = _random.Next(Math.Min(Top, Bottom) - 15, maxY);
            int sizeX = _random.Next(4, 15);
            int sizeY = _random.Next(5, 40);
            if (WorldGenHelper.ActiveTile(x, y, _dirtTileType)) {
                WorldGen.TileRunner(x, y, sizeX, sizeY, _stoneTileType);
            }
        }
        stoneCount = (int)(tileCount * 0.002f * 0.9f);
        for (int i = 0; i < stoneCount; i++) {
            x = _random.Next(startX - 30, endX + 30);
            int y = _random.Next(minY, (int)((Main.worldSurface + Main.rockLayer) * 0.7) + 3);
            int sizeX = _random.Next(4, 10);
            int sizeY = _random.Next(5, 30);
            if (WorldGenHelper.ActiveTile(x, y, _dirtTileType)) {
                WorldGen.TileRunner(x, y, sizeX, sizeY, _stoneTileType);
            }
        }
        stoneCount = (int)(tileCount * 0.002f / 2f);
        for (int i = 0; i < stoneCount; i++) {
            x = _random.Next(startX - 30, endX + 30);
            int y = _random.Next(minY + 15, (int)((Main.worldSurface + Main.rockLayer) * 0.9) + 5);
            int sizeX = _random.Next(2, 7);
            int sizeY = _random.Next(2, 23);
            if (WorldGenHelper.ActiveTile(x, y, _dirtTileType)) {
                WorldGen.TileRunner(x, y, sizeX, sizeY, _stoneTileType);
            }
        }
        stoneCount = (int)(tileCount * 0.005f * 0.61f);
        for (int i = 0; i < stoneCount; i++) {
            x = _random.Next(startX - 30, endX + 30);
            int y = _random.Next(Bottom - EdgeY * 2, (int)Main.rockLayer + _biomeHeight);
            int sizeX = _random.Next(4, 10);
            int sizeY = _random.Next(5, 30);
            if (WorldGenHelper.ActiveTile(x, y, _dirtTileType)) {
                WorldGen.TileRunner(x, y, sizeX, sizeY, _stoneTileType);
            }
        }
    }

    private void Step8_AddCaves() {
        int tileCount = (int)(Main.maxTilesX * 0.5) * 375;
        int startX = Left - 25;
        int endX = Right + 25;
        int worldSize = Main.maxTilesX / 4200;
        int minY = BackwoodsVars.FirstTileYAtCenter + EdgeY / 2;
        int maxY = Bottom + EdgeY;
        int x;
        int maxCaves = (int)(tileCount * 0.0001625f * 0.5f);
        for (int i = 0; i < maxCaves; i++) {
            x = _random.Next(startX - 50, endX + 50);
            int y = _random.Next(minY + 5, maxY + 5);
            if (WorldGenHelper.ActiveTile(x, y, _dirtTileType) || WorldGenHelper.ActiveTile(x, y, _stoneTileType)) {
                int type = -1;
                if (_random.NextBool(6)) {
                    type = -2;
                }
                int sizeX = _random.Next(5, 15);
                int sizeY = _random.Next(30, 200);
                WorldGen.TileRunner(x, y, sizeX, sizeY, type);
            }
        }
    }

    private void GrowTrees() {
        for (int i = Left - 5; i <= Right + 5; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY - EdgeY; j < CenterY + 20; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (WorldGenHelper.ActiveTile(i, j, _grassTileType) && !_backwoodsPlants.Contains(WorldGenHelper.GetTileSafely(i, j - 1).TileType) && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock) {
                    WorldGenHelper.GrowTreeWithBranches<TreeBranch>(i, j);
                }
            }
        }
    }

    private void SkipTilesByTileType(int tileType, int? step = null, int offsetPositionDirectionOnCheck = 1, int tileCountToCheck = 50) {
        int attempts = 0, maxAttempts = 10000;
        step ??= _biomeWidth / 5 * GenVars.dungeonSide;
        while (++attempts < maxAttempts) {
            if (!WorldGenHelper.TileCountNearby(tileType, CenterX, CenterY, tileCountToCheck)) {
                break;
            }
            while (WorldGenHelper.TileCountNearby(tileType, CenterX, CenterY, tileCountToCheck)) {
                CenterX += (int)step * offsetPositionDirectionOnCheck;
            }
        }
    }

    private void SpreadMossGrass(int i, int j) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        tile.HasTile = true;
        tile.TileType = _mossGrowthTileType;
        tile.TileFrameX = 0;
        tile.TileFrameY = (short)(_random.Next(3) * 18);
        WorldGen.SquareTileFrame(i, j);
    }

    // adapted vanilla
    private void SpreadMoss(int x, int y) {
        if (!WorldGen.InWorld(x, y))
            return;

        ushort mossTile = _mossTileType;
        List<Point> list = new List<Point>();
        List<Point> list2 = new List<Point>();
        HashSet<Point> hashSet = new HashSet<Point>();
        list2.Add(new Point(x, y));
        int attempts = 0;
        while (list2.Count > 0) {
            if (++attempts > 10000) {
                return;
            }
            list.Clear();
            list.AddRange(list2);
            list2.Clear();
            while (list.Count > 0) {
                if (++attempts > 10000) {
                    return;
                }

                Point item = list[0];
                if (!WorldGen.InWorld(item.X, item.Y, 1)) {
                    list.Remove(item);
                    continue;
                }

                hashSet.Add(item);
                list.Remove(item);
                Tile tile = Main.tile[item.X, item.Y];
                if (WorldGen.SolidTile(item.X, item.Y)) {
                    if (tile.HasTile) {
                        if (tile.TileType == _stoneTileType)
                            tile.TileType = mossTile;
                    }

                    continue;
                }

                Point item2 = new Point(item.X - 1, item.Y);
                if (!hashSet.Contains(item2))
                    list2.Add(item2);

                item2 = new Point(item.X + 1, item.Y);
                if (!hashSet.Contains(item2))
                    list2.Add(item2);

                item2 = new Point(item.X, item.Y - 1);
                if (!hashSet.Contains(item2))
                    list2.Add(item2);

                item2 = new Point(item.X, item.Y + 1);
                if (!hashSet.Contains(item2))
                    list2.Add(item2);
            }
        }
    }
}