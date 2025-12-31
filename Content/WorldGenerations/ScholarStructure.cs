using Microsoft.Xna.Framework;

using ReLogic.Utilities;

using RoA.Common;
using RoA.Common.World;
using RoA.Content.Tiles.LiquidsSpecific;
using RoA.Content.Tiles.Miscellaneous;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

sealed class ScholarStructure : IInitializer {
    class ScholarStructure_TightPlacer : ModSystem {
        public override void PostUpdateInput() {
            //if (Main.mouseRightRelease && Main.mouseRight) {
            //    int x = Player.tileTargetX,
            //        y = Player.tileTargetY;
            //    float angle = (new Vector2(x, y) * 16f).AngleTo(new Point16(Main.maxTilesX / 2, Main.maxTilesY / 2).ToWorldCoordinates());
            //    angle -= MathHelper.PiOver2;
            //    PlaceTight2(x, y, angle);
            //}
        }
    }

    private delegate bool SlabState(int x, int y, int scale);

    private static class SlabStates {
        public static bool Empty(int x, int y, int scale) => false;
        public static bool Solid(int x, int y, int scale) => true;
        public static bool HalfBrick(int x, int y, int scale) => y >= scale / 2;
        public static bool BottomRightFilled(int x, int y, int scale) => x >= scale - y;
        public static bool BottomLeftFilled(int x, int y, int scale) => x < y;
        public static bool TopRightFilled(int x, int y, int scale) => x > y;
        public static bool TopLeftFilled(int x, int y, int scale) => x < scale - y;
    }

    private readonly struct Slab {
        public readonly SlabState State;
        public readonly bool HasWall;

        public bool IsSolid => State != new SlabState(SlabStates.Empty);

        private Slab(SlabState state, bool hasWall) {
            State = state;
            HasWall = hasWall;
        }

        public Slab WithState(SlabState state) => new Slab(state, HasWall);
        public static Slab Create(SlabState state, bool hasWall) => new Slab(state, hasWall);
    }

    private const int SCALE = 3;
    private Slab[,] _slabs = null!;

    private static ushort PLACEHOLDERTILETYPE => TileID.Adamantite;
    private static ushort PLACEHOLDERTILETYPE2 => TileID.Mythril;

