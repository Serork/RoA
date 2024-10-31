using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;

using Newtonsoft.Json.Linq;

using RoA.Common.BackwoodsSystems;
using RoA.Common.Sets;
using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Decorations;
using RoA.Content.Tiles.Furniture;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Terraria;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Content.World.Generations;

public static class CustomHouseUtils {
    internal static bool[] BlacklistedTiles = TileID.Sets.Factory.CreateBoolSet(true, 225, 41, 43, 44, 226, 203, 112, 25, 151, 21, 467);
    internal static bool[] BeelistedTiles = TileID.Sets.Factory.CreateBoolSet(true, 41, 43, 44, 226, 203, 112, 25, 151, 21, 467);

    public static HouseBuilderCustom CreateBuilder(Point origin, StructureMap structures) {
        List<Rectangle> list = CreateRooms(origin);
        if (list.Count == 0 || !AreRoomLocationsValid(list))
            return HouseBuilderCustom.Invalid;

        HouseType houseType = GetHouseType(list);
        if (!AreRoomsValid(list, structures, houseType))
            return HouseBuilderCustom.Invalid;

        return new ElderwoodHouseBuilder(list);
    }

    private static List<Rectangle> CreateRooms(Point origin) {
        if (!WorldUtils.Find(origin, Searches.Chain(new Searches.Down(10), new Conditions.IsSolid()), out var result) || result == origin)
            return new List<Rectangle>();

        Rectangle item = FindRoom(result);
        Rectangle rectangle = FindRoom(new Point(item.Center.X, item.Y + 1));
        Rectangle rectangle2 = FindRoom(new Point(item.Center.X, item.Y + item.Height + 10));
        rectangle2.Y = item.Y + item.Height - 1;
        double roomSolidPrecentage = GetRoomSolidPrecentage(rectangle);
        double roomSolidPrecentage2 = GetRoomSolidPrecentage(rectangle2);
        item.Y += 3;
        rectangle.Y += 3;
        rectangle2.Y += 3;
        List<Rectangle> list = new List<Rectangle>();
        if (WorldGen.genRand.NextDouble() > roomSolidPrecentage)
            list.Add(rectangle);

        list.Add(item);
        if (WorldGen.genRand.NextDouble() > roomSolidPrecentage2)
            list.Add(rectangle2);

        return list;
    }

    private static Rectangle FindRoom(Point origin) {
        Point result;
        bool flag = WorldUtils.Find(origin, Searches.Chain(new Searches.Left(10), new Conditions.IsSolid()), out result);
        Point result2;
        bool num = WorldUtils.Find(origin, Searches.Chain(new Searches.Right(10), new Conditions.IsSolid()), out result2);
        if (!flag)
            result = new Point(origin.X - 25, origin.Y);

        if (!num)
            result2 = new Point(origin.X + 25, origin.Y);

        Rectangle result3 = new Rectangle(origin.X, origin.Y, 0, 0);
        if (origin.X - result.X > result2.X - origin.X) {
            result3.X = result.X;
            result3.Width = Utils.Clamp(result2.X - result.X, 15, 30);
        }
        else {
            result3.Width = Utils.Clamp(result2.X - result.X, 15, 30);
            result3.X = result2.X - result3.Width;
        }

        Point result4;
        bool flag2 = WorldUtils.Find(result, Searches.Chain(new Searches.Up(10), new Conditions.IsSolid()), out result4);
        Point result5;
        bool num2 = WorldUtils.Find(result2, Searches.Chain(new Searches.Up(10), new Conditions.IsSolid()), out result5);
        if (!flag2)
            result4 = new Point(origin.X, origin.Y - 10);

        if (!num2)
            result5 = new Point(origin.X, origin.Y - 10);

        result3.Height = Utils.Clamp(Math.Max(origin.Y - result4.Y, origin.Y - result5.Y), 8, 12);
        result3.Y -= result3.Height;
        return result3;
    }

    private static double GetRoomSolidPrecentage(Rectangle room) {
        double num = room.Width * room.Height;
        Ref<int> @ref = new Ref<int>(0);
        WorldUtils.Gen(new Point(room.X, room.Y), new Shapes.Rectangle(room.Width, room.Height), Actions.Chain(new Modifiers.IsSolid(), new Actions.Count(@ref)));
        return (double)@ref.Value / num;
    }

    private static int SortBiomeResults(Tuple<HouseType, int> item1, Tuple<HouseType, int> item2) => item2.Item2.CompareTo(item1.Item2);

