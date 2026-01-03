using Microsoft.Xna.Framework;

using ModLiquidLib.ModLoader;

using ReLogic.Utilities;

using RoA.Common.CustomCollision;
using RoA.Common.Projectiles;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Content.Tiles.Ambient.LargeTrees;
using RoA.Content.Tiles.Miscellaneous;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Core.Utility;

static partial class TileHelper {
    public static string GetTileTexture<T>() where T : ModTile => TileLoader.GetTile(ModContent.TileType<T>()).Texture;

    public static bool CustomSolidCollision_CheckForIceBlocks(Entity source, Vector2 Position, int Width, int Height, bool[] conditions = null, bool shouldDestroyIceBlock = false, Action<Player>? onDestroyingIceBlock = null, params ushort[] extraTypes) {
        int value = (int)(Position.X / 16f) - 1;
        int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
        int value3 = (int)(Position.Y / 16f) - 1;
        int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
        int num = Utils.Clamp(value, 0, Main.maxTilesX - 1);
        value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
        value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 1);
        value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 1);
        CustomTileCollision.GenerateIceBlockPositions(num, value2, value3, value4);
        Vector2 vector = default(Vector2);
        for (int i = num; i < value2; i++) {
            for (int j = value3; j < value4; j++) {
                bool flag = false;
                if (CustomTileCollision.ExtraTileCollisionBlocks_Solid.Contains(new Point16(i, j))) {
                    flag = true;
                }
                if (flag || (Main.tile[i, j] != null && !Main.tile[i, j].IsActuated && Main.tile[i, j].HasTile && ((Main.tileSolid[Main.tile[i, j].TileType] && !Main.tileSolidTop[Main.tile[i, j].TileType]) || (conditions != null && conditions[Main.tile[i, j].TileType]) || extraTypes.Contains(Main.tile[i, j].TileType)))) {
                    vector.X = i * 16;
                    vector.Y = j * 16;
                    int num2 = 16;
                    if (Main.tile[i, j].IsHalfBlock) {
                        vector.Y += 8f;
                        num2 -= 8;
                    }

                    if (Position.X + (float)Width > vector.X && Position.X < vector.X + 16f && Position.Y + (float)Height > vector.Y && Position.Y < vector.Y + (float)num2) {
                        if (flag && shouldDestroyIceBlock) {
                            foreach (IceBlock.IceBlockEnumerateData iceBlockEnumerateData in IceBlock.EnumerateIceBlockPositions2()) {
                                Point16 iceBlockPosition = iceBlockEnumerateData.IceBlockPosition;
                                if (iceBlockPosition.X == i && iceBlockPosition.Y == j) {
                                    iceBlockEnumerateData.Projectile.Kill(iceBlockEnumerateData.Index);
                                    if (source is Player player) {
                                        onDestroyingIceBlock?.Invoke(player);
                                    }
                                    return true;
                                }
                            }
                        }

                        return true;
                    }
                }
            }
        }

        return false;
    }

    public static bool IsHoney(int x, int y) => Main.tile[x, y].LiquidType == LiquidID.Honey;
    public static bool IsLava(int x, int y) => Main.tile[x, y].LiquidType == LiquidID.Lava;
    public static bool IsShimmer(int x, int y) => Main.tile[x, y].LiquidType == LiquidID.Shimmer;
    public static bool IsPermafrost(int x, int y) => false;
    public static bool IsTar(int x, int y) => Main.tile[x, y].LiquidType == LiquidLoader.LiquidType<Content.Liquids.Tar>();

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

    public static Point16 GetTileTopLeft(int i, int j, ushort type) {
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

    public static Point16 GetTileTopLeft2(int i, int j, ushort type) {
        int left = i;
        int top = j;
        TileObjectData tileData = TileObjectData.GetTileData(Main.tile[i, top]);
        if (tileData == null) {
            return Point16.Zero;
        }
        while ((Main.tile[i, top].TileFrameX % TileObjectData.GetTileData(Main.tile[i, top]).CoordinateFullWidth) != 0) {
            if (!WorldGen.InWorld(i, top) || !Main.tile[i, top].ActiveTile(type)) {
                break;
            }
            --i;
        }
        while ((Main.tile[left, j].TileFrameY % TileObjectData.GetTileData(Main.tile[i, top]).CoordinateFullHeight) != 0) {
            if (!WorldGen.InWorld(left, j) || !Main.tile[left, j].ActiveTile(type)) {
                break;
            }
            --j;
        }
        return new Point16(i, j);
    }

    public static Point16 GetTileTopLeft2<T>(int i, int j) where T : ModTile {
        ushort type = (ushort)ModContent.TileType<T>();
        int left = i;
        int top = j;
        TileObjectData tileData = TileObjectData.GetTileData(Main.tile[i, top]);
        if (tileData == null) {
            return Point16.Zero;
        }
        while ((Main.tile[i, top].TileFrameX % TileObjectData.GetTileData(Main.tile[i, top]).CoordinateFullWidth) != 0) {
            if (!WorldGen.InWorld(i, top) || !Main.tile[i, top].ActiveTile(type)) {
                break;
            }
            --i;
        }
        while ((Main.tile[left, j].TileFrameY % TileObjectData.GetTileData(Main.tile[i, top]).CoordinateFullHeight) != 0) {
            if (!WorldGen.InWorld(left, j) || !Main.tile[left, j].ActiveTile(type)) {
                break;
            }
            --j;
        }
        return new Point16(i, j);
    }

    public static ushort GetDistanceToFirstEmptyTileAround(int i, int j, ushort checkDistance = 10, float startCheckAngle = 0f, float defaultCheckAngle = MathHelper.TwoPi / 10f, Predicate<Point16>? extraCondition = null) {
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
                if (!WorldGenHelper.ActiveTile(checkX, checkY) && (extraCondition == null || extraCondition(new Point16(checkX, checkY)))) {
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
