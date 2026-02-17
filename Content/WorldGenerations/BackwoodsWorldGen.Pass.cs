using Microsoft.Xna.Framework;

using ReLogic.Utilities;

using RoA.Common.BackwoodsSystems;
using RoA.Common.Configs;
using RoA.Common.Sets;
using RoA.Common.World;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Vanity;
using RoA.Content.Items.Placeable.Miscellaneous;
using RoA.Content.Items.Potions;
using RoA.Content.Items.Weapons.Magic;
using RoA.Content.Items.Weapons.Melee;
using RoA.Content.Items.Weapons.Ranged;
using RoA.Content.Items.Weapons.Summon;
using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Danger;
using RoA.Content.Tiles.Furniture;
using RoA.Content.Tiles.LiquidsSpecific;
using RoA.Content.Tiles.Miscellaneous;
using RoA.Content.Tiles.Platforms;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Station;
using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

// one hella mess
// god forgive me
sealed class BackwoodsBiomePass(string name, double loadWeight) : GenPass(name, loadWeight) {
    public static readonly ushort[] SandInvalidTileTypesToKill = { TileID.HardenedSand, TileID.Sandstone };
    public static readonly ushort[] SandInvalidWallTypesToKill = { WallID.SandstoneBrick, 187, 220, 222, 221, 275, 308, 310, 309, 216, 217, 219, 218, 304, 305, 307, 306, 216, 187, 304, 275 };
    public static readonly ushort[] MidInvalidTileTypesToKill = { TileID.LihzahrdBrick, TileID.HardenedSand, TileID.Sandstone, TileID.Ebonstone, TileID.Crimstone, TileID.Marble, TileID.Granite };
    public static readonly ushort[] MidInvalidTileTypesToKill2 = { TileID.HardenedSand, TileID.Sandstone, TileID.Marble, TileID.Granite };
    public static readonly ushort[] MidReplaceWallTypes = { WallID.HiveUnsafe, WallID.MudUnsafe, WallID.MudWallEcho, WallID.EbonstoneEcho, WallID.EbonstoneUnsafe, WallID.CrimstoneEcho, WallID.CrimstoneUnsafe };
    public static readonly ushort[] SkipBiomeInvalidTileTypeToKill = { TileID.HardenedSand, TileID.Sandstone, TileID.Ebonstone, TileID.Crimstone };
    public static readonly ushort[] MidMustKillTileTypes = { TileID.Ebonstone, TileID.Crimstone };
    public static readonly ushort[] MidMustSkipWallTypes = { WallID.HiveUnsafe, WallID.SandstoneBrick, WallID.Granite, WallID.GraniteUnsafe, WallID.MarbleUnsafe, WallID.Marble };
    public static readonly ushort[] MidMustKillWallTypes = { WallID.EbonstoneEcho, WallID.EbonstoneUnsafe, WallID.CrimstoneEcho, WallID.CrimstoneUnsafe };
    public static readonly ushort[] SandTileTypes = { TileID.Sand, TileID.Crimsand, TileID.Ebonsand };

    private static ushort AltarPlaceholderTileType => TileID.WoodBlock;
    private static ushort AltarPlaceholderTileType2 => TileID.StoneSlab;
    private static ushort CliffPlaceholderTileType => TileID.StoneSlab;

    public static List<ushort>? MidInvalidWallTypesToKill;
    public static List<ushort>? SkipBiomeInvalidWallTypeToKill;

    public static List<ushort>? MustSkipTileTypes;
    public static List<ushort>? MustSkipWallTypes;

    public static void Unload() {
        MidInvalidWallTypesToKill?.Clear();
        MidInvalidWallTypesToKill = null;
        SkipBiomeInvalidWallTypeToKill?.Clear();
        SkipBiomeInvalidWallTypeToKill = null;

        MustSkipTileTypes?.Clear();
        MustSkipTileTypes = null;
        MustSkipWallTypes?.Clear();
        MustSkipWallTypes = null;
    }

    private HashSet<ushort> _backwoodsPlants = [];
    private HashSet<Point> _biomeSurface = [], _altarTiles = [];
    private GenerationProgress _progress = null;
    private Point _positionToPlaceBiome;
    private int _biomeWidth, _biomeHeight;
    private ushort _dirtTileType, _grassTileType, _stoneTileType, _mossTileType, _mossGrowthTileType, _elderwoodTileType, _elderwoodTileType2, _elderwoodTileType3, _leavesTileType;
    private ushort _dirtWallType, _grassWallType, _flowerGrassWallType, _elderwoodWallType, _elderwoodWallType2, _leavesWallType;
    private ushort _fallenTreeTileType, _plantsTileType, _bushTileType, _elderWoodChestTileType, _altarTileType, _mintTileType, _vinesTileType, _vinesTileType2, _potTileType;
    private int _lastCliffX;
    private bool _toLeft;
    private int _leftTreeX, _rightTreeX;
    private byte _nextHerb;
    private Point _gatewayLocation;
    private Vector2D _gatewayVelocity;
    private int _nextItemIndex, _nextItemIndex2;
    private bool _costumeAdded;
    private bool _wandsAdded;
    private bool _oneChestPlacedInBigTree;

    private int CenterX {
        get => _positionToPlaceBiome.X;
        set {
            _positionToPlaceBiome.X = Math.Clamp(value, GenVars.beachBordersWidth, Main.maxTilesX - GenVars.beachBordersWidth);
        }
    }

    internal int CenterY {
        get => _positionToPlaceBiome.Y;
        set {
            _positionToPlaceBiome.Y = value/*Math.Max(value, (int)GenVars.worldSurface - 100)*/;
        }
    }

    private int Top => CenterY - _biomeHeight;
    private int Bottom => CenterY + _biomeHeight;
    private int Left => CenterX - _biomeWidth;
    private int Right => CenterX + _biomeWidth;
    private Point TopLeft => new(Left, WorldGenHelper.SafeFloatingIslandY);
    private Point TopRight => new(Right, WorldGenHelper.SafeFloatingIslandY);
    private int EdgeX => _biomeWidth / 4;
    internal int EdgeY => _biomeHeight / 4;

    private void SetUpMessage(LocalizedText message, float? value = null, GenerationProgress progress = null) {
        _progress ??= progress;
        _progress.Message = message.Value;
        if (value.HasValue) {
            _progress.Value = value.Value;
        }
    }

    private class NoForestPotsInBackwoods : ILoadable {
        void ILoadable.Load(Mod mod) {
            On_WorldGen.PlacePot += On_WorldGen_PlacePot;
        }

        private bool On_WorldGen_PlacePot(On_WorldGen.orig_PlacePot orig, int x, int y, ushort type, int style) {
            int potType = ModContent.TileType<BackwoodsPot>();
            if (type != potType && !(style >= 25 && style <= 27) &&
                x > BackwoodsVars.BackwoodsCenterX - BackwoodsVars.BackwoodsHalfSizeX - 100 && x < BackwoodsVars.BackwoodsCenterX + BackwoodsVars.BackwoodsHalfSizeX + 100
                && y < BackwoodsVars.BackwoodsCenterY + BackwoodsVars.BackwoodsSizeY / 2 + BackwoodsVars.BackwoodsSizeY / 3) {
                if (MustSkipWallTypes != null && MustSkipWallTypes.Contains(Main.tile[x, y].WallType)) {
                    return orig(x, y, type, style);
                }
                return WorldGen.PlacePot(x, y, (ushort)potType, _random.Next(4));
            }

            return orig(x, y, type, style);
        }

        void ILoadable.Unload() { }
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration) {
        ElderwoodHouseBuilder._nextPaintingIndex = (byte)WorldGen.genRand.Next(ElderwoodHouseBuilder.PAINTINGCOUNT);

        SetUpMessage(Language.GetOrRegister("Mods.RoA.WorldGen.Backwoods0"), 0f, progress);
        Step0_Setup();
        Step1_FindPosition();
        Step2_ClearZone();
        SetUpMessage(Language.GetOrRegister("Mods.RoA.WorldGen.Backwoods"));
        Step3_GenerateBase();
        BackwoodsVars.FirstTileYAtCenter = (ushort)(WorldGenHelper.GetFirstTileY(CenterX, true) + 15);
        Step4_CleanUp();
        Step5_CleanUp();
        BackwoodsVars.FirstTileYAtCenter = (ushort)(WorldGenHelper.GetFirstTileY(CenterX, true) + 5);
        BackwoodsVars.BackwoodsTileForBackground = (ushort)(WorldGenHelper.GetFirstTileY2(CenterX, skipWalls: true) + 2);
        CenterY = BackwoodsVars.BackwoodsTileForBackground + _biomeHeight / 2;
        BackwoodsVars.BackwoodsSizeY = (ushort)_biomeHeight;
        BackwoodsVars.BackwoodsCenterY = (ushort)CenterY;
        SetUpMessage(Language.GetOrRegister("Mods.RoA.WorldGen.Backwoods4"), 1f);
        Step11_AddOre();
        Step8_AddCaves();
        Step8_2_AddCaves();
        PlaceGateway();
        Step_AddAltarMound();
        foreach (Point surface in _biomeSurface) {
            if (!(surface.X > CenterX - 10 && surface.X < CenterX + 10) && _random.NextBool(4)) {
                WorldUtils.Gen(surface, new Shapes.Mound(20, 5), Actions.Chain(new Modifiers.Blotches(2, 1, 0.8), new Modifiers.OnlyTiles(_dirtTileType), new Actions.SetTile(_dirtTileType), new Actions.SetFrames(frameNeighbors: true)));
            }
        }
        Step7_AddStone();
        Step7_2_AddStone();
        Step12_AddRoots();
        Step6_SpreadGrass();
        Step6_2_SpreadGrass();
        Step13_GrowBigTrees();
        Step9_SpreadMoss();
        //Step_AddJawTraps();
        Step_AddGems();
        Step_AddSpikes();
        Step_AddPills();
        Step10_SpreadMossGrass();
        Step6_SpreadGrass();

        Step_AddWallRootsAndMoss();

        //GenVars.structures.AddProtectedStructure(new Rectangle(Left - 20, Top - 20, _biomeWidth * 2 + 20, _biomeHeight * 2 + 20), 20);
    }

    private void Step_AddWallRootsAndMoss() {
        int minY = (int)Main.worldSurface + 10;

        for (int i = Left - 50; i < Right + 50; i++) {
            for (int j = minY; j < Bottom + EdgeY / 2; j++) {
                if ((WorldGenHelper.ActiveTile(i, j, _dirtTileType) || WorldGenHelper.ActiveTile(i, j, _stoneTileType) ||
                    WorldGenHelper.ActiveTile(i, j, WallID.DirtUnsafe)) &&
                    _random.NextChance(0.0035) && _random.NextChance(0.75)) {
                    WallRoot(i, j);
                }
            }
        }

        for (int i = Left - 50; i < Right + 50; i++) {
            for (int j = minY; j < Bottom + EdgeY / 2; j++) {
                if ((WorldGenHelper.ActiveTile(i, j, _mossTileType)) &&
                    _random.NextChance(0.035) && _random.NextChance(0.75)) {
                    MossRoot(i, j);
                }
            }
        }
    }

    private void MossRoot(int i, int j) {
        int min = (int)(_random.Next(4, 9));
        int max = (int)(_random.Next(1, 3));
        double strenth = _random.NextFloat(max, min) * 3;
        double step = strenth / 3;
        Vector2 direction = Vector2.One.RotatedByRandom(Math.PI) * _random.NextFloat(0f, 6.5f);
        WorldGenHelper.TileWallRunner(i + (int)direction.X, j + (int)direction.Y, strenth, (int)step, 0, (ushort)ModContent.WallType<TealMossWall2>(),
            addTile: true, noYChange: true, onlyWall: true, shouldntHasTile: true, skipWalls: _elderwoodWallType);
    }

    private void WallRoot(int i, int j) {
        float angle = (1 + _random.Next(3)) / 3f * 2f + 0.57075f;
        int k = (int)(_random.Next(12, 72) * 2);
        int min = (int)(_random.Next(4, 9) * 1.25);
        int max = (int)(_random.Next(1, 3) * 1.25);
        WorldUtils.Gen(new Point(i, j), new ShapeRoot(angle, k, min, max), Actions.Chain(
            new Modifiers.SkipWalls(_elderwoodWallType, WallID.LihzahrdBrickUnsafe),
            new Modifiers.SkipWalls(SkipBiomeInvalidWallTypeToKill.ToArray()),
            new Actions.PlaceWall((ushort)ModContent.WallType<Tiles.Walls.BackwoodsRootWall2>(), false)));
    }

    private void Step_AddJawTraps() {
        for (int i = Left - 10; i <= Right + 10; i++) {
            for (int j = BackwoodsVars.FirstTileYAtCenter + 10; j < Bottom + EdgeY / 2; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                bool flag = true;
                int num = 10;
                ushort type = (ushort)ModContent.TileType<Tiles.Danger.JawTrap>();
                for (int i2 = i - num; i2 <= i + num; i2++) {
                    for (int j2 = j - num; j2 <= j + num; j2++) {
                        if (Main.tile[i2, j2].HasTile && Main.tile[i2, j2].TileType == type) {
                            flag = false;
                        }
                    }
                }
                bool flag3 = WorldGen.noTrapsWorldGen;
                int chance = 3;
                if (WorldGen.noTrapsWorldGen)
                    chance = ((!WorldGen.tenthAnniversaryWorldGen && !WorldGen.notTheBees) ? 1 : 2);
                else if (WorldGen.getGoodWorldGen)
                    chance = 2;

                if (/*(!tile.AnyWall() || (tile.AnyWall() && j < Main.worldSurface)) && */_random.NextBool(chance) && _random.NextBool(chance / 2) && flag && !MidInvalidTileTypesToKill.Contains(WorldGenHelper.GetTileSafely(i, j + 1).TileType)) {
                    int x = i, y = j;
                    bool flag2 = false;
                    if (WorldGen.SolidTile2(x, y + 1) && WorldGen.SolidTile2(x + 1, y + 1) && !Main.tile[x, y - 1].HasTile && !Main.tile[x + 1, y - 1].HasTile &&
                        ((!Main.tile[x, y].HasTile && !Main.tile[x + 1, y].HasTile) || (Main.tileCut[WorldGenHelper.GetTileSafely(x, y).TileType] && Main.tileCut[WorldGenHelper.GetTileSafely(x + 1, y).TileType])))
                        flag2 = true;

                    if (tile.WallType == WallID.LihzahrdBrickUnsafe || tile.WallType == WallID.LihzahrdBrick || 
                        MustSkipWallTypes.Contains(tile.WallType)) {
                        flag2 = false;
                    }

                    if (Main.tile[x, y].TileType == TileID.RollingCactus || Main.tile[x, y + 1].TileType == TileID.RollingCactus) {
                        flag2 = false;
                    }

                    if (flag2) {
                        Tile tile2 = Main.tile[x, y];
                        tile2.HasTile = true;
                        Main.tile[x, y].TileFrameY = 0;
                        Main.tile[x, y].TileFrameX = 0;
                        Main.tile[x, y].TileType = type;
                        tile2 = Main.tile[x + 1, y];
                        tile2.HasTile = true;
                        Main.tile[x + 1, y].TileFrameY = 0;
                        Main.tile[x + 1, y].TileFrameX = 20;
                        Main.tile[x + 1, y].TileType = type;

                        ModContent.GetInstance<Tiles.Danger.JawTrap.JawTrapTE>().Place(x, y);
                    }
                }
            }
        }
    }

    private void PlaceGateway(bool again = false) {
        int attempts = 100000;
        int yMin = Math.Min(BackwoodsVars.FirstTileYAtCenter + 20, (int)Main.worldSurface + 5);
        int yMax = Math.Max(BackwoodsVars.FirstTileYAtCenter + 20, (int)Main.worldSurface + 5);
        while (--attempts > 0) {
            int x = _random.Next(Left, Right);
            int y = _random.Next(yMin - 1, yMax + 1);
            int type = ModContent.TileType<NexusGateway>();
            if (again) {
                x = _gatewayLocation.X;
                y = _gatewayLocation.Y;
                if (Main.tile[x, y].TileType == type) {
                    break;
                }
                else {
                    x = _random.Next(Left, Right);
                    y = _random.Next(yMin - 1, yMax + 1);
                }
            }
            if (!TileObject.CanPlace(x, y, type, 0, 1, out var objectData)) {
                _gatewayLocation = Point.Zero;
                continue;
            }

            objectData.random = -1;
            if (TileObject.Place(objectData)) {
                for (int i = -10; i < 11; i++) {
                    for (int j = -20; j < 0; j++) {
                        if (Main.tile[x + i, y + j].TileType == TileID.Sand) {
                            Main.tile[x + i, y + j].TileType = TileID.Dirt;
                        }
                    }
                }
                _gatewayLocation = new Point(x, y);
                WorldGenHelper.ModifiedTileRunner(x + 3, y + 13, 20, 1, _dirtTileType, true, overRide: true, ignoreTileTypes: [type, _stoneTileType, _grassTileType], resetSlope: true);
                break;
            }
        }
    }

