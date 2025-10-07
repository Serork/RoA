using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Content.Tiles.Platforms;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.CustomCollision;

sealed class CustomTileCollision : IInitializer {
    public static Dictionary<Point, (Projectile, Vector2)> ExtraTileCollisionBlocks_Vectors { get; private set; } = [];
    public static HashSet<Point16> ExtraTileCollisionBlocks_Solid { get; private set; } = [];
    public static HashSet<Point16> ExtraTileCollisionBlocks_Platforms { get; private set; } = [];

    void ILoadable.Load(Mod mod) {
        On_Collision.TileCollision += On_Collision_TileCollision;
    }

    void ILoadable.Unload() {
        ExtraTileCollisionBlocks_Solid.Clear();
        ExtraTileCollisionBlocks_Solid = null!;

        ExtraTileCollisionBlocks_Platforms.Clear();
        ExtraTileCollisionBlocks_Platforms = null!;

        ExtraTileCollisionBlocks_Vectors.Clear();
        ExtraTileCollisionBlocks_Vectors = null!;
    }

    private static void GenerateTectonicCanePositions() {
        foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<TectonicCaneProjectile>()) {
            List<Vector2> positions = [];
            for (int i = -2; i < 3; i++) {
                Vector2 position = projectile.position - Vector2.UnitY * 120f * projectile.ai[0];
                position.X += i * 10f;
                bool flag2 = true;
                for (int i2 = 0; i2 < 2; i2++) {
                    Point pos = position.ToTileCoordinates() + new Point(0, i2);
                    if (WorldGenHelper.GetTileSafely(pos).HasTile &&
                        Main.tileSolid[WorldGenHelper.GetTileSafely(pos).TileType]) {
                        flag2 = false;
                        break;
                    }
                }
                bool flag = projectile.ai[0] > 0.1f;
                if ((flag2 && flag) || !flag) {
                    positions.Add(position);
                }
            }
            foreach (Vector2 position in positions) {
                ExtraTileCollisionBlocks_Vectors.TryAdd(position.ToTileCoordinates(), (projectile, position));
            }
        }
    }

    public static void GenerateIceBlockPositions(int num5, int value2, int value3, int value4) {
        foreach (Projectile projectile in TrackedEntitiesSystem.GetTrackedProjectile<IceBlock>()) {
            foreach (Point16 iceBlockPosition in projectile.As<IceBlock>().IceBlockPositions) {
                for (int i = num5; i < value2; i++) {
                    for (int j = value3; j < value4; j++) {
                        if (iceBlockPosition.X == i && iceBlockPosition.Y == j) {
                            ExtraTileCollisionBlocks_Solid.Add(new Point16(i, j));
                        }
                    }
                }
            }
        }
    }

    public static void GenerateTreeBranchPositions(int num5, int value2, int value3, int value4) {
        ushort type = (ushort)ModContent.TileType<TreeBranch>();
        for (int i = num5; i < value2; i++) {
            for (int j = value3; j < value4; j++) {
                if (TileLoader.GetTile(Main.tile[i, j].TileType) is TreeBranch) {
                    if (Main.tile[i + 1, j].TileType == TileID.Trees) {
                        ExtraTileCollisionBlocks_Platforms.Add(new Point16(i - 1, j));
                    }
                    if (Main.tile[i - 1, j].TileType == TileID.Trees) {
                        ExtraTileCollisionBlocks_Platforms.Add(new Point16(i + 1, j));
                    }
                }
            }
        }
    }

    private Vector2 On_Collision_TileCollision(On_Collision.orig_TileCollision orig, Vector2 Position, Vector2 Velocity, int Width, int Height, bool fallThrough, bool fall2, int gravDir) {
        Terraria.Collision.up = false;
        Terraria.Collision.down = false;
        Vector2 result = Velocity;
        Vector2 vector = Velocity;
        Vector2 vector2 = Position + Velocity;
        Vector2 vector3 = Position;
        int value = (int)(Position.X / 16f) - 1;
        int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
        int value3 = (int)(Position.Y / 16f) - 1;
        int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
        int num = -1;
        int num2 = -1;
        int num3 = -1;
        int num4 = -1;
        int num5 = Utils.Clamp(value, 0, Main.maxTilesX - 1);
        value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
        value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 1);
        value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 1);
        ExtraTileCollisionBlocks_Vectors.Clear();
        GenerateTectonicCanePositions();
        float num6 = (value4 + 3) * 16;
        Vector2 vector4 = default(Vector2);
        bool targetDummy = false;
        if (ExtraTileCollisionBlocks_Vectors.Count > 0) {
            for (int i = num5; i < value2; i++) {
                for (int j = value3; j < value4; j++) {
                    int num1451 = TETrainingDummy.Find(i, j);
                    if (num1451 != -1) {
                        targetDummy = true;
                    }
                }
            }
        }
        ExtraTileCollisionBlocks_Solid.Clear();
        ExtraTileCollisionBlocks_Platforms.Clear();
        GenerateIceBlockPositions(num5, value2, value3, value4);
        GenerateTreeBranchPositions(num5, value2, value3, value4);
        for (int i = num5; i < value2; i++) {
            for (int j = value3; j < value4; j++) {
                bool customCollision = false;
                Point16 tilePosition = new(i, j);
                bool platform = false;
                if (ExtraTileCollisionBlocks_Vectors.TryGetValue(new Point(i, j), out (Projectile, Vector2) turple)) {
                    customCollision = true;
                }
                if (ExtraTileCollisionBlocks_Platforms.Contains(tilePosition)) {
                    customCollision = true;
                    platform = true;
                }
                if (targetDummy) {
                    customCollision = false;
                }
                if (!customCollision && !ExtraTileCollisionBlocks_Solid.Contains(tilePosition) && (Main.tile[i, j] == null || !Main.tile[i, j].HasTile || Main.tile[i, j].IsActuated || (!Main.tileSolid[Main.tile[i, j].TileType] && (!Main.tileSolidTop[Main.tile[i, j].TileType] || Main.tile[i, j].TileFrameY != 0))))
                    continue;
                vector4.X = i * 16;
                vector4.Y = j * 16;
                if (customCollision) {
                    vector4 = platform ? tilePosition.ToWorldCoordinates() : turple.Item2;
                    vector4.Y -= platform ? 8f : 2f;
                }
                int num7 = 16;
                if (Main.tile[i, j].IsHalfBlock) {
                    vector4.Y += 8f;
                    num7 -= 8;
                }

                bool flag4 = !customCollision || (platform || turple.Item1.ai[0] == 1f);
                if (flag4) {
                    if (!(vector2.X + (float)Width > vector4.X) ||
                        !(vector2.X < vector4.X + 16f) ||
                        !(vector2.Y + (float)Height > vector4.Y) ||
                        !(vector2.Y < vector4.Y + (float)num7))
                        continue;
                }

                bool flag = false;
                bool flag2 = false;
                if (Main.tile[i, j].Slope > (SlopeType)2) {
                    if (Main.tile[i, j].Slope == (SlopeType)3 && vector3.Y + Math.Abs(Velocity.X) >= vector4.Y && vector3.X >= vector4.X)
                        flag2 = true;

                    if (Main.tile[i, j].Slope == (SlopeType)4 && vector3.Y + Math.Abs(Velocity.X) >= vector4.Y && vector3.X + (float)Width <= vector4.X + 16f)
                        flag2 = true;
                }
                else if (Main.tile[i, j].Slope > 0) {
                    flag = true;
                    if (Main.tile[i, j].Slope == (SlopeType)1 && vector3.Y + (float)Height - Math.Abs(Velocity.X) <= vector4.Y + (float)num7 && vector3.X >= vector4.X)
                        flag2 = true;

                    if (Main.tile[i, j].Slope == (SlopeType)2 && vector3.Y + (float)Height - Math.Abs(Velocity.X) <= vector4.Y + (float)num7 && vector3.X + (float)Width <= vector4.X + 16f)
                        flag2 = true;
                }

                if (flag2)
                    continue;

                if (vector3.Y + (float)Height <= vector4.Y) {
                    Terraria.Collision.down = true;
                    bool flag10 = !Main.tileSolidTop[Main.tile[i, j].TileType] || customCollision || !fallThrough;
                    bool flag11 = flag10 || !(Velocity.Y <= 1f || fall2);
                    if (flag11 && num6 > vector4.Y) {
                        num3 = i;
                        num4 = j;
                        if (num7 < 16)
                            num4++;

                        if (num3 != num && !flag) {
                            result.Y = vector4.Y - (vector3.Y + (float)Height) + ((gravDir == -1) ? (-0.01f) : 0f);
                            num6 = vector4.Y;
                            if (customCollision && fallThrough) {
                                result.Y += 0.01f;
                            }
                            if (!flag4) {
                                result.Y -= 20f;
                            }
                        }
                    }
                }
                else if (vector3.X + (float)Width <= vector4.X && !Main.tileSolidTop[Main.tile[i, j].TileType] && !customCollision) {
                    if (i < 1 || (Main.tile[i - 1, j].Slope != (SlopeType)2 && Main.tile[i - 1, j].Slope != (SlopeType)4)) {
                        num = i;
                        num2 = j;
                        if (num2 != num4)
                            result.X = vector4.X - (vector3.X + (float)Width);

                        if (num3 == num)
                            result.Y = vector.Y;
                    }
                }
                else if (vector3.X >= vector4.X + 16f && !Main.tileSolidTop[Main.tile[i, j].TileType] && !customCollision) {
                    if (Main.tile[i + 1, j].Slope != (SlopeType)1 && Main.tile[i + 1, j].Slope != (SlopeType)3) {
                        num = i;
                        num2 = j;
                        if (num2 != num4)
                            result.X = vector4.X + 16f - vector3.X;
                        if (num3 == num)
                            result.Y = vector.Y;
                    }
                }
                else if (vector3.Y >= vector4.Y + (float)num7 && !Main.tileSolidTop[Main.tile[i, j].TileType] && !customCollision) {
                    Terraria.Collision.up = true;
                    num3 = i;
                    num4 = j;
                    result.Y = vector4.Y + (float)num7 - vector3.Y + ((gravDir == 1) ? 0.01f : 0f);
                    if (num4 == num2)
                        result.X = vector.X;
                }
            }
        }

        return result;
    }
}
