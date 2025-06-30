using Microsoft.Xna.Framework;

using ReLogic.Utilities;

using RoA.Content.Tiles.Miscellaneous;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static partial class TileHelper {
    public static bool DrawingTiles { get; private set; }

    public static Vector2 ScreenOffset {
        get {
            Vector2 vector = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen) {
                vector = Vector2.Zero;
            }
            return vector;
        }
    }

    public static Point16 GetTileTopLeft<T>(int i, int j) where T : ModTile {
        ushort type = (ushort)ModContent.TileType<TreeDryad>();
        int left = i;
        int top = j;
        while (Main.tile[i, top].TileFrameX != 0) {
            if (!WorldGen.InWorld(i, top) || !Main.tile[i, top].ActiveTile(type)) {
                break;
            }
            --i;
        }
        while (Main.tile[left, j].TileFrameY != 0) {
            if (!WorldGen.InWorld(left, j) || !Main.tile[left, j].ActiveTile(type)) {
                break;
            }
            --j;
        }
        return new Point16(i + 1, j + 1);
    }

    public static ushort GetDistanceToFirstEmptyTileAround(int i, int j, ushort checkDistance = 10, float startCheckAngle = 0f, float defaultCheckAngle = MathHelper.PiOver4) {
        float currentCheckAngle = 0f;
        if (startCheckAngle <= 0f) {
            startCheckAngle = defaultCheckAngle;
        }
        float maxCheckAngle = MathHelper.TwoPi + startCheckAngle;
        List<ushort> distances = [];
        while (currentCheckAngle < maxCheckAngle) {
            ushort currentCheckDistance = 0;
            int checkX = i, checkY = j;
            while (currentCheckDistance++ < checkDistance) {
                Vector2D velocity = Vector2D.UnitY.RotatedBy(currentCheckAngle);
                checkX += (int)Math.Floor(velocity.X);
                checkY += (int)Math.Floor(velocity.Y);
                if (!WorldGenHelper.ActiveTile(checkX, checkY)) {
                    distances.Add(currentCheckDistance);
                    break;
                }
            }
            currentCheckAngle += startCheckAngle;
        }
        return (ushort)(distances.Count == 0 ? 0 : distances.Min());
    }

    public static bool HasNoDuplicateNeighbors(int i, int j, ushort checkTileType) {
        bool result = false;
        for (int k = 0; k < 4; k++) {
            int checkX = 0, checkY = 0;
            if (k == 0) {
                checkX = -1;
            }
            else if (k == 1) {
                checkX = 1;
            }
            else if (k == 2) {
                checkY = 1;
            }
            else if (k == 3) {
                checkY = -1;
            }
            checkX += i;
            checkY += j;
            Tile checkTile = Main.tile[checkX, checkY];
            if (!checkTile.ActiveTile(checkTileType)) {
                result = true;
            }
        }
        return result;
    }

    public static bool HasNoDuplicateNeighbors<T>(int i, int j) where T : ModTile => HasNoDuplicateNeighbors(i, j, (ushort)ModContent.TileType<T>());

    static partial void LoadImpl() {
        On_TileDrawing.PreDrawTiles += On_TileDrawing_PreDrawTiles;
        On_TileDrawing.PostDrawTiles += On_TileDrawing_PostDrawTiles;
    }

    private static void On_TileDrawing_PostDrawTiles(On_TileDrawing.orig_PostDrawTiles orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets) {
        orig(self, solidLayer, forRenderTargets, intoRenderTargets);
        DrawingTiles = false;
    }

    private static void On_TileDrawing_PreDrawTiles(On_TileDrawing.orig_PreDrawTiles orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets) {
        orig(self, solidLayer, forRenderTargets, intoRenderTargets);
        DrawingTiles = true;
    }
}
