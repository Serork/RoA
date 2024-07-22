using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Plants;

using System;
using System.Reflection;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static class TileHelper {
    public static FieldInfo _addSpecialPointSpecialPositions;
    public static FieldInfo _addSpecialPointSpecialsCount;

    public static T GetTE<T>(int i, int j) where T : ModTileEntity {
        if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity entity) && entity is T) {
            return entity as T;
        }

        throw new Exception("TileEntity not found");
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
        _addSpecialPointSpecialPositions = typeof(TileDrawing).GetField("_specialPositions", BindingFlags.NonPublic | BindingFlags.Instance);
        _addSpecialPointSpecialsCount = typeof(TileDrawing).GetField("_specialsCount", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public static void Unload() {
        _addSpecialPointSpecialPositions = null;
        _addSpecialPointSpecialsCount = null;
    }

    public static void AddSpecialPoint(int x, int y, int type) {
        TileDrawing tileDrawing = Main.instance.TilesRenderer;
        if (_addSpecialPointSpecialPositions.GetValue(tileDrawing) is Point[][] _specialPositions) {
            if (_addSpecialPointSpecialsCount.GetValue(tileDrawing) is int[] _specialsCount) {
                _specialPositions[type][_specialsCount[type]++] = new Point(x, y);
            }
        }
    }
}