    private void Step_AddHerbs() {
        for (int i = 0; i < (int)((double)(Main.maxTilesX * Main.maxTilesY) * 0.0000175); i++) {
            int x = _random.Next(Left, Right),
                y = _random.Next(BackwoodsVars.FirstTileYAtCenter + EdgeY, Bottom);

            Tile tile = WorldGenHelper.GetTileSafely(x, y);
            byte spreadX = (byte)(_random.Next(15, 30) / 2),
                 spreadY = (byte)(_random.Next(15, 30) / 2);
            Dictionary<ushort, int> dictionary = [];
            ushort tileType = (ushort)ModContent.TileType<Herbs>();
            WorldUtils.Gen(new Point(x - spreadX, y - spreadY), new Shapes.Rectangle(spreadX * 2, spreadY * 2), new Actions.TileScanner(tileType).Output(dictionary));
            if (dictionary[tileType] > 0) {
                continue;
            }

            if (_nextHerb > 6) {
                _nextHerb = 0;
            }
            byte plant = (byte)_nextHerb;
            ushort[] validTiles = [TileID.Dirt, _grassTileType];
            if (validTiles.Contains(tile.TileType)) {
                for (int placeX = x - spreadX; placeX < x + spreadX; placeX++) {
                    for (int placeY = y - spreadY; placeY < y + spreadY; placeY++) {
                        tile = WorldGenHelper.GetTileSafely(placeX, placeY);
                        if (tile.WallType == _grassWallType || tile.WallType == _leavesWallType) {
                            continue;
                        }
                        if (tile.AnyLiquid()) {
                            continue;
                        }

                        Tile aboveTile = WorldGenHelper.GetTileSafely(placeX, placeY - 1);
                        if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] && validTiles.Contains(tile.TileType) && !aboveTile.HasTile && !WorldGenHelper.GetTileSafely(placeX, placeY - 2).HasTile) {
                            if (aboveTile.WallType == _grassWallType || tile.WallType == _leavesWallType) {
                                continue;
                            }
                            if (aboveTile.AnyLiquid()) {
                                continue;
                            }
                            tile.IsHalfBlock = false;
                            tile.Slope = 0;
                            aboveTile.ClearTile();
                            aboveTile.HasTile = true;
                            aboveTile.TileType = tileType;
                            aboveTile.TileFrameX = (short)(18 * plant);
                            aboveTile.TileFrameY = 0;
                        }
                    }
                }
            }
            bool flag = _nextHerb == 4;
            if (flag || _nextHerb == 6) {
                if (flag) {
                    if (_random.NextBool(y < CenterY + EdgeY ? 2 : 3)) {
                        _nextHerb += 2;
                    }
                }
                if (_random.NextBool() && !flag) {
                    _nextHerb++;
                }
                else {
                    if (!flag) {
                        _nextHerb += (byte)(_random.NextBool() ? 2 : 1);
                    }
                }
            }
            else {
                _nextHerb++;
            }
        }
    }

    private void Step_AddAltarMound() {
        int x = CenterX + 10;
        int posX = x;
        int y = WorldGenHelper.GetFirstTileY2(posX, true, true) + 2;
        if (y > BackwoodsVars.FirstTileYAtCenter) {
            y = BackwoodsVars.FirstTileYAtCenter;
        }
        y += 2;
        WorldUtils.Gen(new Point(posX, y), new Shapes.Mound(20, 8), Actions.Chain(new Modifiers.Blotches(2, 1, 2, 4, 0.8), new Actions.SetTile(_dirtTileType), new Actions.PlaceWall(_dirtWallType), new Actions.SetFrames(frameNeighbors: true)));
        WorldUtils.Gen(new Point(posX, y + 6), new Shapes.Mound(20, 8), Actions.Chain(new Modifiers.Blotches(2, 1, 2, 4, 0.8), new Actions.SetTile(_dirtTileType), new Actions.PlaceWall(_dirtWallType), new Actions.SetFrames(frameNeighbors: true)));
    }

    private void Step_AddWebs() {
        for (int num874 = 0; num874 < (int)((double)(Main.maxTilesX * Main.maxTilesY) * 0.000025); num874++) {
            int num875 = _random.Next(Left - 20, Right + 20);
            int num876 = _random.Next(BackwoodsVars.FirstTileYAtCenter + EdgeY, Bottom);

            if (!Main.tile[num875, num876].HasTile && ((double)num876 > Main.worldSurface || Main.tile[num875, num876].WallType > 0)) {
                while (!Main.tile[num875, num876].HasTile && num876 > (int)GenVars.worldSurfaceLow) {
                    num876--;
                }

                num876++;
                int num877 = 1;
                if (_random.Next(2) == 0)
                    num877 = -1;

                for (; !Main.tile[num875, num876].HasTile && num875 > 10 && num875 < Main.maxTilesX - 10; num875 += num877) {
                }

                num875 -= num877;
                if (((double)num876 > Main.worldSurface || Main.tile[num875, num876].WallType > 0) && (Main.tile[num875, num876].WallType != _grassWallType && Main.tile[num875, num876].WallType != _leavesWallType)) {
                    if (MustSkipWallTypes.Contains(Main.tile[num875, num876].WallType)) {
                        continue;
                    }
                    WorldGen.TileRunner(num875, num876, _random.Next(4, 11), _random.Next(2, 4), 51, addTile: true, num877, -1.0, noYChange: false, overRide: false);
                }
            }
        }
    }

    private void Step_AddChests() {
        // adapted vanilla
        for (int num536 = 0; num536 < (int)((double)Main.maxTilesX * 0.00085); num536++) {
            double value8 = (double)num536 / ((double)Main.maxTilesX * 0.005);
            bool flag30 = false;
            int num537 = 0;
            while (!flag30) {
                int num538 = _random.Next(Left - 50, Right + 50);
                int num539 = _random.Next(WorldGenHelper.SafeFloatingIslandY, (int)Main.worldSurface + 10);
                while (WorldGen.oceanDepths(num538, num539)) {
                    num538 = _random.Next(Left - 50, Right + 50);
                    num539 = _random.Next(WorldGenHelper.SafeFloatingIslandY, (int)Main.worldSurface + 10);
                }

                bool flag31 = false;
                bool flag32 = false;
                if (!Main.tile[num538, num539].HasTile) {
                    bool flag = Main.tile[num538, num539].WallType == _elderwoodWallType;
                    if (((/*Main.tile[num538, num539].WallType == _dirtWallType ||*/ (Main.tile[num538, num539].WallType == _leavesWallType && num539 > BackwoodsVars.FirstTileYAtCenter + 5) || Main.tile[num538, num539].WallType == _grassWallType || Main.tile[num538, num539].WallType == _flowerGrassWallType || Main.tile[num538, num539].WallType == WallID.GrassUnsafe || Main.tile[num538, num539].WallType == WallID.FlowerUnsafe) && _random.NextBool(10)) || flag)
                        flag31 = true;
                    if (flag) {
                        flag32 = true;
                    }
                }
                else {
                    int num540 = 50;
                    int num541 = num538;
                    int num542 = num539;
                    int num543 = 1;
                    for (int num544 = num541 - num540; num544 <= num541 + num540; num544 += 2) {
                        for (int num545 = num542 - num540; num545 <= num542 + num540; num545 += 2) {
                            bool flag = Main.tile[num544, num545].WallType == _elderwoodWallType || Main.tile[num544, num545].WallType == _elderwoodWallType2;
                            if ((double)num545 < BackwoodsVars.FirstTileYAtCenter + 30 && !Main.tile[num544, num545].HasActuator && flag) {
                                num543++;
                                flag31 = true;
                                num538 = num544;
                                num539 = num545;
                                if (flag) {
                                    flag32 = true;
                                }
                            }
                        }
                    }
                }

                int x = num538 + (flag32 ? _random.Next(1, 3) : 0);
                int y = num539;
                int num = 10;
                for (int i = x - num; i <= x + num; i++) {
                    for (int j = y - num; j <= y + num; j++) {
                        if (WorldGenHelper.GetTileSafely(i, j).HasTile && WorldGenHelper.GetTileSafely(i, j).TileType == 21) {
                            flag31 = false;
                        }
                    }
                }
                if (flag31 && WorldGen.AddBuriedChest(x, y, 0, notNearOtherChests: true, -1, trySlope: false, 0)) {
                    flag30 = true;
                }
                else {
                    num537++;
                    if (num537 >= 2000)
                        flag30 = true;
                }
            }
        }

        //for (int num536 = 0; num536 < (int)((double)Main.maxTilesX * 0.005); num536++) {
        //    double value8 = (double)num536 / ((double)Main.maxTilesX * 0.005);
        //    bool flag30 = false;
        //    int num537 = 0;
        //    while (!flag30) {
        //        int num538 = _random.Next(Left - 100, Right + 100);
        //        int num539 = _random.Next((int)Main.worldSurface + 10, Bottom + EdgeY * 2);
        //        while (WorldGen.oceanDepths(num538, num539)) {
        //            num538 = _random.Next(Left - 100, Right + 100);
        //            num539 = _random.Next((int)Main.worldSurface + 10, Bottom + EdgeY * 2);
        //        }

        //        bool flag31 = false;
        //        bool flag32 = false;
        //        if (Main.tile[num538, num539 + 1].TileType == _mossTileType) {
        //            if (_random.NextBool(6)) {
        //                flag31 = true;
        //            }
        //        }

        //        if (flag31 && WorldGen.AddBuriedChest(num538 + (flag32 ? _random.Next(1, 3) : 0), num539, 0, notNearOtherChests: true, -1, trySlope: false, 0)) {
        //            flag30 = true;
        //        }
        //        else {
        //            num537++;
        //            if (num537 >= 2000)
        //                flag30 = true;
        //        }
        //    }
        //}
    }

    private void Step_AddGrassWalls() {
        // adapted vanilla
        int y = Math.Min(CenterY - EdgeY, (int)Main.worldSurface + 10);
        if (CenterX > Main.maxTilesX / 2) {
            int count = 3 + 4 * (WorldGenHelper.WorldSize - 1);
            count += (int)(Main.worldSurface - BackwoodsVars.FirstTileYAtCenter) / 50;
            bool flag = false;
            for (int num298 = Left - 30; num298 < Right + 30; num298++) {
                for (int num299 = BackwoodsVars.FirstTileYAtCenter - 10; (double)num299 < y; num299++) {
                    if (num298 > Left - _random.Next(15, 35) && num298 < Right + _random.Next(15, 35)) {
                        if (_random.Next(count > count / 2 ? 2 : 3) == 0 || _random.Next(count > count / 2 ? 2 : 3) == 0 || flag) {
                            bool flag8 = false;
                            int num300 = -1;
                            int num301 = -1;
                            bool flag10 = false;
                            if (Main.tile[num298, num299].HasTile && Main.tile[num298, num299].TileType == _grassTileType && (Main.tile[num298, num299].WallType == _dirtWallType || Main.tile[num298, num299].WallType == _leavesWallType)) {
                                for (int num302 = num298 - 1; num302 <= num298 + 1; num302++) {
                                    for (int num303 = num299 - 1; num303 <= num299 + 1; num303++) {
                                        flag10 = !flag && _random.NextBool() && (Main.tile[num302, num303].WallType == _dirtWallType && ((_random.NextBool(5) || (count > count / 2 && _random.NextBool(4))) && count > 0));
                                        if ((Main.tile[num302, num303].WallType == 0 || flag10) && !WorldGen.SolidTile(num302, num303)) {
                                            flag8 = true;
                                            flag = false;
                                        }
                                    }
                                }

                                if (flag8) {
                                    for (int num304 = num298 - 2; num304 <= num298 + 2; num304++) {
                                        for (int num305 = num299 - 2; num305 <= num299 + 2; num305++) {
                                            if ((Main.tile[num304, num305].WallType == 2 || Main.tile[num304, num305].WallType == 59 || Main.tile[num304, num305].WallType == _dirtWallType) && !WorldGen.SolidTile(num304, num305)) {
                                                num300 = num304;
                                                num301 = num305;
                                            }
                                        }
                                    }
                                }
                            }

                            if (flag8 && num300 > -1 && num301 > -1 && WorldGen.countDirtTiles(num300, num301) < (flag10 ? 2000 : WorldGen.maxTileCount)) {
                                try {
                                    ushort wallType = 63;
                                    if (flag10) {
                                        count--;
                                    }
                                    //if (flag10 && Main.rand.NextBool()) {
                                    //    wallType = _dirtWallType;
                                    //}

                                    WorldGenHelper.CustomWall2(num300, num301, wallType, Left - 25, Right + 25, () => {
                                        WorldGenHelper.CustomSpreadGrass(num300, num301, TileID.Dirt, _grassTileType, growUnderground: true);
                                        WorldGenHelper.CustomSpreadGrass(num300, num301, _dirtTileType, _grassTileType, growUnderground: true);
                                    }, WallID.LivingWoodUnsafe, WallID.SandstoneBrick, WallID.SmoothSandstone, WallID.HardenedSand, WallID.Sandstone, WallID.GraniteUnsafe, WallID.MarbleUnsafe, WallID.Marble, WallID.Granite);
                                    //WorldGen.Spread.Wall2(num300, num301, wallType);
                                }
                                catch {
                                }
                            }
                        }
                    }
                }
            }

            for (int num306 = Left - 30; num306 < Right + 30; num306++) {
                for (int num307 = WorldGenHelper.SafeFloatingIslandY; (double)num307 < y - 1.0; num307++) {
                    if (num306 > Left - _random.Next(15, 35) && num306 < Right + _random.Next(15, 35)) {
                        if (Main.tile[num306, num307].WallType == 63 && _random.Next(10) == 0)
                            Main.tile[num306, num307].WallType = _grassWallType;

                        if (Main.tile[num306, num307].HasTile && (Main.tile[num306, num307].TileType == 0 || Main.tile[num306, num307].TileType == _dirtTileType)) {
                            bool flag9 = false;
                            for (int num308 = num306 - 2; num308 <= num306 + 2; num308++) {
                                for (int num309 = num307 - 2; num309 <= num307 + 2; num309++) {
                                    if (Main.tile[num308, num309].WallType == 63 || Main.tile[num308, num309].WallType == _grassWallType) {
                                        flag9 = true;
                                        break;
                                    }
                                }
                            }

                            if (flag9) {
                                WorldGen.grassSpread = 0;
                                WorldGenHelper.CustomSpreadGrass(num306, num307, TileID.Dirt, _grassTileType, growUnderground: true);
                                WorldGenHelper.CustomSpreadGrass(num306, num307, _dirtTileType, _grassTileType, growUnderground: true);
                            }
                        }
                    }
                }
            }

            for (int num306 = Left - 30; num306 < Right + 30; num306++) {
                for (int num307 = WorldGenHelper.SafeFloatingIslandY; (double)num307 < y - 1.0; num307++) {
                    if (num306 > Left - _random.Next(15, 35) && num306 < Right + _random.Next(15, 35)) {
                        if (Main.tile[num306, num307].HasTile && (Main.tile[num306, num307].TileType == 0 || Main.tile[num306, num307].TileType == _dirtTileType)) {
                            bool flag9 = false;
                            for (int num308 = num306 - 2; num308 <= num306 + 2; num308++) {
                                for (int num309 = num307 - 2; num309 <= num307 + 2; num309++) {
                                    if (Main.tile[num308, num309].WallType == 63 || Main.tile[num308, num309].WallType == _grassWallType) {
                                        flag9 = true;
                                        break;
                                    }
                                }
                            }
                            if (flag9) {
                                WorldGen.grassSpread = 0;
                                WorldGenHelper.CustomSpreadGrass(num306, num307, TileID.Dirt, _grassTileType, growUnderground: true);
                                WorldGenHelper.CustomSpreadGrass(num306, num307, _dirtTileType, _grassTileType, growUnderground: true);
                            }
                        }
                    }
                }
            }
        }
        else {
            int count = 3 + 4 * (WorldGenHelper.WorldSize - 1);
            count += (int)(Main.worldSurface - BackwoodsVars.FirstTileYAtCenter) / 50;
            bool flag = false;
            for (int num298 = Right + 30; num298 > Left - 30; num298--) {
                for (int num299 = BackwoodsVars.FirstTileYAtCenter - 10; (double)num299 < y; num299++) {
                    if (num298 < Right + _random.Next(15, 35) && num298 > Left - _random.Next(15, 35)) {
                        if (_random.Next(count > count / 2 ? 2 : 3) == 0 || _random.Next(count > count / 2 ? 2 : 3) == 0 || flag) {
                            bool flag8 = false;
                            int num300 = -1;
                            int num301 = -1;
                            bool flag10 = false;
                            if (Main.tile[num298, num299].HasTile && Main.tile[num298, num299].TileType == _grassTileType && (Main.tile[num298, num299].WallType == _dirtWallType || Main.tile[num298, num299].WallType == _leavesWallType)) {
                                for (int num302 = num298 - 1; num302 <= num298 + 1; num302++) {
                                    for (int num303 = num299 - 1; num303 <= num299 + 1; num303++) {
                                        flag10 = !flag && _random.NextBool() && (Main.tile[num302, num303].WallType == _dirtWallType && ((_random.NextBool(5) || (count > count / 2 && _random.NextBool(4))) && count > 0));
                                        if ((Main.tile[num302, num303].WallType == 0 || flag10) && !WorldGen.SolidTile(num302, num303)) {
                                            flag8 = true;
                                            flag = false;
                                        }
                                    }
                                }

                                if (flag8) {
                                    for (int num304 = num298 - 2; num304 <= num298 + 2; num304++) {
                                        for (int num305 = num299 - 2; num305 <= num299 + 2; num305++) {
                                            if ((Main.tile[num304, num305].WallType == 2 || Main.tile[num304, num305].WallType == 59 || Main.tile[num304, num305].WallType == _dirtWallType) && !WorldGen.SolidTile(num304, num305)) {
                                                num300 = num304;
                                                num301 = num305;
                                            }
                                        }
                                    }
                                }
                            }

                            if (flag8 && num300 > -1 && num301 > -1 && WorldGen.countDirtTiles(num300, num301) < (flag10 ? 2000 : WorldGen.maxTileCount)) {
                                try {
                                    ushort wallType = 63;
                                    if (flag10) {
                                        count--;
                                    }
                                    //if (flag10 && Main.rand.NextBool()) {
                                    //    wallType = _dirtWallType;
                                    //}

                                    WorldGenHelper.CustomWall2(num300, num301, wallType, Left - 25, Right + 25, () => {
                                        WorldGenHelper.CustomSpreadGrass(num300, num301, TileID.Dirt, _grassTileType, growUnderground: true);
                                        WorldGenHelper.CustomSpreadGrass(num300, num301, _dirtTileType, _grassTileType, growUnderground: true);
                                    }, WallID.LivingWoodUnsafe, WallID.SandstoneBrick, WallID.SmoothSandstone, WallID.HardenedSand, WallID.Sandstone, WallID.GraniteUnsafe, WallID.MarbleUnsafe, WallID.Marble, WallID.Granite);
                                    //WorldGen.Spread.Wall2(num300, num301, wallType);
                                }
                                catch {
                                }
                            }
                        }
                    }
                }
            }

            for (int num306 = Right + 30; num306 > Left - 30; num306--) {
                for (int num307 = WorldGenHelper.SafeFloatingIslandY; (double)num307 < y - 1.0; num307++) {
                    if (num306 < Right + _random.Next(15, 35) && num306 > Left - _random.Next(15, 35)) {
                        if (Main.tile[num306, num307].WallType == 63 && _random.Next(10) == 0)
                            Main.tile[num306, num307].WallType = _grassWallType;

                        if (Main.tile[num306, num307].HasTile && (Main.tile[num306, num307].TileType == 0 || Main.tile[num306, num307].TileType == _dirtTileType)) {
                            bool flag9 = false;
                            for (int num308 = num306 - 2; num308 <= num306 + 2; num308++) {
                                for (int num309 = num307 - 2; num309 <= num307 + 2; num309++) {
                                    if (Main.tile[num308, num309].WallType == 63 || Main.tile[num308, num309].WallType == _grassWallType) {
                                        flag9 = true;
                                        break;
                                    }
                                }
                            }

                            if (flag9) {
                                WorldGen.grassSpread = 0;
                                WorldGenHelper.CustomSpreadGrass(num306, num307, TileID.Dirt, _grassTileType, growUnderground: true);
                                WorldGenHelper.CustomSpreadGrass(num306, num307, _dirtTileType, _grassTileType, growUnderground: true);
                            }
                        }
                    }
                }
            }

            for (int num306 = Right + 30; num306 > Left - 30; num306--) {
                for (int num307 = WorldGenHelper.SafeFloatingIslandY; (double)num307 < y - 1.0; num307++) {
                    if (num306 < Right + _random.Next(15, 35) && num306 > Left - _random.Next(15, 35)) {
                        if (Main.tile[num306, num307].HasTile && (Main.tile[num306, num307].TileType == 0 || Main.tile[num306, num307].TileType == _dirtTileType)) {
                            bool flag9 = false;
                            for (int num308 = num306 - 2; num308 <= num306 + 2; num308++) {
                                for (int num309 = num307 - 2; num309 <= num307 + 2; num309++) {
                                    if (Main.tile[num308, num309].WallType == 63 || Main.tile[num308, num309].WallType == _grassWallType) {
                                        flag9 = true;
                                        break;
                                    }
                                }
                            }
                            if (flag9) {
                                WorldGen.grassSpread = 0;
                                WorldGenHelper.CustomSpreadGrass(num306, num307, TileID.Dirt, _grassTileType, growUnderground: true);
                                WorldGenHelper.CustomSpreadGrass(num306, num307, _dirtTileType, _grassTileType, growUnderground: true);
                            }
                        }
                    }
                }
            }
        }

        for (int num686 = 0; num686 < _biomeWidth; num686++) {
            int num687 = _random.Next(Left - 50, Right + 50);
            int num688 = _random.Next(BackwoodsVars.FirstTileYAtCenter - 10, (int)Main.worldSurface + 10);
            if (Main.tile[num687, num688].WallType == _grassWallType || Main.tile[num687, num688].WallType == 63)
                FlowerGrassRunner(num687, num688);
        }
    }

    private void Step_AddSpikes() {
        // adapted vanilla
        ushort stalactite = (ushort)ModContent.TileType<BackwoodsRockSpikes1>(),
               stalactiteSmall = (ushort)ModContent.TileType<BackwoodsRockSpikes3>(),
               stalagmite = (ushort)ModContent.TileType<BackwoodsRockSpikes2>(),
               stalagmiteSmall = (ushort)ModContent.TileType<BackwoodsRockSpikes4>();
        for (int x = Left - 100; x <= Right + 100; x++) {
            for (int y = (int)Main.worldSurface; y < Bottom + EdgeY * 2; y++) {
                // stalactite
                if (WorldGen.SolidTile(x, y - 1) && !Main.tile[x, y].HasTile && !Main.tile[x, y + 1].HasTile) {
                    if (Main.tile[x, y - 1].TileType == _mossTileType && _random.NextBool(5)) {
                        int variation = _random.Next(3);
                        int num3 = variation * 18;
                        bool preferSmall = _random.NextBool();
                        Tile tile = Main.tile[x, y];
                        tile.HasTile = true;
                        if (preferSmall) {
                            tile.TileType = stalactiteSmall;
                            tile.TileFrameX = (short)num3;
                            tile.TileFrameY = 0;
                        }
                        else {
                            tile.TileType = stalactite;
                            tile.TileFrameX = (short)num3;
                            tile.TileFrameY = 0;

                            tile = Main.tile[x, y + 1];
                            tile.HasTile = true;
                            tile.TileType = stalactite;
                            tile.TileFrameX = (short)num3;
                            tile.TileFrameY = 18;
                        }
                    }
                }

                // stalagmite
                if (WorldGen.SolidTile(x, y + 1) && !Main.tile[x, y].HasTile && !Main.tile[x, y - 1].HasTile) {
                    if (Main.tile[x, y + 1].TileType == _mossTileType && _random.NextBool(5)) {
                        int variation = _random.Next(3);
                        int num3 = variation * 18;
                        bool preferSmall = _random.NextBool();
                        Tile tile = Main.tile[x, y];
                        tile.HasTile = true;
                        if (preferSmall) {
                            tile.TileType = stalagmiteSmall;
                            tile.TileFrameX = (short)num3;
                            tile.TileFrameY = 0;
                        }
                        else {
                            tile.TileType = stalagmite;
                            tile.TileFrameX = (short)num3;
                            tile.TileFrameY = 18;

                            tile = Main.tile[x, y - 1];
                            tile.HasTile = true;
                            tile.TileType = stalagmite;
                            tile.TileFrameX = (short)num3;
                            tile.TileFrameY = 0;
                        }
                    }
                }
            }
        }
    }

    private void Step_AddPills() {
        // adapted vanilla
        for (int i = Left - 50; i <= Right + 50; i++) {
            for (int j = CenterY - EdgeY; j < Bottom + EdgeY * 2; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                bool flag2 = i > Right + 10 || i < Left - 10;
                bool elderwoodWall = false;
                for (int checkX = i - 1; checkX < i + 2; checkX++) {
                    for (int checkY = j - 1; checkY < j + 2; checkY++) {
                        if (Main.tile[checkX, checkY].WallType == _elderwoodWallType) {
                            elderwoodWall = true;
                        }
                    }
                }
                if (elderwoodWall) {
                    bool rootboundChest = false;
                    for (int checkX = i - 10; checkX < i + 11; checkX++) {
                        for (int checkY = j - 10; checkY < j + 11; checkY++) {
                            if (Main.tile[checkX, checkY].TileType == ModContent.TileType<ElderwoodChest2>()) {
                                rootboundChest = true;
                            }
                        }
                    }
                    if (!rootboundChest) {
                        elderwoodWall = false;
                    }
                }
                bool flag3 = (elderwoodWall && _random.NextChance(0.75)) || tile.WallType == _elderwoodWallType;
                if ((_random.NextBool(flag3 ? 3 : 7) || (flag2 && _random.NextChance(0.2))) && WorldGen.SolidTile2(tile)) {
                    if (_random.NextBool(flag3 ? 1 : tile.TileType == _elderwoodTileType ? 2 : 4) && ((!flag3 && _random.NextChance(0.85)) || flag3) && tile.TileType != ModContent.TileType<ElderwoodDoorClosed>() && !MidInvalidTileTypesToKill.Contains(tile.TileType)) {
                        bool flag = true;
                        if (WorldGenHelper.GetTileSafely(i, j).WallType == WallID.LihzahrdBrickUnsafe || WorldGenHelper.GetTileSafely(i, j).WallType == WallID.LihzahrdBrick ||
                            MustSkipWallTypes.Contains(WorldGenHelper.GetTileSafely(i, j).WallType)) {
                            flag = false;
                        }
                        if (Main.tile[i + 1, j + 1].TileType == TileID.RollingCactus) {
                            flag = false;
                        }
                        if (flag) {
                            WorldGenHelper.Place1x2Right(i + 1, j, (ushort)ModContent.TileType<BackwoodsRoots2_3>(), 24, _random.Next(3));
                        }
                    }
                }
            }
        }
        for (int i = Left - 50; i <= Right + 50; i++) {
            for (int j = CenterY - EdgeY; j < Bottom + EdgeY * 2; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                bool flag2 = i > Right + 10 || i < Left - 10;
                bool elderwoodWall = false;
                for (int checkX = i - 1; checkX < i + 2; checkX++) {
                    for (int checkY = j - 1; checkY < j + 2; checkY++) {
                        if (Main.tile[checkX, checkY].WallType == _elderwoodWallType) {
                            elderwoodWall = true;
                        }
                    }
                }
                if (elderwoodWall) {
                    bool rootboundChest = false;
                    for (int checkX = i - 10; checkX < i + 11; checkX++) {
                        for (int checkY = j - 10; checkY < j + 11; checkY++) {
                            if (Main.tile[checkX, checkY].TileType == ModContent.TileType<ElderwoodChest2>()) {
                                rootboundChest = true;
                            }
                        }
                    }
                    if (!rootboundChest) {
                        elderwoodWall = false;
                    }
                }
                bool flag3 = (elderwoodWall && _random.NextChance(0.75)) || tile.WallType == _elderwoodWallType;
                if ((_random.NextBool(flag3 ? 3 : 7) || (flag2 && _random.NextChance(0.2))) && WorldGen.SolidTile2(tile)) {
                    if (_random.NextBool(flag3 ? 1 : tile.TileType == _elderwoodTileType ? 2 : 4) && ((!flag3 && _random.NextChance(0.85)) || flag3) && tile.TileType != ModContent.TileType<ElderwoodDoorClosed>() && !MidInvalidTileTypesToKill.Contains(tile.TileType)) {
                        bool flag = true;
                        if (WorldGenHelper.GetTileSafely(i, j).WallType == WallID.LihzahrdBrickUnsafe || WorldGenHelper.GetTileSafely(i, j).WallType == WallID.LihzahrdBrick ||
                            MustSkipWallTypes.Contains(WorldGenHelper.GetTileSafely(i, j).WallType)) {
                            flag = false;
                        }
                        if (Main.tile[i - 1, j + 1].TileType == TileID.RollingCactus) {
                            flag = false;
                        }
                        if (flag) {
                            WorldGenHelper.Place1x2Left(i - 1, j, (ushort)ModContent.TileType<BackwoodsRoots2_4>(), 24, _random.Next(3));
                        }
                    }
                }
            }
        }
        for (int i = Left - 50; i <= Right + 50; i++) {
            for (int j = CenterY - EdgeY; j < Bottom + EdgeY * 2; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                bool flag2 = i > Right + 10 || i < Left - 10;
                bool elderwoodWall = false;
                for (int checkX = i - 1; checkX < i + 2; checkX++) {
                    for (int checkY = j - 1; checkY < j + 2; checkY++) {
                        if (Main.tile[checkX, checkY].WallType == _elderwoodWallType) {
                            elderwoodWall = true;
                        }
                    }
                }
                if (elderwoodWall) {
                    bool rootboundChest = false;
                    for (int checkX = i - 10; checkX < i + 11; checkX++) {
                        for (int checkY = j - 10; checkY < j + 11; checkY++) {
                            if (Main.tile[checkX, checkY].TileType == ModContent.TileType<ElderwoodChest2>()) {
                                rootboundChest = true;
                            }
                        }
                    }
                    if (!rootboundChest) {
                        elderwoodWall = false;
                    }
                }
                bool flag3 = (elderwoodWall && _random.NextChance(0.75)) || tile.WallType == _elderwoodWallType;
                if ((_random.NextBool(flag3 ? 3 : 7) || (flag2 && _random.NextChance(0.2))) && WorldGen.SolidTile2(tile)) {
                    if (_random.NextBool(flag3 ? 1 : tile.TileType == _elderwoodTileType ? 2 : 4) && ((!flag3 && _random.NextChance(0.85)) || flag3) && !MidInvalidTileTypesToKill.Contains(tile.TileType)) {
                        bool flag = true;
                        if (WorldGenHelper.GetTileSafely(i, j).WallType == WallID.LihzahrdBrickUnsafe || WorldGenHelper.GetTileSafely(i, j).WallType == WallID.LihzahrdBrick ||
                            MustSkipWallTypes.Contains(WorldGenHelper.GetTileSafely(i, j).WallType)) {
                            flag = false;
                        }
                        if (Main.tile[i, j - 1 + 1].TileType == TileID.RollingCactus) {
                            flag = false;
                        }
                        if (flag) {
                            WorldGenHelper.Place2x1(i, j - 1, (ushort)ModContent.TileType<BackwoodsRoots2>(), style: _random.Next(3));
                        }
                    }
                }
            }
        }
        for (int i = Left - 50; i <= Right + 50; i++) {
            for (int j = CenterY - EdgeY; j < Bottom + EdgeY * 2; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                bool flag2 = i > Right + 10 || i < Left - 10;
                bool elderwoodWall = false;
                for (int checkX = i - 1; checkX < i + 2; checkX++) {
                    for (int checkY = j - 1; checkY < j + 2; checkY++) {
                        if (Main.tile[checkX, checkY].WallType == _elderwoodWallType) {
                            elderwoodWall = true;
                        }
                    }
                }
                if (elderwoodWall) {
                    bool rootboundChest = false;
                    for (int checkX = i - 10; checkX < i + 11; checkX++) {
                        for (int checkY = j - 10; checkY < j + 11; checkY++) {
                            if (Main.tile[checkX, checkY].TileType == ModContent.TileType<ElderwoodChest2>()) {
                                rootboundChest = true;
                            }
                        }
                    }
                    if (!rootboundChest) {
                        elderwoodWall = false;
                    }
                }
                bool flag3 = (elderwoodWall && _random.NextChance(0.75)) || tile.WallType == _elderwoodWallType;
                if ((_random.NextBool(flag3 ? 3 : 7) || (flag2 && _random.NextChance(0.2))) && WorldGen.SolidTile2(tile)) {
                    if (_random.NextBool(flag3 ? 1 : tile.TileType == _elderwoodTileType ? 2 : 4) && ((!flag3 && _random.NextChance(0.85)) || flag3) && !MidInvalidTileTypesToKill.Contains(tile.TileType)) {
                        bool flag = true;
                        if (WorldGenHelper.GetTileSafely(i, j).WallType == WallID.LihzahrdBrickUnsafe || WorldGenHelper.GetTileSafely(i, j).WallType == WallID.LihzahrdBrick ||
                            MustSkipWallTypes.Contains(WorldGenHelper.GetTileSafely(i, j).WallType)) {
                            flag = false;
                        }
                        if (Main.tile[i, j + 1 + 1].TileType == TileID.RollingCactus) {
                            flag = false;
                        }
                        if (flag) {
                            WorldGenHelper.Place2x1Top(i, j + 1, (ushort)ModContent.TileType<BackwoodsRoots2_2>(), _random.Next(3));
                        }
                    }
                }
            }
        }

        // problem here
        for (int i = Left - 100; i <= Right + 100; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < Bottom + EdgeY * 2; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                Tile belowTile = WorldGenHelper.GetTileSafely(i, j + 1);
                if (tile.ActiveTile(_mossGrowthTileType) && WorldGen.SolidTile2(belowTile) && !MidInvalidTileTypesToKill.Contains(belowTile.TileType)) {
                    if (belowTile.TileType != TileID.RollingCactus && _random.NextBool(8)) {
                        WorldGen.PlaceTile(i, j + 1, _random.NextBool() ? (ushort)ModContent.TileType<BackwoodsRocks1>() : (ushort)ModContent.TileType<BackwoodsRocks2>(), true, style: _random.Next(3));
                    }
                }
            }
        }

        for (int i = Left - 50; i <= Right + 50; i++) {
            for (int j = CenterY - EdgeY; j < Bottom + EdgeY * 2; j++) {
                Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                bool flag2 = j < BackwoodsVars.FirstTileYAtCenter + 20 || i > Right + 10 || i < Left - 10;
                if ((_random.NextBool(4) || (flag2 && _random.NextChance(0.5))) && WorldGen.SolidTile2(tile) && !MidInvalidTileTypesToKill.Contains(tile.TileType)) {
                    if (_random.NextBool(4)) {
                        if (Main.tile[i, j - 1 + 1].TileType != TileID.RollingCactus) {
                            WorldGenHelper.Place3x2(i, j - 1, (ushort)ModContent.TileType<BackwoodsSpecial3>(), _random.Next(8));
                        }
                    }
                }
            }
        }

        for (int i = Left - 50; i <= Right + 50; i++) {
            for (int j = CenterY - EdgeY; j < Bottom + EdgeY * 2; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                bool flag2 = i > Right + 10 || i < Left - 10;
                if ((_random.NextBool(7) || (flag2 && _random.NextChance(0.2))) && WorldGen.SolidTile2(tile) && !MidInvalidTileTypesToKill.Contains(tile.TileType)) {
                    if (_random.NextBool(4)) {
                        if (Main.tile[i, j - 1 + 1].TileType != TileID.RollingCactus) {
                            WorldGen.Place2x1(i, j - 1, (ushort)ModContent.TileType<BackwoodsSpecial2>(), _random.Next(6));
                        }
                    }
                }
            }
        }
        for (int i = Left - 100; i <= Right + 100; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < Bottom + EdgeY * 2; j++) {
                Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(_mossTileType)) {
                    bool flag2 = j < BackwoodsVars.FirstTileYAtCenter + 20 || i > Right + 10 || i < Left - 10;
                    if ((_random.NextBool(4) || (flag2 && _random.NextChance(0.5))) && WorldGen.SolidTile2(tile) && !MidInvalidTileTypesToKill.Contains(tile.TileType)) {
                        if (Main.tile[i, j - 1 + 1].TileType != TileID.RollingCactus) {
                            WorldGenHelper.Place3x2(i, j - 1, (ushort)ModContent.TileType<BackwoodsRocks3x2>(), _random.Next(6));
                        }
                    }
                }
            }
        }
        for (int i = Left - 100; i <= Right + 100; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < Bottom + EdgeY * 2; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(_mossTileType)) {
                    bool flag2 = j < BackwoodsVars.FirstTileYAtCenter + 20 || i > Right + 10 || i < Left - 10;
                    if ((_random.NextBool(7) || (flag2 && _random.NextChance(0.2))) && WorldGen.SolidTile2(tile) && !MidInvalidTileTypesToKill.Contains(tile.TileType)) {
                        if (Main.tile[i, j - 1 + 1].TileType != TileID.RollingCactus) {
                            WorldGen.Place2x1(i, j - 1, (ushort)ModContent.TileType<BackwoodsRocks02>(), _random.Next(6));
                        }
                    }
                }
            }
        }
        for (int i = Left - 100; i <= Right + 100; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < CenterY + 20; j++) {
                Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (_random.NextBool(2) && WorldGen.SolidTile2(tile) && tile.ActiveTile(_mossTileType) && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock && !aboveTile.HasTile && !MidInvalidTileTypesToKill.Contains(tile.TileType)) {
                    if (Main.tile[i, j + 1].TileType != TileID.RollingCactus) {
                        tile = WorldGenHelper.GetTileSafely(i, j - 1);
                        tile.HasTile = true;
                        tile.TileFrameY = 0;
                        tile.TileType = _random.NextBool() ? (ushort)ModContent.TileType<BackwoodsRocks1>() : (ushort)ModContent.TileType<BackwoodsRocks2>();
                        tile.TileFrameX = (short)(18 * _random.Next(6));
                    }
                    break;
                }
            }
        }
    }

    private void Step_AddGems() {
        // adapted vanilla
        for (int num198 = 0; (double)num198 < (double)Main.maxTilesX * 0.25; num198++) {
            int num199 = _random.Next(CenterY - EdgeY / 2, GenVars.lavaLine);
            int num200 = _random.Next(Left - 25, Right + 25);
            if (Main.tile[num200, num199].HasTile && (Main.tile[num200, num199].TileType == _dirtTileType || Main.tile[num200, num199].TileType == _stoneTileType || Main.tile[num200, num199].TileType == TileID.Dirt)) {
                int num201 = _random.Next(1, 4);
                int num202 = _random.Next(1, 4);
                int num203 = _random.Next(1, 4);
                int num204 = _random.Next(1, 4);
                int num205 = _random.Next(12);
                int num206 = 0;
                num206 = ((num205 >= 3) ? ((num205 < 6) ? 1 : ((num205 < 8) ? 2 : ((num205 < 10) ? 3 : ((num205 >= 11) ? 5 : 4)))) : 0);
                for (int num207 = num200 - num201; num207 < num200 + num202; num207++) {
                    for (int num208 = num199 - num203; num208 < num199 + num204; num208++) {
                        if (!Main.tile[num207, num208].HasTile) {
                            bool flag = false;
                            for (int i2 = -1; i2 < 1; i2++) {
                                for (int j2 = -1; j2 < 1; j2++) {
                                    if (Math.Abs(i2) != Math.Abs(j2) &&
                                        TileID.Sets.Suffocate[WorldGenHelper.GetTileSafely(num207 + i2, num208 + j2).TileType]) {
                                        flag = true;
                                        break;
                                    }
                                }
                            }
                            if (!flag) {
                                WorldGen.PlaceTile(num207, num208, 178, mute: true, forced: false, -1, num206);
                            }
                        }
                    }
                }
            }
        }
    }

    private void Step8_AddCaves() {
        int tileCount = (int)(Main.maxTilesX * Main.maxTilesY * 0.2);
        int startX = Left - 25;
        int endX = Right + 25;
        int worldSize = Main.maxTilesX / 4200;
        int minY = BackwoodsVars.FirstTileYAtCenter + EdgeY / 2;
        int maxY = Bottom + EdgeY;
        int x;
        float modifier = 1.75f;
        int maxCaves = (int)(tileCount * 0.0001625f * modifier);
        for (int i = 0; i < maxCaves; i++) {
            x = _random.Next(startX - 100, endX + 100);
            int y = _random.Next(minY + 5, maxY + 5);
            if (_random.NextBool(5) && y > BackwoodsVars.FirstTileYAtCenter && y < BackwoodsVars.FirstTileYAtCenter + EdgeY / 2 && (WorldGenHelper.ActiveTile(x, y, _dirtTileType) || WorldGenHelper.ActiveTile(x, y, _stoneTileType))) {
                int type = -2;
                int sizeX = _random.Next(5, 15) / 2;
                int sizeY = _random.Next(30, 200) / 2;
                WorldGen.TileRunner(x, y, sizeX, sizeY, type);
            }
            if (_random.NextBool(5) && (WorldGenHelper.ActiveTile(x, y, _dirtTileType) || WorldGenHelper.ActiveTile(x, y, _stoneTileType))) {
                int type14 = -2;
                int num1026 = _random.Next(startX - 100, endX + 100);
                int num1027 = _random.Next(minY + 5, maxY + 5);
                int num1028 = _random.Next(2, 5);
                int num1029 = _random.Next(2, 20);
                WorldGen.TileRunner(num1026, num1027, num1028, num1029, type14);
            }
            if (WorldGenHelper.ActiveTile(x, y, _dirtTileType) || WorldGenHelper.ActiveTile(x, y, _stoneTileType)) {
                int type = -1;
                if (y < BackwoodsVars.FirstTileYAtCenter + 5) {
                    type = -2;
                }
                int sizeX = _random.Next(5, 15);
                int sizeY = _random.Next(30, 200);
                WorldGen.TileRunner(x, y, sizeX, sizeY, type);
            }
        }
    }


    private void Step8_2_AddCaves() {
        // adapted vanilla
        float modifier = 0.5f;
        int num992 = (int)((double)Main.maxTilesX * 0.002 * modifier);
        int num993 = (int)((double)Main.maxTilesX * 0.0007 * modifier);
        int num994 = (int)((double)Main.maxTilesX * 0.0003 * modifier);
        int minY = BackwoodsVars.FirstTileYAtCenter;
        int maxY = CenterY - EdgeY;
        for (int num995 = 0; num995 < num992; num995++) {
            int num996 = _random.Next(Left - 50, Right + 50);
            int attemps = 10;
            while (--attemps > 0 && ((double)num996 > (double)Main.maxTilesX * 0.45 && (double)num996 < (double)Main.maxTilesX * 0.55) || num996 < GenVars.leftBeachEnd + 20 || num996 > GenVars.rightBeachStart - 20) {
                num996 = _random.Next(Left - 50, Right + 50);
            }

            for (int num997 = minY; (double)num997 < maxY; num997++) {
                float progress = (float)(num997 - minY) / (maxY - minY);
                attemps = 10;
                while (--attemps > 0 && (((double)num996 > (double)CenterX - EdgeX / 2 && (double)num996 < (double)CenterX + EdgeX / 2) || ((double)num996 > (double)_leftTreeX - 10 && (double)num996 < (double)_leftTreeX + 10) || ((double)num996 > (double)_rightTreeX - 10 && (double)num996 < (double)_rightTreeX + 10)) && num997 < BackwoodsVars.FirstTileYAtCenter + EdgeY) {
                    num996 = _random.Next(Left - 50, Right + 50);
                }
                if (WorldGenHelper.ActiveTile(num996, num997, _dirtTileType) || WorldGenHelper.ActiveTile(num996, num997, _stoneTileType)) {
                    if (!_random.NextChance(progress * 1.25f)) {
                        continue;
                    }
                    WorldGen.TileRunner(num996, num997, _random.Next(3, 6), _random.Next(5, 50), -1, addTile: false, (double)_random.Next(-10, 11) * 0.1, 1.0);
                    break;
                }
            }
        }

        for (int num998 = 0; num998 < num993; num998++) {
            int num999 = _random.Next(Left - 50, Right + 50);
            int attemps = 10;
            while (--attemps > 0 && ((double)num999 > (double)Main.maxTilesX * 0.43 && (double)num999 < (double)Main.maxTilesX * 0.5700000000000001) || num999 < GenVars.leftBeachEnd + 20 || num999 > GenVars.rightBeachStart - 20) {
                num999 = _random.Next(Left - 50, Right + 50);
            }

            for (int num1000 = minY; (double)num1000 < maxY; num1000++) {
                float progress = (float)(num1000 - minY) / (maxY - minY);
                attemps = 10;
                while (--attemps > 0 && (((double)num999 > (double)CenterX - EdgeX / 2 && (double)num999 < (double)CenterX + EdgeX / 2) || ((double)num999 > (double)_leftTreeX - 10 && (double)num999 < (double)_leftTreeX + 10) || ((double)num999 > (double)_rightTreeX - 10 && (double)num999 < (double)_rightTreeX + 10)) && num1000 < BackwoodsVars.FirstTileYAtCenter + EdgeY) {
                    num999 = _random.Next(Left - 50, Right + 50);
                }
                if (WorldGenHelper.ActiveTile(num999, num1000, _dirtTileType) || WorldGenHelper.ActiveTile(num999, num1000, _stoneTileType)) {
                    if (!_random.NextChance(progress * 1.25f)) {
                        continue;
                    }
                    WorldGen.TileRunner(num999, num1000, _random.Next(10, 15), _random.Next(50, 130), -1, addTile: false, (double)_random.Next(-10, 11) * 0.1, 2.0);
                    break;
                }
            }
        }

        for (int num1001 = 0; num1001 < num994; num1001++) {
            int num1002 = _random.Next(Left - 50, Right + 50);
            int attemps = 10;
            while (--attemps > 0 && ((double)num1002 > (double)Main.maxTilesX * 0.4 && (double)num1002 < (double)Main.maxTilesX * 0.6) || num1002 < GenVars.leftBeachEnd + 20 || num1002 > GenVars.rightBeachStart - 20) {
                num1002 = _random.Next(Left - 50, Right + 50);
            }

            for (int num1003 = minY; (double)num1003 < maxY; num1003++) {
                float progress = (float)(num1003 - minY) / (maxY - minY);
                attemps = 10;
                while (--attemps > 0 && (((double)num1002 > (double)CenterX - EdgeX / 2 && (double)num1002 < (double)CenterX + EdgeX / 2) || ((double)num1002 > (double)_leftTreeX - 10 && (double)num1002 < (double)_leftTreeX + 10) || ((double)num1002 > (double)_rightTreeX - 10 && (double)num1002 < (double)_rightTreeX + 10)) && num1003 < BackwoodsVars.FirstTileYAtCenter + EdgeY) {
                    num1002 = _random.Next(Left - 50, Right + 50);
                }
                if (WorldGenHelper.ActiveTile(num1002, num1003, _dirtTileType) || WorldGenHelper.ActiveTile(num1002, num1003, _stoneTileType)) {
                    if (!_random.NextChance(progress * 1.25f)) {
                        continue;
                    }
                    WorldGen.TileRunner(num1002, num1003, _random.Next(12, 25), _random.Next(150, 500), -1, addTile: false, (double)_random.Next(-10, 11) * 0.1, 4.0);
                    WorldGen.TileRunner(num1002, num1003, _random.Next(8, 17), _random.Next(60, 200), -1, addTile: false, (double)_random.Next(-10, 11) * 0.1, 2.0);
                    WorldGen.TileRunner(num1002, num1003, _random.Next(5, 13), _random.Next(40, 170), -1, addTile: false, (double)_random.Next(-10, 11) * 0.1, 2.0);
                    break;
                }
            }
        }

        for (int num1004 = 0; num1004 < (int)((double)Main.maxTilesX * 0.0004); num1004++) {
            int num1005 = _random.Next(Left - 50, Right + 50);
            int attemps = 10;
            while (--attemps > 0 && ((double)num1005 > (double)Main.maxTilesX * 0.4 && (double)num1005 < (double)Main.maxTilesX * 0.6) || num1005 < GenVars.leftBeachEnd + 20 || num1005 > GenVars.rightBeachStart - 20) {
                num1005 = _random.Next(Left - 50, Right + 50);
            }

            for (int num1006 = minY; (double)num1006 < maxY; num1006++) {
                attemps = 10;
                while (--attemps > 0 && (((double)num1005 > (double)CenterX - EdgeX / 2 && (double)num1005 < (double)CenterX + EdgeX / 2) || ((double)num1005 > (double)_leftTreeX - 10 && (double)num1005 < (double)_leftTreeX + 10) || ((double)num1005 > (double)_rightTreeX - 10 && (double)num1005 < (double)_rightTreeX + 10)) && num1006 < BackwoodsVars.FirstTileYAtCenter + EdgeY) {
                    num1005 = _random.Next(Left - 50, Right + 50);
                }
                if (WorldGenHelper.ActiveTile(num1005, num1006, _dirtTileType) || WorldGenHelper.ActiveTile(num1005, num1006, _stoneTileType)) {
                    WorldGen.TileRunner(num1005, num1006, _random.Next(7, 12), _random.Next(150, 250), -1, addTile: false, 0.0, 1.0, noYChange: true);
                    break;
                }
            }
        }

        double num1007 = (double)Main.maxTilesX / 4200.0;
        for (int num1008 = 0; (double)num1008 < 6.0 * num1007; num1008++) {
            int num1009 = CenterY + EdgeY;
            int num1010 = Bottom;
            if (num1009 >= num1010)
                num1009 = num1010 - 1;

            WorldGen.Caverer(_random.Next(Left - 50, Right + 50), _random.Next(num1009, num1010));
        }
    }

    private void Step7_2_AddStone() {
        foreach (Point surface in _biomeSurface) {
            for (int j = 0; j < 3; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(surface.X, surface.Y + j);
                if (tile.ActiveTile(_stoneTileType)) {
                    tile.TileType = _dirtTileType;
                }
            }
        }
        foreach (Point surface in _biomeSurface) {
            Tile tile = WorldGenHelper.GetTileSafely(surface.X, surface.Y);
            if ((((double)surface.X > (double)CenterX - 10 && (double)surface.X < (double)CenterX + 10)) && surface.Y < BackwoodsVars.FirstTileYAtCenter + EdgeY) {
                continue;
            }
            if (_random.NextChance(0.014) && tile.ActiveTile(_dirtTileType)) {
                int sizeX = _random.Next(4, 9);
                int sizeY = _random.Next(20, 30) * 2;
                WorldGen.TileRunner(surface.X, surface.Y, sizeX, sizeY, _stoneTileType, speedY: 1f, speedX: 1f * _random.NextFloat(0f, 0.75f) * _random.NextBool().ToDirectionInt());
            }
        }
    }

    private void Step16_PlaceAltar() {
        int x = CenterX + _random.Next(4);
        int posX = x;

        int extraY = _random.Next(2);
        int y = WorldGenHelper.GetFirstTileY2(posX, true, true) + 1 - 2 - extraY;
        Platform(posX + 6, y);

        y = WorldGenHelper.GetFirstTileY2(posX - 6, true, true) - 1 - extraY;
        Spike(posX - 6, y, MathHelper.Pi + 1.3f + _random.NextFloat(-0.15f, 0.15f), 4, 3);

        y = WorldGenHelper.GetFirstTileY2(posX, true, true) - 2 - extraY;
        Spike(posX, y, MathHelper.Pi + 1.2f + _random.NextFloat(-0.15f, 0.15f));

        y = WorldGenHelper.GetFirstTileY2(posX + 14, true, true) - 2 - extraY;
        Spike(posX + 14, y, MathHelper.Pi + 0.5f + _random.NextFloat(-0.15f, 0.15f), 6);

        y = WorldGenHelper.GetFirstTileY2(posX + 21, true, true) - 1 - extraY;
        Spike(posX + 21, y, MathHelper.Pi + 0.9f, 4, 3);

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
        }
        Point checkedPos = _altarTiles.First();
        int tileX = checkedPos.X, tileY = checkedPos.Y;
        for (int i = tileX - 40; i < tileX + 40; i++) {
            for (int j = tileY - 40; j < tileY + 40; j++) {
                ushort treeBranch = (ushort)ModContent.TileType<TreeBranch>();
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(treeBranch) && !WorldGenHelper.GetTileSafely(i - 1, j).ActiveTile(TileID.Trees) && !WorldGenHelper.GetTileSafely(i + 1, j).ActiveTile(TileID.Trees)) {
                    WorldGen.KillTile(i, j);
                    if (WorldGenHelper.GetTileSafely(i, j - 2).WallType != _leavesWallType) {
                        //WallBush2(i, j - 2, false);
                    }
                    //if (_random.NextChance(0.35)) {
                    //    WorldGenHelper.ReplaceWall(i, j, _leavesWallType);
                    //}
                }
            }
        }
        index = 3;
        while (index-- > 0) {
            bool placed = false;
            foreach (Point pos in _altarTiles) {
                int i = pos.X, j = pos.Y;
                if (WorldGenHelper.ActiveTile(i, j, AltarPlaceholderTileType) &&
                    WorldGenHelper.ActiveTile(i + 1, j, AltarPlaceholderTileType)) {
                    WorldGenHelper.Place3x2(i + 1, j - 1, _altarTileType, onPlaced: () => {
                        placed = true;

                        for (int i2 = i - 3; i2 < i + 4; i2++) {
                            for (int j2 = j - 1; j2 < j + 2; j2++) {
                                if (WorldGenHelper.ActiveTile(i2, j2, TileID.Dirt) || WorldGenHelper.ActiveTile(i2, j2, _dirtTileType)) {
                                    Main.tile[i2, j2].TileType = _grassTileType;
                                    Main.tile[i2, j2].WallType = _grassWallType;
                                }
                            }
                        }

                        //for (int xSize = 0; xSize < 3; xSize++) {
                        //    for (int ySize = 0; ySize < 2; ySize++) {
                        //        ModContent.GetInstance<OvergrownAltarTE>().Place(i + xSize, j - 2 + ySize);
                        //    }
                        //}
                    });
                }

                if (placed) {
                    AltarHandler.SetPosition(new Point(i + 1, j - 2));

                    break;
                }
            }
        }
        for (int i = tileX - 20; i < tileX + 20; i++) {
            for (int j = tileY - 20; j < tileY + 20; j++) {
                Tile tile = Framing.GetTileSafely(i, j);
                if (WorldGenHelper.ActiveTile(i, j, AltarPlaceholderTileType)) {
                    WorldGenHelper.GetTileSafely(i, j).TileType = _grassTileType;
                    WorldGenHelper.GetTileSafely(i, j).WallType = _grassWallType;
                }
                if (WorldGenHelper.ActiveTile(i, j, AltarPlaceholderTileType2)) {
                    bool flag = _random.NextBool();
                    WorldGenHelper.GetTileSafely(i, j).TileType = flag ? TileID.Dirt : _grassTileType;
                    if (!flag) {
                        WorldGenHelper.GetTileSafely(i, j).WallType = _grassWallType;
                    }
                }
            }
        }

        for (int i = tileX - 20; i < tileX + 20; i++) {
            for (int j = tileY - 20; j < tileY + 20; j++) {
                if (WorldGenHelper.ActiveTile(i, j, TileID.Dirt) &&
                    (WorldGenHelper.ActiveTile(i, j + 1, _leavesTileType) || WorldGenHelper.ActiveTile(i, j + 1, _grassTileType)) &&
                    (WorldGenHelper.ActiveTile(i + 1, j, _grassTileType) || WorldGenHelper.ActiveTile(i + 1, j, _leavesTileType))) {
                    WorldGenHelper.ReplaceTile(i, j, _grassTileType);
                    WorldGenHelper.GetTileSafely(i, j).WallType = _grassWallType;
                }
                // pots
                if (WorldGen.SolidTile(i, j + 1) && !MidInvalidTileTypesToKill.Contains(WorldGenHelper.GetTileSafely(i, j).TileType) && !Main.tileCut[WorldGenHelper.GetTileSafely(i, j).TileType] && _random.NextBool(2)) {
                    if (MustSkipWallTypes.Contains(Main.tile[i, j].WallType)) {
                        continue;
                    }
                    if (Main.tile[i, j + 1].TileType != TileID.RollingCactus) {
                        WorldGen.PlacePot(i, j, _potTileType, _random.Next(4));
                    }
                }
            }
        }

        for (x = CenterX - 30; x < CenterX + 30; x++) {
            for (y = BackwoodsVars.FirstTileYAtCenter - 10; y < BackwoodsVars.FirstTileYAtCenter + 30; y++) {
                Tile tile = WorldGenHelper.GetTileSafely(x, y);
                if (tile.HasTile) {
                    if (tile.TileType == TileID.Trees && WorldGenHelper.GetTileSafely(x, y + 1).TileType == _elderwoodTileType) {
                        WorldGen.KillTile(x, y);
                    }
                }
            }
        }

        BackwoodsVars.BackwoodsTileForBackground = (ushort)WorldGenHelper.GetFirstTileY2(CenterX, skipWalls: true);
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
                if (i > x - 10 && !Main.tileSolid[tile.TileType]) {
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
                    if (_random.NextChance(0.5)) {
                        //WorldGenHelper.ReplaceWall(i, j, _leavesWallType);
                    }
                    else {
                        WorldGen.KillWall(i, j);
                    }
                }
            }
            for (int j = y + 1; j < y + 4; j++) {
                if (j != y + 1 || i != x && i != x + 1 && i != x + 2) {
                    if (j != y + 3 || j == y + 3 && i > x - 2 && i < x + 4) {
                        for (int i2 = -3; i2 < 4; i2++) {
                            for (int j2 = -1; j2 < 4; j2++) {
                                if (!(i + i2 > x - 2 && i + i2 < x + 4) && Main.tile[i + i2, j + j2].TileType != _grassTileType && Main.tile[i + i2, j + j2].TileType != _elderwoodTileType && Main.tile[i + i2, j + j2].HasTile) {
                                    //WorldGenHelper.ReplaceWall(i + i2, j + j2, _leavesWallType);
                                    if (_random.NextChance(0.5)) {
                                        WorldGenHelper.ReplaceTile(i + i2, j + j2, _leavesTileType);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for (int j = y + 1; j < y + 4; j++) {
                if (j != y + 1 || i != x && i != x + 1 && i != x + 2) {
                    if (j != y + 3 || j == y + 3 && i > x - 2 && i < x + 4) {
                        for (int i2 = -1; i2 < 1; i2++) {
                            for (int j2 = -1; j2 < 1; j2++) {
                                bool flag = Math.Abs(i2) == Math.Abs(j2);
                                if ((_random.NextChance(0.75) && !flag) || flag) {
                                    //WorldGenHelper.ReplaceWall(i, j, _leavesWallType);
                                    if (_random.NextChance(0.75)) {
                                        WorldGenHelper.ReplaceTile(i, j, _leavesTileType);
                                    }
                                    else {
                                        WorldGenHelper.ReplaceTile(i, j, _grassTileType);
                                        WorldGenHelper.GetTileSafely(i, j).WallType = _grassWallType;
                                    }
                                }
                                if (WorldGenHelper.ActiveTile(i + i2, j + j2, TileID.Dirt)) {
                                    WorldGenHelper.ReplaceTile(i + i2, j + j2, _grassTileType);
                                    WorldGenHelper.GetTileSafely(i + i2, j + j2).WallType = _grassWallType;
                                }
                            }
                        }
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

    private void Spike(int x, int y, float a, int distance = 8, int size = 4, int endingSize = 1, bool unkillableTile = false) {
        float i2 = x;
        float j2 = y;
        int startingSize = size;
        float angle = a;
        int type = unkillableTile ? _elderwoodTileType3 : _elderwoodTileType;
        for (int num3 = 0; num3 < distance; num3++) {
            float num4 = num3 / (float)distance;
            float num5 = MathHelper.Lerp(startingSize, endingSize, num4);
            float angle2 = angle;
            angle += (angle2 - (float)Math.PI / 2f) * 0.1f * (1f - num4);
            for (int i = 0; i < (int)Math.Max(1, num5); i++) {
                for (int j = 0; j < (int)Math.Max(1, num5); j++) {
                    int x3 = (int)i2 + i;
                    int y3 = (int)j2 + j;
                    WorldGenHelper.ReplaceTile(x3, y3, type);
                    if (Main.tile[x3 - 1, y3].TileType == type && Main.tile[x3 + 1, y3].TileType == type && Main.tile[x3, y3 - 1].TileType == type && Main.tile[x3, y3 + 1].TileType == type && Main.tile[x3 - 1, y3 - 1].TileType == type && Main.tile[x3 + 1, y3 - 1].TileType == type && Main.tile[x3 + 1, y3 + 1].TileType == type && Main.tile[x3 - 1, y3 + 1].TileType == type) {
                        WorldGenHelper.ReplaceWall(x3, y3, _elderwoodWallType);
                    }
                }
            }
            i2 += (float)Math.Cos(angle);
            j2 += (float)Math.Sin(angle);
        }

        for (int x2 = x - 15; x2 < x + 15; x2++) {
            bool flag2 = false;
            for (int y2 = y - 15; y2 < y + 15; y2++) {
                if (flag2) {
                    break;
                }
                if (Main.tile[x2, y2].ActiveTile(type)) {
                    int count = 8;
                    for (int i3 = -1; i3 < 2; i3++) {
                        for (int j3 = -1; j3 < 2; j3++) {
                            if (i3 != 0 || j3 != 0) {
                                if (!Main.tile[x2 + i3, y2 + j3].HasTile || Main.tile[x2 + i3, y2 + j3].TileType != type) {
                                    count--;
                                }
                            }
                        }
                    }
                    if (count == 1) {
                        for (int i3 = -1; i3 < 2; i3++) {
                            if (flag2) {
                                break;
                            }
                            for (int j3 = -1; j3 < 2; j3++) {
                                if (i3 != 0 || j3 != 0) {
                                    if (Main.tile[x2 + i3, y2 + j3].TileType == type) {
                                        bool flag = _random.NextBool();
                                        WorldGenHelper.ReplaceTile(x2 + i3 + (!flag ? (i3 > 0 ? -1 : 1) : 0), y2 + j3 - (flag ? 1 : 0), type);
                                        flag2 = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        for (int x2 = x - 10; x2 < x + 10; x2++) {
            bool flag2 = false;
            for (int y2 = y - 15; y2 < y - 5; y2++) {
                if (flag2) {
                    break;
                }
                if (Main.tile[x2, y2].ActiveTile(type)) {
                    int count = 0;
                    for (int i3 = 0; i3 < 2; i3++) {
                        for (int j3 = 0; j3 < 3; j3++) {
                            if (i3 != 0 || j3 != 0) {
                                if (Main.tile[x2 + i3, y2 + j3].ActiveTile(type)) {
                                    count++;
                                }
                            }
                        }
                    }
                    if (count == 5) {
                        bool flag3 = true;
                        bool flag4 = true;
                        if (Main.tile[x2 - 1, y2 + 2].HasTile && Main.tile[x2, y2 + 3].HasTile) {
                            flag3 = false;
                        }
                        if (Main.tile[x2 + 2, y2 + 2].HasTile && Main.tile[x2 + 1, y2 + 3].HasTile) {
                            flag4 = false;
                        }
                        if (Main.tile[x2, y2 - 1].HasTile || Main.tile[x2 + 1, y2 - 1].HasTile) {
                            flag3 = true;
                        }
                        if (Main.tile[x2, y2 - 1].HasTile || Main.tile[x2 - 1, y2 - 1].HasTile) {
                            flag4 = true;
                        }
                        if (!flag3 || !flag4) {
                            WorldGen.KillTile(x2 + (flag4 ? 1 : 0), y2);
                            return;
                        }
                    }
                }
            }
        }
    }

    public void BackwoodsLootRooms(GenerationProgress progress, GameConfiguration configuration) {
        progress.Message = Language.GetOrRegister("Mods.RoA.WorldGen.Backwoods3").Value;

        for (int i = Left - 25; i < Right + 25; i++) {
            for (int j = Top - 15; j < Bottom + 15; j++) {
                if (WorldGenHelper.ActiveTile(i, j, 138)) {
                    int j2 = 0;
                    bool flag = false;
                    while (!flag && j2 < 50) {
                        j2++;
                        for (int i2 = 0; i2 < 2; i2++) {
                            if (WorldGenHelper.ActiveTile(i + i2, j + j2, 135)) {
                                flag = true;
                                break;
                            }
                            if (!WorldGenHelper.ActiveTile(i + i2, j + j2, 138) && !WorldGenHelper.ActiveTile(i + i2, j + j2, 130) && !WorldGenHelper.ActiveTile(i + i2, j + j2, TileID.InactiveStoneBlock) && !WorldGenHelper.ActiveTile(i + i2, j + j2, TileID.Stone)) {
                                WorldGen.KillTile(i + i2, j + j2);
                            }
                        }
                    }
                }
            }
        }

        int roomCount = 5 * WorldGenHelper.WorldSize;
        for (int i = 0; i < roomCount; i++) {
            progress.Set((float)(i + 1) / roomCount);

            GenerateLootRoom2();
        }

        //PlaceGateway();

        Step17_AddStatues();
    }

    private void WallBush_Moss(int i, int j, bool ignoreWalls = true) {
        float progressX = 1f;
        if (i < Left) {
            progressX *= (1f - MathF.Abs(((float)i - Left) / 50f));
        }
        if (i > Right) {
            progressX *= (((float)i - Right) / 50f);
        }
        progressX = MathUtils.Clamp01(progressX);
        int sizeX = (int)(_random.Next(8, 14) * progressX);
        int sizeY = _random.Next(2, 5);
        int sizeY2 = sizeY;
        ushort tealMossWallType = (ushort)ModContent.WallType<TealMossWall2>();
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
                    double max = (double)sizeX * 0.5 * (1.0 + _random.Next(-5, 8) * 0.025);
                    for (int x3 = x - 1; x3 < x + 1; x3++) {
                        for (int y3 = y - 1; y3 < y + 1; y3++) {
                            if (ignoreWalls && (WorldGenHelper.ActiveTile(x3, y3, tealMossWallType) || WorldGenHelper.GetTileSafely(x3, y3).WallType == _elderwoodWallType)) {
                                return;
                            }
                        }
                    }
                    if (min < max && WorldGenHelper.GetTileSafely(x, y).WallType != tealMossWallType && (WorldGenHelper.GetTileSafely(x, y).WallType != _elderwoodWallType && (!WorldGen.SolidTile2(x, y) || WorldGenHelper.ActiveTile(x, y, _mossTileType)))) {
                        WorldGenHelper.ReplaceWall(x, y, tealMossWallType);
                    }
                }
            }
        }
    }

    private void WallBush(int i, int j, bool ignoreWalls = true) {
        float progressX = 1f;
        if (i < Left) {
            progressX *= (1f - MathF.Abs(((float)i - Left) / 100f));
        }
        if (i > Right) {
            progressX *= (1f - ((float)i - Right) / 100f);
        }
        progressX = MathUtils.Clamp01(progressX);
        int sizeX = (int)(_random.Next(8, 17) * progressX);
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
                    //if (i > CenterX - 10 && i < CenterX + 22) {
                    //    continue;
                    //}
                    double min = Math.Abs((double)(x - i)) + Math.Abs((double)(y - j));
                    double max = (double)sizeX * 0.5 * (1.0 + _random.Next(-5, 10) * 0.025);
                    for (int x3 = x - 1; x3 < x + 1; x3++) {
                        for (int y3 = y - 1; y3 < y + 1; y3++) {
                            if (ignoreWalls && (WorldGenHelper.ActiveTile(x3, y3, _leavesTileType) || WorldGenHelper.GetTileSafely(x3, y3).WallType == _elderwoodWallType)) {
                                return;
                            }
                        }
                    }
                    if (min < max && WorldGenHelper.GetTileSafely(x, y).WallType != _leavesWallType && (WorldGenHelper.GetTileSafely(x, y).WallType != _elderwoodWallType && (!WorldGen.SolidTile2(x, y) || WorldGenHelper.ActiveTile(x, y, _grassTileType)))) {
                        WorldGenHelper.ReplaceWall(x, y, _leavesWallType);
                    }
                }
            }
        }
    }

    private void WallBush2(int i, int j, bool ignoreWalls = true) {
        int sizeX = 5;
        int sizeY = 3;
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
                    double max = (double)sizeX * 0.5 * (1.0 + 3 * 0.025);
                    //for (int x3 = x - 1; x3 < x + 1; x3++) {
                    //    for (int y3 = y - 1; y3 < y + 1; y3++) {
                    //        if (ignoreWalls && (WorldGenHelper.ActiveTile(x3, y3, _leavesTileType) || WorldGenHelper.GetTileSafely(x3, y3).WallType == _elderwoodWallType)) {
                    //            return;
                    //        }
                    //    }
                    //}
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
        int num1050 = 0;
        //int y = CenterY - EdgeY / 2;
        double y = Math.Max(CenterY - EdgeY, Main.worldSurface);
        int maxLeft = Left - 50;
        int maxRight = Right + 50;
        ushort[] invalidWalls = [(ushort)ModContent.WallType<TealMossWall2>(), (ushort)ModContent.WallType<BackwoodsRootWall2>(), WallID.JungleUnsafe1, WallID.JungleUnsafe2, WallID.JungleUnsafe3, WallID.JungleUnsafe4, WallID.LihzahrdBrickUnsafe, 59, WallID.CaveUnsafe, WallID.Cave2Unsafe, WallID.Cave3Unsafe, WallID.Cave4Unsafe, WallID.Cave5Unsafe, WallID.Cave7Unsafe, WallID.CaveWall, WallID.CaveWall2];
        ushort[] invalidWalls2 = [23, 24, 42, 45, 10, 179, 181, 196, 197, 198, 199, 212, 213, 214, 215, 208, 209, 210, 211];
        for (int num1048 = maxLeft; num1048 < maxRight; num1048++) {
            num1047 += _random.Next(-1, 2);
            if (num1047 < 0)
                num1047 = 0;
            if (num1047 > 10)
                num1047 = 10;
            for (int num1049 = BackwoodsVars.FirstTileYAtCenter - 10; num1049 < Bottom + EdgeY + num1047; num1049++) {
                if (!(num1049 < y + 10.0 && !((double)num1049 > y + (double)num1047))) {
                    if (num1048 > maxLeft + _random.NextFloat() * 15 && num1048 < maxRight - _random.NextFloat() * 15) {
                        if (!invalidWalls2.Contains(Main.tile[num1048, num1049].WallType) && !invalidWalls.Contains(Main.tile[num1048, num1049].WallType) && !MidInvalidWallTypesToKill.Contains(Main.tile[num1048, num1049].WallType) && !SkipBiomeInvalidWallTypeToKill.Contains(Main.tile[num1048, num1049].WallType) && !MidMustSkipWallTypes.Contains(Main.tile[num1048, num1049].WallType) && ((Main.tile[num1048, num1049].WallType != _grassWallType && Main.tile[num1048, num1049].WallType != _leavesWallType) || num1049 > y + (double)num1047) && Main.tile[num1048, num1049].WallType != _elderwoodWallType) {
                            if (!SkipBiomeInvalidWallTypeToKill.Contains(Main.tile[num1048, num1049].WallType) && !Main.wallDungeon[Main.tile[num1048, num1049].WallType]) {
                                Main.tile[num1048, num1049].WallType = WallID.None;
                            }
                        }
                    }
                }
            }
        }
        //for (int num1048 = maxLeft; num1048 < maxRight; num1048++) {
        //    num1047 += _random.Next(-1, 2);
        //    if (num1047 < 0)
        //        num1047 = 0;
        //    if (num1047 > 10)
        //        num1047 = 10;
        //    int min = (int)(y - EdgeY);
        //    for (int num1049 = min + num1047; num1049 < y + num1047; num1049++) {
        //        if (num1048 > maxLeft + _random.NextFloat() * 15 && num1048 < maxRight - _random.NextFloat() * 15) {
        //            if (Main.tile[num1048, num1049].WallType == _grassWallType || Main.tile[num1048, num1049].WallType == _flowerGrassWallType) {
        //                Main.tile[num1048, num1049].WallType = (ushort)(_random.NextBool(5) ? 59 : _dirtWallType);
        //            }
        //        }
        //    }
        //}
    }

    private void Step13_GrowBigTrees() {
        int left = _toLeft ? (_lastCliffX != 0 ? _lastCliffX + 5 : Left) : Left;
        _leftTreeX = _random.Next(left + 15, left + 30);
        while (_leftTreeX > CenterX - 10) {
            _leftTreeX--;
        }
        GrowBigTree(_leftTreeX);
        int right = !_toLeft ? (_lastCliffX != 0 ? _lastCliffX - 5 : Right) : Right;
        _rightTreeX = _random.Next(right - 30, right - 15);
        while (_rightTreeX < CenterX + 17) {
            _rightTreeX++;
        }
        GrowBigTree(Math.Max(_rightTreeX, CenterX + 40), false);
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

    private void GrowBigTree(int i, bool isLeft = true) {
        int j = WorldGenHelper.GetFirstTileY(i, true);
        j += 2;
        int attempts = EdgeX - EdgeX / 3;
        while (--attempts > 0 && (!WorldGenHelper.GetTileSafely(i, j + 1).HasTile || j > _biomeSurface.FirstOrDefault(x => x.X == i).Y + 3)) {
            if (i <= CenterX + 25 && i >= CenterX - 25) {
                break;
            }
            i += isLeft ? 1 : -1;
            j = WorldGenHelper.GetFirstTileY(i, true);
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
            int attempts = 10;
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
                    WorldUtils.Gen(new Point(i + (directedRight ? 2 : -2), y), new BigTreeBranchRoot((directedRight ? ((float)Math.PI - 2.8f) : 2.8f) + _random.NextFloat(!directedRight ? -0.1075f : -0.1225f, !directedRight ? 0.06f : 0.07f), _leavesTileType, _leavesWallType, _random.Next(7, 11), 3, 1), Actions.Chain(new SetTileAndWall(_elderwoodTileType, _elderwoodWallType), new Modifiers.SkipTiles(_leavesTileType), new Modifiers.SkipWalls(_leavesWallType)));
                    index--;
                    takenYs.Add((y - 4, y + 3));
                }
            }
        }
        if (_random.Next(10) <= 9)
            placeBranch(false);
        if (_random.Next(10) <= 9)
            placeBranch();

        //// walls
        //for (int i2 = -25; i2 < 25; i2++) {
        //    for (int j2 = -(height + 10); j2 < 5; j2++) {
        //        int x2 = i + i2;
        //        int y2 = j + j2;
        //        Tile tile = WorldGenHelper.GetTileSafely(x2, y2);
        //        if (Main.tile[x2 - 1, y2].HasTile && Main.tile[x2 + 1, y2].HasTile && Main.tile[x2, y2 - 1].HasTile && Main.tile[x2, y2 + 1].HasTile && Main.tile[x2 - 1, y2 - 1].HasTile && Main.tile[x2 + 1, y2 - 1].HasTile && Main.tile[x2 + 1, y2 + 1].HasTile && Main.tile[x2 - 1, y2 + 1].HasTile) {
        //            //if (tile.ActiveTile(_leavesTileType)) {
        //            //    WorldGenHelper.ReplaceWall(x2, y2, _leavesWallType);
        //            //}
        //            //if (tile.ActiveTile(_elderwoodTileType)) {
        //            //    WorldGenHelper.ReplaceWall(x2, y2, _elderwoodWallType);
        //            //}
        //        }
        //    }
        //}
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
                //if (tile.WallType == _dirtWallType) {
                //    tile.WallType = WallID.None;
                //}
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
                if (j2 == -3 && (i2 == 0 + offset || i2 == 1 + offset || (increasedWidth && i2 == 2 + offset))) {
                    WorldGen.KillTile(startX + i2, startY + j2 - 1);
                }
                if (i2 > -2 && i2 < 2) {
                    WorldGenHelper.ReplaceWall(startX + i2, startY + j2, _elderwoodWallType);
                }
            }
        }

        bool flag5 = false;
        if ((_random.NextBool(8) && _oneChestPlacedInBigTree) || !_oneChestPlacedInBigTree) {
            for (int i2 = -2; i2 <= 2; i2++) {
                if (flag5) {
                    break;
                }
                for (int j2 = -3; j2 <= 0; j2++) {
                    if (i2 > -2 && i2 < 2) {
                        WorldGen.AddBuriedChest(startX + i2 + _random.Next(1, 3), startY + j2, 0, notNearOtherChests: true, -1, trySlope: false, 0);
                        flag5 = true;
                        _oneChestPlacedInBigTree = true;
                        break;
                    }
                }
            }
        }

        // fallen trees
        for (int k = 0; k < 1; k++) {
            bool flag = false;
            for (int i2 = i - 15; i2 < i; i2++) {
                int j2 = WorldGenHelper.GetFirstTileY(i2, type: _grassTileType);
                WorldGenHelper.Place3x2(i2, j2 - 1, _fallenTreeTileType, styleX: _random.Next(2), onPlaced: () => {
                    flag = true;
                });
                if (flag) {
                    break;
                }
            }
            flag = false;
            for (int i2 = i + 15; i2 > i; i2--) {
                int j2 = WorldGenHelper.GetFirstTileY(i2, type: _grassTileType);
                WorldGenHelper.Place3x2(i2, j2 - 1, _fallenTreeTileType, styleX: _random.Next(2), onPlaced: () => {
                    flag = true;
                });
                if (flag) {
                    break;
                }
            }
        }

        // pots
        for (int i2 = -25; i2 < 25; i2++) {
            for (int j2 = -(height + 10); j2 < 5; j2++) {
                int x2 = i + i2;
                int y2 = j + j2;
                ushort tileType = WorldGenHelper.GetTileSafely(x2, y2 + 1).TileType;
                if (WorldGen.SolidTile(x2, y2 + 1) && tileType == _elderwoodTileType && _random.NextBool(3)) {
                    if (MustSkipWallTypes.Contains(Main.tile[x2, y2].WallType)) {
                        continue;
                    }
                    if (Main.tile[x2, y2 + 1].TileType != TileID.RollingCactus) {
                        WorldGen.PlacePot(x2, y2, _potTileType, _random.Next(4));
                    }
                }
            }
        }

        for (int sizeX = -_random.Next(2, 4) - 2; sizeX <= _random.Next(2, 4) + 3; sizeX++) {
            if (sizeX < -1) {
                continue;
            }

            int y2 = j - height + height / 3 + 1;
            WorldUtils.Gen(new Point(i + 1, y2), new ShapeRoot(sizeX / 3f * 2f + 0.57075f, (int)(_random.Next(12, 18) * 0.6), 2, 1), new SetTileAndWall(_elderwoodTileType, _elderwoodWallType));
        }

    }

    private void Step12_AddRoots() {
        int minY = BackwoodsVars.FirstTileYAtCenter + _biomeHeight / 10;
        for (int i = Left - 25; i < Right + 25; i++) {
            for (int j = minY; j < Bottom + EdgeY; j++) {
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
            for (int j = Top - 10; j < CenterY - EdgeY / 2; j++) {
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
        for (int i = Left - 50; i < Right + 50; i++) {
            for (int j = CenterY - EdgeY; j < Bottom + EdgeY; j++) {
                bool elderwoodWall = false;
                for (int checkX = i - 1; checkX < i + 2; checkX++) {
                    for (int checkY = j - 1; checkY < j + 2; checkY++) {
                        if (Main.tile[checkX, checkY].WallType == _elderwoodWallType) {
                            elderwoodWall = true;
                        }
                    }
                }
                bool flag2 = false;
                if (elderwoodWall) {
                    bool rootboundChest = false;
                    for (int checkX = i - 10; checkX < i + 11; checkX++) {
                        for (int checkY = j - 10; checkY < j + 11; checkY++) {
                            if (Main.tile[checkX, checkY].TileType == ModContent.TileType<ElderwoodChest2>()) {
                                rootboundChest = true;
                            }
                        }
                    }
                    if (!rootboundChest) {
                        elderwoodWall = false;
                    }
                    else if (_random.NextChance(0.625)) {
                        elderwoodWall = false;
                        flag2 = true;
                    }
                }
                if (!flag2) {
                    elderwoodWall |= WorldGenHelper.GetTileSafely(i, j).WallType == _elderwoodWallType;
                }
                if (WorldGen.SolidTile(i, j, true) && WorldGenHelper.GetTileSafely(i, j).TileType != ModContent.TileType<ElderwoodDoorClosed>() && WorldGenHelper.GetTileSafely(i, j).LiquidAmount <= 0 && WorldGenHelper.GetTileSafely(i, j).Slope == 0 && !WorldGenHelper.GetTileSafely(i, j).IsHalfBlock &&
                    _random.NextBool(WorldGenHelper.GetTileSafely(i, j).TileType == _mossTileType || WorldGenHelper.GetTileSafely(i, j).TileType == _stoneTileType ? 14 : WorldGenHelper.GetTileSafely(i, j).TileType == _elderwoodTileType ?
                    elderwoodWall ? 1 : 8 : 10)) {
                    if (_random.NextChance(elderwoodWall ? 1 : 0.65)) {
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
                                bool flag = true;
                                if (Main.tile[i2, j2].WallType == WallID.LihzahrdBrickUnsafe || Main.tile[i2, j2].WallType == WallID.LihzahrdBrick ||
                                    MustSkipWallTypes.Contains(WorldGenHelper.GetTileSafely(i2, j2).WallType)) {
                                    flag = false;
                                }
                                if (flag) {
                                    SpreadMossGrass2(i2, j2);
                                }
                            }
                        }
                    }
                }
            }
        }
        for (int i = Left - 100; i < Right + 100; i++) {
            for (int j = Top - 10; j < Bottom + EdgeY; j++) {
                if (WorldGenHelper.ActiveTile(i, j, _mossTileType) && WorldGenHelper.GetTileSafely(i, j).Slope == 0 && !WorldGenHelper.GetTileSafely(i, j).IsHalfBlock) {
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

    private void GenerateLootRoom1(int posX = 0, int posY = 0) {
        //GetRandomPosition(posX, posY, out int baseX, out int baseY, false);

        int baseX = _random.Next(Left + 15, Right - 15);
        int y = Math.Min(CenterY - EdgeY, (int)Main.worldSurface + 10);
        int baseY = _random.Next(y + (int)(EdgeY * 2f * _random.NextFloat()), Bottom - 20);
        Point origin = new(baseX, baseY);

        ushort[] skipTileTypes = [_dirtTileType, TileID.Dirt, _stoneTileType, _mossTileType];
        while (true) {
            int attempts = 1000;
            Tile tile = WorldGenHelper.GetTileSafely(baseX, baseY);
            ushort tileType = tile.TileType;
            while (GatewayNearby(baseX, baseY) || !skipTileTypes.Contains(tileType) || SandTileTypes.Contains(tileType) || SkipBiomeInvalidTileTypeToKill.Contains(tileType) || SkipBiomeInvalidWallTypeToKill.Contains(tile.WallType) || MidInvalidTileTypesToKill.Contains(tileType) || MidInvalidWallTypesToKill.Contains(tile.WallType) || WorldGenHelper.GetTileSafely(baseX, baseY).AnyWall()) {
                baseX = _random.Next(Left + 15, Right - 15);
                baseY = _random.Next(y + (int)(EdgeY * 2f * _random.NextFloat()), Bottom - 20);
                if (attempts-- <= 0) {
                    break;
                }
            }
            bool flag = false;
            int check = 30;
            attempts = 50;
            for (int x2 = baseX - check; x2 < baseX + check; x2++) {
                for (int y2 = baseY - check; y2 < baseY + check; y2++) {
                    if (WorldGenHelper.ActiveTile(x2, y2, _elderWoodChestTileType)) {
                        flag = true;
                        break;
                    }
                }
                if (flag) {
                    baseX = _random.Next(Left + 15, Right - 15);
                    baseY = _random.Next(y + (int)(EdgeY * 2f * _random.NextFloat()), Bottom - 20);
                    break;
                }
            }
            if (flag) {
                if (attempts-- <= 0) {
                    break;
                }
                continue;
            }
            else {
                break;
            }
        }

        int num = 40;
        for (int i = origin.X - num; i <= origin.X + num; i++) {
            for (int j = origin.Y - num; j <= origin.Y + num; j++) {
                if (WorldGenHelper.GetTileSafely(i, j).HasTile && WorldGenHelper.GetTileSafely(i, j).TileType == 21)
                    return;
            }
        }
        for (int i = origin.X - num; i <= origin.X + num; i++) {
            for (int j = origin.Y - num; j <= origin.Y + num; j++) {
                if (WorldGenHelper.GetTileSafely(i, j).HasTile && WorldGenHelper.GetTileSafely(i, j).TileType != 21 && TileID.Sets.BasicChest[WorldGenHelper.GetTileSafely(i, j).TileType])
                    return;
            }
        }
        for (int i = origin.X - num; i <= origin.X + num; i++) {
            for (int j = origin.Y - num; j <= origin.Y + num; j++) {
                if (WorldGenHelper.GetTileSafely(i, j).HasTile && TileSets.Paintings.Contains(WorldGenHelper.GetTileSafely(i, j).TileType))
                    return;
            }
        }
        for (int i = origin.X - num; i <= origin.X + num; i++) {
            for (int j = origin.Y - num; j <= origin.Y + num; j++) {
                if (WorldGenHelper.GetTileSafely(i, j).HasTile && TileID.Sets.Platforms[WorldGenHelper.GetTileSafely(i, j).TileType])
                    return;
            }
        }
        for (int i = origin.X - num; i <= origin.X + num; i++) {
            for (int j = origin.Y - num; j <= origin.Y + num; j++) {
                if (WorldGenHelper.GetTileSafely(i, j).HasTile && WorldGenHelper.GetTileSafely(i, j).TileType == TileID.Cobweb)
                    return;
            }
        }
        for (int i = origin.X - num; i <= origin.X + num; i++) {
            for (int j = origin.Y - num; j <= origin.Y + num; j++) {
                if (WorldGenHelper.GetTileSafely(i, j).HasTile && WorldGenHelper.GetTileSafely(i, j).TileType == TileID.LihzahrdBrick)
                    return;
                if (WorldGenHelper.GetTileSafely(i, j).WallType == WallID.LihzahrdBrickUnsafe || MustSkipWallTypes.Contains(WorldGenHelper.GetTileSafely(i, j).WallType))
                    return;
            }
        }
        for (int i = origin.X - num; i <= origin.X + num; i++) {
            for (int j = origin.Y - num; j <= origin.Y + num; j++) {
                if (WorldGenHelper.GetTileSafely(i, j).HasTile && WorldGenHelper.GetTileSafely(i, j).TileType == ModContent.TileType<NexusGateway>())
                    return;
            }
        }

        HouseBuilderCustom houseBuilder = CustomHouseUtils.CreateBuilder(origin, GenVars.structures);
        if (!houseBuilder.IsValid) {
            return;
        }

        //bool painting1 = false, painting2 = false, painting3 = false;
        //for (int i = Left + 15; i < Right - 15; i++) {
        //    for (int j = BackwoodsVars.FirstTileYAtCenter; j < Bottom - 20; j++) {
        //        if (WorldGenHelper.GetTileSafely(i, j).HasTile) {
        //            if (WorldGenHelper.GetTileSafely(i, j).TileType == ModContent.TileType<MillionDollarPainting>()) {
        //                painting1 = true;
        //            }
        //            if (WorldGenHelper.GetTileSafely(i, j).TileType == ModContent.TileType<Moss>()) {
        //                painting2 = true;
        //            }
        //            if (WorldGenHelper.GetTileSafely(i, j).TileType == ModContent.TileType<TheLegend>()) {
        //                painting3 = true;
        //            }
        //        }
        //    }
        //}

        houseBuilder.ChestChance = origin.Y < Main.worldSurface ? 0.75 : 0.95;
        houseBuilder.Place(new HouseBuilderContext(), GenVars.structures);
    }

    private void GetRandomPosition(int posX, int posY, out int baseX, out int baseY, bool rootLootRoom = true) {
        int startX = Left + 2;
        int endX = Right - 2;
        int centerY = rootLootRoom ? (BackwoodsVars.FirstTileYAtCenter + EdgeY) : ((int)Main.worldSurface + 15);
        int minY = centerY;
        int generateY = Bottom - 10;
        if (posX == 0) {
            baseX = _random.Next(startX, endX);
        }
        else {
            baseX = posX;
        }
        WeightedRandom<float> weightedRandom = new WeightedRandom<float>(WorldGen.genRand);
        weightedRandom.Add(0f + 0.1f * _random.NextFloat(), 0.25f);
        weightedRandom.Add(0.1f + 0.1f * _random.NextFloat(), 0.25f);
        weightedRandom.Add(0.35f + 0.1f * _random.NextFloat(), 0.75f);
        weightedRandom.Add(0.5f + 0.1f * _random.NextFloatRange(1f), 0.75f);
        weightedRandom.Add(0.65f + 0.1f * _random.NextFloatRange(1f), 0.85f);
        weightedRandom.Add(0.8f + 0.1f * _random.NextFloatRange(1f), 0.85f);
        if (posY == 0) {
            baseY = (int)(minY + (generateY - minY) * weightedRandom);
        }
        else {
            baseY = posY;
        }

        ushort[] skipTileTypes = [_dirtTileType, TileID.Dirt, _stoneTileType, _mossTileType];
        int attempts2 = 100;
        while (true) {
            int attempts = 100;
            while (GatewayNearby(baseX, baseY) || !skipTileTypes.Contains(WorldGenHelper.GetTileSafely(baseX, baseY).TileType) || WorldGenHelper.GetTileSafely(baseX, baseY).AnyWall()) {
                baseX = _random.Next(startX, endX);
                baseY = (int)(minY + (generateY - minY) * weightedRandom);
                if (attempts-- <= 0) {
                    break;
                }
            }
            bool flag = false;
            int check = 30 * WorldGenHelper.WorldSize;
            attempts = 100;
            for (int x2 = baseX - check; x2 < baseX + check; x2++) {
                for (int y2 = baseY - check; y2 < baseY + check; y2++) {
                    if (WorldGenHelper.ActiveTile(x2, y2, _elderWoodChestTileType) ||
                        Main.tile[x2, y2].WallType == _elderwoodWallType) {
                        flag = true;
                        break;
                    }
                }
                if (flag) {
                    baseX = _random.Next(startX, endX);
                    baseY = (int)(minY + (generateY - minY) * weightedRandom);
                    break;
                }
            }
            attempts2--;
            if (attempts2 <= 0) {
                break;
            }
            if (flag) {
                if (attempts-- <= 0) {
                    break;
                }
                continue;
            }
            else {
                break;
            }
        }
    }

    private void GenerateLootRoom2(int posX = 0, int posY = 0) {
        GetRandomPosition(posX, posY, out int baseX, out int baseY);

        Point origin = new(baseX, baseY);

        int attempts = 1000;
        while (--attempts > 0) {
            int num = Math.Max(50, _biomeWidth / 3);
            bool flag = true;
            for (int i = origin.X - num; i <= origin.X + num; i++) {
                if (!flag) {
                    break;
                }
                for (int j = origin.Y - num; j <= origin.Y + num; j++) {
                    if (!WorldGenHelper.GetTileSafely(i, j).HasTile) {
                        continue;
                    }
                    Tile tile = WorldGenHelper.GetTileSafely(i, j);
                    ushort tileType = tile.TileType;
                    if (TileID.Sets.BasicChest[tile.TileType] || SkipBiomeInvalidTileTypeToKill.Contains(tileType) || SkipBiomeInvalidWallTypeToKill.Contains(tile.WallType) || MidInvalidTileTypesToKill.Contains(tileType) || MidInvalidWallTypesToKill.Contains(tile.WallType)) {
                        flag = false;
                        GetRandomPosition(posX, posY, out baseX, out baseY, false);
                        break;
                    }
                }
            }
            origin = new(baseX, baseY);
            if (flag) {
                break;
            }
        }

        float x = (float)baseX, y = (float)baseY;
        ushort placeholderTileType = _elderwoodTileType2,
               placeholderWallType = _elderwoodWallType2;

        // base
        int distance = 30;
        int startSize = 9, endSize = 2;
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
                    int x3 = (int)x + i;
                    int y3 = (int)y + j;
                    Main.tile[x3, y3].ClearTile();
                    WorldGen.PlaceTile(x3, y3, placeholderTileType, true, true);
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
                Tile tile = WorldGenHelper.GetTileSafely(x2, y2);
                if (!Main.tile[x2 - 1, y2].ActiveTile2(placeholderTileType) || !Main.tile[x2 + 1, y2].ActiveTile2(placeholderTileType) || !Main.tile[x2, y2 - 1].ActiveTile2(placeholderTileType) || !Main.tile[x2, y2 + 1].ActiveTile2(placeholderTileType) ||
                    !Main.tile[x2 - 1, y2 - 1].ActiveTile2(placeholderTileType) || !Main.tile[x2 + 1, y2 - 1].ActiveTile2(placeholderTileType) || !Main.tile[x2 + 1, y2 + 1].ActiveTile2(placeholderTileType) || !Main.tile[x2 - 1, y2 + 1].ActiveTile2(placeholderTileType)) {
                    if (tile.ActiveWall(placeholderWallType)) {
                        if (y2 < Main.worldSurface) {
                            WorldGenHelper.ReplaceWall(x2, y2, WallID.DirtUnsafe);
                        }
                        else {
                            WorldGen.KillWall(x2, y2);
                        }
                    }
                }

                if (tile.ActiveTile(placeholderTileType) && !tile.AnyWall() && y2 < Main.worldSurface) {
                    tile.WallType = WallID.DirtUnsafe;
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
                    Point pointPosition = new(x2, y2);
                    if (tile.ActiveWall(placeholderWallType) && !killTiles.Contains(pointPosition)) {
                        killTiles.Add(pointPosition);
                    }
                }
            }
        }
        foreach (Point killPos in killTiles) {
            WorldGen.KillTile(killPos.X, killPos.Y);
        }
        for (int x2 = baseX - distance; x2 < baseX + distance; x2++) {
            for (int y2 = baseY - distance; y2 < baseY + distance; y2++) {
                Tile tile = WorldGenHelper.GetTileSafely(x2, y2);
                bool flag3 = false;
                for (int x22 = x2 - 1; x22 < x2 + 2; x22++) {
                    for (int y22 = y2 - 1; y22 < y2 + 2; y22++) {
                        if (Main.tile[x22, y22].WallType == placeholderWallType) {
                            flag3 = true;
                        }
                    }
                }
                if ((tile.ActiveTile(placeholderTileType) || flag3) && !tile.AnyWall() && y2 < Main.worldSurface) {
                    tile.WallType = WallID.DirtUnsafe;
                }
            }
        }

        // place chest
        bool chestPlaced = false;
        int killTileCount = killTiles.Count;
        List<Point> killTiles2 = [];
        int attempts2 = 50;
        bool placedTorch = false;
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
                WorldGenHelper.PlaceChest(killPos.X, killPos.Y, _elderWoodChestTileType, style: 1, onPlaced: (chest) => {
                    chestPlaced = true;
                    int slotId = 0;
                    void addItemInChest(int itemType, int itemStack, double chance = 1.0) {
                        if (_random.NextChance(chance)) {
                            Item item = chest.item[slotId];
                            (bool, Item?) hasItemInChest = (false, null);
                            foreach (Item chestItem in chest.item) {
                                if (chestItem.IsEmpty()) {
                                    continue;
                                }
                                if (chestItem.type == itemType) {
                                    hasItemInChest = (true, chestItem);
                                }
                            }
                            if (!hasItemInChest.Item1) {
                                chest.item[slotId].SetDefaults(itemType);
                                chest.item[slotId].stack = itemStack;
                                slotId++;
                            }
                            else {
                                hasItemInChest.Item2.stack += itemStack;
                            }
                        }
                    }
                    int firstItemType = _nextItemIndex switch {
                        0 => ModContent.ItemType<Bane>(),
                        1 => ModContent.ItemType<OvergrownSpear>(),
                        2 => ModContent.ItemType<MothStaff>(),
                        3 => ModContent.ItemType<DoubleFocusCharm>(),
                        4 => ModContent.ItemType<BeastBow>()
                    };
                    Action[] addings = [
                        () => {
                            _nextItemIndex++;
                            _nextItemIndex2++;
                            if (_nextItemIndex2 >= 3) {
                                _nextItemIndex2 = 0;
                            }
                            if (_nextItemIndex >= 5) {
                                _nextItemIndex = 0;
                            }
                            addItemInChest(firstItemType, 1);
                            int secondItemType;
                            switch (_nextItemIndex2) {
                                case 0:
                                    secondItemType = ModContent.ItemType<BunnyHat>();
                                    break;
                                case 1:
                                    secondItemType = ModContent.ItemType<BunnyJacket>();
                                    break;
                                default:
                                    secondItemType = ModContent.ItemType<BunnyPants>();
                                    break;
                            }
                            bool flag10 = false;
                            if (!_wandsAdded || _random.NextBool(20)) {
                                _wandsAdded = true;
                                addItemInChest(ModContent.ItemType<LivingPrimordialWand>(), 1, 1);
                                addItemInChest(ModContent.ItemType<LivingPrimordialWand2>(), 1, 1);
                                flag10 = true;
                            }
                            if (!flag10) {
                                addItemInChest(secondItemType, 1);
                            }
                        },
                        //() => {
                        //    bool flag = _random.NextChance(0.75);
                        //    if ( _random.NextChance(0.75)) {
                        //        int itemToAddType = ItemID.Dynamite;
                        //        addItemInChest(itemToAddType, 1);
                        //    }
                        //    else if ( _random.NextChance(0.25)) {
                        //        int itemToAddType = ItemID.Bomb;
                        //        addItemInChest(itemToAddType, _random.Next(15, 20));
                        //    }
                        //},
                        () => addItemInChest(ItemID.Dynamite,
                                             1,
                                             0.33),
                        () => addItemInChest(_random.NextBool(2) ? ItemID.JestersArrow : (WorldGen.SavedOreTiers.Gold == TileID.Silver ? ItemID.SilverBullet : ItemID.TungstenBullet),
                                             _random.Next(25, 51),
                                             0.5),
                        () => addItemInChest(WorldGen.SavedOreTiers.Gold == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar,
                                             _random.Next(15, 30),
                                             0.5),
                        () => addItemInChest(_random.NextBool(2) ? ItemID.HealingPotion : ItemID.RestorationPotion,
                                              _random.Next(6, 11),
                                             0.5),
                        () => addItemInChest(_random.Next([ItemID.RestorationPotion, ItemID.HealingPotion]),
                                             _random.Next(3, 9),
                                             0.5),
                        () => addItemInChest(_random.Next([ItemID.GravitationPotion, ModContent.ItemType<ResiliencePotion>(),
                            ItemID.ArcheryPotion, ModContent.ItemType<DryadBloodPotion>(), ItemID.MiningPotion]),
                                             _random.Next(1, 3),
                                             0.75),
                        () => addItemInChest(_random.Next([ItemID.InvisibilityPotion, ItemID.ObsidianSkinPotion, ItemID.BattlePotion,
                            ModContent.ItemType<WeightPotion>(), ItemID.HunterPotion]),
                                             _random.Next(1, 3),
                                             0.66),
                        () => addItemInChest(ItemID.RecallPotion,
                                             _random.Next(1, 3),
                                             0.5),
                        () => addItemInChest(_random.Next([ItemID.Torch, ItemID.Glowstick]),
                                             _random.Next(15, 30),
                                             0.5),
                        () => addItemInChest(_random.Next([ItemID.GoldCoin]),
                                             _random.Next(2, 4),
                                             0.75),
                    ];
                    foreach (Action add in addings) {
                        add();
                    }
                });
            }
            if (chestPlaced) {
                break;
            }
            killTileCount--;
        }

        for (int x2 = baseX - distance; x2 < baseX + distance; x2++) {
            for (int y2 = baseY - distance; y2 < baseY + distance; y2++) {
                if (WorldGenHelper.ActiveTile(x2, y2, placeholderTileType)) {
                    WorldGenHelper.ReplaceTile(x2, y2, _elderwoodTileType);
                }
            }
        }

        for (int x2 = baseX - distance; x2 < baseX + distance; x2++) {
            for (int y2 = baseY - distance; y2 < baseY + distance; y2++) {
                if (WorldGenHelper.ActiveWall(x2, y2, placeholderWallType)) {
                    WorldGenHelper.ReplaceWall(x2, y2, _elderwoodWallType);
                }
            }
        }

        attempts2 = 20;
        while (!placedTorch) {
            if (attempts2-- <= 0) {
                break;
            }
            Point killPos = killTiles[_random.Next(killTiles.Count)];
            Tile tile = WorldGenHelper.GetTileSafely(killPos.X, killPos.Y);
            if ((tile.ActiveWall(placeholderWallType) || tile.ActiveWall(_elderwoodWallType)) && !tile.HasTile) {
                if (_random.NextBool(45)) {
                    WorldGen.PlaceTile(killPos.X, killPos.Y, TileID.Torches, style: 1);
                    if (TileID.Sets.Torch[Main.tile[killPos.X, killPos.Y].TileType]) {
                        placedTorch = true;
                    }
                }
            }
            if (placedTorch) {
                break;
            }
        }

        //size = distance / 2;
        //WorldGenHelper.SlopeAreaNatural(baseX, baseY, size, placeholderTileType);
    }

    public void BackwoodsCleanup(GenerationProgress progress, GameConfiguration config) {
        for (int i = Left - 100; i < Right + 100; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < Bottom + 15; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(_dirtTileType)) {
                    tile.TileType = TileID.Dirt;
                }
                //if (tile.AnyLiquid()) {
                //    tile.LiquidAmount = 0;
                //}
            }
        }

        /*if (ModLoader.HasMod("SpiritMod"))*/
        {
            for (int i = Left - 50; i <= Right + 50; i++) {
                for (int j = WorldGenHelper.SafeFloatingIslandY; j < CenterY; j++) {
                    if (WorldGenHelper.ActiveTile(i, j, _dirtTileType)) {
                        WorldGenHelper.GetTileSafely(i, j).TileType = TileID.Dirt;
                    }
                }
            }
        }

        Step_AddGrassWalls();
    }

    public void BackwoodsOnLast0(GenerationProgress progress, GameConfiguration config) {
        Point altarCoords = AltarHandler.GetAltarPosition();
        int i = altarCoords.X, j = altarCoords.Y;
        for (int i2 = i - 7; i2 < i + 8; i2++) {
            for (int j2 = j - 1; j2 < j + 2; j2++) {
                if (WorldGenHelper.ActiveTile(i2, j2, TileID.Dirt) || WorldGenHelper.ActiveTile(i2, j2, _dirtTileType)) {
                    Main.tile[i2, j2].TileType = _grassTileType;
                }
                //if (WorldGenHelper.ActiveTile(i2, j2, TileID.Dirt) &&
                //    (WorldGenHelper.ActiveTile(i2, j2 + 1, _leavesTileType) || WorldGenHelper.ActiveTile(i2, j2 + 1, _grassTileType)) &&
                //    (WorldGenHelper.ActiveTile(i2 + 1, j2, _grassTileType) || WorldGenHelper.ActiveTile(i2 + 1, j2, _leavesTileType))) {
                //    WorldGenHelper.ReplaceTile(i2, j2, _grassTileType);
                //}
            }
        }

        int maxLeft = Left - 50;
        int maxRight = Right + 50;
        int num1047 = 0;
        for (int num1048 = maxLeft; num1048 < maxRight; num1048++) {
            num1047 += _random.Next(-1, 2);
            if (num1047 < 0)
                num1047 = 0;
            if (num1047 > 10)
                num1047 = 10;
            for (int num1052 = (int)(Main.worldSurface - 15 + num1047); num1052 < (int)(Main.worldSurface + _biomeHeight / 2); num1052++) {
                if (num1048 > maxLeft + _random.NextFloat() * 15 && num1048 < maxRight - _random.NextFloat() * 15 && (Main.tile[num1048, num1052].WallType == _grassWallType || Main.tile[num1048, num1052].WallType == _flowerGrassWallType)) {
                    Main.tile[num1048, num1052].WallType = (ushort)(_random.NextBool(5) ? 59 : _dirtWallType);
                }
                if (num1048 > maxLeft + _random.NextFloat() * 15 && num1048 < maxRight - _random.NextFloat() * 15 && Main.tile[num1048, num1052].WallType == WallID.JungleUnsafe) {
                    Main.tile[num1048, num1052].WallType = (ushort)(_random.NextBool(5) ? _grassWallType : _flowerGrassWallType);
                }
            }
        }

        for (int x = maxLeft - 50; x < maxRight + 50; x++) {
            for (int y = WorldGenHelper.SafeFloatingIslandY; y < Bottom + EdgeY; y++) {
                if (WorldGenHelper.ActiveTile(i, j, TileID.FallenLog) && WorldGenHelper.ActiveTile(i, j + 1, TileID.Grass)) {
                    WorldGenHelper.GetTileSafely(i, j + 1).TileType = _grassTileType;
                }
                if (WorldGenHelper.ActiveTile(i, j, TileID.Pots)) {
                    if (MustSkipWallTypes.Contains(Main.tile[i, j].WallType)) {
                        continue;
                    }
                    if (Main.tile[i, j + 1].TileType != TileID.RollingCactus) {
                        WorldGen.KillTile(i, j);
                        WorldGen.PlacePot(i, j, _potTileType, _random.Next(4));
                    }
                }
            }
        }

        //bool hasRemnants = ModLoader.HasMod("Remnants");
        //if (hasRemnants) {
        //    for (int x = Left; x <= Right; x++) {
        //        for (int y = BackwoodsVars.FirstTileYAtCenter - 30; y < Bottom; y++) {
        //            Tile tile = WorldGenHelper.GetTileSafely(x, y);
        //            if (tile.WallType == WallID.JungleUnsafe) {
        //                tile.WallType = _dirtWallType;
        //            }
        //            if (tile.TileType == TileID.JungleGrass) {
        //                tile.TileType = _grassTileType;
        //            }
        //            if (tile.TileType == TileID.LivingMahogany) {
        //                tile.TileType = _elderwoodTileType;
        //            }
        //            if (tile.TileType == TileID.RichMahoganyBeam) {
        //                tile.TileType = TileID.WoodenBeam;
        //            }
        //            if (tile.TileType == TileID.Mud) {
        //                tile.TileType = TileID.Dirt;
        //            }
        //        }
        //    }
        //}
    }

    public void BackwoodsOnLast1(GenerationProgress progress, GameConfiguration config) {
        //Point altarCoords = AltarHandler.GetAltarPosition();
        //int i = altarCoords.X, j = altarCoords.Y;
        //for (int i2 = i - 7; i2 < i + 8; i2++) {
        //    for (int j2 = j - 1; j2 < j + 2; j2++) {
        //        if (WorldGenHelper.ActiveTile(i2, j2, TileID.Dirt) || WorldGenHelper.ActiveTile(i2, j2, _dirtTileType)) {
        //            Main.tile[i2, j2].TileType = _grassTileType;
        //        }
        //    }
        //}

        Step_AddJawTraps();

        //bool hasRemnants = ModLoader.HasMod("Remnants");
        //if (hasRemnants) {
        //    for (int x = Left; x <= Right; x++) {
        //        for (int y = BackwoodsVars.FirstTileYAtCenter - 30; y < Bottom; y++) {
        //            Tile tile = WorldGenHelper.GetTileSafely(x, y);
        //            if (tile.WallType == WallID.JungleUnsafe) {
        //                tile.WallType = _dirtWallType;
        //            }
        //        }
        //    }
        //}

        for (int x = Left - 100; x <= Right + 100; x++) {
            for (int y = BackwoodsVars.FirstTileYAtCenter - 30; y < Bottom + EdgeY + 10; y++) {
                Tile tile = WorldGenHelper.GetTileSafely(x, y);
                if (tile.HasTile) {
                    //for (int x2 = x - 1; x2 < x + 2; x2++) {
                    //    for (int y2 = y - 1; y2 < y + 2; y2++) {
                    //        WorldGen.SquareTileFrame(x2, y2);
                    //    }
                    //}
                    if (tile.TileType == TileID.WaterDrip && _random.NextChance(0.8)) {
                        tile.HasTile = false;
                    }
                }
            }
        }

        for (int i = Left - 100; i <= Right + 100; i++) {
            for (int j = (int)Main.worldSurface - 10; j < Bottom + EdgeY / 2; j++) {
                Tile solidTile = WorldGenHelper.GetTileSafely(i, j - 1);
                if (WorldGen.SolidTile(i, j - 1) && Main.tile[i, j - 1].HasUnactuatedTile && WorldGen.genRand.NextBool(25)) {
                    if (solidTile.TileType == _stoneTileType) {
                        Stalactite_GenPass.PlaceStalactite(i, j, _stoneTileType, (ushort)ModContent.TileType<GrimrockStalactite>(), ModContent.GetInstance<GrimrockStalactiteTE>(), true);
                    }
                    else if (solidTile.TileType == _mossTileType) {
                        Stalactite_GenPass.PlaceStalactite(i, j, _mossTileType, (ushort)ModContent.TileType<GrimrockStalactite>(), ModContent.GetInstance<GrimrockStalactiteTE>(), true);
                    }
                }
            }
        }

        if (!WorldGen.InWorld(_gatewayLocation.X, _gatewayLocation.Y, 30)) {
            return;
        }

        int num = 30;
        int num2 = 30;
        double num3 = num2;
        Vector2D vector2D = default(Vector2D);
        vector2D.X = _gatewayLocation.X;
        vector2D.Y = _gatewayLocation.Y;
        Vector2D vector2D2 = _gatewayVelocity;
        while (num > 0.0 && num3 > 0.0) {
            double num4 = num * (num3 / num2);
            num3 -= 1.0;
            int num5 = (int)(vector2D.X - num4 * 0.5);
            int num6 = (int)(vector2D.X + num4 * 0.5);
            int num7 = (int)(vector2D.Y - num4 * 0.5);
            int num8 = (int)(vector2D.Y + num4 * 0.5);
            if (num5 < 0)
                num5 = 0;

            if (num6 > Main.maxTilesX)
                num6 = Main.maxTilesX;

            if (num7 < 0)
                num7 = 0;

            if (num8 > Main.maxTilesY)
                num8 = Main.maxTilesY;

            for (int k = num5; k < num6; k++) {
                for (int l = num7; l < num8; l++) {
                    if (Math.Abs((double)k - vector2D.X) + Math.Abs((double)l - vector2D.Y) < num * 0.5 * (1.0 + (double)_random.Next(-10, 11) * 0.015)) {
                        WorldGen.grassSpread = 0;
                        WorldGenHelper.CustomSpreadGrass(k, l, TileID.Dirt, _grassTileType, growUnderground: true);
                        WorldGenHelper.CustomSpreadGrass(k, l, _dirtTileType, _grassTileType, growUnderground: true);
                        if (Main.tile[k, l].WallType == 59 || Main.tile[k, l].WallType == _dirtWallType) {
                            Main.tile[k, l].WallType = _grassWallType;
                            if (_random.NextBool(3)) {
                                FlowerGrassRunner(k, l);
                            }
                        }
                        Main.tile[k, l].LiquidAmount = 0;

                        if (Main.tile[k, l].TileType == _vinesTileType) {
                            WorldGenHelper.PlaceVines(k, l, 5, _vinesTileType);
                        }
                        if (Main.tile[k, l].TileType == _vinesTileType2) {
                            WorldGenHelper.PlaceVines(k, l, 5, _vinesTileType2);
                        }
                    }
                }
            }

            vector2D += vector2D2;
            vector2D2.X += (double)_random.Next(-10, 11) * 0.05;
            if (vector2D2.X > 1.0)
                vector2D2.X = 1.0;

            if (vector2D2.X < -1.0)
                vector2D2.X = -1.0;

            vector2D2.Y += (double)_random.Next(-10, 4) * 0.05;
            if (vector2D2.Y > 1.0)
                vector2D2.Y = 1.0;

            if (vector2D2.Y < -1.0)
                vector2D2.Y = -1.0;
        }

        for (int k = 0; k < 5; k++) {
            for (int i = _gatewayLocation.X - 20; i <= _gatewayLocation.X + 20; i++) {
                for (int j = _gatewayLocation.Y - 20; j < _gatewayLocation.Y + 20; j++) {
                    Tile tile = WorldGenHelper.GetTileSafely(i, j);
                    if ((tile.TileType == _grassTileType || tile.TileType == _leavesTileType) && !Main.tile[i, j + 1].HasTile) {
                        WorldGen.PlaceTile(i, j + 1, (tile.WallType == _grassWallType || Main.tile[i, j + 1].WallType == _grassWallType || tile.WallType == _leavesWallType || Main.tile[i, j + 1].WallType == _leavesWallType) ? _vinesTileType2 : _vinesTileType);
                    }
                    if (tile.TileType == _vinesTileType) {
                        WorldGenHelper.PlaceVines(i, j, 5, _vinesTileType);
                    }
                    if (tile.TileType == _vinesTileType2) {
                        WorldGenHelper.PlaceVines(i, j, 5, _vinesTileType2);
                    }
                }
            }
        }
    }

    public void BackwoodsOnLast(GenerationProgress progress, GameConfiguration config) {
        Step_AddCatTails();
        Step_AddLilypads();
        Step_AddLilypads();

        for (int x = Left - 100; x <= Right + 100; x++) {
            for (int y = BackwoodsVars.FirstTileYAtCenter + 5; y < Main.worldSurface; y++) {
                Tile aboveTile = WorldGenHelper.GetTileSafely(x, y - 1);
                Tile tile = WorldGenHelper.GetTileSafely(x, y);
                if (tile.ActiveTile(_grassTileType) && WorldGen.SolidTile(x, y) && !aboveTile.HasTile) {
                    tile = WorldGenHelper.GetTileSafely(x, y - 1);
                    if (_random.NextBool(5) && (tile.WallType == _grassWallType || tile.WallType == _flowerGrassWallType || tile.WallType == _leavesWallType)) {
                        int num3 = 20;
                        int num4 = 2;
                        int num5 = 0;
                        num3 = (int)((double)num3 * ((double)Main.maxTilesX / 4200.0));
                        int num6 = Utils.Clamp(x - num3, 4, Main.maxTilesX - 4);
                        int num7 = Utils.Clamp(x + num3, 4, Main.maxTilesX - 4);
                        int num8 = Utils.Clamp(y - num3, 4, Main.maxTilesY - 4);
                        int num9 = Utils.Clamp(y + num3, 4, Main.maxTilesY - 4);
                        for (int i2 = num6; i2 <= num7; i2++) {
                            for (int j2 = num8; j2 <= num9; j2++) {
                                int checkTileType = Main.tile[i2, j2].TileType;
                                if (checkTileType == _mintTileType) {
                                    num5++;
                                }
                            }
                        }
                        if (num5 < num4) {
                            tile.HasTile = true;
                            tile.TileFrameY = 0;
                            tile.TileType = _mintTileType;
                            tile.TileFrameX = (short)(18 * 2);
                            //ModContent.GetInstance<MiracleMintTE>().Place(x, y - 1);
                        }
                    }
                    else {
                        tile.HasTile = true;
                        tile.TileFrameY = 0;
                        tile.TileType = _plantsTileType;
                        tile.TileFrameX = (short)(18 * _random.Next(20));
                    }
                    break;
                }
            }
        }

        Step_AddSpikes();
        Step_AddHerbs();

        Point altarCoords = AltarHandler.GetAltarPosition();
        int i = altarCoords.X, j = altarCoords.Y;
        for (int i2 = i - 7; i2 < i + 8; i2++) {
            for (int j2 = j - 1; j2 < j + 2; j2++) {
                if (WorldGenHelper.ActiveTile(i2, j2, TileID.Dirt) || WorldGenHelper.ActiveTile(i2, j2, _dirtTileType)) {
                    Main.tile[i2, j2].TileType = _grassTileType;
                }
                //if (WorldGenHelper.ActiveTile(i2, j2, TileID.Dirt) &&
                //    (WorldGenHelper.ActiveTile(i2, j2 + 1, _leavesTileType) || WorldGenHelper.ActiveTile(i2, j2 + 1, _grassTileType)) &&
                //    (WorldGenHelper.ActiveTile(i2 + 1, j2, _grassTileType) || WorldGenHelper.ActiveTile(i2 + 1, j2, _leavesTileType))) {
                //    WorldGenHelper.ReplaceTile(i2, j2, _grassTileType);
                //}
            }
        }
        //Step_AddPills();
        Step_AddChests();
        Step10_SpreadMossGrass();

        // place bushes
        for (i = Left - 100; i <= Right + 100; i++) {
            for (j = WorldGenHelper.SafeFloatingIslandY; j < CenterY - EdgeY; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(_grassTileType)) {
                    if (!WorldGen.SolidTile2(i, j - 1)) {
                        bool flag = false;
                        //if ((((double)i > (double)CenterX + 3 && (double)i < (double)CenterX + 10)) && j < BackwoodsVars.FirstTileYAtCenter + 5) {
                        //    flag = true;
                        //}
                        if (i > CenterX && i < CenterX + 4 && j < BackwoodsVars.FirstTileYAtCenter + 10) {
                            continue;
                        }
                        bool flag2 = i > CenterX + 2 && i < CenterX + 15;
                        if (_random.NextChance(0.5)) {
                            WallBush(i, j + 3 + (flag ? _random.Next(-1, 2) : 0), !flag);
                            i += _random.Next(2, 8);
                        }
                        if (flag2) {
                            int sizeX = 10;
                            int sizeY = 5;
                            int sizeY2 = sizeY;
                            while (sizeY2 > 0) {
                                double progress2 = sizeX * ((double)sizeY2 / sizeY);
                                --sizeY2;
                                int x1 = (int)(i - progress2 * 0.5);
                                int x2 = (int)(i + progress2 * 0.5);
                                int y1 = (int)(j - progress2 * 0.5);
                                int y2 = (int)(j + progress2 * 0.5);
                                for (int x = x1; x < x2; x++) {
                                    for (int y = y1; y < y2; y++) {
                                        double min = Math.Abs((double)(x - i)) + Math.Abs((double)(y - j));
                                        double max = (double)sizeX * 0.5 * (1.0 + 5 * 0.025);
                                        if (min < max) {
                                            WorldGenHelper.ReplaceWall(x, y + 2, _leavesWallType);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (tile.ActiveTile(_mossTileType) && !WorldGenHelper.GetTileSafely(i, j).ActiveWall()) {
                    if (!WorldGen.SolidTile2(i, j - 1)) {
                        bool flag = false;
                        if ((((double)i > (double)CenterX + 3 && (double)i < (double)CenterX + 15)) && j < BackwoodsVars.FirstTileYAtCenter + 20) {
                            flag = true;
                        }
                        if (_random.NextChance(0.375)) {
                            WallBush_Moss(i, j + 3 + (flag ? _random.Next(-1, 2) : 0), !flag);
                            i += _random.Next(2, 8);
                        }
                    }
                }
            }
        }

        //for (i = Left - 100; i < Right + 100; i++) {
        //    for (j = WorldGenHelper.SafeFloatingIslandY; j < Bottom + 15; j++) {
        //        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        //        if (tile.ActiveTile(_dirtTileType)) {
        //            tile.TileType = _grassTileType;
        //        }
        //        //if (tile.AnyLiquid()) {
        //        //    tile.LiquidAmount = 0;
        //        //}
        //    }
        //}

        Step6_SpreadGrass(true);

        GatewayExtra();

        // place vines again
        for (i = Left - 50; i <= Right + 50; i++) {
            for (j = WorldGenHelper.SafeFloatingIslandY; j < CenterY + EdgeY; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if ((tile.TileType == _grassTileType || tile.TileType == _leavesTileType) && !Main.tile[i, j + 1].HasTile) {
                    if (_random.NextBool(tile.TileType == _grassTileType ? 2 : 3)) {
                        WorldGen.PlaceTile(i, j + 1, (tile.WallType == _grassWallType || Main.tile[i, j + 1].WallType == _grassWallType || tile.WallType == _leavesWallType || Main.tile[i, j + 1].WallType == _leavesWallType) ? _vinesTileType2 : _vinesTileType);
                    }
                }
                if (tile.TileType == _vinesTileType) {
                    WorldGenHelper.PlaceVines(i, j, _random.Next(1, 5), _vinesTileType);
                }
                if (tile.TileType == _vinesTileType2) {
                    WorldGenHelper.PlaceVines(i, j, _random.Next(3, 7), _vinesTileType2);
                }
            }
        }
    }

    private void GatewayExtra() {
        if (_gatewayLocation == Point.Zero) {
            return;
        }

        int xOffsetX = _gatewayLocation.X + 10;
        int xOffsetY = _gatewayLocation.Y + 10;
        for (int i = -xOffsetX; i < xOffsetX; i++) {
            for (int j = -xOffsetY; j < xOffsetY; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.HasTile && tile.TileType == ModContent.TileType<NexusGateway>()) {
                    for (int i3 = -1; i3 < 2; i3++) {
                        for (int j3 = -1; j3 < 2; j3++) {
                            if (i3 != 0 || j3 != 0) {
                                Tile tile2 = WorldGenHelper.GetTileSafely(i + i3, j + j3);
                                if (tile2.HasTile && (tile2.TileType == _dirtTileType || tile2.TileType == TileID.Dirt)) {
                                    tile2.TileType = _grassTileType;
                                }
                            }
                        }
                    }
                }
            }
        }
        double num = 18;
        double num2 = 24;
        double num3 = num2;
        Vector2D vector2D = default(Vector2D);
        vector2D.X = _gatewayLocation.X;
        vector2D.Y = _gatewayLocation.Y;
        Vector2D vector2D2 = default(Vector2D);
        vector2D2.X = (double)_random.Next(-10, 15) * 0.1;
        vector2D2.Y = (double)-1 * 0.1;
        _gatewayVelocity = vector2D2;
        while (num > 0.0 && num3 > 0.0) {
            double num4 = num * (num3 / num2);
            num3 -= 1.0;
            int num5 = (int)(vector2D.X - num4 * 0.5);
            int num6 = (int)(vector2D.X + num4 * 0.5);
            int num7 = (int)(vector2D.Y - num4 * 0.5);
            int num8 = (int)(vector2D.Y + num4 * 0.5);
            if (num5 < 0)
                num5 = 0;

            if (num6 > Main.maxTilesX)
                num6 = Main.maxTilesX;

            if (num7 < 0)
                num7 = 0;

            if (num8 > Main.maxTilesY)
                num8 = Main.maxTilesY;

            for (int k = num5; k < num6; k++) {
                for (int l = num7; l < num8; l++) {
                    if (Math.Abs((double)k - vector2D.X) + Math.Abs((double)l - vector2D.Y) < num * 0.5 * (1.0 + (double)_random.Next(-10, 11) * 0.015)) {
                        WorldGen.grassSpread = 0;
                        WorldGenHelper.CustomSpreadGrass(k, l, TileID.Dirt, _grassTileType, growUnderground: true);
                        WorldGenHelper.CustomSpreadGrass(k, l, _dirtTileType, _grassTileType, growUnderground: true);
                        //if (!WorldGen.SolidTile(k, l) || Main.tile[k, l].TileType == _elderwoodTileType || Main.tile[k, l].TileType == _elderwoodTileType2 ||  Main.tile[k, l].TileType == _mossTileType) {
                        //    if (WorldGenHelper.GetTileSafely(k, l).WallType != _elderwoodWallType) {
                        //        Main.tile[k, l].WallType = 0;
                        //    }
                        //}
                        Main.tile[k, l].LiquidAmount = 0;

                        if (Main.tile[k, l].TileType == _vinesTileType) {
                            WorldGenHelper.PlaceVines(k, l, 5, _vinesTileType);
                        }
                        if (Main.tile[k, l].TileType == _vinesTileType2) {
                            WorldGenHelper.PlaceVines(k, l, 5, _vinesTileType2);
                        }
                    }
                }
            }

            vector2D += vector2D2;
            vector2D2.X += (double)_random.Next(-10, 11) * 0.035;
            if (vector2D2.X > 1.0)
                vector2D2.X = 1.0;

            if (vector2D2.X < -1.0)
                vector2D2.X = -1.0;

            vector2D2.Y += (double)_random.Next(-10, 1) * 0.035;
            if (vector2D2.Y > 0.2)
                vector2D2.Y = 0.2;

            if (vector2D2.Y < -1.0)
                vector2D2.Y = -1.0;
        }
    }

    private bool GatewayNearby(int x, int y) {
        int fluff = 20;
        for (int i = x - fluff; i < x + fluff + 1; i++) {
            for (int j = y - fluff; j < y + fluff + 1; j++) {
                if (i == _gatewayLocation.X && j == _gatewayLocation.Y) {
                    return true;
                }
            }
        }

        return false;
    }

    public void BackwoodsTilesReplacement(GenerationProgress progress, GameConfiguration config) {
        //progress.Message = Language.GetOrRegister("Mods.RoA.WorldGen.Backwoods2").Value;

        for (int i = Left - 50; i < Right + 50; i++) {
            for (int j = BackwoodsVars.FirstTileYAtCenter - EdgeY; j < Bottom + EdgeY * 2; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                bool flag = tile.TileFrameX >= 1116 && tile.TileFrameX <= 1206 && tile.TileFrameY > 0;
                if (tile.ActiveTile(185) && (tile.TileFrameX <= 216 || flag)) {
                    if (flag) {
                        tile.TileType = (ushort)ModContent.TileType<BackwoodsRocks01>();
                    }
                    else {
                        tile.TileType = (ushort)ModContent.TileType<BackwoodsRocks0>();
                    }
                }
                if (tile.ActiveTile(186) && tile.TileFrameX < 702) {
                    WorldGen.KillTile(i, j);
                    WorldGenHelper.Place3x2(i, j, (ushort)ModContent.TileType<BackwoodsRocks3x2>(), _random.Next(6));
                }

                // place extra pots
                ushort tileType = WorldGenHelper.GetTileSafely(i, j + 1).TileType;
                if (WorldGen.SolidTile(i, j + 1) && !MidInvalidTileTypesToKill.Contains(tileType) && tileType != _leavesTileType && _random.NextBool(3)) {
                    if (MustSkipWallTypes.Contains(Main.tile[i, j].WallType)) {
                        continue;
                    }
                    if (Main.tile[i, j + 1].TileType != TileID.RollingCactus) {
                        WorldGen.PlacePot(i, j, _potTileType, _random.Next(4));
                    }
                }
            }
        }

        Step_AddWebs();
    }

    public void BackwoodsOtherPlacements(GenerationProgress progress, GameConfiguration config) {
        progress.Message = Language.GetOrRegister("Mods.RoA.WorldGen.Backwoods2").Value;

        void cleanUp() {
            int num1047 = 0;
            int num1048 = 0;
            int maxLeft = Left - 30;
            int maxRight = Right + 30;
            for (int i = maxLeft; i < maxRight; i++) {
                num1048 += _random.Next(-1, 2);
                if (num1048 < 0)
                    num1048 = 0;

                if (num1048 > 10)
                    num1048 = 10;
                for (int j = WorldGenHelper.SafeFloatingIslandY; j < Bottom; j++) {
                    if (i > maxLeft + _random.NextFloat() * 15 && i < maxRight - _random.NextFloat() * 15) {
                        Tile tile = WorldGenHelper.GetTileSafely(i, j);
                        //if (tile.ActiveTile(_mossTileType)) {
                        //    Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
                        //    Tile leftTile = WorldGenHelper.GetTileSafely(i - 1, j);
                        //    if (aboveTile.ActiveTile(_mossTileType) && (leftTile.ActiveTile(_mossTileType) || leftTile.ActiveTile(_grassTileType))) {
                        //        tile.TileType = _stoneTileType;
                        //    }
                        //    Tile belowTile = WorldGenHelper.GetTileSafely(i, j + 1);
                        //    if (leftTile.ActiveTile(_mossTileType) && belowTile.Slope != SlopeType.Solid) {
                        //        tile.TileType = _stoneTileType;
                        //    }
                        //}
                        if (tile.ActiveTile(TileID.Dirt)) {
                            Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
                            if (aboveTile.ActiveTile(TileID.Trees)) {
                                WorldGenHelper.ReplaceTile(i, j, TileID.Dirt);
                                WorldGenHelper.CustomSpreadGrass(i, j, TileID.Dirt, _grassTileType, growUnderground: true);
                            }
                        }
                        if (tile.ActiveTile(TileID.Vines)) {
                            tile.TileType = _vinesTileType;
                        }
                        if (tile.ActiveTile(TileID.VineFlowers)) {
                            tile.TileType = _vinesTileType2;
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
                                WorldGenHelper.CustomSpreadGrass(i, j, TileID.Dirt, _grassTileType, growUnderground: true);
                            }
                        }
                        num1047 += _random.Next(-1, 2);
                        if (num1047 < 0)
                            num1047 = 0;

                        if (num1047 > 5)
                            num1047 = 5;
                        bool edgeLeft = i < Left - 10, edgeRight = i > Right + 10;
                        bool edgeX = edgeRight || edgeLeft;
                        int extra = 5 - (i < Left - 25 || i > Right + 25 ? 4 : i < Left - 20 || i > Right + 20 ? 3 : i < Left - 15 || i > Right + 15 ? 2 : i < Left - 10 || i > Right + 10 ? 2 : 1);
                        bool flag2 = _random.NextBool(6 - extra);
                        bool flag0 = (edgeRight || edgeLeft) && flag2;
                        bool flag = flag0 || !edgeX;
                        tile = WorldGenHelper.GetTileSafely(i, j);
                        if (flag) {
                            if (tile.WallType == WallID.MudUnsafe) {
                                tile.WallType = _dirtWallType;
                                //if (!edgeX) {
                                //    tile.WallType = _dirtWallType;
                                //}
                                //else {
                                //    int i2 = i;
                                //    if (j > BackwoodsVars.FirstTileYAtCenter + 10) {
                                //        i2 -= (edgeRight ? num1048 : -num1048) - (edgeRight ? _random.Next(-2, 3) : -_random.Next(-2, 3));
                                //    }
                                //    if (edgeRight) {
                                //        i2 -= 10 + _random.Next(-2, 3);
                                //    }
                                //    else {
                                //        i2 += 10 + _random.Next(-2, 3);
                                //    }
                                //    Tile tile2 = WorldGenHelper.GetTileSafely(i2, j);
                                //    //if (tile2.TileType == TileID.Dirt || tile2.TileType == _dirtTileType) {
                                //    //    tile2.TileType = TileID.Mud;
                                //    //}
                                //    tile2.WallType = _dirtWallType;
                                //}
                            }
                            ushort[] invalidWalls = [WallID.FlowerUnsafe, WallID.GrassUnsafe/*, WallID.JungleUnsafe*/];
                            if (invalidWalls.Contains(tile.WallType)) {
                                tile.WallType = _grassWallType;
                            }
                        }
                    }
                }
            }
        }

        progress.Set(0.1f);
        cleanUp();

        progress.Set(0.2f);
        foreach (Point surface in _biomeSurface) {
            for (int j = 3; j > -_biomeHeight / 3 + 10; j--) {
                if (surface.Y + j < WorldGenHelper.SafeFloatingIslandY) {
                    continue;
                }

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
                    //bool flag = j <= 0 && tile.ActiveTile(_mossTileType);
                    //if (MidMustKillTileTypes.Contains(tile.TileType) || flag ||
                    //    ((j < 0) && ((!belowTile.HasTile && belowTile.WallType != WallID.None) ||
                    //    (tile.HasTile && tile.WallType == WallID.None && !belowTile.HasTile)))) {
                    //    WorldGen.KillTile(surface.X, surface.Y + j);
                    //    if (tile.WallType != _elderwoodWallType) {
                    //        tile.WallType = WallID.None;
                    //    }
                    //}
                }
            }
        }

        progress.Set(0.3f);
        cleanUp();

        Step_AddGrassWalls();

        progress.Set(0.4f);
        for (int i = 0; i < 4; i++) {
            GrowTrees();
        }

        progress.Set(0.5f);
        for (int i = Left; i < Right; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < Bottom - EdgeY; j++) {
                int extraX = _random.Next(-2, 3);
                if (i > Left + 5 && i < Right - 5) {
                    extraX = 0;
                }
                int extraY = _random.Next(-2, 3);
                if (j < Bottom - EdgeY - 5) {
                    extraY = 0;
                }
                Tile tile = WorldGenHelper.GetTileSafely(i + extraX, j + extraY);
                if (tile.ActiveTile(TileID.Grass)) {
                    WorldGenHelper.ReplaceTile(i + extraX, j + extraY, _grassTileType);
                }
                if (tile.ActiveTile(TileID.Stone)) {
                    WorldGenHelper.ReplaceTile(i + extraX, j + extraY, _stoneTileType);
                }
            }
        }

        progress.Set(0.6f);
        foreach (Point surface in _biomeSurface) {
            for (int j = -10; j < 4; j++) {
                if (surface.Y + j < WorldGenHelper.SafeFloatingIslandY) {
                    continue;
                }

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
        Step6_SpreadGrass(true);
        Step9_SpreadMoss();
        //Step10_SpreadMossGrass();

        double num377 = (double)Main.maxTilesX * 0.035;
        if (WorldGen.noTrapsWorldGen)
            num377 = ((!WorldGen.tenthAnniversaryWorldGen && !WorldGen.notTheBees) ? (num377 * 100.0) : (num377 * 5.0));
        else if (WorldGen.getGoodWorldGen)
            num377 *= 1.5;
        if (Main.starGame)
            num377 *= Main.starGameMath(0.2);

        for (int num378 = 0; (double)num378 < num377; num378++) {
            for (int num379 = 0; num379 < 1150; num379++) {
                if (WorldGen.noTrapsWorldGen) {
                    int num380 = _random.Next(Left - 20, Right + 20);
                    int num381 = _random.Next((int)Main.worldSurface, Bottom);

                    if (((double)num381 > Main.worldSurface || Main.tile[num380, num381].WallType > 0) && WorldGen.placeTrap(num380, num381, 0))
                        break;
                }
                else {
                    int num382 = _random.Next(Left - 20, Right + 20);
                    int num383 = _random.Next((int)Main.worldSurface, Bottom);
                    while (WorldGen.oceanDepths(num382, num383)) {
                        num382 = _random.Next(Left - 20, Right + 20);
                        num383 = _random.Next((int)Main.worldSurface, Bottom);
                    }

                    if (Main.tile[num382, num383].WallType == 0 && WorldGen.placeTrap(num382, num383, 0))
                        break;
                }
            }
        }

        progress.Set(0.8f);
        // destroy non solid above leaves
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
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if ((tile.TileType == _grassTileType || tile.TileType == _leavesTileType) && !Main.tile[i, j + 1].HasTile) {
                    if (_random.NextBool(tile.TileType == _grassTileType ? 2 : 3)) {
                        WorldGen.PlaceTile(i, j + 1, (tile.WallType == _grassWallType || Main.tile[i, j + 1].WallType == _grassWallType || tile.WallType == _leavesWallType || Main.tile[i, j + 1].WallType == _leavesWallType) ? _vinesTileType2 : _vinesTileType);
                    }
                }
                if (tile.TileType == _vinesTileType) {
                    WorldGenHelper.PlaceVines(i, j, _random.Next(1, 5), _vinesTileType);
                }
                if (tile.TileType == _vinesTileType2) {
                    WorldGenHelper.PlaceVines(i, j, _random.Next(3, 7), _vinesTileType2);
                }
            }
        }

        progress.Set(1f);
        Step16_PlaceAltar();

        progress.Message = Language.GetOrRegister("Mods.RoA.WorldGen.Backwoods5").Value;

        // place plants
        for (int i = Left - 100; i <= Right + 100; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < CenterY + 20; j++) {
                Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(_grassTileType) && aboveTile.ActiveTile(TileID.Trees)) {
                    int count2 = 8;
                    for (int i3 = -1; i3 < 2; i3++) {
                        for (int j3 = -1; j3 < 2; j3++) {
                            if (i3 != 0 || j3 != 0) {
                                if (!Main.tile[i + i3, j + j3].HasTile) {
                                    count2--;
                                }
                            }
                        }
                    }
                    if (count2 <= 1) {
                        WorldGen.KillTile(i, j);
                        WorldGen.KillTile(i, j - 1);
                    }
                }
                if (tile.ActiveTile(_grassTileType) && !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock && !aboveTile.HasTile) {
                    tile = WorldGenHelper.GetTileSafely(i, j - 1);
                    tile.HasTile = true;
                    tile.TileFrameY = 0;
                    if (_random.NextChance(0.15)) {
                        tile.TileType = _bushTileType;
                        tile.TileFrameX = (short)(34 * _random.Next(4));
                    }
                    else {
                        if (_random.NextBool(21)) {
                            tile.TileType = _mintTileType;
                            tile.TileFrameX = (short)(18 * (_random.Next(8) < 2 ? 2 : _random.NextBool() ? 0 : 1));
                            //ModContent.GetInstance<MiracleMintTE>().Place(i, j - 1);
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

        for (int i = Left - 15; i <= Right + 15; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < CenterY; j++) {
                bool flag = j < BackwoodsVars.FirstTileYAtCenter + 15 ? _random.NextBool(6) : _random.NextBool(4);
                if (WorldGenHelper.GetTileSafely(i, j).WallType == _grassWallType && flag) {
                    WorldGen.PlaceLiquid(i, j, 0, (byte)(255 - _random.Next(100)));
                }
            }
        }
        WorldGen.gen = false;
        void settleLiquids() {
            Liquid.worldGenTilesIgnoreWater(ignoreSolids: true);
            Liquid.QuickWater(3);
            WorldGen.WaterCheck();
            int num606 = 0;
            Liquid.quickSettle = true;
            int num607 = 10;
            while (num606 < num607) {
                int num608 = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;
                num606++;
                double num609 = 0.0;
                int num610 = num608 * 5;
                while (Liquid.numLiquid > 0) {
                    num610--;
                    if (num610 < 0)
                        break;

                    double num611 = (double)(num608 - (Liquid.numLiquid + LiquidBuffer.numLiquidBuffer)) / (double)num608;
                    if (Liquid.numLiquid + LiquidBuffer.numLiquidBuffer > num608)
                        num608 = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;

                    if (num611 > num609)
                        num609 = num611;
                    else
                        num611 = num609;

                    //if (num606 == 1)
                    //    progress.Set(num611 / 3.0 + 0.33);

                    int num612 = 10;
                    if (num606 > num612)
                        num612 = num606;

                    Liquid.UpdateLiquid();
                }

                WorldGen.WaterCheck();
                //progress.Set((double)num606 * 0.1 / 3.0 + 0.66);
            }

            Liquid.quickSettle = false;
            Liquid.worldGenTilesIgnoreWater(ignoreSolids: false);
        }
        settleLiquids();
        WorldGen.gen = true;

        Step14_ClearRockLayerWalls();
        cleanUp();

        Step_WallVariety();

        PlaceGateway(true);

        double count = (WorldGenHelper.BigWorld ? (Main.maxTilesX * 0.04) : WorldGenHelper.SmallWorld ? (Main.maxTilesX * 0.08) : (Main.maxTilesX * 0.055)) */* * 0.5f */1f;
        for (int num555 = 0; num555 < count; num555++) {
            //progress.Set((float)(num555 + 1) / count);
            GenerateLootRoom1();
        }

        Step9_SpreadMoss();
        //Step10_SpreadMossGrass();

        //Step_AddJawTraps();

        Step_AddPills();
        Step_AddSpikes();

        Step10_SpreadMossGrass();

        for (int num686 = 0; num686 < _biomeWidth * 2; num686++) {
            int num687 = _random.Next(Left - 30, Right + 30);
            int y = Math.Min(CenterY - EdgeY, (int)Main.worldSurface);
            int num688 = _random.Next((int)y - EdgeY - _random.Next(EdgeY), (int)Main.worldSurface + 20);
            if (Main.tile[num687, num688].WallType == 2 || Main.tile[num687, num688].WallType == _dirtWallType)
                WorldGenHelper.ModifiedDirtyRockRunner(num687, num688, _dirtWallType, 2);
        }


        //Step15_AddLootRooms();
    }

    private void FlowerGrassRunner(int i, int j) {
        double num = _random.Next(2, 6);
        double num2 = _random.Next(5, 50);
        double num3 = num2;
        Vector2D vector2D = default(Vector2D);
        vector2D.X = i;
        vector2D.Y = j;
        Vector2D vector2D2 = default(Vector2D);
        vector2D2.X = (double)_random.Next(-10, 11) * 0.1;
        vector2D2.Y = (double)_random.Next(-10, 11) * 0.1;
        while (num > 0.0 && num3 > 0.0) {
            double num4 = num * (num3 / num2);
            num3 -= 1.0;
            int num5 = (int)(vector2D.X - num4 * 0.5);
            int num6 = (int)(vector2D.X + num4 * 0.5);
            int num7 = (int)(vector2D.Y - num4 * 0.5);
            int num8 = (int)(vector2D.Y + num4 * 0.5);
            if (num5 < 0)
                num5 = 0;

            if (num6 > Main.maxTilesX)
                num6 = Main.maxTilesX;

            if (num7 < 0)
                num7 = 0;

            if (num8 > Main.maxTilesY)
                num8 = Main.maxTilesY;

            for (int k = num5; k < num6; k++) {
                for (int l = num7; l < num8; l++) {
                    if (Math.Abs((double)k - vector2D.X) + Math.Abs((double)l - vector2D.Y) < num * 0.5 * (1.0 + (double)_random.Next(-10, 11) * 0.015) && (Main.tile[k, l].WallType == _grassWallType || Main.tile[k, l].WallType == 63))
                        Main.tile[k, l].WallType = _flowerGrassWallType;
                }
            }

            vector2D += vector2D2;
            vector2D2.X += (double)_random.Next(-10, 11) * 0.05;
            if (vector2D2.X > 1.0)
                vector2D2.X = 1.0;

            if (vector2D2.X < -1.0)
                vector2D2.X = -1.0;

            vector2D2.Y += (double)_random.Next(-10, 11) * 0.05;
            if (vector2D2.Y > 1.0)
                vector2D2.Y = 1.0;

            if (vector2D2.Y < -1.0)
                vector2D2.Y = -1.0;
        }
    }

    private void Step7_AddStone() {
        int tileCount = (int)(Main.maxTilesX * Main.maxTilesY * 0.0005) * 375;
        int startX = Left - 1;
        int endX = Right + 1;
        int worldSize = Main.maxTilesX / 4200;
        int k = worldSize == 1 ? (int)(_biomeHeight * 0.25) : worldSize == 2 ? (int)(_biomeHeight * 0.2) : (int)(_biomeHeight * 0.15);
        int y2 = BackwoodsVars.FirstTileYAtCenter;
        int minY = CenterY - EdgeY;
        int stoneCount = (int)(tileCount * 0.000525f);
        int x;
        for (int i = 0; i < stoneCount; i++) {
            x = _random.Next(startX - 50, endX + 50);
            int y = _random.Next(y2, minY);
            if ((((double)x > (double)CenterX - 10 && (double)x < (double)CenterX + 10)) && y < BackwoodsVars.FirstTileYAtCenter + EdgeY) {
                continue;
            }
            int sizeX = _random.Next(4, 9);
            int sizeY = _random.Next(5, 18);
            if (WorldGenHelper.ActiveTile(x, y, _dirtTileType)) {
                float progress = (float)(y - y2) / (minY - y2);
                if (!_random.NextChance(0.25f + progress * 1.25f)) {
                    continue;
                }
                WorldGen.TileRunner(x, y, sizeX, sizeY, _stoneTileType);
            }
        }
        minY = CenterY - EdgeY;
        stoneCount = (int)(tileCount * 0.0035f);
        int maxY = CenterY + (int)(EdgeY * 1.5f);
        for (int i = 0; i < stoneCount; i++) {
            x = _random.Next(startX - 50, endX + 50);
            int y = _random.Next(minY, maxY);
            int sizeX = _random.Next(2, 7);
            int sizeY = _random.Next(2, 23);
            if (WorldGenHelper.ActiveTile(x, y, _dirtTileType)) {
                WorldGen.TileRunner(x, y, sizeX, sizeY, _stoneTileType);
            }
        }
        stoneCount = (int)(tileCount * 0.0055f);
        for (int i = 0; i < stoneCount; i++) {
            x = _random.Next(startX - 50, endX + 50);
            int maxY2 = Bottom + EdgeY * 2;
            int y = _random.Next(maxY, maxY2);
            int sizeX = _random.Next(4, 10);
            int sizeY = _random.Next(5, 30);
            if (WorldGenHelper.ActiveTile(x, y, _dirtTileType)) {
                WorldGen.TileRunner(x, y, sizeX, sizeY, _stoneTileType);
            }
        }
    }

    private void Step_WallVariety() {
        double num568 = (double)(Main.maxTilesX * Main.maxTilesY) / 5040000.0;
        int num569 = (int)(1000.0 * num568);
        int num570 = num569;
        ShapeData shapeData = new ShapeData();
        while (num569 > 0) {
            Point point2 = new Point(_random.Next(Left - 15, Right + 15), _random.Next(BackwoodsVars.FirstTileYAtCenter, Bottom + 15));
            //while (Vector2D.Distance(new Vector2D(point2.X, point2.Y), GenVars.shimmerPosition) < (double)WorldGen.shimmerSafetyDistance) {
            //    point2 = new Point(_random.Next(Left - 15, Right + 15), _random.Next(Top - 15, Bottom + 15));
            //}
            Tile tile6 = Main.tile[point2.X, point2.Y];
            Tile tile7 = Main.tile[point2.X, point2.Y - 1];
            ushort num571 = 0;
            if ((tile6.TileType == _stoneTileType || tile6.TileType == _mossTileType || tile6.TileType == TileID.Stone) && tile7.WallType == 0)
                num571 = (((double)point2.Y < CenterY + EdgeY/* + EdgeY / 3*/) ? ((ushort)(196 + _random.Next(4))) : (_random.NextBool() ? (ushort)(_random.NextBool() ? 170 : 171) : ((ushort)(212 + _random.Next(4)))));

            if (tile6.HasTile && num571 != 0) {
                bool flag34 = WorldUtils.Gen(new Point(point2.X, point2.Y - 1), new ShapeFloodFill(1000), Actions.Chain(new Modifiers.IsNotSolid(), new Actions.Blank().Output(shapeData)));
                if (shapeData.Count > 50 && flag34) {
                    WorldUtils.Gen(new Point(point2.X, point2.Y), new ModShapes.OuterOutline(shapeData, useDiagonals: true, useInterior: true), Actions.Chain(new Modifiers.SkipWalls(87), new Modifiers.SkipWalls(_elderwoodWallType, _grassWallType), new Actions.PlaceWall(num571)));
                }

                shapeData.Clear();

                num569--;
            }
        }
        num568 = (double)(Main.maxTilesX * Main.maxTilesY) / 5040000.0;
        num569 = (int)(300.0 * num568);
        num570 = num569;
        shapeData = new ShapeData();
        while (num569 > 0) {
            Point point2 = new Point(_random.Next(Left - 15, Right + 15), _random.Next(CenterY - EdgeY / 2, Bottom + 15));
            //while (Vector2D.Distance(new Vector2D(point2.X, point2.Y), GenVars.shimmerPosition) < (double)WorldGen.shimmerSafetyDistance) {
            //    point2 = new Point(_random.Next(Left - 15, Right + 15), _random.Next(Top - 15, Bottom + 15));
            //}
            Tile tile6 = Main.tile[point2.X, point2.Y];
            Tile tile7 = Main.tile[point2.X, point2.Y - 1];
            ushort num571 = 0;
            if ((tile6.TileType == _stoneTileType || tile6.TileType == _mossTileType || tile6.TileType == TileID.Stone) && tile7.WallType == 0)
                num571 = (((double)point2.Y < CenterY + EdgeY/* + EdgeY / 3*/) ? ((ushort)0) : (_random.NextBool() ? (ushort)(_random.NextBool() ? 170 : 171) : ((ushort)(212 + _random.Next(4)))));

            if (tile6.HasTile && num571 != 0) {
                bool flag34 = WorldUtils.Gen(new Point(point2.X, point2.Y - 1), new ShapeFloodFill(1000), Actions.Chain(new Modifiers.IsNotSolid(), new Actions.Blank().Output(shapeData)));
                if (shapeData.Count > 50 && flag34) {
                    WorldUtils.Gen(new Point(point2.X, point2.Y), new ModShapes.OuterOutline(shapeData, useDiagonals: true, useInterior: true), Actions.Chain(new Modifiers.SkipWalls(87), new Modifiers.SkipWalls(_elderwoodWallType, _grassWallType), new Actions.PlaceWall(num571)));
                }

                shapeData.Clear();

                num569--;
            }
        }
    }

    public static Point PlaceCattail(int x, int j) {
        int num = j;
        Point result = new(-1, -1);
        if (x < 50 || x > Main.maxTilesX - 50 || num < 50 || num > Main.maxTilesY - 50)
            return result;

        if (Main.tile[x, num].HasTile || Main.tile[x, num].LiquidAmount == 0 || Main.tile[x, num].LiquidType != 0)
            return result;

        while (Main.tile[x, num].LiquidAmount > 0 && num > 50) {
            num--;
        }

        num++;
        if (Main.tile[x, num].HasTile || Main.tile[x, num - 1].HasTile || Main.tile[x, num].LiquidAmount == 0 || Main.tile[x, num].LiquidType != 0)
            return result;

        //if (Main.tile[x, num].WallType != 0 && Main.tile[x, num].wall != 80 && Main.tile[x, num].wall != 81 && Main.tile[x, num].wall != 69 && (Main.tile[x, num].wall < 63 || Main.tile[x, num].wall > 68))
        //    return result;

        int tileType = ModContent.TileType<BackwoodsCatTail>();
        int num2 = 7;
        int num3 = 0;
        for (int i = x - num2; i <= x + num2; i++) {
            for (int k = num - num2; k <= num + num2; k++) {
                if (Main.tile[i, k].HasTile && Main.tile[i, k].TileType == tileType) {
                    num3++;
                    break;
                }
            }
        }

        if (num3 > 3)
            return result;

        int l;
        for (l = num; (!Main.tile[x, l].HasTile || !Main.tileSolid[Main.tile[x, l].TileType] || Main.tileSolidTop[Main.tile[x, l].TileType]) && l < Main.maxTilesY - 50; l++) {
            if (Main.tile[x, l].HasTile)
                return result;
        }

        int catTailDistance = 8;
        int num4 = catTailDistance - 1;
        if (l - num > num4)
            return result;

        if (l - num < 2)
            return result;

        int type = Main.tile[x, l].TileType;
        //if (!Main.tile[x, l].HasUnactuatedTile)
        //    return result;

        int num5 = -1;

        if (BackwoodsVars.BackwoodsTileTypes.Contains((ushort)type)) {
            num5 = 0;
        }
        //switch (type) {
        //    case 2:
        //    case 477:
        //        num5 = 0;
        //        break;
        //    case 53:
        //        if (x < WorldGen.beachDistance || x > Main.maxTilesX - beachDistance)
        //            return result;
        //        num5 = 18;
        //        break;
        //    case 199:
        //    case 234:
        //    case 662:
        //        num5 = 54;
        //        break;
        //    case 23:
        //    case 112:
        //    case 661:
        //        num5 = 72;
        //        break;
        //    case 70:
        //        num5 = 90;
        //        break;
        //}

        if (num5 < 0)
            return result;

        if (Main.tile[x, l].TopSlope && WorldGen.gen && _random.Next(3) != 0) {
            Tile checkTile = Main.tile[x, l];
            checkTile.Slope = 0;
        }
        else if (Main.tile[x, l].TopSlope || Main.tile[x, l].IsHalfBlock)
            return result;

        num = l - 1;
        Tile tile = Main.tile[x, num];
        if (tile.HasTile) {
            return result;
        }
        tile.HasTile = true;
        tile.TileType = (ushort)tileType;
        tile.TileFrameX = 0;
        tile.TileFrameY = (short)num5;
        tile.IsHalfBlock = false;
        tile.Slope = 0;
        tile.CopyPaintAndCoating(Main.tile[x, num + 1]);
        //WorldGen.SquareTileFrame(x, num);
        return new Point(x, num);
    }

    public static void GrowCattail(int x, int j) {
        //if (Main.netMode == 1)
        //    return;

        int num = j;
        while (Main.tile[x, num].LiquidAmount > 0 && num > 50) {
            num--;
        }

        num++;
        int i;
        for (i = num; (!Main.tile[x, i].HasTile || !Main.tileSolid[Main.tile[x, i].TileType] || Main.tileSolidTop[Main.tile[x, i].TileType]) && i < Main.maxTilesY - 50; i++) {
        }

        int tileType = ModContent.TileType<BackwoodsCatTail>();
        num = i - 1;
        while (Main.tile[x, num].HasTile && Main.tile[x, num].TileType == tileType) {
            num--;
        }

        num++;
        if (Main.tile[x, num].TileFrameX == 90 && Main.tile[x, num - 1].HasTile && Main.tileCut[Main.tile[x, num - 1].TileType]) {
            WorldGen.KillTile(x, num - 1);
            if (Main.netMode == 2)
                NetMessage.SendData(17, -1, -1, null, 0, x, num - 1);
        }

        if (Main.tile[x, num - 1].HasTile)
            return;

        if (Main.tile[x, num].TileFrameX == 0) {
            Main.tile[x, num].TileFrameX = 18;
            //WorldGen.SquareTileFrame(x, num);
            //if (Main.netMode == 2)
            //    NetMessage.SendTileSquare(-1, x, num);
        }
        else if (Main.tile[x, num].TileFrameX == 18) {
            Main.tile[x, num].TileFrameX = (short)(18 * _random.Next(2, 5));
            Tile tile = Main.tile[x, num - 1];
            tile.HasTile = true;
            tile.TileType = (ushort)tileType;
            tile.TileFrameX = 90;
            tile.TileFrameY = Main.tile[x, num].TileFrameY;
            tile.IsHalfBlock = false;
            tile.Slope = 0;
            Main.tile[x, num - 1].CopyPaintAndCoating(Main.tile[x, num]);
            //WorldGen.SquareTileFrame(x, num);
            //if (Main.netMode == 2)
            //    NetMessage.SendTileSquare(-1, x, num);
        }
        else if (Main.tile[x, num].TileFrameX == 90) {
            if (Main.tile[x, num - 1].LiquidAmount == 0) {
                if (!Main.tile[x, num - 2].HasTile && (Main.tile[x, num].LiquidAmount > 0 || Main.tile[x, num + 1].LiquidAmount > 0 || Main.tile[x, num + 2].LiquidAmount > 0) && _random.Next(3) == 0) {
                    Main.tile[x, num].TileFrameX = 108;
                    Tile tile = Main.tile[x, num - 1];
                    tile.HasTile = true;
                    tile.TileType = (ushort)tileType;
                    tile.TileFrameX = 90;
                    tile.TileFrameY = Main.tile[x, num].TileFrameY;
                    tile.IsHalfBlock = false;
                    tile.Slope = 0;
                    tile.CopyPaintAndCoating(Main.tile[x, num]);
                    //WorldGen.SquareTileFrame(x, num);
                }
                else {
                    int num2 = _random.Next(3);
                    Main.tile[x, num].TileFrameX = (short)(126 + num2 * 18);
                    Tile tile = Main.tile[x, num - 1];
                    tile.HasTile = true;
                    tile.TileType = (ushort)tileType;
                    tile.TileFrameX = (short)(180 + num2 * 18);
                    tile.TileFrameY = Main.tile[x, num].TileFrameY;
                    tile.IsHalfBlock = false;
                    tile.Slope = 0;
                    tile.CopyPaintAndCoating(Main.tile[x, num]);
                    //WorldGen.SquareTileFrame(x, num);
                }
            }
            else {
                Main.tile[x, num].TileFrameX = 108;
                Tile tile = Main.tile[x, num - 1];
                tile.HasTile = true;
                tile.TileType = (ushort)tileType;
                tile.TileFrameX = 90;
                tile.TileFrameY = Main.tile[x, num].TileFrameY;
                tile.IsHalfBlock = false;
                tile.Slope = 0;
                tile.CopyPaintAndCoating(Main.tile[x, num]);
                //WorldGen.SquareTileFrame(x, num);
            }
        }

        //WorldGen.SquareTileFrame(x, num - 1, resetFrame: false);
        //if (Main.netMode == 2)
        //    NetMessage.SendTileSquare(-1, x, num - 1, 1, 2);
    }

    private void Step_AddCatTails() {
        for (int i = Left - 100; i <= Right + 100; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < Bottom + EdgeY; j++) {
                if (WorldGenHelper.GetTileSafely(i, j).LiquidAmount > 0 && !WorldGenHelper.GetTileSafely(i, j).HasTile && !Main.tile[i, j - 1].HasTile) {
                    PlaceBackwoodsCattail(i, j);
                }
            }
        }
    }

    public static void PlaceBackwoodsCattail(int i, int j) {
        int right = BackwoodsVars.BackwoodsCenterX + BackwoodsVars.BackwoodsHalfSizeX - 100;
        int left = BackwoodsVars.BackwoodsCenterX - BackwoodsVars.BackwoodsHalfSizeX + 100;
        int x = i, num = j;

        Point point = PlaceCattail(i, j);
        if (WorldGen.InWorld(point.X, point.Y)) {
            int num31 = _random.Next(14);
            for (int num32 = 0; num32 < num31; num32++) {
                GrowCattail(point.X, point.Y);
            }

            //WorldGen.SquareTileFrame(point.X, point.Y);
        }
    }

    private void Step_AddLilypads() {
        for (int i = Left - 100; i <= Right + 100; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < Bottom + EdgeY; j++) {
                if (WorldGenHelper.GetTileSafely(i, j).LiquidAmount > 0 && !(Main.tile[i, j + 1].HasTile && Main.tile[i, j + 1].TileType == ModContent.TileType<BackwoodsLilypad>())) {
                    PlaceBackwoodsLilypad(i, j, Right, Left);
                }
                //if (Main.netMode == 2)
                //    NetMessage.SendTileSquare(-1, i, j);
                //else if (genRand.Next(600) == 0) {
                //    PlaceTile(i, j, 519, mute: true);
                //    if (Main.netMode == 2)
                //        NetMessage.SendTileSquare(-1, i, j);
                //}
            }
        }
    }

    public static bool PlaceBackwoodsLilypad(int x, int j, int right, int left) {
        int num = j;
        int tileType = ModContent.TileType<BackwoodsLilypad>();
        if (x < 50 || x > Main.maxTilesX - 50 || num < 50 || num > Main.maxTilesY - 50)
            return false;

        if (x < left || x > right || num < 50 || num > Main.maxTilesY - 50)
            return false;

        if (num > BackwoodsVars.BackwoodsCenterY + BackwoodsVars.BackwoodsSizeY / 2 + 200)
            return false;

        if (Main.tile[x, num].HasTile || Main.tile[x, num].LiquidAmount == 0 || Main.tile[x, num].LiquidType != 0)
            return false;

        while (Main.tile[x, num].LiquidAmount > 0 && num > 50) {
            num--;
        }

        num++;
        if (Main.tile[x, num].HasTile || Main.tile[x, num - 1].HasTile || Main.tile[x, num].LiquidAmount == 0 || Main.tile[x, num].LiquidType != 0)
            return false;

        //if (Main.tile[x, num].WallType != 0 && Main.tile[x, num].WallType != 15 && Main.tile[x, num].WallType != 70 && (Main.tile[x, num].WallType < 63 || Main.tile[x, num].WallType > 68))
        //    return false;

        int num2 = 5;
        int num3 = 0;
        for (int i = x - num2; i <= x + num2; i++) {
            for (int k = num - num2; k <= num + num2; k++) {
                if (Main.tile[i, k].HasTile && Main.tile[i, k].TileType == tileType)
                    num3++;
            }
        }

        if (num3 > 3)
            return false;

        int l;
        for (l = num; (!Main.tile[x, l].HasTile || !Main.tileSolid[Main.tile[x, l].TileType] || Main.tileSolidTop[Main.tile[x, l].TileType]) && l < Main.maxTilesY - 50; l++) {
            if (Main.tile[x, l].HasTile && Main.tile[x, l].TileType == ModContent.TileType<BackwoodsCatTail>())
                return false;
        }

        int num4 = 12;
        if (l - num > num4)
            return false;

        if (l - num < 3)
            return false;

        int type = Main.tile[x, l].TileType;
        int num5 = -1;
        //if (type == 2 || type == 477)
        //    num5 = 0;

        //if (type == 109 || type == 109 || type == 116)
        //    num5 = 18;

        //if (type == 60)
        //    num5 = 36;

        if (BackwoodsVars.BackwoodsTileTypes.Contains((ushort)type)) {
            num5 = 0;
        }

        if (num5 < 0)
            return false;

        Tile tile = Main.tile[x, num];
        if (tile.HasTile) {
            return false;
        }
        tile.HasTile = true;
        Main.tile[x, num].TileType = (ushort)tileType;
        if (_random.Next(2) == 0) {
            Main.tile[x, num].TileFrameX = (short)(18 * _random.Next(3));
        }
        else if (_random.Next(15) == 0) {
            Main.tile[x, num].TileFrameX = (short)(18 * _random.Next(18));
        }
        else {
            int num6 = (right - left) / 5;
            if (x < left + num6)
                Main.tile[x, num].TileFrameX = (short)(18 * _random.Next(6, 9));
            else if (x < left + num6 * 2)
                Main.tile[x, num].TileFrameX = (short)(18 * _random.Next(9, 12));
            else if (x < left + num6 * 3)
                Main.tile[x, num].TileFrameX = (short)(18 * _random.Next(3, 6));
            else if (x < left + num6 * 4)
                Main.tile[x, num].TileFrameX = (short)(18 * _random.Next(15, 18));
            else
                Main.tile[x, num].TileFrameX = (short)(18 * _random.Next(12, 15));
        }

        Main.tile[x, num].TileFrameY = (short)num5;
        //WorldGen.SquareTileFrame(x, num);
        return true;
    }

    private void Step17_AddStatues() {
        int statueCount = 15 + 10 * (WorldGenHelper.WorldSize - 1);
        int minY = BackwoodsVars.FirstTileYAtCenter + 15;
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
            int i = _random.Next(Left - 15, Right + 15);
            int j = _random.Next(Top, Bottom);

            if (WorldGenHelper.GetTileSafely(i, j).HasTile && WorldGenHelper.GetTileSafely(i, j).TileType == _dirtTileType) {
                WorldGen.TileRunner(i, j, _random.Next(5, 12), _random.Next(5, 12), tier1, false, 0f, 0f, false, true);
            }
        }

        for (int iron = 0; iron < (int)(oreAmount * 7E-05); iron++) {
            int i = _random.Next(Left - 15, Right + 15);
            int j = _random.Next(Top, Bottom);

            if (WorldGenHelper.GetTileSafely(i, j).HasTile && WorldGenHelper.GetTileSafely(i, j).TileType == _dirtTileType) {
                WorldGen.TileRunner(i, j, _random.Next(4, 10), _random.Next(4, 10), tier2, false, 0f, 0f, false, true);
            }
        }

        int y2 = BackwoodsVars.FirstTileYAtCenter;
        int minY = y2 + _biomeHeight / 3;
        for (int silver = 0; silver < (int)(oreAmount * 6E-05); silver++) {
            int i = _random.Next(Left - 15, Right + 15);
            int j = _random.Next(minY, Bottom);

            if (WorldGenHelper.GetTileSafely(i, j).HasTile && WorldGenHelper.GetTileSafely(i, j).TileType == _dirtTileType) {
                WorldGen.TileRunner(i, j, _random.Next(3, 8), _random.Next(3, 8), tier3, false, 0f, 0f, false, true);
            }
        }

        for (int gold = 0; gold < (int)(oreAmount * 5E-05); gold++) {
            int i = _random.Next(Left - 15, Right + 15);
            int j = _random.Next(minY + _biomeHeight / 3, Bottom);

            if (WorldGenHelper.GetTileSafely(i, j).HasTile && WorldGenHelper.GetTileSafely(i, j).TileType == _dirtTileType) {
                WorldGen.TileRunner(i, j, _random.Next(3, 8), _random.Next(3, 8), tier4, false, 0f, 0f, false, true);
            }
        }
    }

    private void Step9_SpreadMoss() {
        for (int i = Left - 50; i < Right + 50; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < Bottom + EdgeY + 25; j++) {
                if (WorldGenHelper.ActiveTile(i, j, _stoneTileType) &&
                    (!WorldGenHelper.GetTileSafely(i - 1, j).HasTile || !WorldGenHelper.GetTileSafely(i + 1, j).HasTile ||
                    !WorldGenHelper.GetTileSafely(i, j - 1).HasTile || !WorldGenHelper.GetTileSafely(i, j + 1).HasTile)) {
                    Spread(i, j, _stoneTileType, _mossTileType);
                }
            }
        }
    }

    private void Step5_CleanUp() {
        foreach (Point surface in _biomeSurface) {
            for (int j = -(_biomeHeight / 3 + 10); j < -2; j++) {
                if (surface.Y + j < WorldGenHelper.SafeFloatingIslandY) {
                    continue;
                }

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
            for (int j = -_biomeHeight / 4; j < 0; j++) {
                if (surface.Y + j < WorldGenHelper.SafeFloatingIslandY) {
                    continue;
                }

                if (WorldGenHelper.IsCloud(surface.X, surface.Y + j)) {
                    break;
                }
                Tile tile = WorldGenHelper.GetTileSafely(surface.X, surface.Y + j);
                if (SandTileTypes.Contains(tile.TileType)/* || MidInvalidTileTypesToKill.Contains(tile.TileType)*/) {
                    WorldGen.KillTile(surface.X, surface.Y + j);
                }
            }
        }

        foreach (Point surface in _biomeSurface) {
            for (int j = -_biomeHeight / 3; j < 2; j++) {
                if (surface.Y + j < WorldGenHelper.SafeFloatingIslandY) {
                    continue;
                }

                if (WorldGenHelper.IsCloud(surface.X, surface.Y + j)) {
                    break;
                }
                Tile tile = WorldGenHelper.GetTileSafely(surface.X, surface.Y + j);
                if (SandTileTypes.Contains(tile.TileType)) {
                    if (j < 0) {
                        WorldGen.KillTile(surface.X, surface.Y + j);
                    }
                    //else if (_random.NextBool(3)) {
                    //    WorldGenHelper.ReplaceTile(surface.X, surface.Y + j, _dirtTileType);
                    //}
                }
            }
        }

        bool init = false;
        int startY = -10, endY = (int)Main.worldSurface - 20;
        int sizeY = (int)MathF.Abs(endY - startY);
        int[] sandX = new int[sizeY + 1];
        foreach (Point surface in _biomeSurface) {
            for (int j = startY; j < endY; j++) {
                float progress = (float)(j - startY) / (endY - startY);
                int index = (int)(progress * sizeY);
                if (!init) {
                    sandX[index] = sandX[Math.Max(0, index - 1)];
                    sandX[index] += _random.Next(-1, 2);
                    if (MathF.Abs(sandX[index]) > 5) {
                        sandX[index] = 5 * MathF.Sign(sandX[index]);
                    }
                }
            }
            for (int j = startY; j < endY; j++) {
                float progress = (float)(j - startY) / (endY - startY);
                int index = (int)(progress * sizeY);
                if (surface.Y + j < WorldGenHelper.SafeFloatingIslandY) {
                    continue;
                }

                if (WorldGenHelper.IsCloud(surface.X + sandX[index], surface.Y + j)) {
                    break;
                }
                for (int x = -2; x < 3; x++) {
                    int x2 = (int)(x * _random.NextBool().ToInt());
                    //if (surface.Y + j < surface.Y) {
                    //    x2 = 0;
                    //}
                    x2 = x;
                    Tile tile = WorldGenHelper.GetTileSafely(surface.X + sandX[index] + x2, surface.Y + j);
                    if (SandTileTypes.Contains(tile.TileType)) {
                        WorldGenHelper.ReplaceTile(surface.X + sandX[index] + x2, surface.Y + j, _dirtTileType);
                    }
                }
            }
            init = true;
        }

        for (int i = Left - 100; i < Right + 100; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < Bottom; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(CliffPlaceholderTileType)) {
                    WorldGenHelper.ReplaceTile(i, j, _dirtTileType);
                    if (_random.NextChance(0.005)) {
                        int sizeX2 = _random.Next(4, 9);
                        int sizeY2 = _random.Next(5, 18);
                        WorldGen.TileRunner(i, j, sizeX2, sizeY2, _stoneTileType);
                    }
                }
            }
        }
    }

    public void ReplaceAllSnowBlockForSpiritModSupport(GenerationProgress progress, GameConfiguration config) {
        _grassTileType = (ushort)ModContent.TileType<BackwoodsGrass>();
        for (int i = Left - 105; i < Right + 105; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < Bottom; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.ActiveTile(TileID.Ebonstone)) {
                    tile.TileType = _grassTileType;
                }
            }
        }
        if (/*ModLoader.HasMod("SpiritReforged") || */BackwoodsWorldGen._extraModSupport) {
            for (int num696 = Left - 100; num696 < Right + 100; num696++) {
                double num697 = (double)num696 / (double)Main.maxTilesX;
                bool flag43 = true;
                for (int num698 = 0; (double)num698 < Main.worldSurface; num698++) {
                    if (flag43) {
                        if (Main.tile[num696, num698].WallType == 2 || Main.tile[num696, num698].WallType == 40 || Main.tile[num696, num698].WallType == 64 || Main.tile[num696, num698].WallType == 86)
                            Main.tile[num696, num698].WallType = 0;

                        if (Main.tile[num696, num698].TileType != 53 && Main.tile[num696, num698].TileType != 112 && Main.tile[num696, num698].TileType != 234) {
                            if (Main.tile[num696 - 1, num698].WallType == 2 || Main.tile[num696 - 1, num698].WallType == 40 || Main.tile[num696 - 1, num698].WallType == 40)
                                Main.tile[num696 - 1, num698].WallType = 0;

                            if ((Main.tile[num696 - 2, num698].WallType == 2 || Main.tile[num696 - 2, num698].WallType == 40 || Main.tile[num696 - 2, num698].WallType == 40) && _random.Next(2) == 0)
                                Main.tile[num696 - 2, num698].WallType = 0;

                            if ((Main.tile[num696 - 3, num698].WallType == 2 || Main.tile[num696 - 3, num698].WallType == 40 || Main.tile[num696 - 3, num698].WallType == 40) && _random.Next(2) == 0)
                                Main.tile[num696 - 3, num698].WallType = 0;

                            if (Main.tile[num696 + 1, num698].WallType == 2 || Main.tile[num696 + 1, num698].WallType == 40 || Main.tile[num696 + 1, num698].WallType == 40)
                                Main.tile[num696 + 1, num698].WallType = 0;

                            if ((Main.tile[num696 + 2, num698].WallType == 2 || Main.tile[num696 + 2, num698].WallType == 40 || Main.tile[num696 + 2, num698].WallType == 40) && _random.Next(2) == 0)
                                Main.tile[num696 + 2, num698].WallType = 0;

                            if ((Main.tile[num696 + 3, num698].WallType == 2 || Main.tile[num696 + 3, num698].WallType == 40 || Main.tile[num696 + 3, num698].WallType == 40) && _random.Next(2) == 0)
                                Main.tile[num696 + 3, num698].WallType = 0;

                            if (Main.tile[num696, num698].HasTile)
                                flag43 = false;
                        }
                    }
                    else if (Main.tile[num696, num698].WallType == 0 && Main.tile[num696, num698 + 1].WallType == 0 && Main.tile[num696, num698 + 2].WallType == 0 && Main.tile[num696, num698 + 3].WallType == 0 && Main.tile[num696, num698 + 4].WallType == 0 && Main.tile[num696 - 1, num698].WallType == 0 && Main.tile[num696 + 1, num698].WallType == 0 && Main.tile[num696 - 2, num698].WallType == 0 && Main.tile[num696 + 2, num698].WallType == 0 && !Main.tile[num696, num698].HasTile && !Main.tile[num696, num698 + 1].HasTile && !Main.tile[num696, num698 + 2].HasTile && !Main.tile[num696, num698 + 3].HasTile) {
                        flag43 = true;
                    }
                }
            }

            for (int num699 = Right + 100; num699 >= Left - 100; num699--) {
                double num700 = (double)num699 / (double)Main.maxTilesX;
                bool flag44 = true;
                for (int num701 = 0; (double)num701 < Main.worldSurface; num701++) {
                    if (flag44) {
                        if (Main.tile[num699, num701].WallType == 2 || Main.tile[num699, num701].WallType == 40 || Main.tile[num699, num701].WallType == 64)
                            Main.tile[num699, num701].WallType = 0;

                        if (Main.tile[num699, num701].TileType != 53) {
                            if (Main.tile[num699 - 1, num701].WallType == 2 || Main.tile[num699 - 1, num701].WallType == 40 || Main.tile[num699 - 1, num701].WallType == 40)
                                Main.tile[num699 - 1, num701].WallType = 0;

                            if ((Main.tile[num699 - 2, num701].WallType == 2 || Main.tile[num699 - 2, num701].WallType == 40 || Main.tile[num699 - 2, num701].WallType == 40) && _random.Next(2) == 0)
                                Main.tile[num699 - 2, num701].WallType = 0;

                            if ((Main.tile[num699 - 3, num701].WallType == 2 || Main.tile[num699 - 3, num701].WallType == 40 || Main.tile[num699 - 3, num701].WallType == 40) && _random.Next(2) == 0)
                                Main.tile[num699 - 3, num701].WallType = 0;

                            if (Main.tile[num699 + 1, num701].WallType == 2 || Main.tile[num699 + 1, num701].WallType == 40 || Main.tile[num699 + 1, num701].WallType == 40)
                                Main.tile[num699 + 1, num701].WallType = 0;

                            if ((Main.tile[num699 + 2, num701].WallType == 2 || Main.tile[num699 + 2, num701].WallType == 40 || Main.tile[num699 + 2, num701].WallType == 40) && _random.Next(2) == 0)
                                Main.tile[num699 + 2, num701].WallType = 0;

                            if ((Main.tile[num699 + 3, num701].WallType == 2 || Main.tile[num699 + 3, num701].WallType == 40 || Main.tile[num699 + 3, num701].WallType == 40) && _random.Next(2) == 0)
                                Main.tile[num699 + 3, num701].WallType = 0;

                            if (Main.tile[num699, num701].HasTile)
                                flag44 = false;
                        }
                    }
                    else if (Main.tile[num699, num701].WallType == 0 && Main.tile[num699, num701 + 1].WallType == 0 && Main.tile[num699, num701 + 2].WallType == 0 && Main.tile[num699, num701 + 3].WallType == 0 && Main.tile[num699, num701 + 4].WallType == 0 && Main.tile[num699 - 1, num701].WallType == 0 && Main.tile[num699 + 1, num701].WallType == 0 && Main.tile[num699 - 2, num701].WallType == 0 && Main.tile[num699 + 2, num701].WallType == 0 && !Main.tile[num699, num701].HasTile && !Main.tile[num699, num701 + 1].HasTile && !Main.tile[num699, num701 + 2].HasTile && !Main.tile[num699, num701 + 3].HasTile) {
                        flag44 = true;
                    }
                }
            }
        }
    }

    private void Step0_Setup() {
        _oneChestPlacedInBigTree = false;
        _wandsAdded = false;
        _nextItemIndex = _nextItemIndex2 = 0;

        MidInvalidWallTypesToKill = [WallID.IridescentBrick, WallID.GoldBrick, WallID.RichMaogany, WallID.TinBrick, WallID.MudstoneBrick, WallID.LihzahrdBrickUnsafe, WallID.SandstoneBrick, WallID.EbonstoneEcho, WallID.EbonstoneUnsafe, WallID.CrimstoneEcho, WallID.CrimstoneUnsafe, WallID.GraniteUnsafe, WallID.MarbleUnsafe, WallID.Marble];
        SkipBiomeInvalidWallTypeToKill = [WallID.IridescentBrick, WallID.GoldBrick, WallID.RichMaogany, WallID.TinBrick, WallID.MudstoneBrick, WallID.LivingWoodUnsafe, WallID.SandstoneBrick, WallID.SmoothSandstone, WallID.HardenedSand, WallID.Sandstone, WallID.GraniteUnsafe, WallID.MarbleUnsafe, WallID.Marble, WallID.Granite];
        ushort tarWallType = (ushort)ModContent.WallType<SolidifiedTarWall_Unsafe>();
        MidInvalidWallTypesToKill.Add(tarWallType);
        SkipBiomeInvalidWallTypeToKill.Add(tarWallType);

        MustSkipWallTypes = [WallID.BlueDungeonSlabUnsafe, WallID.BlueDungeonTileUnsafe, WallID.BlueDungeonUnsafe, WallID.GreenDungeonSlabUnsafe, WallID.GreenDungeonUnsafe, WallID.GreenDungeonTileUnsafe,
            WallID.PinkDungeonSlabUnsafe, WallID.PinkDungeonTileUnsafe, WallID.PinkDungeonUnsafe];

        _backwoodsPlants.Clear();
        _biomeSurface.Clear();
        _altarTiles.Clear();
        _backwoodsPlants = [];
        _biomeSurface = [];
        _altarTiles = [];

        var config = ModContent.GetInstance<RoAServerConfig>();
        _biomeWidth = (int)(100 * config.BackwoodsWidthMultiplier);
        _biomeHeight = (int)(162 * config.BackwoodsHeightMultiplier);
        _biomeWidth += (int)(_biomeWidth * 1.35f * WorldGenHelper.WorldSize2);
        _biomeHeight += (int)(_biomeHeight * 1.35f * WorldGenHelper.WorldSize2);

        bool hasRemnants = ModLoader.HasMod("Remnants");

        //CenterX = GenVars.JungleX;

        int num785 = 400;
        var genRand = WorldGen.genRand;
        int num778 = Main.maxTilesX;
        int num779 = 0;
        int num780 = Main.maxTilesX;
        int num781 = 0;
        int num789 = num780;
        int num790 = num781;
        int num791 = num778;
        int num792 = num779;
        bool flag50 = false;
        int num793 = 0;
        int num794 = 0;
        int num795 = 0;
        bool drunkWorldGen = WorldGen.drunkWorldGen;
        bool remixWorldGen = WorldGen.remixWorldGen;
        bool tenthAnniversaryWorldGen = WorldGen.tenthAnniversaryWorldGen;
        CenterY = (int)Main.worldSurface;
        int attemtps = 0;
        while (!flag50) {
            float attemptProgress = (float)attemtps / 100;
            int num786 = (int)(_biomeWidth * 2 - _biomeWidth * attemptProgress);
            flag50 = true;
            int num796 = Main.maxTilesX / 2;
            //int num797 = 200;
            if (attemtps <= 100) {
                attemtps++;
            }
            int num797_2 = Main.maxTilesX / (5 + attemtps / 15);
            //if (drunkWorldGen) {
            //    num797 = 100;
            //    num793 = ((!GenVars.crimsonLeft) ? genRand.Next((int)((double)Main.maxTilesX * 0.5), Main.maxTilesX - num785) : genRand.Next(num785, (int)((double)Main.maxTilesX * 0.5)));
            //}
            //else {
            //    num793 = genRand.Next(num785, Main.maxTilesX - num785);
            //}
            num793 = genRand.Next(num785, Main.maxTilesX - num785);

            num794 = num793;
            num795 = num793;
            if (num794 < num785)
                num794 = num785;

            if (num795 > Main.maxTilesX - num785)
                num795 = Main.maxTilesX - num785;

            //if (GenVars.dungeonSide < 0 && num794 < 400)
            //    num794 = 400;
            //else if (GenVars.dungeonSide > 0 && num794 > Main.maxTilesX - 400)
            //    num794 = Main.maxTilesX - 400;

            if (WorldGenHelper.TileCountNearby(TileID.SnowBlock, num793, CenterY)) {
                flag50 = false;
            }
            if (WorldGenHelper.TileCountNearby(TileID.Sand, num793, CenterY)) {
                flag50 = false;
            }
            if (WorldGenHelper.TileCountNearby(TileID.Mud, num793, CenterY)) {
                flag50 = false;
            }
            if (WorldGenHelper.TileCountNearby(TileID.JungleGrass, num793, CenterY)) {
                flag50 = false;
            }

            if (num793 - _biomeWidth < (GenVars.beachBordersWidth + num785 / 2) || num793 + _biomeWidth > Main.maxTilesX - (GenVars.beachBordersWidth + num785 / 2)) {
                flag50 = false;
            }

            if (num793 > GenVars.snowOriginLeft - num786 * 2.5f && num793 < GenVars.snowOriginRight + num786 * 2.5f) {
                flag50 = false;
            }

            if (num793 > GenVars.jungleMinX - num786 / 2 && num793 < GenVars.jungleMaxX + num786 / 2) {
                flag50 = false;
            }

            if ((num793 > GenVars.jungleOriginX && num793 < GenVars.UndergroundDesertLocation.X + GenVars.UndergroundDesertLocation.Width / 2) ||
                (num793 < GenVars.jungleOriginX && num793 > GenVars.UndergroundDesertLocation.X + GenVars.UndergroundDesertLocation.Width / 2)) {
                flag50 = false;
            }

            if (num794 < num796 + num797_2 && num795 > num796 - num797_2)
                flag50 = false;

            if (num794 > GenVars.dungeonLocation - num786 * 2 && num795 < GenVars.dungeonLocation + num786 * 2)
                flag50 = false;

            if (!remixWorldGen) {
                if (num793 > GenVars.UndergroundDesertLocation.X - 50 && num793 < GenVars.UndergroundDesertLocation.X + GenVars.UndergroundDesertLocation.Width + 50)
                    flag50 = false;

                if (num794 > GenVars.UndergroundDesertLocation.X - 50 && num794 < GenVars.UndergroundDesertLocation.X + GenVars.UndergroundDesertLocation.Width + 50)
                    flag50 = false;

                if (num795 > GenVars.UndergroundDesertLocation.X - 50 && num795 < GenVars.UndergroundDesertLocation.X + GenVars.UndergroundDesertLocation.Width + 50)
                    flag50 = false;

                if (num794 < num790 && num795 > num789) {
                    num789++;
                    num790--;
                    flag50 = false;
                }

                if (num794 < num792 && num795 > num791) {
                    num791++;
                    num792--;
                    flag50 = false;
                }
            }
        }
        CenterX = num793;

        //if (hasRemnants) {
        //    CenterX = Main.maxTilesX - 150;
        //    bool flag = false;
        //    bool scanMudInAreaAndSkipSand(Point checkPosition, int areaSize = 50) {
        //        for (int x = checkPosition.X - areaSize; x < checkPosition.X + areaSize; x++) {
        //            for (int y = checkPosition.Y - areaSize; y < checkPosition.Y + areaSize; y++) {
        //                Tile checkTile = WorldGenHelper.GetTileSafely(x, y);
        //                if (checkTile.TileType == TileID.Mud) {
        //                    return true;
        //                }
        //                if (checkTile.TileType == TileID.HardenedSand) {
        //                    flag = true;
        //                    return true;
        //                }
        //            }
        //        }
        //        return false;
        //    }
        //    while (!scanMudInAreaAndSkipSand(new Point(CenterX, CenterY))) {
        //        CenterX--;
        //        if (CenterX < 150) {
        //            flag = true;
        //            break;
        //        }
        //    }
        //    if (flag) {
        //        CenterX = 150;
        //        while (!scanMudInAreaAndSkipSand(new Point(CenterX, CenterY))) {
        //            CenterX++;
        //            if (CenterX > Main.maxTilesX - 150) {
        //                break;
        //            }
        //        }
        //    }
        //}

        bool hasSpirit = true/*ModLoader.HasMod("SpiritMod")*/;

        _dirtTileType = (ushort)ModContent.TileType<BackwoodsDirt>();
        _grassTileType = hasSpirit ? TileID.Ebonstone : (ushort)ModContent.TileType<BackwoodsGrass>();
        _stoneTileType = (ushort)ModContent.TileType<BackwoodsStone>();
        _mossTileType = (ushort)ModContent.TileType<BackwoodsGreenMoss>();
        _mossGrowthTileType = (ushort)ModContent.TileType<MossGrowth>();
        _elderwoodTileType = (ushort)ModContent.TileType<LivingElderwood>();
        _elderwoodTileType2 = (ushort)ModContent.TileType<LivingElderwood2>();
        _elderwoodTileType3 = (ushort)ModContent.TileType<LivingElderwood3>();
        _leavesTileType = (ushort)ModContent.TileType<LivingElderwoodlLeaves>();
        _dirtWallType = WallID.DirtUnsafe;
        _grassWallType = (ushort)ModContent.WallType<Tiles.Walls.BackwoodsGrassWall>();
        _flowerGrassWallType = (ushort)ModContent.WallType<Tiles.Walls.BackwoodsFlowerGrassWall>();
        _elderwoodWallType = (ushort)ModContent.WallType<ElderwoodWall3>();
        _elderwoodWallType2 = (ushort)ModContent.WallType<ElderwoodWall2>();
        _leavesWallType = (ushort)ModContent.WallType<LivingBackwoodsLeavesWall2>();
        _elderWoodChestTileType = (ushort)ModContent.TileType<ElderwoodChest2>();
        _altarTileType = (ushort)ModContent.TileType<OvergrownAltar>();
        _vinesTileType = (ushort)ModContent.TileType<BackwoodsVines>();
        _vinesTileType2 = (ushort)ModContent.TileType<BackwoodsVinesFlower>();

        _potTileType = (ushort)ModContent.TileType<BackwoodsPot>();

        _backwoodsPlants.Add(_fallenTreeTileType = (ushort)ModContent.TileType<FallenTree>());
        _backwoodsPlants.Add(_plantsTileType = (ushort)ModContent.TileType<BackwoodsPlants>());
        _backwoodsPlants.Add(_bushTileType = (ushort)ModContent.TileType<BackwoodsBush>());
        _backwoodsPlants.Add(_mintTileType = (ushort)ModContent.TileType<Tiles.Plants.MiracleMint>());
    }

    private void Step1_FindPosition() {
        //int tileCountToCheck = _biomeWidth - _biomeWidth / 3;
        //SkipTilesByTileType(TileID.Mud, tileCountToCheck: tileCountToCheck);
        //SkipTilesByTileType(TileID.Sand, tileCountToCheck: tileCountToCheck * 2, offsetPositionDirectionOnCheck: -1);

        //int mid = Main.maxTilesX / 2;
        //if (ModLoader.TryGetMod("SpiritReforged", out Mod mod)) {
        //    SkipTilesByTileType(TileID.HardenedSand, tileCountToCheck: tileCountToCheck * 3, offsetPositionDirectionOnCheck: -1);
        //    Rectangle savannaArea = (Rectangle)mod.Call("GetSavannaArea");
        //    CenterX += -GenVars.dungeonSide * savannaArea.Width / 8;
        //}
        //bool hasRemnants = ModLoader.HasMod("Remnants");
        //float checkWidthMultiplier = hasRemnants ? 5 : 3;
        //CenterY = (int)Main.worldSurface - 200;
        //while (CenterX >= mid && CenterX < mid + _biomeWidth * checkWidthMultiplier) {
        //    CenterX++;
        //}
        //while (CenterX <= mid && CenterX > mid - _biomeWidth * checkWidthMultiplier) {
        //    CenterX--;
        //}
        BackwoodsVars.BackwoodsCenterX = (ushort)CenterX;
        CenterY = WorldGenHelper.GetFirstTileY2(CenterX, skipWalls: true);
        _biomeHeight += (int)MathF.Abs((float)Main.worldSurface - CenterY) / 4;
        CenterY += _biomeHeight / 2;
        BackwoodsVars.BackwoodsHalfSizeX = (ushort)(_biomeWidth / 2);
    }

    private void Step2_ClearZone() {
        int num1047 = 0;
        int num1048 = 0;
        for (int i = Left; i < Right; i++) {
            //_progress.Set(((float)i - Left) / (Right - Left));
            for (int j = WorldGenHelper.SafeFloatingIslandY - 20; j < Bottom; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (WorldGenHelper.IsCloud(i, j)) {
                    continue;
                }
                if (SkipBiomeInvalidWallTypeToKill.Contains(tile.WallType)) {
                    continue;
                }
                bool flag2 = !MidInvalidTileTypesToKill.Contains(tile.TileType) || (MidInvalidTileTypesToKill.Contains(tile.TileType) && j < BackwoodsVars.FirstTileYAtCenter + 15);
                if (!SandInvalidTileTypesToKill.Contains(tile.TileType) && !SandInvalidWallTypesToKill.Contains(tile.WallType) && flag2) {
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
                        int minY = BackwoodsVars.FirstTileYAtCenter;
                        killTile = false;
                        if (j < minY) {
                            killTile = true;
                            killSand = true;
                            //spreadY += _random.Next(-2, 3);
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
                            bool flag = killSand;
                            if (flag || !killSand) {
                                if (flag) {
                                    WorldGenHelper.ReplaceTile(i, j, _dirtTileType);
                                }
                                else {
                                    WorldGen.KillTile(i, j);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void Step3_GenerateBase() {
        int topLeftTileX = TopLeft.X - 10, topLeftTileY = WorldGenHelper.GetFirstTileY(topLeftTileX);
        int topRightTileX = TopRight.X + 10, topRightTileY = WorldGenHelper.GetFirstTileY(topRightTileX);
        int surfaceY = 0;
        int angle = 25;
        int between = Math.Clamp(topRightTileY - topLeftTileY, -angle, angle);
        int leftY = WorldGenHelper.GetFirstTileY(topLeftTileX), rightY = WorldGenHelper.GetFirstTileY(topRightTileX);
        int max = Math.Min(leftY, rightY) == leftY ? topLeftTileX : topRightTileX;
        int extraHeight = _biomeHeight / 7;
        CenterY += extraHeight;
        _biomeHeight += extraHeight;
        _biomeHeight += (int)MathF.Abs(topLeftTileY - topRightTileY) / 2;
        _toLeft = max == topLeftTileX;
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
            int j = y2/* - 5*/;
            int waterYRandomness = 0;
            while (j <= Bottom) {
                bool edgeLeft = i < topLeftTileX + 15, edgeRight = i > topRightTileX - 15;
                bool mid = i > topLeftTileX + _biomeWidth / 3 - 20 && i < topRightTileX - _biomeWidth / 3 + 20;
                bool edgeX = edgeLeft || edgeRight, jungleEdge = (GenVars.JungleX > Main.maxTilesX / 2) ? edgeLeft : edgeRight;
                bool edge = j > y2 + 25 && (edgeX || j > Bottom - 25);
                if (edge) {
                    int randomnessX = (int)(_random.Next(-20, 21) * 1f);
                    int strength = _random.Next(15, 40), step = _random.Next(2, 13);
                    WorldGenHelper.ModifiedTileRunnerForBackwoods(i + randomnessX, j, strength, step, _dirtTileType, _dirtWallType, true, 0f, 0f, true, true, true, false);
                    j += strength;
                }
                else {
                    Tile tile = WorldGenHelper.GetTileSafely(i, j);
                    if (mid || (!mid && !SkipBiomeInvalidWallTypeToKill.Contains(tile.WallType)) || tile.AnyLiquid()) {
                        bool killTile = true, killWater = false;
                        if (MidInvalidTileTypesToKill2.Contains(tile.TileType) || MidMustSkipWallTypes.Contains(tile.WallType) ||
                            SandInvalidWallTypesToKill.Contains(tile.WallType) || SandInvalidTileTypesToKill.Contains(tile.TileType)) {
                            killTile = false;
                        }
                        if (tile.AnyLiquid()) {
                            //if ((i > CenterX - EdgeX && i < CenterX + EdgeX) || j < BackwoodsVars.FirstTileYAtCenter + 5 + waterYRandomness) {
                            //    killWater = true;
                            //}
                            //else
                            //{
                            //    killTile = false;
                            //}
                        }
                        if (SandTileTypes.Contains(tile.TileType)) {
                            if (j > y2 + 5) {
                                killTile = false;
                            }
                        }
                        if (killTile || (mid && MidMustKillTileTypes.Contains(tile.TileType))) {
                            void replaceTile(int? tileType = null, ushort? wallType = null) {
                                WorldGenHelper.ReplaceTile(i, j + (killWater ? waterYRandomness : 0), tileType ?? _dirtTileType);
                                tile.WallType = wallType ?? _dirtWallType;
                            }
                            replaceTile();
                            if ((tile.WallType == WallID.MudUnsafe || tile.WallType == WallID.MudWallEcho)) {
                                tile.WallType = _dirtWallType;
                            }
                        }
                        if (mid && (tile.TileType == TileID.Ebonsand || tile.TileType == TileID.Crimsand)) {
                            tile.TileType = TileID.Sand;
                        }
                    }
                }
                j++;
            }
            waterYRandomness += _random.Next(-1, 2);
            if (waterYRandomness < 0)
                waterYRandomness = 0;

            if (waterYRandomness > 5)
                waterYRandomness = 5;
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

        AddCliffIfNeeded(topLeftTileX, topRightTileX);

        /*if (ModLoader.HasMod("SpiritMod")) */
        {
            for (int i = Left - 50; i <= Right + 50; i++) {
                for (int j = WorldGenHelper.SafeFloatingIslandY; j < CenterY; j++) {
                    if (WorldGenHelper.ActiveTile(i, j, TileID.Dirt)) {
                        WorldGenHelper.GetTileSafely(i, j).TileType = _dirtTileType;
                    }
                }
            }
        }
    }

    private void AddCliffIfNeeded(int topLeftTileX, int topRightTileX) {
        Point cliffTileCoords = Point.Zero;
        cliffTileCoords.X = _toLeft ? topLeftTileX - 10 : (topRightTileX + 10);
        cliffTileCoords.Y = WorldGenHelper.GetFirstTileY2(cliffTileCoords.X, true, true);
        int lastSurfaceY = _biomeSurface.Last().Y;
        int cliffX = cliffTileCoords.X;
        int startY = cliffTileCoords.Y;
        bool first = true;
        int dir = _toLeft ? -1 : 1;
        int randomness = 0;
        int x = cliffX + (randomness + 1) * dir;
        int max = startY + _biomeHeight / 2;
        int[] randomnessPoints = new int[max - startY + 1];
        while (startY < lastSurfaceY) {
            bool flag = Math.Abs(cliffX - cliffTileCoords.X) > 20;
            int testJ = startY;
            while (testJ <= max) {
                x = cliffX + (randomnessPoints[max - testJ] + 1) * dir;
                if (testJ < startY + 3) {
                    for (int j = testJ - 35; j < testJ; j++) {
                        if (Main.tile[x, j].TileType != _dirtTileType) {
                            WorldGen.KillTile(x, j);
                        }
                    }
                }
                WorldGenHelper.ReplaceTile(x, testJ, CliffPlaceholderTileType);
                WorldGenHelper.ReplaceWall(x, testJ, _dirtWallType);
                testJ++;
                if (first) {
                    randomness += _random.Next(-1, 2);
                    randomnessPoints[max - testJ] = randomness;
                }
                if (/*SkipBiomeInvalidTileTypeToKill.Contains(Main.tile[x, testJ].TileType) ||*/
                    SandTileTypes.Contains(Main.tile[x, testJ].TileType)) {
                    break;
                }
                if (testJ >= max) {
                    first = false;
                }
            }
            while (_random.Next(0, 5) <= _random.Next(1, !flag ? 2 : 4)) {
                startY++;
                if (startY > lastSurfaceY) {
                    break;
                }
            }
            if ((_random.NextChance(0.75) && flag) || !flag) {
                cliffX -= dir;
            }
        }
        _lastCliffX = x;
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
                //if (!SandTileTypes.Contains(tile.TileType) && !tile.ActiveTile(TileID.SnowBlock) && !tile.ActiveTile(TileID.Mud) && !tile.ActiveTile(TileID.Stone)) {
                //    int count = 0;
                //    if (WorldGenHelper.ActiveTile(i - 1, j)) {
                //        count++;
                //    }
                //    if (WorldGenHelper.ActiveTile(i + 1, j)) {
                //        count++;
                //    }
                //    if (WorldGenHelper.ActiveTile(i, j - 1)) {
                //        count++;
                //    }
                //    if (WorldGenHelper.ActiveTile(i, j + 1)) {
                //        count++;
                //    }
                //    if (count >= 3) {
                //        WorldGenHelper.ReplaceTile(i, j, _dirtTileType);
                //    }
                //}
                int minY = BackwoodsVars.FirstTileYAtCenter + EdgeY;
                bool flag = true;
                for (int checkX = i - 5; checkX < i + 6; checkX++) {
                    if (!flag) {
                        break;
                    }
                    for (int checkY = j - 5; checkY < j + 6; checkY++) {
                        if (WorldGenHelper.GetTileSafely(checkX, checkY).WallType == WallID.CrimstoneUnsafe) {
                            flag = false;
                            break;
                        }
                    }
                }
                if (j > minY - EdgeY && j < CenterY + EdgeY && tile.WallType == WallID.None && flag) {
                    tile.WallType = _dirtWallType;
                }
            }
        }
    }

    private void Step6_SpreadGrass(bool flag = false) {
        int y = Math.Min(CenterY - EdgeY, (int)Main.worldSurface + 10);
        double randomnessY = y + 50.0;
        for (int i = Left - 50; i < Right + 50; i++) {
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < Main.worldSurface; j++) {
                randomnessY += (double)_random.NextFloat(-2f, 3f);
                randomnessY = Math.Clamp(randomnessY, y + 30.0, y + 60.0);
                bool spread = false;
                for (int j2 = BackwoodsVars.FirstTileYAtCenter - 50; j2 < randomnessY; j2++) {
                    Tile tile = WorldGenHelper.GetTileSafely(i, j2);
                    if (tile.HasTile) {
                        if (j2 < y - 1.0 && !spread) {
                            if (tile.TileType == _dirtTileType || tile.TileType == TileID.Dirt) {
                                WorldGen.grassSpread = 0;
                                WorldGenHelper.CustomSpreadGrass(i, j2, _dirtTileType, _grassTileType, growUnderground: true, maxY: Math.Min(CenterY - EdgeY + 10, (int)Main.worldSurface + 10));
                                if (flag) {
                                    WorldGen.grassSpread = 0;
                                    WorldGenHelper.CustomSpreadGrass(i, j2, TileID.Dirt, _grassTileType, growUnderground: true, maxY: Math.Min(CenterY - EdgeY + 10, (int)Main.worldSurface + 10));
                                }
                            }
                        }
                        spread = true;
                    }
                }
            }
        }
    }

    private void GrowTrees() {
        int left = _toLeft ? (_lastCliffX != 0 ? _lastCliffX : Left) : Left;
        int right = !_toLeft ? (_lastCliffX != 0 ? _lastCliffX : Right) : Right;
        for (int i = left - 100; i <= right + 100; i++) {
            if (i < left - 20 || i > right + 20) {
                for (int j = WorldGenHelper.SafeFloatingIslandY; j < BackwoodsVars.FirstTileYAtCenter + 20; j++) {
                    Tile tile = WorldGenHelper.GetTileSafely(i, j);
                    if (WorldGenHelper.ActiveTile(i, j, _grassTileType) && !_backwoodsPlants.Contains(WorldGenHelper.GetTileSafely(i, j - 1).TileType) && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock) {
                        WorldGen.GrowTree(i, j);
                    }
                }
            }
        }
        //int bigTreeCount = (int)(_biomeWidth / 50f);
        //int altarPosXToCheck = CenterX + 5;
        //for (int k = 0; k < bigTreeCount / 2; k++) {
        //    bool flag = false;
        //    for (int i = left - 10; i <= right + 10; i++) {
        //        if (flag) {
        //            break;
        //        }
        //        if (i > altarPosXToCheck - 10 && i < altarPosXToCheck + 15) {
        //            continue;
        //        }
        //        if (i < altarPosXToCheck - _biomeWidth / 2 || i > altarPosXToCheck + 2 + _biomeWidth / 2) {
        //            continue;
        //        }
        //        for (int j = WorldGenHelper.SafeFloatingIslandY; j < BackwoodsVars.FirstTileYAtCenter + 10; j++) {
        //            bool flag2 = false;
        //            for (int checkX = -30; checkX < 31; checkX++) {
        //                if (WorldGenHelper.GetTileSafely(i + checkX, j + 1).TileType == ModContent.TileType<BackwoodsBigTree>() || 
        //                    WorldGenHelper.GetTileSafely(i + checkX, j + 1).TileType == _elderwoodTileType) { 
        //                    flag2 = true;
        //                    break;
        //                }
        //            }
        //            if (!flag2 && BackwoodsBigTree.TryGrowBigTree(i, j + 1, placeRand: _random, ignoreAcorns: true, ignoreTrees: true, gen: true)) {
        //                flag = true;
        //                break;
        //            }
        //        }
        //    }
        //}
        for (int i = left - 20; i <= right + 20; i++) {
            if (i > CenterX - 10 && i < CenterX + 22) {
                continue;
            }
            for (int j = WorldGenHelper.SafeFloatingIslandY; j < BackwoodsVars.FirstTileYAtCenter + 20; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (WorldGenHelper.ActiveTile(i, j, _grassTileType) && !_backwoodsPlants.Contains(WorldGenHelper.GetTileSafely(i, j - 1).TileType) && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock) {
                    WorldGenHelper.GrowTreeWithBranches<TreeBranch>(i, j, branchChance: 10);
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

    private void SpreadMossGrass2(int i, int j) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        tile.HasTile = true;
        tile.TileType = (ushort)ModContent.TileType<BackwoodsRoots1>();
        //tile.TileFrameX = (short)(_random.Next(3) * 18);
    }

    // adapted vanilla
    private void SpreadMossGrass(int i, int j) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        tile.HasTile = true;
        tile.TileType = _mossGrowthTileType;
        tile.TileFrameX = 0;
        tile.TileFrameY = (short)(_random.Next(3) * 18);
        //WorldGen.SquareTileFrame(i, j);
    }

    // adapted vanilla
    private void Spread(int x, int y, ushort baseTileType, ushort desireTileType) {
        if (!WorldGen.InWorld(x, y))
            return;

        ushort tileType = desireTileType;
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
                        if (tile.TileType == baseTileType)
                            tile.TileType = tileType;
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