    private static bool AreRoomLocationsValid(IEnumerable<Rectangle> rooms) {
        foreach (Rectangle room in rooms) {
            if (room.Y + room.Height > Main.maxTilesY - 220)
                return false;
        }

        return true;
    }

    private static HouseType GetHouseType(IEnumerable<Rectangle> rooms) {
        Dictionary<ushort, int> dictionary = new Dictionary<ushort, int>();
        ushort wood = (ushort)ModContent.TileType<LivingElderwood>(),
               stone = (ushort)ModContent.TileType<BackwoodsStone>(), 
               grass = (ushort)ModContent.TileType<BackwoodsGrass>(),
               moss = (ushort)ModContent.TileType<BackwoodsGreenMoss>();
        foreach (Rectangle room in rooms) {
            WorldUtils.Gen(new Point(room.X - 10, room.Y - 10), new Shapes.Rectangle(room.Width + 20, room.Height + 20), new Actions.TileScanner(0, wood, stone, grass, moss).Output(dictionary));
        }

        List<Tuple<HouseType, int>> list = new List<Tuple<HouseType, int>>();
        list.Add(Tuple.Create(HouseType.Wood, dictionary[0] + dictionary[wood] + dictionary[stone] + dictionary[grass] + dictionary[moss]));
        list.Sort(SortBiomeResults);
        return list[0].Item1;
    }

    private static bool AreRoomsValid(IEnumerable<Rectangle> rooms, StructureMap structures, HouseType style) {
        foreach (Rectangle room in rooms) {
            //if (WorldUtils.Find(new Point(room.X - 2, room.Y - 2), Searches.Chain(new Searches.Rectangle(room.Width + 4, room.Height + 4).RequireAll(mode: false), new Conditions.HasLava()), out var _))
            //    return false;

            //if (!structures.CanPlace(room, BlacklistedTiles, 5)) {
            //    return false;
            //}
        }

        return true;
    }
}

public class HouseBuilderCustom {
    private const int VERTICAL_EXIT_WIDTH = 3;

    public static readonly HouseBuilderCustom Invalid = new HouseBuilderCustom();

    public readonly HouseType Type;
    public readonly bool IsValid;

    internal static bool _painting1, _painting2, _painting3;

    protected ushort[] SkipTilesDuringWallAging = new ushort[5] {
        245,
        246,
        240,
        241,
        242
    };

    public double ChestChance { get; set; }

    public ushort TileType { get; protected set; }

    public ushort WallType { get; protected set; }

    public ushort BeamType { get; protected set; }

    public ushort PlatformTileType { get; protected set; }

    public ushort DoorTileType { get; protected set; }

    public ushort TableTileType { get; protected set; }

    public bool UsesTables2 { get; protected set; }

    public ushort WorkbenchTileType { get; protected set; }

    public ushort PianoTileType { get; protected set; }

    public int BookcaseTileType { get; protected set; }

    public ushort ChairTileType { get; protected set; }

    public ushort ChestTileType { get; protected set; }

    public bool UsesContainers2 { get; protected set; }

    public ushort ChandelierTileType { get; protected set; }

    public ReadOnlyCollection<Rectangle> Rooms { get; private set; }

    public Rectangle TopRoom => Rooms.First();

    public Rectangle BottomRoom => Rooms.Last();

    private UnifiedRandom _random => WorldGen.genRand;

    private Tilemap _tiles => Main.tile;

    private HouseBuilderCustom() {
        IsValid = false;
    }

    protected HouseBuilderCustom(HouseType type, IEnumerable<Rectangle> rooms) {
        Type = type;
        IsValid = true;
        List<Rectangle> list = rooms.ToList();
        list.Sort((Rectangle lhs, Rectangle rhs) => lhs.Top.CompareTo(rhs.Top));
        Rooms = list.AsReadOnly();
    }

    protected virtual void AgeRoom(Rectangle room) {
    }

    public virtual void Place(HouseBuilderContext context, StructureMap structures) {
        PlaceEmptyRooms();
        //foreach (Rectangle room in Rooms) {
        //    structures.AddProtectedStructure(room, 8);
        //}

        PlaceStairs();
        PlaceDoors();
        PlacePlatforms();
        PlaceSupportBeams();
        PlaceBiomeSpecificPriorityTool(context);
        FillRooms();
        foreach (Rectangle room2 in Rooms) {
            AgeRoom(room2);
        }

        PlaceChests();
        PlaceBiomeSpecificTool(context);
    }