    private static string ScholarStructureMap =>
        "725\r\n760\r\n4174\r\n4224\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,54,2,0,True,|\r\nTrue,0,False,False,False,False,273,54,0,0,0,True,|\r\nTrue,0,False,False,True,False,273,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,273,90,54,1,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,36,54,2,0,True,|\r\nTrue,0,False,False,False,False,273,54,90,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,36,0,0,True,|\r\nTrue,147,0,False,False,False,False,273,36,36,0,0,True,|\r\nTrue,147,0,False,False,True,False,273,54,126,0,0,True,|\r\nTrue,147,0,False,False,False,False,273,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,90,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,54,1,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,72,54,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,54,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,1,162,36,2,0,True,|\r\nTrue,0,False,False,False,False,1,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,36,0,0,True,|\r\nTrue,147,0,False,False,False,False,273,90,72,3,0,True,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,147,0,False,False,True,False,42,18,0,0,0,True,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,147,0,False,False,False,False,273,72,72,4,0,True,|\r\nTrue,0,False,False,False,False,273,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,273,90,54,1,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,54,0,0,True,|\r\nTrue,0,False,False,False,False,273,90,54,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,72,18,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,1,126,54,0,0,True,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,147,0,False,False,False,False,42,18,18,0,0,True,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,147,0,False,False,False,False,273,72,162,4,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,54,1,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,36,54,2,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,273,72,18,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,72,4,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,1,54,54,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,147,0,False,False,False,False,273,72,54,0,0,False,|\r\nTrue,147,0,False,False,False,False,273,18,0,0,0,False,|\r\nTrue,147,0,False,False,False,False,273,18,54,0,0,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,72,4,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,90,54,1,0,True,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,0,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,72,3,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,1,18,54,0,0,True,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,147,0,False,False,False,False,273,36,54,2,0,False,|\r\nTrue,147,0,False,False,False,False,273,36,18,0,0,False,|\r\nTrue,147,0,False,False,False,False,273,54,126,0,0,False,|\r\nTrue,147,0,False,False,False,False,273,36,18,0,0,False,|\r\nTrue,147,0,False,False,False,False,273,54,54,1,0,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,36,72,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,90,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,72,18,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,0,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,72,18,0,0,True,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,147,0,False,False,False,False,273,72,54,2,0,False,|\r\nTrue,147,0,False,False,False,False,273,18,18,0,0,False,|\r\nTrue,21,0,False,False,False,False,273,18,72,3,0,False,|\r\nFalse,21,0,False,False,False,False,|\r\nTrue,21,0,False,False,False,False,273,0,72,4,0,False,|\r\nTrue,147,0,False,False,False,False,273,18,18,0,0,False,|\r\nTrue,147,0,False,False,False,False,273,90,54,1,0,False,|\r\nTrue,147,0,False,False,False,False,245,216,0,0,0,True,|\r\nTrue,147,0,False,False,False,False,245,234,0,0,0,True,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,36,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,72,0,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,147,0,False,False,False,False,38,90,72,3,0,True,|\r\nFalse,27,0,False,False,False,False,|\r\nFalse,27,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,147,0,False,False,False,False,273,0,18,0,0,False,|\r\nTrue,21,0,False,False,False,False,273,54,72,3,0,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nTrue,21,0,False,False,False,False,273,72,162,4,0,False,|\r\nTrue,147,0,False,False,False,False,273,72,18,0,0,False,|\r\nTrue,147,0,False,False,False,False,245,216,18,0,0,True,|\r\nTrue,147,0,False,False,False,False,245,234,18,0,0,True,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,27,0,False,False,False,False,19,36,0,0,0,True,|\r\nTrue,27,0,False,False,False,False,19,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,72,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,72,126,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,54,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,72,36,0,0,True,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,147,0,False,False,False,False,273,90,0,0,0,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nTrue,147,0,False,False,False,False,273,90,0,0,0,False,|\r\nTrue,147,0,False,False,False,False,245,216,36,0,0,True,|\r\nTrue,147,0,False,False,False,False,245,234,36,0,0,True,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,0,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,54,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,72,162,4,0,True,|\r\nTrue,0,False,False,False,False,273,36,36,0,0,True,|\r\nTrue,139,28,False,False,False,False,273,54,72,0,0,True,|\r\nTrue,147,0,False,False,False,False,104,0,0,0,0,True,|\r\nTrue,21,0,False,False,False,False,104,18,0,0,0,True,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,147,0,False,False,False,False,273,144,144,0,0,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nTrue,147,0,False,False,False,False,273,144,144,0,0,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,147,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,36,72,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,273,90,162,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,0,18,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,72,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,104,0,18,0,0,True,|\r\nTrue,21,0,False,False,False,False,104,18,18,0,0,True,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,0,36,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,72,36,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,0,18,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,72,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,104,0,36,0,0,True,|\r\nTrue,21,0,False,False,False,False,104,18,36,0,0,True,|\r\nFalse,21,0,False,False,False,False,|\r\nTrue,5,0,False,False,False,False,38,108,0,1,0,True,|\r\nFalse,5,0,False,False,False,False,|\r\nTrue,5,0,False,False,False,False,174,18,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,50,0,0,0,0,True,|\r\nTrue,21,0,False,False,False,False,50,0,0,0,0,True,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nTrue,21,0,False,False,False,False,13,72,0,0,0,True,|\r\nFalse,5,0,False,False,False,False,|\r\nTrue,5,0,False,False,False,False,50,54,0,0,0,True,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,21,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,0,36,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,72,0,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,0,0,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,72,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,104,0,54,0,0,True,|\r\nTrue,27,0,False,False,False,False,104,18,54,0,0,True,|\r\nFalse,27,0,False,False,False,False,|\r\nTrue,5,0,False,False,False,False,38,90,36,0,0,True,|\r\nTrue,5,0,False,False,False,False,1,54,54,0,0,True,|\r\nTrue,5,0,False,False,False,False,14,918,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,14,936,0,0,0,True,|\r\nTrue,27,0,False,False,False,False,14,954,0,0,0,True,|\r\nFalse,27,0,False,False,False,False,|\r\nFalse,27,0,False,False,False,False,|\r\nTrue,27,0,False,False,False,False,15,18,0,0,0,True,|\r\nTrue,27,0,False,False,False,False,14,0,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,14,18,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,14,36,0,0,0,True,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,27,0,False,False,False,False,|\r\nFalse,27,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,0,18,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,72,0,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,0,36,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,72,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,104,0,72,0,0,True,|\r\nTrue,5,0,False,False,False,False,104,18,72,0,0,True,|\r\nTrue,5,0,False,False,False,False,1,36,54,0,0,True,|\r\nTrue,5,0,False,False,False,False,30,0,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,1,54,72,0,0,True,|\r\nTrue,5,0,False,False,False,False,14,918,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,14,936,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,14,954,18,0,0,True,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nTrue,5,0,False,False,False,False,15,18,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,14,0,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,14,18,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,14,36,18,0,0,True,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,0,0,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,72,36,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,72,54,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,90,72,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,54,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,139,28,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,72,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,162,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,144,72,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,0,0,0,True,|\r\nTrue,147,0,False,False,False,False,191,54,0,0,28,True,|\r\nTrue,0,False,False,False,False,273,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,273,90,54,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,36,72,4,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,72,36,0,0,True,|\r\nFalse,27,0,False,False,False,False,|\r\nTrue,27,0,False,False,False,False,50,36,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,50,36,0,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,4,0,False,False,False,False,19,90,0,0,0,True,|\r\nFalse,139,28,False,False,False,False,|\r\nTrue,4,0,False,False,False,False,19,90,0,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,50,36,0,0,0,True,|\r\nTrue,27,0,False,False,False,False,50,72,0,0,0,True,|\r\nFalse,27,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,30,0,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,18,0,28,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,72,3,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,273,0,0,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,27,0,False,False,False,False,30,90,54,0,0,True,|\r\nTrue,27,0,False,False,False,False,19,54,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,18,0,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,4,0,False,False,False,False,19,36,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,18,0,0,0,True,|\r\nFalse,139,28,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,4,0,False,False,False,False,19,36,0,0,0,True,|\r\nTrue,27,0,False,False,False,False,19,72,0,0,0,True,|\r\nTrue,27,0,False,False,False,False,30,72,54,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,72,36,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,472,72,54,2,0,True,|\r\nTrue,0,False,False,False,False,472,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,27,0,False,False,False,False,30,72,36,0,0,True,|\r\nFalse,27,0,False,False,False,False,|\r\nTrue,4,0,False,False,False,False,50,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,50,54,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,36,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,18,0,0,0,True,|\r\nFalse,139,28,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,4,0,False,False,False,False,50,0,0,0,0,True,|\r\nTrue,27,0,False,False,False,False,50,36,0,0,0,True,|\r\nFalse,27,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,30,0,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,54,1,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,472,72,54,2,0,True,|\r\nTrue,0,False,False,False,False,472,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,27,0,False,False,False,False,191,36,18,0,0,True,|\r\nTrue,27,0,False,False,False,False,191,90,54,0,0,True,|\r\nTrue,27,0,False,False,False,False,19,54,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,18,0,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,4,0,False,False,False,False,19,36,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,139,28,False,False,False,False,19,18,0,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,4,0,False,False,False,False,19,36,0,0,0,True,|\r\nTrue,27,0,False,False,False,False,19,72,0,0,0,True,|\r\nTrue,27,0,False,False,False,False,191,0,54,0,0,True,|\r\nTrue,27,0,False,False,False,False,191,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,54,1,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,472,72,54,2,0,True,|\r\nTrue,0,False,False,False,False,472,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,27,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,27,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,27,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,18,0,0,True,|\r\nTrue,4,0,False,False,False,False,191,54,54,0,0,True,|\r\nFalse,27,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,4,0,False,False,False,False,19,36,0,0,0,True,|\r\nTrue,139,28,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,18,0,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,27,0,False,False,False,False,|\r\nTrue,27,0,False,False,False,False,30,36,54,0,0,True,|\r\nTrue,27,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,27,0,False,False,False,False,30,54,18,0,0,True,|\r\nTrue,27,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,27,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,90,54,1,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,472,72,54,2,0,True,|\r\nTrue,0,False,False,False,False,472,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,139,27,False,False,False,False,191,90,72,0,0,True,|\r\nTrue,27,0,False,False,False,False,19,36,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,139,28,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,27,0,False,False,False,False,19,18,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,191,0,72,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,54,1,0,True,|\r\nTrue,0,False,False,False,False,472,0,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,18,0,0,True,|\r\nTrue,147,0,False,False,False,False,191,36,36,0,0,True,|\r\nTrue,147,0,False,False,False,False,30,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,72,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,90,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,273,36,54,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,54,90,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,36,0,0,0,False,|\r\nTrue,147,0,False,False,False,False,273,36,0,0,0,False,|\r\nTrue,147,0,False,False,False,False,273,54,90,0,0,False,|\r\nTrue,147,0,False,False,False,False,273,36,0,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,36,0,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,54,90,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,54,54,0,0,False,|\r\nTrue,139,27,False,False,False,False,124,90,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,72,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,273,54,108,0,0,True,|\r\nTrue,0,False,False,False,False,273,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,72,0,0,0,True,|\r\nTrue,0,False,False,False,False,472,0,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,27,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,72,0,0,True,|\r\nTrue,4,0,False,False,False,False,50,36,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,50,72,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,50,72,0,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,90,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,273,0,0,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,54,18,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,18,18,0,0,False,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,False,|\r\nTrue,147,29,False,False,False,False,273,54,18,0,0,False,|\r\nTrue,147,29,False,False,False,False,273,18,18,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,18,18,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,54,18,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,72,0,0,0,False,|\r\nTrue,139,27,False,False,False,False,124,90,18,0,0,True,|\r\nTrue,4,0,False,False,False,False,50,36,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,50,54,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,174,18,0,0,0,True,|\r\nTrue,6,0,False,False,False,False,191,36,72,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,472,72,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,0,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,78,0,False,False,False,False,191,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,36,0,0,True,|\r\nTrue,4,0,False,False,False,False,30,36,36,0,0,True,|\r\nTrue,78,0,False,False,False,False,30,36,36,0,0,True,|\r\nTrue,4,0,False,False,False,False,191,18,72,3,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,6,0,False,False,False,False,124,90,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,0,0,0,0,True,|\r\nTrue,147,0,False,False,False,False,101,18,0,0,0,True,|\r\nTrue,147,0,False,False,False,False,101,36,0,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,90,36,0,0,True,|\r\nTrue,5,0,False,False,False,False,273,0,126,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,36,18,0,0,False,|\r\nTrue,5,29,False,False,False,False,273,36,18,0,0,False,|\r\nTrue,147,29,False,False,False,False,273,54,108,0,0,False,|\r\nTrue,147,29,False,False,False,False,273,36,36,0,0,False,|\r\nTrue,147,29,False,False,False,False,273,36,18,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,54,108,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,36,18,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,72,18,0,0,False,|\r\nTrue,139,27,False,False,False,False,124,90,18,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,0,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,18,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,36,0,0,0,True,|\r\nTrue,6,0,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,4,0,False,False,False,False,191,72,72,4,0,True,|\r\nTrue,78,0,False,False,False,False,191,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,78,0,False,False,False,False,30,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,72,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,72,72,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,36,0,0,True,|\r\nTrue,4,0,False,False,False,False,30,90,72,3,0,True,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,6,0,False,False,False,False,124,90,36,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,0,18,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,18,18,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,36,18,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,90,36,0,0,True,|\r\nTrue,5,0,False,False,False,False,273,0,36,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,18,18,0,0,False,|\r\nTrue,0,False,False,False,False,273,18,18,0,0,False,|\r\nTrue,147,29,False,False,False,False,273,90,72,3,0,False,|\r\nFalse,147,29,False,False,False,False,|\r\nTrue,147,29,False,False,False,False,273,0,72,4,0,False,|\r\nTrue,5,29,False,False,False,False,273,54,18,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,18,18,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,72,0,0,0,False,|\r\nTrue,139,27,False,False,False,False,124,90,36,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,0,18,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,18,18,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,36,18,0,0,True,|\r\nTrue,6,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,4,0,False,False,False,False,124,90,18,0,0,True,|\r\nTrue,4,0,False,False,False,False,30,0,72,4,0,True,|\r\nTrue,0,False,False,False,False,30,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,72,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,78,0,False,False,False,False,|\r\nFalse,78,0,False,False,False,False,|\r\nFalse,78,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,78,0,False,False,False,False,|\r\nFalse,78,0,False,False,False,False,|\r\nFalse,78,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,6,0,False,False,False,False,124,90,18,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,0,36,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,18,36,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,36,36,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,90,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,273,0,18,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,54,108,0,0,False,|\r\nTrue,5,29,False,False,False,False,273,72,18,0,0,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nTrue,5,29,False,False,False,False,273,0,18,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,54,108,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,72,18,0,0,False,|\r\nTrue,139,27,False,False,False,False,124,90,18,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,0,36,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,18,36,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,36,36,0,0,True,|\r\nTrue,6,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,78,0,False,False,False,False,|\r\nFalse,78,0,False,False,False,False,|\r\nFalse,78,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nFalse,78,0,False,False,False,False,|\r\nFalse,78,0,False,False,False,False,|\r\nFalse,78,0,False,False,False,False,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,4,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,50,54,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,174,18,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,50,18,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,13,36,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,50,0,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,174,18,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,6,0,False,False,False,False,124,90,36,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,0,54,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,18,54,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,36,54,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,90,36,0,0,True,|\r\nTrue,5,0,False,False,False,False,273,0,72,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,54,36,0,0,False,|\r\nTrue,5,29,False,False,False,False,273,18,72,3,0,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nTrue,5,29,False,False,False,False,273,0,72,4,0,False,|\r\nTrue,5,0,False,False,False,False,273,54,36,0,0,False,|\r\nTrue,5,0,False,False,False,False,273,18,72,0,0,False,|\r\nTrue,139,27,False,False,False,False,124,90,0,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,0,54,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,18,54,0,0,True,|\r\nTrue,4,0,False,False,False,False,101,36,54,0,0,True,|\r\nTrue,6,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,13,18,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,50,18,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,50,72,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,174,18,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,50,36,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,50,36,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,6,0,False,False,False,False,124,90,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,19,36,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,19,18,0,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,5,29,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,90,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,19,36,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,19,0,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,19,18,0,0,0,True,|\r\nTrue,6,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,18,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,18,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,18,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,18,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,6,0,False,False,False,False,124,90,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,0,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,18,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,36,0,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,5,29,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,90,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,0,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,18,0,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,36,0,0,0,True,|\r\nTrue,6,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,18,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,18,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,18,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,18,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,36,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,36,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,36,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,36,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,36,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,36,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,6,0,False,False,False,False,124,90,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,0,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,18,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,36,18,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,5,29,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,90,36,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,0,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,18,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,36,18,0,0,True,|\r\nTrue,6,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,36,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,36,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,36,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,36,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,36,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,36,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,36,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,54,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,54,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,54,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,54,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,54,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,54,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,6,0,False,False,False,False,124,90,36,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,0,36,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,18,36,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,36,36,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,5,29,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,90,0,0,0,True,|\r\nTrue,147,0,False,False,False,False,101,0,36,0,0,True,|\r\nTrue,147,0,False,False,False,False,101,18,36,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,36,36,0,0,True,|\r\nTrue,6,0,False,False,False,False,124,90,18,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,54,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,54,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,54,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,101,0,54,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,18,54,0,0,True,|\r\nTrue,78,0,False,False,False,False,101,36,54,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,78,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,78,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,30,162,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,0,0,0,True,|\r\nTrue,6,0,False,False,False,False,30,36,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,191,54,0,0,0,True,|\r\nTrue,78,0,False,False,False,False,191,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,0,0,0,True,|\r\nTrue,27,0,False,False,False,False,30,54,0,0,0,True,|\r\nTrue,6,0,False,False,False,False,191,90,54,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,0,54,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,18,54,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,36,54,0,0,True,|\r\nTrue,139,27,False,False,False,False,124,90,0,0,0,True,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,147,29,False,False,False,False,|\r\nFalse,5,29,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nFalse,5,0,False,False,False,False,|\r\nTrue,139,27,False,False,False,False,124,90,18,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,0,54,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,18,54,0,0,True,|\r\nTrue,5,0,False,False,False,False,101,36,54,0,0,True,|\r\nTrue,6,0,False,False,False,False,191,36,54,0,0,True,|\r\nTrue,4,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,90,54,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,38,72,72,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,1,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,147,0,False,False,False,False,191,18,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,0,0,0,True,|\r\nTrue,139,27,False,False,False,False,191,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,147,29,False,False,False,False,30,54,0,0,0,True,|\r\nTrue,147,29,False,False,False,False,30,18,0,0,0,True,|\r\nTrue,147,29,False,False,False,False,30,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,54,0,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,0,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,4,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,191,72,0,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,38,36,72,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,191,54,72,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,38,0,72,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,1,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,139,27,False,False,False,False,38,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,30,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,72,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,38,36,72,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,72,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,38,0,72,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,90,72,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,38,0,72,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,90,72,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,38,0,72,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,18,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,54,72,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,38,72,72,0,0,True,|\r\nTrue,0,False,False,False,False,38,90,72,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nTrue,0,False,False,False,False,38,36,72,0,0,True,|\r\nTrue,0,False,False,False,False,38,36,36,0,0,True,|\r\nTrue,0,False,False,False,False,38,18,72,0,0,True,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\nFalse,0,False,False,False,False,|\r\n";
    
