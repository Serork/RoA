using Microsoft.Xna.Framework;

using RiseofAges.Common.Utilities.Extensions;

using RoA.Content.Tiles.Plants;

using System;
using System.Collections.Generic;
using System.Reflection;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static class TileHelper {
    private static Point[][] _addSpecialPointSpecialPositions; 
    private static int[] _addSpecialPointSpecialsCount;
    private static List<Point> _addVineRootsPositions;

    public static T GetTE<T>(int i, int j) where T : ModTileEntity {
        if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity entity) && entity is T) {
            return entity as T;
        }

        return null;
    }

    public static void MergeWith(ushort type, params ushort[] types) {
        for (int i = 0; i < types.Length; ++i) {
            Main.tileMerge[type][types[i]] = true;
            Main.tileMerge[types[i]][type] = true;
        }
    }

    public static void Solid(ushort type, bool mergeDirt = true, bool blendAll = true, bool blockLight = true, bool brick = true) {
        Main.tileBrick[type] = brick;
        Main.tileSolid[type] = true;

        Main.tileMergeDirt[type] = mergeDirt;
        Main.tileBlendAll[type] = blendAll;
        Main.tileBlockLight[type] = blockLight;
    }

    public static void Load() {
        _addSpecialPointSpecialPositions = (Point[][])typeof(TileDrawing).GetFieldValue("_specialPositions", Main.instance.TilesRenderer);
        _addSpecialPointSpecialsCount = (int[])typeof(TileDrawing).GetFieldValue("_specialsCount", Main.instance.TilesRenderer);
        _addVineRootsPositions = (List<Point>)typeof(TileDrawing).GetFieldValue("_vineRootsPositions", Main.instance.TilesRenderer);
    }

    public static void Unload() {
        _addSpecialPointSpecialPositions = null;
        _addSpecialPointSpecialsCount = null;
        _addVineRootsPositions = null;
    }

    public static void AddSpecialPoint(int i, int j, ushort tileType) => _addSpecialPointSpecialPositions[tileType][_addSpecialPointSpecialsCount[tileType]++] = new Point(i, j);
    public static void AddVineRootPosition(Point item) {
        if (!_addVineRootsPositions.Contains(item)) {
            _addVineRootsPositions.Add(item);
        }
    }
}
