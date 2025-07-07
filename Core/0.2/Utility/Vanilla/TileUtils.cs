using Microsoft.Xna.Framework;

using ReLogic.Utilities;

using RoA.Content.Tiles.Ambient.LargeTrees;
using RoA.Content.Tiles.Miscellaneous;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static partial class TileHelper {
    public static ushort GetTreeDustType(Point16 position) {
        ushort backwoodsBigTreeTileType = (ushort)ModContent.TileType<BackwoodsBigTree>();
        return WorldGenHelper.GetTileSafely(position).TileType == backwoodsBigTreeTileType ? (ushort)TileLoader.GetTile(backwoodsBigTreeTileType).DustType : GetTreeKillDustType(position);
    }

    public static bool IsTreeTrunk(Tile tile) =>
                            (tile.ActiveTile(TileID.Trees) || tile.ActiveTile(TileID.PalmTree) || tile.ActiveTile(TileID.VanityTreeSakura) || tile.ActiveTile(TileID.VanityTreeYellowWillow)) &&
                           !(tile.TileFrameX >= 1 * 22 && tile.TileFrameX <= 2 * 22 &&
                             tile.TileFrameY >= 6 * 22 && tile.TileFrameY <= 8 * 22) &&
                           !(tile.TileFrameX == 3 * 22 &&
                             tile.TileFrameY >= 0 && tile.TileFrameY <= 2 * 22) &&
                           !(tile.TileFrameX == 4 * 22 &&
                             tile.TileFrameY >= 3 * 22 && tile.TileFrameY <= 5 * 22) &&
                             tile.TileFrameY <= 198;

    public static ushort GetTreeKillDustType(Point16 position) => GetTreeKillDustType(position.X, position.Y);
    public static ushort GetTreeKillDustType(int i, int j) {
        Tile tileCache = Main.tile[i, j];
        int num = 0;
        if (tileCache.TileType == 5) {
            num = 7;
            if (i > 5 && i < Main.maxTilesX - 5) {
                int num15 = i;
                int k = j;
                if (tileCache.TileFrameX == 66 && tileCache.TileFrameY <= 45)
                    num15++;

                if (tileCache.TileFrameX == 88 && tileCache.TileFrameY >= 66 && tileCache.TileFrameY <= 110)
                    num15--;

                if (tileCache.TileFrameX == 22 && tileCache.TileFrameY >= 132 && tileCache.TileFrameY <= 176)
                    num15--;

                if (tileCache.TileFrameX == 44 && tileCache.TileFrameY >= 132 && tileCache.TileFrameY <= 176)
                    num15++;

                if (tileCache.TileFrameX == 44 && tileCache.TileFrameY >= 132 && tileCache.TileFrameY <= 176)
                    num15++;

                if (tileCache.TileFrameX == 44 && tileCache.TileFrameY >= 198)
                    num15++;

                if (tileCache.TileFrameX == 66 && tileCache.TileFrameY >= 198)
                    num15--;

                for (; Main.tile[num15, k] != null && (!Main.tile[num15, k].HasTile || !Main.tileSolid[Main.tile[num15, k].TileType]); k++) {
                }

                if (Main.tile[num15, k] != null) {
                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 23)
                        num = 77;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 661)
                        num = 77;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 60)
                        num = 78;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 70)
                        num = 26;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 109)
                        num = 79;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 199)
                        num = 121;

                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 662)
                        num = 121;

                    // Extra patch context.
                    if (Main.tile[num15, k].HasTile && Main.tile[num15, k].TileType == 147)
                        num = 122;

                    TileLoader.TreeDust(Main.tile[num15, k], ref num);
                }
            }
        }

        if (tileCache.TileType == 323) {
            num = 215;
            if (i > 5 && i < Main.maxTilesX - 5) {
                int l;
                for (l = j; Main.tile[i, l] != null && (!Main.tile[i, l].HasTile || !Main.tileSolid[Main.tile[i, l].TileType]); l++) {
                }

                if (Main.tile[i, l] != null) {
                    if (Main.tile[i, l].HasTile && Main.tile[i, l].TileType == 234)
                        num = 121;

                    if (Main.tile[i, l].HasTile && Main.tile[i, l].TileType == 116)
                        num = 79;

                    // Extra patch context.
                    if (Main.tile[i, l].HasTile && Main.tile[i, l].TileType == 112)
                        num = 77;

                    TileLoader.PalmTreeDust(Main.tile[i, l], ref num);
                }
            }
        }

        return (ushort)num;
    }

    public static float TileSize => 16f;

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
        ushort type = (ushort)ModContent.TileType<T>();
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

    public static ushort GetDistanceToFirstEmptyTileAround(int i, int j, ushort checkDistance = 10, float startCheckAngle = 0f, float defaultCheckAngle = MathHelper.TwoPi / 10f) {
        if (startCheckAngle <= 0f) {
            startCheckAngle = defaultCheckAngle;
        }
        float currentCheckAngle = 0f;
        float maxCheckAngle = MathHelper.TwoPi;
        List<ushort> distances = [];
        while (currentCheckAngle < maxCheckAngle) {
            ushort currentCheckDistance = 0;
            int checkX = i, checkY = j;
            while (currentCheckDistance++ < checkDistance) {
                Vector2D velocity = Vector2D.One.RotatedBy(currentCheckAngle);
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