    void ILoadable.Load(Mod mod) {
        WorldCommon.ModifyWorldGenTasksEvent += WorldCommon_ModifyWorldGenTasksEvent;
        WorldCommon.PostUpdateNPCsEvent += WorldCommon_PostUpdateNPCsEvent;
    }

    private void WorldCommon_PostUpdateNPCsEvent() {
        //if (Main.mouseRight && Main.mouseRightRelease) {
        //    PlaceTight(Player.tileTargetX, Player.tileTargetY, true, 5, 10);
        //}
    }

    private void WorldCommon_ModifyWorldGenTasksEvent(System.Collections.Generic.List<Terraria.WorldBuilding.GenPass> tasks, ref double totalWeight) {
        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shimmer"));

        genIndex += 35;

        tasks.Insert(genIndex, new PassLegacy("Scholar Structure", ScholarStructure_Generator, 0.2074f));
    }

    private void ScholarStructure_Generator(GenerationProgress progress, GameConfiguration configuration) {
        UnifiedRandom genRand = WorldGen.genRand;

        Point origin = Point.Zero;

        bool flag56 = true;
        while (flag56) {
            int num932 = genRand.Next((int)((double)Main.maxTilesX * 0.3), (int)((double)Main.maxTilesX * 0.7));

            int num933 = genRand.Next((int)Main.worldSurface + 50, Main.maxTilesY - 300);
            flag56 = false;
            int num934 = 100;
            for (int num936 = num932 - num934; num936 < num932 + num934; num936 += 3) {
                for (int num937 = num933 - num934; num937 < num933 + num934; num937 += 3) {
                    if (WorldGen.InWorld(num936, num937)) {
                        ushort[] skipTileTypes = [TileID.Crimstone, TileID.Ebonstone, TileID.MushroomGrass, 147, 161, 162, 60, 368, 367, (ushort)ModContent.TileType<SolidifiedTar>()];
                        if (Main.tile[num936, num937].HasTile && skipTileTypes.Contains(Main.tile[num936, num937].TileType)) {
                            flag56 = true;
                            break;
                        }

                        if (GenVars.UndergroundDesertLocation.Contains(new Point(num936, num937))) {
                            flag56 = true;
                            break;
                        }
                    }
                    else {
                        flag56 = true;
                    }
                }
            }

            if (!flag56) {
                origin = new(num932, num933);
            }
        }

        Point baseOrigin = origin;

        _slabs ??= new Slab[56, 26];
        int sizeX = 40;
        int sizeY = 20;
        int sizeBetween = (sizeY * 4 - 10) / 3;
        origin.X -= sizeX * 3 / 2;
        origin.Y -= sizeY * 3 / 2;
        for (int i = -1; i < sizeX + 1; i++) {
            double num4 = (double)(i - sizeX / 2) / (double)sizeX + 0.5;
            int num5 = (int)((0.5 - Math.Abs(num4 - 0.5)) * 1) - 5;
            for (int j = -1; j < sizeY + 1; j++) {
                bool hasWall = true;
                bool flag = false;
                bool flag2 = IsGroupSolid(i * 3 + origin.X, j * 3 + origin.Y, 10);
                int num6 = Math.Abs(j - sizeY / 2) - sizeBetween / 6 + num5;
                if (num6 > 20) {
                    flag = flag2;
                    hasWall = false;
                }
                else if (num6 > 0) {
                    flag = j - sizeY / 2 > 0 || flag2;
                    hasWall = j - sizeY / 2 < 0 || num6 <= 2;
                }
                else if (num6 == 0) {
                    flag = j - sizeY / 2 > 0 || flag2;
                }

                //if (Math.Abs(num4 - 0.5) > 0.5 + genRand.NextDouble() * 0.05) {
                //    hasWall = false;
                //    flag = false;
                //}

                //flag = true;

                _slabs[i + 1, j + 1] = Slab.Create(flag ? new SlabState(SlabStates.Solid) : new SlabState(SlabStates.Empty), hasWall);
            }
        }

        int num7 = sizeX / 2;
        int num8 = sizeY / 2;
        int num9 = (num8 + 1) * (num8 + 1);
        double value = -1;
        double num10 = 0;
        double value2 = 1;
        double num11 = 0.0;
        List<(Point16, bool)> tightPositions = [];
        for (int m = 0; m <= sizeX; m++) {
            double num12 = (double)num8 / (double)num7 * (double)(m - num7);
            int num13 = Math.Min(num8, (int)Math.Sqrt(Math.Max(0.0, (double)num9 - num12 * num12)));
            num11 = ((m >= sizeX / 2) ? (num11 + Utils.Lerp(num10, value2, (double)m / (double)(sizeX / 2) - 1.0))
                : (num11 + Utils.Lerp(value, num10, (double)m / (double)(sizeX / 2))));
            for (int n = num8 - num13; n <= num8 + num13; n++) {
                bool bottom = n > num8;
                int x = m * 3 + origin.X + WorldGen.genRand.Next(-3, 4);
                int y = n * 3 + origin.Y + (int)num11 + (int)(Math.Sin(m * 0.275f * genRand.NextFloat(0.75f, 1f)) * 5);
                PlaceSlab(_slabs[m + 1, n + 1], x, y, 6);
                //if (_slabs[m + 1, n + 1].IsSolid && m % 6 == 0 && !bottom) {
                //    PlaceTight(x, y, bottom);
                //}
                if (_slabs[m + 1, n + 1].IsSolid && m % 6 == 0 && !bottom) {
                    //PlaceTight2(x, y);
                }
            }
        }
        int sizeX2 = num7 * 3,
            sizeY2 = num8 * 3;
        Point16 center = new((int)(origin.X + sizeX2), (int)(origin.Y + sizeY2 - 10));
        for (int i = -sizeX2 + 10; i <= sizeX2 - 5;) {
            int x = center.X + i;
            int y = center.Y;
            int attempts = 3;
            while (!Main.tile[x, y].HasTile || 
                (Main.tile[x, y].TileType == PLACEHOLDERTILETYPE && attempts-- > 0) ||
                Main.tile[x, y].TileType == PLACEHOLDERTILETYPE2) {
                y--;
            }
            y -= 1;
            Point16 center2 = new((int)(origin.X + sizeX2 + i / 6), (int)(origin.Y + sizeY2 + 10));
            float angle = (new Vector2(x, y) * 16f).AngleTo(center2.ToWorldCoordinates());
            angle += genRand.NextFloatRange(0.5f);                    
            angle -= MathHelper.PiOver2;
            float maxAngle = 0.75f;
            angle = Utils.Clamp(angle, -maxAngle, maxAngle);
            PlaceTight2(x, y, angle, out int sizeX3);
            i += sizeX3;
        }
        for (int i = -sizeX2 + 2; i <= sizeX2 + 5 - 3;) {
            int x = center.X + i;
            if (Math.Abs(x - center.X) < 37) {
                i++;
                continue;
            }
            int y = center.Y + 10;
            int attempts = 3;
            while (!Main.tile[x, y].HasTile ||
                (Main.tile[x, y].TileType == PLACEHOLDERTILETYPE && attempts-- > 0) ||
                Main.tile[x, y].TileType == PLACEHOLDERTILETYPE2) {
                y++;
            }
            y += sizeY2 / 7;
            Point16 center3 = new((int)(origin.X + sizeX2 + i / genRand.NextFloat(2f, 3f)), (int)(origin.Y + sizeY2 - 10));
            float angle = (new Vector2(x, y) * 16f).AngleTo(center3.ToWorldCoordinates());
            angle += genRand.NextFloatRange(0.5f);
            angle -= MathHelper.PiOver2;
            float maxAngle = 0.75f;
            PlaceTight2(x, y, angle, out int sizeX3, true);
            i += sizeX3;
        }

        for (int i = origin.X - 30; i < origin.X + sizeX * 3 + 30; i++) {
            for (int j = origin.Y - 30; j < origin.Y + sizeY * 3 + 30; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                int chance = 25;
                chance += (int)((float)(j - Main.rockLayer) / (Main.maxTilesY - 300 - Main.rockLayer) * 15);
                if (tile.HasTile && (tile.TileType == PLACEHOLDERTILETYPE || tile.TileType == PLACEHOLDERTILETYPE2)) {
                    if (genRand.NextBool(chance)) {
                        WorldGen.TileRunner(i, j, genRand.Next(2, 6), genRand.Next(2, 40), TileID.Dirt, addTile: false, overRide: true);
                        ////WorldGenHelper.ModifiedTileRunner(i, j, genRand.Next(5, 15), genRand.Next(1, 10), TileID.Dirt, addTile: false, overRide: true);
                    }
                }
            }
        }

        for (int i = origin.X - 30; i < origin.X + sizeX * 3 + 30; i++) {
            for (int j = origin.Y - 30; j < origin.Y + sizeY * 3 + 30; j++) {
                WorldUtils.TileFrame(i, j, frameNeighbors: true);
                WorldGen.SquareWallFrame(i, j);
                Tile.SmoothSlope(i, j);
            }
        }

        for (int i = origin.X - 30; i < origin.X + sizeX * 3 + 30; i++) {
            for (int j = origin.Y - 30; j < origin.Y + sizeY * 3 + 30; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.HasTile && (tile.TileType == PLACEHOLDERTILETYPE || tile.TileType == PLACEHOLDERTILETYPE2)) {
                    tile.TileType = TileID.Stone;
                }
            }
        }