    private class SkipChestsTiles : GenAction {
        public override bool Apply(Point origin, int x, int y, params object[] args) {
            if (!GenBase._tiles[x, y].HasTile)
                return UnitApply(origin, x, y, args);

            if (TileID.Sets.BasicChest[GenBase._tiles[x, y].TileType] || !Main.tileSolid[GenBase._tiles[x, y].TileType])
                return Fail();

            return UnitApply(origin, x, y, args);
        }
    }

    private void PlaceEmptyRooms() {
        foreach (Rectangle room in Rooms) {
            WorldUtils.Gen(new Point(room.X, room.Y), new Shapes.Rectangle(room.Width, room.Height), Actions.Chain(new SkipChestsTiles(), new Actions.ClearTile(), new Actions.SetTileKeepWall(TileType), new Actions.SetFrames()));
            WorldUtils.Gen(new Point(room.X + 1, room.Y + 1), new Shapes.Rectangle(room.Width - 2, room.Height - 2), Actions.Chain(new SkipChestsTiles(), new Actions.ClearTile(), new Actions.PlaceWall(WallType)));
        }
    }

    private void FillRooms() {
        int x = TableTileType;
        if (UsesTables2)
            x = 469;

        Point[] choices = new Point[7] {
            new Point(x, 0),
            new Point(16, 0),
            new Point(WorkbenchTileType, 0),
            new Point(86, 0),
            new Point(PianoTileType, 0),
            new Point(94, 0),
            new Point(BookcaseTileType, 0)
        };

        foreach (Rectangle room in Rooms) {
            int num = room.Width / 8;
            int num2 = room.Width / (num + 1);
            int num3 = _random.Next(2);
            for (int i = 0; i < num; i++) {
                int num4 = (i + 1) * num2 + room.X;
                switch (i + num3 % 2) {
                    case 0: {
                        int num5 = room.Y + Math.Min(room.Height / 2, room.Height - 5);
                        bool flag2 = _random.NextBool(8);
                        bool flag = !_painting1 || !_painting2 || !_painting3;
                        if (flag || flag2) {
                            int attempts = 1000;
                            while (--attempts > 0) {
                                int value = _random.Next(3);
                                if (value == 0) {
                                    if (!_painting1 || (_painting1 && flag2)) {
                                        if (WorldGenHelper.Place6x4Wall(num4, num5, (ushort)ModContent.TileType<MillionDollarPainting>(), 0, WallType)) {
                                            _painting1 = true;
                                            break;
                                        }
                                    }
                                }
                                else if (value == 1) {
                                    if (!_painting2 || (_painting2 && flag2)) {
                                        if (WorldGenHelper.Place4x4Wall(num4, num5, (ushort)ModContent.TileType<Moss>(), 0, WallType)) {
                                            _painting2 = true;
                                            break;
                                        }
                                    }
                                }
                                else if (value == 2) {
                                    if (!_painting3 || (_painting3 && flag2)) {
                                        if (WorldGenHelper.Place4x4Wall(num4, num5, (ushort)ModContent.TileType<TheLegend>(), 0, WallType)) {
                                            _painting3 = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            PaintingEntry paintingEntry = WorldGen.RandHousePicture();
                            WorldGen.PlaceTile(num4, num5, paintingEntry.tileType, mute: true, forced: false, -1, paintingEntry.style);
                        }
                        //WorldGen.PlaceTile(num4, num5, paintingEntry.tileType, mute: true, forced: false, -1, 0);
                        break;
                    }
                    case 1: {
                        int num5 = room.Y + 1;
                        WorldGen.PlaceTile(num4, num5, ChandelierTileType, mute: true, forced: false, -1);
                        if (_random.NextChance(0.975)) {
                            for (int j = -1; j < 2; j++) {
                                for (int k = 0; k < 3; k++) {
                                    _tiles[j + num4, k + num5].TileFrameX += 54;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            int num6 = room.Width / 8 + 3;
            WorldGen.SetupStatueList();
            while (num6 > 0) {
                int num7 = _random.Next(room.Width - 3) + 1 + room.X;
                int num8 = room.Y + room.Height - 2;
                switch (_random.Next(5)) {
                    case 0:
                        if (_random.NextBool()) {
                            WorldGen.PlaceTile(num7, num8, _random.NextBool() ? (ushort)ModContent.TileType<BackwoodsRocks1>() : (ushort)ModContent.TileType<BackwoodsRocks2>(), true, style: _random.Next(3));
                        }
                        else {
                            WorldGen.PlaceSmallPile(num7, num8, _random.Next(31, 34), 1, 185);
                        }
                        break;
                    case 1:
                        if (_random.NextBool()) {
                            WorldGenHelper.Place3x2(num7, num8, (ushort)ModContent.TileType<BackwoodsRocks3x2>(), _random.Next(6));
                        }
                        else {
                            WorldGen.PlaceTile(num7, num8, 186, mute: true, forced: false, -1, _random.Next(22, 26));
                        }
                        break;
                    case 2: {
                        int num9 = _random.Next(2, GenVars.statueList.Length);
                        WorldGen.PlaceTile(num7, num8, GenVars.statueList[num9].X, mute: true, forced: false, -1, GenVars.statueList[num9].Y);
                        if (GenVars.StatuesWithTraps.Contains(num9))
                            WorldGen.PlaceStatueTrap(num7, num8);

                        break;
                    }
                    case 3: {
                        Point point = Utils.SelectRandom(_random, choices);
                        WorldGen.PlaceTile(num7, num8, point.X, mute: true, forced: false, -1, point.Y);
                        break;
                    }
                    case 4: {
                        WorldGenHelper.Place3x2(num7, num8, (ushort)ModContent.TileType<BackwoodsRocks3x2>(), _random.Next(6));
                        break;
                    }
                }

                num6--;
            }
        }
    }

    private void PlaceStairs() {
        foreach (Tuple<Point, Point> item3 in CreateStairsList()) {
            Point item = item3.Item1;
            Point item2 = item3.Item2;
            int num = ((item2.X > item.X) ? 1 : (-1));
            ShapeData shapeData = new ShapeData();
            for (int i = 0; i < item2.Y - item.Y; i++) {
                shapeData.Add(num * (i + 1), i);
            }

            WorldUtils.Gen(item, new ModShapes.All(shapeData), Actions.Chain(new Actions.PlaceTile(PlatformTileType), new Actions.SetSlope((num == 1) ? 1 : 2), new Actions.SetFrames(frameNeighbors: true)));
            WorldUtils.Gen(new Point(item.X + ((num == 1) ? 1 : (-4)), item.Y - 1), new Shapes.Rectangle(4, 1), Actions.Chain(new Actions.Clear(), new Actions.PlaceWall(WallType), new Actions.PlaceTile(PlatformTileType), new Actions.SetFrames(frameNeighbors: true)));
        }
    }

    private List<Tuple<Point, Point>> CreateStairsList() {
        List<Tuple<Point, Point>> list = new List<Tuple<Point, Point>>();
        for (int i = 1; i < Rooms.Count; i++) {
            Rectangle rectangle = Rooms[i];
            Rectangle rectangle2 = Rooms[i - 1];
            int num = rectangle2.X - rectangle.X;
            int num2 = rectangle.X + rectangle.Width - (rectangle2.X + rectangle2.Width);
            if (num > num2)
                list.Add(new Tuple<Point, Point>(new Point(rectangle.X + rectangle.Width - 1, rectangle.Y + 1), new Point(rectangle.X + rectangle.Width - rectangle.Height + 1, rectangle.Y + rectangle.Height - 1)));
            else
                list.Add(new Tuple<Point, Point>(new Point(rectangle.X, rectangle.Y + 1), new Point(rectangle.X + rectangle.Height - 1, rectangle.Y + rectangle.Height - 1)));
        }

        return list;
    }

    private void PlaceDoors() {
        foreach (Point item in CreateDoorList()) {
            WorldUtils.Gen(item, new Shapes.Rectangle(1, 3), new Actions.ClearTile(frameNeighbors: true));
            WorldGen.PlaceTile(item.X, item.Y, DoorTileType, mute: true, forced: true, -1);
        }
    }

    private List<Point> CreateDoorList() {
        List<Point> list = new List<Point>();
        foreach (Rectangle room in Rooms) {
            if (FindSideExit(new Rectangle(room.X + room.Width, room.Y + 1, 1, room.Height - 2), isLeft: false, out var exitY))
                list.Add(new Point(room.X + room.Width - 1, exitY));

            if (FindSideExit(new Rectangle(room.X, room.Y + 1, 1, room.Height - 2), isLeft: true, out exitY))
                list.Add(new Point(room.X, exitY));
        }

        return list;
    }

    private void PlacePlatforms() {
        foreach (Point item in CreatePlatformsList()) {
            WorldUtils.Gen(item, new Shapes.Rectangle(3, 1), Actions.Chain(new Actions.ClearMetadata(), new Actions.PlaceTile(PlatformTileType), new Actions.SetFrames(frameNeighbors: true)));
        }
    }

    private List<Point> CreatePlatformsList() {
        List<Point> list = new List<Point>();
        Rectangle topRoom = TopRoom;
        Rectangle bottomRoom = BottomRoom;
        if (FindVerticalExit(new Rectangle(topRoom.X + 2, topRoom.Y, topRoom.Width - 4, 1), isUp: true, out var exitX))
            list.Add(new Point(exitX, topRoom.Y));

        if (FindVerticalExit(new Rectangle(bottomRoom.X + 2, bottomRoom.Y + bottomRoom.Height - 1, bottomRoom.Width - 4, 1), isUp: false, out exitX))
            list.Add(new Point(exitX, bottomRoom.Y + bottomRoom.Height - 1));

        return list;
    }

    private void PlaceSupportBeams() {
        foreach (Rectangle item in CreateSupportBeamList()) {
            if (item.Height > 1 && _tiles[item.X, item.Y - 1].TileType != 19 && !TileID.Sets.BasicChest[_tiles[item.X, item.Y - 1].TileType]) {
                WorldUtils.Gen(new Point(item.X, item.Y), new Shapes.Rectangle(item.Width, item.Height), Actions.Chain(new Actions.SetTileKeepWall(BeamType), new Actions.SetFrames(frameNeighbors: true)));
                Tile tile = _tiles[item.X, item.Y + item.Height];
                tile.Slope = 0;
                tile.IsHalfBlock = false;
            }
        }
    }

    private List<Rectangle> CreateSupportBeamList() {
        List<Rectangle> list = new List<Rectangle>();
        int num = Rooms.Min((Rectangle room) => room.Left);
        int num2 = Rooms.Max((Rectangle room) => room.Right) - 1;
        int num3 = 6;
        while (num3 > 4 && (num2 - num) % num3 != 0) {
            num3--;
        }

        for (int i = num; i <= num2; i += num3) {
            for (int j = 0; j < Rooms.Count; j++) {
                Rectangle rectangle = Rooms[j];
                if (i < rectangle.X || i >= rectangle.X + rectangle.Width)
                    continue;

                int num4 = rectangle.Y + rectangle.Height;
                int num5 = 50;
                for (int k = j + 1; k < Rooms.Count; k++) {
                    if (i >= Rooms[k].X && i < Rooms[k].X + Rooms[k].Width)
                        num5 = Math.Min(num5, Rooms[k].Y - num4);
                }

                if (num5 > 0) {
                    Point result;
                    bool flag = WorldUtils.Find(new Point(i, num4), Searches.Chain(new Searches.Down(num5), new Conditions.IsSolid()), out result);
                    if (num5 < 50) {
                        flag = true;
                        result = new Point(i, num4 + num5);
                    }

                    if (flag)
                        list.Add(new Rectangle(i, num4, 1, result.Y - num4));
                }
            }
        }

        return list;
    }

    private static bool FindVerticalExit(Rectangle wall, bool isUp, out int exitX) {
        Point result;
        bool result2 = WorldUtils.Find(new Point(wall.X + wall.Width - 3, wall.Y + (isUp ? (-5) : 0)), Searches.Chain(new Searches.Left(wall.Width - 3), new Conditions.IsSolid().Not().AreaOr(3, 5)), out result);
        exitX = result.X;
        return result2;
    }

    private static bool FindSideExit(Rectangle wall, bool isLeft, out int exitY) {
        Point result;
        bool result2 = WorldUtils.Find(new Point(wall.X + (isLeft ? (-4) : 0), wall.Y + wall.Height - 3), Searches.Chain(new Searches.Up(wall.Height - 3), new Conditions.IsSolid().Not().AreaOr(4, 3)), out result);
        exitY = result.Y;
        return result2;
    }

    private void PlaceChests() {
        if (_random.NextDouble() > ChestChance)
            return;

        bool flag = false;
        foreach (Rectangle room in Rooms) {
            int num = room.Height - 1 + room.Y;
            bool num2 = num > (int)Main.worldSurface;
            ushort chestTileType = (ushort)((num2 && UsesContainers2) ? 467 : ChestTileType);
            int style = 0;
            for (int i = 0; i < 10; i++) {
                if (flag = WorldGen.AddBuriedChest(_random.Next(2, room.Width - 2) + room.X, num, 0, notNearOtherChests: false, style, trySlope: false, chestTileType))
                    break;
            }

            if (flag)
                break;

            for (int j = room.X + 2; j <= room.X + room.Width - 2; j++) {
                if (flag = WorldGen.AddBuriedChest(j, num, 0, notNearOtherChests: false, style, trySlope: false, chestTileType))
                    break;
            }

            if (flag)
                break;
        }

        if (!flag) {
            foreach (Rectangle room2 in Rooms) {
                int num3 = room2.Y - 1;
                bool num4 = num3 > (int)Main.worldSurface;
                ushort chestTileType2 = (ushort)((num4 && UsesContainers2) ? 467 : ChestTileType);
                int style2 = 0;
                for (int k = 0; k < 10; k++) {
                    if (flag = WorldGen.AddBuriedChest(_random.Next(2, room2.Width - 2) + room2.X, num3, 0, notNearOtherChests: false, style2, trySlope: false, chestTileType2))
                        break;
                }

                if (flag)
                    break;

                for (int l = room2.X + 2; l <= room2.X + room2.Width - 2; l++) {
                    if (flag = WorldGen.AddBuriedChest(l, num3, 0, notNearOtherChests: false, style2, trySlope: false, chestTileType2))
                        break;
                }

                if (flag)
                    break;
            }
        }

        if (flag)
            return;

        for (int m = 0; m < 1000; m++) {
            int i2 = _random.Next(Rooms[0].X - 30, Rooms[0].X + 30);
            int num5 = _random.Next(Rooms[0].Y - 30, Rooms[0].Y + 30);
            bool num6 = num5 > (int)Main.worldSurface;
            ushort chestTileType3 = (ushort)((num6 && UsesContainers2) ? 467 : ChestTileType);
            int style3 = 0;
            if (flag = WorldGen.AddBuriedChest(i2, num5, 0, notNearOtherChests: false, style3, trySlope: false, chestTileType3))
                break;
        }
    }

    private void PlaceBiomeSpecificPriorityTool(HouseBuilderContext context) {
        if (Type != HouseType.Desert || GenVars.extraBastStatueCount >= GenVars.extraBastStatueCountMax)
            return;

        bool flag = false;
        foreach (Rectangle room in Rooms) {
            int num = room.Height - 2 + room.Y;
            if (WorldGen.remixWorldGen && (double)num > Main.rockLayer)
                return;

            for (int i = 0; i < 10; i++) {
                int num2 = _random.Next(2, room.Width - 2) + room.X;
                WorldGen.PlaceTile(num2, num, 506, mute: true, forced: true);
                if (flag = _tiles[num2, num].HasTile && _tiles[num2, num].TileType == 506)
                    break;
            }

            if (flag)
                break;

            for (int j = room.X + 2; j <= room.X + room.Width - 2; j++) {
                if (flag = WorldGen.PlaceTile(j, num, 506, mute: true, forced: true))
                    break;
            }

            if (flag)
                break;
        }

        if (!flag) {
            foreach (Rectangle room2 in Rooms) {
                int num3 = room2.Y - 1;
                for (int k = 0; k < 10; k++) {
                    int num4 = _random.Next(2, room2.Width - 2) + room2.X;
                    WorldGen.PlaceTile(num4, num3, 506, mute: true, forced: true);
                    if (flag = _tiles[num4, num3].HasTile && _tiles[num4, num3].TileType == 506)
                        break;
                }

                if (flag)
                    break;

                for (int l = room2.X + 2; l <= room2.X + room2.Width - 2; l++) {
                    if (flag = WorldGen.PlaceTile(l, num3, 506, mute: true, forced: true))
                        break;
                }

                if (flag)
                    break;
            }
        }

        if (flag)
            GenVars.extraBastStatueCount++;
    }

    private void PlaceBiomeSpecificTool(HouseBuilderContext context) {
        if (Type == HouseType.Jungle && context.SharpenerCount < _random.Next(2, 5)) {
            bool flag = false;
            foreach (Rectangle room in Rooms) {
                int num = room.Height - 2 + room.Y;
                for (int i = 0; i < 10; i++) {
                    int num2 = _random.Next(2, room.Width - 2) + room.X;
                    WorldGen.PlaceTile(num2, num, 377, mute: true, forced: true);
                    if (flag = _tiles[num2, num].HasTile && _tiles[num2, num].TileType == 377)
                        break;
                }

                if (flag)
                    break;

                for (int j = room.X + 2; j <= room.X + room.Width - 2; j++) {
                    if (flag = WorldGen.PlaceTile(j, num, 377, mute: true, forced: true))
                        break;
                }

                if (flag)
                    break;
            }

            if (flag)
                context.SharpenerCount++;
        }

        if (Type != HouseType.Desert || context.ExtractinatorCount >= _random.Next(2, 5))
            return;

        bool flag2 = false;
        foreach (Rectangle room2 in Rooms) {
            int num3 = room2.Height - 2 + room2.Y;
            for (int k = 0; k < 10; k++) {
                int num4 = _random.Next(2, room2.Width - 2) + room2.X;
                WorldGen.PlaceTile(num4, num3, 219, mute: true, forced: true);
                if (flag2 = _tiles[num4, num3].HasTile && _tiles[num4, num3].TileType == 219)
                    break;
            }

            if (flag2)
                break;

            for (int l = room2.X + 2; l <= room2.X + room2.Width - 2; l++) {
                if (flag2 = WorldGen.PlaceTile(l, num3, 219, mute: true, forced: true))
                    break;
            }

            if (flag2)
                break;
        }

        if (flag2)
            context.ExtractinatorCount++;
    }
}


sealed class ElderwoodHouseBuilder : HouseBuilderCustom {
    public ElderwoodHouseBuilder(IEnumerable<Rectangle> rooms)
        : base(HouseType.Wood, rooms) {
        base.TileType = (ushort)ModContent.TileType<LivingElderwood>();
        base.WallType = (ushort)ModContent.WallType<ElderwoodWall>();
        base.BeamType = 124;
        //if (Main.tenthAnniversaryWorld) {
        //    if (Main.getGoodWorld) {
        //        if (WorldGen.genRand.Next(7) == 0) {
        //            base.TileType = 160;
        //            base.WallType = 44;
        //        }
        //    }
        //    else if (WorldGen.genRand.Next(2) == 0) {
        //        base.TileType = 160;
        //        base.WallType = 44;
        //    }
        //}

        base.PlatformTileType = (ushort)ModContent.TileType<ElderwoodPlatform>();
        base.DoorTileType = (ushort)ModContent.TileType<ElderwoodDoorClosed>();
        base.TableTileType = (ushort)ModContent.TileType<ElderwoodTable>();
        base.WorkbenchTileType = (ushort)ModContent.TileType<ElderwoodWorkbench>();
        base.PianoTileType = (ushort)ModContent.TileType<ElderwoodPiano>();
        base.BookcaseTileType = (ushort)ModContent.TileType<ElderwoodBookcase>();
        base.ChairTileType = (ushort)ModContent.TileType<ElderwoodChair>();
        base.ChestTileType = (ushort)ModContent.TileType<ElderwoodChest2>();
        base.ChandelierTileType = (ushort)ModContent.TileType<ElderwoodChandelier>();
    }

    public class HasValidVall : GenAction {
        private ushort _wallType;

        public HasValidVall(ushort wallType) {
            _wallType = wallType;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args) {
            if (GenBase._tiles[x, y].WallType == _wallType)
                return UnitApply(origin, x, y, args);

            return Fail();
        }
    }

    protected override void AgeRoom(Rectangle room) {
        ushort grassTileType = (ushort)ModContent.TileType<BackwoodsGreenMoss>();
        ushort mossGrowthTileType = (ushort)ModContent.TileType<MossGrowth>();
        WorldUtils.Gen(new Point(room.X, room.Y), new Shapes.Rectangle(room.Width, room.Height), Actions.Chain(new Modifiers.Dither(0.8), new Modifiers.Blotches(2, 0.5), new Modifiers.OnlyTiles(base.TileType), new Actions.SetTile(grassTileType, setSelfFrames: true)));
        WorldUtils.Gen(new Point(room.X + 1, room.Y), new Shapes.Rectangle(room.Width - 2, 1), Actions.Chain(new Modifiers.Dither(0.7), new Modifiers.OnlyTiles(grassTileType), new Modifiers.Offset(0, -1), new Modifiers.IsEmpty(), new Actions.SetTile(mossGrowthTileType)));
        WorldUtils.Gen(new Point(room.X + 1, room.Y + room.Height - 1), new Shapes.Rectangle(room.Width - 2, 1), Actions.Chain(new Modifiers.Dither(0.8), new Modifiers.OnlyTiles(grassTileType), new Modifiers.Offset(0, -1), new Modifiers.IsEmpty(), new Actions.SetTile(mossGrowthTileType)));

        for (int i = 0; i < room.Width * room.Height / 16; i++) {
            int x = WorldGen.genRand.Next(1, room.Width - 1) + room.X;
            int y = WorldGen.genRand.Next(1, room.Height - 1) + room.Y;
            WorldUtils.Gen(new Point(x, y), new Shapes.Rectangle(2, 2), Actions.Chain(new Modifiers.Dither(), new Modifiers.Blotches(2, 2), new HasValidVall(WallType), new Modifiers.IsEmpty(), new Actions.SetTile(51, setSelfFrames: true)));
        }

        WorldUtils.Gen(new Point(room.X, room.Y), new Shapes.Rectangle(room.Width, room.Height), Actions.Chain(new Modifiers.SkipWalls(
            [59, WallID.DirtUnsafe, WallID.CaveUnsafe, WallID.Cave2Unsafe, WallID.Cave3Unsafe, WallID.Cave4Unsafe, WallID.Cave5Unsafe, WallID.Cave7Unsafe, WallID.CaveWall, WallID.CaveWall2]), new Modifiers.Dither(0.85), new Modifiers.Blotches(2, 0.85), new Modifiers.SkipTiles([.. TileSets.Paintings]), new Modifiers.SkipWalls(
            [59, WallID.DirtUnsafe, WallID.CaveUnsafe, WallID.Cave2Unsafe, WallID.Cave3Unsafe, WallID.Cave4Unsafe, WallID.Cave5Unsafe, WallID.Cave7Unsafe, WallID.CaveWall, WallID.CaveWall2]),
            ((double)room.Y > (Math.Min(BackwoodsWorldGen.BackwoodsWorldGenPass.CenterY, (int)Main.worldSurface + 10) + BackwoodsWorldGen.BackwoodsWorldGenPass.EdgeY / 2)) ? ((GenAction)new ClearWallCustom(frameNeighbors: true, [59, WallID.DirtUnsafe, WallID.CaveUnsafe, WallID.Cave2Unsafe, WallID.Cave3Unsafe, WallID.Cave4Unsafe, WallID.Cave5Unsafe, WallID.Cave7Unsafe, WallID.CaveWall, WallID.CaveWall2])) : ((GenAction)new Actions.PlaceWall(2))));

        //WorldUtils.Gen(new Point(room.X, room.Y), new Shapes.Rectangle(room.Width, room.Height), Actions.Chain(new Modifiers.Dither(1), new Modifiers.Blotches(), new Modifiers.OnlyWalls(base.WallType), new Modifiers.SkipTiles(SkipTilesDuringWallAging), ((double)room.Y > Main.worldSurface) ? ((GenAction)new Actions.ClearWall(frameNeighbors: true)) : ((GenAction)new Actions.PlaceWall(2))));
        //WorldUtils.Gen(new Point(room.X, room.Y), new Shapes.Rectangle(room.Width, room.Height), Actions.Chain(new Modifiers.Dither(1), new Modifiers.OnlyTiles(30, 321, 158), new Actions.ClearTile(frameNeighbors: true)));
    }

    private class ClearWallCustom : GenAction {
        private bool _frameNeighbors;
        private ushort[] replaceTiles;

        public ClearWallCustom(bool frameNeighbors = false, params ushort[] replaceTiles) {
            _frameNeighbors = frameNeighbors;
            this.replaceTiles = replaceTiles;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args) {
            Tile tile = WorldGenHelper.GetTileSafely(x, y);
            if (!replaceTiles.Contains(tile.WallType)) {
                WorldUtils.ClearWall(x, y, _frameNeighbors);
                return UnitApply(origin, x, y, args);
            }
            return Fail();
            //else {
            //    WorldGenHelper.ReplaceWall(x, y, _dontReplaceWallType);
            //}
        }
    }

    public override void Place(HouseBuilderContext context, StructureMap structures) {
        base.Place(context, structures);
        //RainbowifyOnTenthAnniversaryWorlds();
    }

    //private void RainbowifyOnTenthAnniversaryWorlds() {
    //    if (!Main.tenthAnniversaryWorld || (base.TileType == 160 && WorldGen.genRand.Next(2) == 0))
    //        return;

    //    foreach (Rectangle room in base.Rooms) {
    //        WorldUtils.Gen(new Point(room.X, room.Y), new Shapes.Rectangle(room.Width, room.Height), new Actions.SetTileAndWallRainbowPaint());
    //    }
    //}
}