        baseOrigin.Y += sizeY + 3;
        StructureGenerator.GenerateStructureFromMap(ScholarStructureMap, new Point16(baseOrigin.X, baseOrigin.Y - 2), StructureGenerator.StructureOriginType.BottomCenter,
            cleanUpFluffY: 4);

        int centerX = (int)(origin.X + sizeX * 1.5f);
        int centerY = (int)(origin.Y + sizeY * 1.5f);

        int[] archiveDecorTileTypes = [ModContent.TileType<ScholarsArchive>(), ModContent.TileType<ScholarsDesk>()];
        int archiveX = centerX,
            archiveY = centerY;
        while (!WorldGenHelper.SolidTileNoPlatform(archiveX, archiveY) ||
               Main.tile[archiveX, archiveY].IsActuated) {
            archiveY++;
        }
        archiveY -= 1;
        WorldGenHelper.Place3x4(archiveX, archiveY, (ushort)archiveDecorTileTypes[0], 0);
        WorldGenHelper.Place3x2(archiveX + 3, archiveY, (ushort)archiveDecorTileTypes[1], 0);
        WorldGenHelper.Place3x2(archiveX - 3, archiveY, (ushort)archiveDecorTileTypes[1], 1);

        ushort[] tableTiles = [TileID.Books, TileID.Bottles, TileID.PlatinumCandle, TileID.Candles];
        List<Point16> tableTilePositions = [];
        for (int i = origin.X - 30; i < origin.X + sizeX * 3 + 30; i++) {
            for (int j = origin.Y - 30; j < origin.Y + sizeY * 3 + 30; j++) {
                Tile tile = Main.tile[i, j];
                if (tableTiles.Contains(tile.TileType)) {
                    tableTilePositions.Add(new Point16(i, j + 1));
                    tile.HasTile = false;
                }
            }
        }
        foreach (var tableTilePosition in tableTilePositions) {
            int i = tableTilePosition.X,
                j = tableTilePosition.Y;
            Tile tile = Main.tile[i, j];
            if (Main.tile[i, j - 1].HasTile) {
                continue;
            }
            if (genRand.NextChance(0.1f)) {
                continue;
            }
            if (tile.TileType == TileID.Platforms ||
                tile.TileType == TileID.Bookcases) {
                if (genRand.NextChance(0.85f)) {
                    WorldGen.PlaceTile(i, j - 1, TileID.Books, mute: true);
                }
                else if (genRand.NextChance(0.1f)) {
                    if (genRand.NextBool()) {
                        Tile tile2 = Main.tile[i, j - 1];
                        tile2.HasTile = true;
                        tile2.TileType = TileID.Candles;
                        tile2.TileFrameX = 18;
                    }
                    else {
                        Tile tile2 = Main.tile[i, j - 1];
                        tile2.HasTile = true;
                        tile2.TileType = TileID.PlatinumCandle;
                        tile2.TileFrameX = 18;
                    }
                }
                else if (genRand.NextChance(0.5f)) {
                    if (genRand.NextBool()) {
                        Tile tile2 = Main.tile[i, j - 1];
                        tile2.HasTile = true;
                        tile2.TileType = TileID.Bottles;
                        tile2.TileFrameX = 18;
                    }
                    else {
                        Tile tile2 = Main.tile[i, j - 1];
                        tile2.HasTile = true;
                        tile2.TileType = TileID.Bottles;
                        tile2.TileFrameX = 36;
                    }
                }
            }
            if (tile.TileType == TileID.Tables ||
                tile.TileType == TileID.Tables2) {
                if (genRand.NextChance(0.5f)) {
                    Tile tile2 = Main.tile[i, j - 1];
                    tile2.HasTile = true;
                    tile2.TileType = TileID.Bottles;
                    tile2.TileFrameX
                        = (short)(genRand.NextChance(0.75f) ? genRand.Next(4, 7) * 18 : 0);
                }
                else if (genRand.NextChance(0.85f)) {
                    WorldGen.PlaceTile(i, j - 1, TileID.Books, mute: true);
                }
                else if (genRand.NextChance(0.1f)) {
                    if (genRand.NextBool()) {
                        Tile tile2 = Main.tile[i, j - 1];
                        tile2.HasTile = true;
                        tile2.TileType = TileID.Candles;
                        tile2.TileFrameX = 18;
                    }
                    else {
                        Tile tile2 = Main.tile[i, j - 1];
                        tile2.HasTile = true;
                        tile2.TileType = TileID.PlatinumCandle;
                        tile2.TileFrameX = 18;
                    }
                }
                else if (genRand.NextChance(0.05f)) {
                    if (genRand.NextBool()) {
                        Tile tile2 = Main.tile[i, j - 1];
                        tile2.HasTile = true;
                        tile2.TileType = TileID.Bottles;
                        tile2.TileFrameX = 18;
                    }
                    else {
                        Tile tile2 = Main.tile[i, j - 1];
                        tile2.HasTile = true;
                        tile2.TileType = TileID.Bottles;
                        tile2.TileFrameX = 36;
                    }
                }
            }
        }
        for (int i = origin.X - 30; i < origin.X + sizeX * 3 + 30; i++) {
            for (int j = origin.Y - 30; j < origin.Y + sizeY * 3 + 30; j++) {
                Tile tile = Main.tile[i, j];
                if (Main.tile[i, j - 1].HasTile) {
                    continue;
                }
                if (genRand.NextBool()) {
                    continue;
                }
                if (tile.TileType == TileID.Tables ||
                    tile.TileType == TileID.Tables2) {
                    if (genRand.NextChance(0.35f)) {
                        Tile tile2 = Main.tile[i, j - 1];
                        tile2.HasTile = true;
                        tile2.TileType = TileID.Bottles;
                        tile2.TileFrameX
                            = (short)(genRand.NextChance(0.75f) ? genRand.Next(4, 7) * 18 : 0);
                    }
                    else if (genRand.NextChance(0.85f)) {
                        WorldGen.PlaceTile(i, j - 1, TileID.Books, mute: true);
                    }
                    else if (genRand.NextChance(0.1f)) {
                        if (genRand.NextBool()) {
                            Tile tile2 = Main.tile[i, j - 1];
                            tile2.HasTile = true;
                            tile2.TileType = TileID.Candles;
                            tile2.TileFrameX = 18;
                        }
                        else {
                            Tile tile2 = Main.tile[i, j - 1];
                            tile2.HasTile = true;
                            tile2.TileType = TileID.PlatinumCandle;
                            tile2.TileFrameX = 18;
                        }
                    }
                    else if (genRand.NextChance(0.05f)) {
                        if (genRand.NextBool()) {
                            Tile tile2 = Main.tile[i, j - 1];
                            tile2.HasTile = true;
                            tile2.TileType = TileID.Bottles;
                            tile2.TileFrameX = 18;
                        }
                        else {
                            Tile tile2 = Main.tile[i, j - 1];
                            tile2.HasTile = true;
                            tile2.TileType = TileID.Bottles;
                            tile2.TileFrameX = 36;
                        }
                    }
                }
            }
        }

        for (int i = origin.X - 30; i < origin.X + sizeX * 3 + 30; i++) {
            for (int j = origin.Y - 30; j < origin.Y + sizeY * 3 + 30; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.HasTile) {
                    if (tile.TileType == TileID.WoodBlock || tile.TileType == TileID.LivingWood) {
                        tile.TileType = genRand.NextBool() ? TileID.WoodBlock : TileID.LivingWood;
                    }
                    if (tile.TileType == TileID.StoneSlab/* && tile.HasUnactuatedTile*/ && genRand.NextBool(10)) {
                        int[] overrideOnlyTileTypes = [TileID.StoneSlab];
                        WorldGenHelper.ModifiedTileRunner(i, j, genRand.Next(2, 4), genRand.Next(1, 3), TileID.Stone, addTile: false, overRide: true, overrideOnlyTileTypes: overrideOnlyTileTypes);
                        WorldGenHelper.ModifiedTileRunner(i, j, genRand.Next(2, 4), genRand.Next(1, 3), TileID.GrayBrick, addTile: false, overRide: true, overrideOnlyTileTypes: overrideOnlyTileTypes);
                    }
                }
            }
        }
        List<Point16> holePoints = [];
        for (int i = origin.X - 30; i < origin.X + sizeX * 3 + 30; i++) {
            for (int j = origin.Y - 30; j < origin.Y + sizeY * 3 + 30; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.HasTile) {
                    if ((tile.TileType == TileID.WoodBlock || tile.TileType == TileID.LivingWood) &&
                        genRand.NextBool(4)) {
                        bool flag = false;
                        foreach (var holePoint in holePoints) {
                            if (MathF.Abs(holePoint.X - i) < 3) {
                                flag = true;
                                break;
                            }
                        }
                        if (flag) {
                            continue;
                        }
                        if (!WorldGenHelper.GetTileSafely(i, j + 1).HasTile) {
                            Tile tile2 = Main.tile[i, j];
                            tile2.HasTile = false;
                            Main.tile[i, j - 1].WallType = WallID.GrayBrick;
                            holePoints.Add(new Point16(i, j));
                            if (genRand.NextBool()) {
                                tile2 = Main.tile[i + 1, j - 1];
                                Main.tile[i + 1, j - 2].WallType = WallID.GrayBrick;
                                tile2.HasTile = false;
                                tile2 = Main.tile[i + 1, j - 2];
                                Main.tile[i + 1, j - 3].WallType = WallID.GrayBrick;
                                tile2.Slope = SlopeType.SlopeUpLeft;
                                tile2 = Main.tile[i + 1, j];
                                Main.tile[i + 1, j - 1].WallType = WallID.GrayBrick;
                                tile2.Slope = SlopeType.SlopeDownRight;
                            }
                            else {
                                tile2 = Main.tile[i - 1, j - 1];
                                Main.tile[i - 1, j - 2].WallType = WallID.GrayBrick;
                                tile2.HasTile = false;
                                tile2 = Main.tile[i - 1, j - 2];
                                Main.tile[i - 1, j - 3].WallType = WallID.GrayBrick;
                                tile2.Slope = SlopeType.SlopeUpRight;
                                tile2 = Main.tile[i - 1, j];
                                Main.tile[i - 1, j - 1].WallType = WallID.GrayBrick;
                                tile2.Slope = SlopeType.SlopeDownLeft;
                            }
                            //WorldGenHelper.ModifiedTileRunner(i, j, 2, 1, -1, wallType: WallID.GrayBrick, clearOnlySolids: true);
                        }
                    }
                    if ((tile.TileType == TileID.WoodBlock || tile.TileType == TileID.LivingWood) &&
                        genRand.NextBool(4)) {
                        if (!WorldGenHelper.GetTileSafely(i, j - 1).HasTile) {
                            Tile tile2 = Main.tile[i, j - 1];
                            tile2.HasTile = true;
                            tile2.TileType = tile.TileType;
                            //if (genRand.NextBool()) {
                            //    tile2 = Main.tile[i, j - 2];
                            //    tile2.HasTile = true;
                            //    tile2.TileType = tile.TileType;
                            //    tile2.IsHalfBlock = true;
                            //}
                            if (genRand.NextBool()) {
                                tile2 = Main.tile[i + 1, j - 1];
                                if (tile2.TileType != TileID.Bookcases && !archiveDecorTileTypes.Contains(tile2.TileType)) {
                                    tile2.HasTile = true;
                                    tile2.TileType = tile.TileType;
                                    if (!Main.tile[i + 1, j - 2].HasTile) {
                                        tile2.IsHalfBlock = true;
                                    }
                                }
                            }
                            else {
                                tile2 = Main.tile[i - 1, j - 1];
                                if (tile2.TileType != TileID.Bookcases && !archiveDecorTileTypes.Contains(tile2.TileType)) {
                                    tile2.HasTile = true;
                                    tile2.TileType = tile.TileType;
                                    if (!Main.tile[i - 1, j - 2].HasTile) {
                                        tile2.IsHalfBlock = true;
                                    }
                                }
                            }
                            //WorldGenHelper.ModifiedTileRunner(i, j - 1, 2, 1, tile.TileType, addTile: true, overRide: false);
                        }
                    }
                }
            }
        }

        for (int i = centerX - 10; i < centerX + 10; i++) {
            for (int j = centerY - 25; j < centerY + 7; j++) {
                Tile tile = WorldGenHelper.GetTileSafely(i, j);
                if (tile.HasTile) {
                    bool flag = false;
                    if (WorldGenHelper.WallCountNearby(i, j, 8) > 7) {
                        flag = true;
                    }
                    if (tile.ActiveWall() && flag && genRand.NextBool(40)) {
                        int num877 = 1;
                        if (genRand.Next(2) == 0)
                            num877 = -1;
                        WorldGen.TileRunner(i, j, genRand.Next(4, 11), genRand.Next(2, 4), 51, addTile: true, num877, -1.0, noYChange: false, overRide: false);
                    }
                }
            }
        }

        int checkX = centerX;
        int checkY = centerY + 14;
        while (Main.tile[checkX, checkY].ActiveWall() ||
               WorldGenHelper.IsWallNearby(checkX, checkY, 2)) {
            checkX++;
        }
        checkX -= 3 - genRand.NextBool().ToInt();
        checkY -= 2;
        int num1023 = 6;
        int num1024 = 1;
        WorldGenHelper.ModifiedTileRunner(checkX, checkY, num1023, num1024, -1, clearWallToo: true, clearOnlySolids: true,
            mustKillTileTypes: [TileID.WoodenBeam]);
        checkX = centerX;
        checkY = centerY + 14;
        while (Main.tile[checkX, checkY].ActiveWall() ||
               WorldGenHelper.IsWallNearby(checkX, checkY, 2)) {
            checkX--;
        }
        checkX += 3 - genRand.NextBool().ToInt();
        checkY -= 2;
        WorldGenHelper.ModifiedTileRunner(checkX, checkY, num1023, num1024, -1, clearWallToo: true, clearOnlySolids: true,
            mustKillTileTypes: [TileID.WoodenBeam]);

        checkX = centerX - 8;
        checkY = centerY + 14 - 24;
        checkX += 1;
        checkY -= 1;
        WorldGenHelper.ModifiedTileRunner(checkX, checkY, num1023, num1024, -1, clearOnlyWalls: true);

        for (int i = origin.X - 30; i < origin.X + sizeX * 3 + 30; i++) {
            for (int j = origin.Y - 30; j < origin.Y + sizeY * 3 + 30; j++) {
                Tile tile = Main.tile[i, j];
                Tile tile2 = Main.tile[i, j - 1];
                if (tile.HasTile && !tile2.HasTile) {
                    if (tile.TileType == TileID.WoodBlock || tile.TileType == TileID.LivingWood) {
                        if (genRand.NextBool()) {
                            if (genRand.NextBool(5)) {
                                int x3 = genRand.Next(12, 36);
                                WorldGen.PlaceSmallPile(i, j - 1, x3, 0, 185);
                            }
                            else {
                                WorldGen.PlaceSmallPile(i, j - 1, genRand.Next(6, 12), 0, 185);
                            }
                        }
                    }
                    else if (genRand.NextBool(3) && genRand.NextBool()) {
                        int x3 = genRand.Next(12, 36);
                        WorldGen.PlaceSmallPile(i, j - 1, x3, 0, 185);
                    }
                }
            }
        }

        GenVars.structures.AddProtectedStructure(new Rectangle(origin.X, origin.Y, sizeX * 3, sizeY * 3), 20);
    }

    private static void PlaceTight2(int x, int y, float startAngle, out int sizeX2, bool bottom = false) {
        ushort tileType = PLACEHOLDERTILETYPE2;
        UnifiedRandom rand = WorldGen.genRand;
        int sizeX = rand.Next(12, 20);
        if (bottom) {
            sizeX -= 1;
        }
        sizeX2 = sizeX;
        int startX = x,
            startY = y;
        Vector2D moveDirection = Vector2D.UnitY.RotatedBy(startAngle) * 2;
        while (sizeX-- > 1) {
            int halfSizeX = sizeX / 2;
            int offsetX = 0;
            if (rand.NextBool()) {
                offsetX += rand.NextBool().ToDirectionInt();
            }
            for (int i = -halfSizeX; i <= halfSizeX; i++) {
                int replaceX = startX + i + offsetX,
                    replaceY = startY;
                Tile replacedTile = Main.tile[replaceX, replaceY];
                replacedTile.HasTile = true;
                replacedTile.TileType = tileType;
                replacedTile = Main.tile[replaceX, replaceY + 1];
                replacedTile.HasTile = true;
                replacedTile.TileType = tileType;
                if (bottom) {
                    replacedTile = Main.tile[replaceX + 1, replaceY];
                    replacedTile.HasTile = true;
                    replacedTile.TileType = tileType;
                    replacedTile = Main.tile[replaceX + 1, replaceY + 1];
                    replacedTile.HasTile = true;
                    replacedTile.TileType = tileType;
                }
            }
            startX += (int)(moveDirection.X * rand.NextFloat(0.25f, 1.5f));
            startY += (int)Math.Floor(moveDirection.Y);
        }
        WorldGenHelper.ModifiedTileRunner(x, y, bottom ? 20 : 15, 1, tileType, addTile: true);
    }

    private static void PlaceTight(int x, int y, bool bottom) {
        UnifiedRandom genRand = WorldGen.genRand;
        int num9 = (int)x;
        int num10 = (int)((double)x + 10);
        int m = num9;
        int num16 = 0;
        for (; m < num10; m += 1) {
            int dir = (!bottom).ToDirectionInt();
            int num17 = y - 3 * dir;
            while (!Main.tile[m, num17].HasTile) {
                num17 += -1 * dir;
            }

            num17 += 4 * dir;
            int num18 = 6;
            int num19 = 15;
            int n = m - num18;
            while (num18 > 0) {
                for (n = m - num18; n < m + num18; n++) {
                    Tile tile2 = Main.tile[n, num17];
                    tile2.HasTile = true;
                    Main.tile[n, num17].TileType = TileID.Stone;
                }

                num16++;
                if (num16 > 2) {
                    num16 = 0;
                    num18 += -1;
                    m += genRand.Next(-1, 2);
                }

                if (num19 <= 0)
                    num18 += -1;

                num19--;
                num17 += 1 * dir;
            }

            //n -= genRand.Next(1, 3);
            //Tile tile = Main.tile[n, num17 - 2 * dir];
            //tile.HasTile = true;
            //Main.tile[n, num17 - 2 * dir].TileType = TileID.Stone;
            //tile = Main.tile[n, num17 - 1];
            //tile.HasTile = true;
            //Main.tile[n, num17 - 1 * dir].TileType = TileID.Stone;
            //tile = Main.tile[n, num17];
            //tile.HasTile = true;
            //Main.tile[n, num17].TileType = TileID.Stone;

            //if (genRand.Next(2) == 0) {
            //    Main.tile[n, num17 + 1].active(active: true);
            //    Main.tile[n, num17 + 1].type = 1;
            //    PlaceTight(n, num17 + 2);
            //}
            //else {
            //    PlaceTight(n, num17 + 1);
            //}
        }
    }

    private void PlaceSlab(Slab slab, int originX, int originY, int scale) {
        ushort num = PLACEHOLDERTILETYPE;
        ushort wall = 0;

        int num2 = -1;
        int num3 = scale + 1;
        int num4 = 0;
        int num5 = scale;
        for (int i = num2; i < num3; i++) {
            if ((i == num2 || i == num3 - 1) && WorldGen.genRand.Next(4) == 0)
                continue;

            if (WorldGen.genRand.Next(4) == 0)
                num4--;

            if (WorldGen.genRand.Next(3) == 0)
                num5--;

            if (WorldGen.genRand.Next(3) == 0)
                num5++;

            for (int j = num4; j < num5; j++) {
                Tile tile = Main.tile[originX + i, originY + j];
                ushort[] skipTileTypes = [TileID.Dirt];
                tile.ResetToType(/*TileID.Sets.Ore[tile.TileType] ? TileID.Dirt : skipTileTypes.Contains(tile.TileType) ? tile.TileType : */num);
                bool active = slab.State(i, j, scale);
                tile.HasTile = active;
                if (slab.HasWall) {
                    tile.WallType = wall;
                }

                WorldUtils.TileFrame(originX + i, originY + j, frameNeighbors: true);
                WorldGen.SquareWallFrame(originX + i, originY + j);
                Tile.SmoothSlope(originX + i, originY + j);
            }
        }
    }

    private static bool IsGroupSolid(int x, int y, int scale) {
        int num = 0;
        for (int i = 0; i < scale; i++) {
            for (int j = 0; j < scale; j++) {
                if (WorldGen.SolidOrSlopedTile(x + i, y + j))
                    num++;
            }
        }

        return num > scale / 4 * 3;
    }
}
