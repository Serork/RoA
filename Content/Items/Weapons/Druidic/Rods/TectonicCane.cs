using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

using static Terraria.GameContent.Animations.Actions.Sprites;

namespace RoA.Content.Items.Weapons.Druidic.Rods;

sealed class TectonicCane : BaseRodItem<TectonicCane.TectonicCaneBase> {
    protected override ushort ShootType() => (ushort)ModContent.ProjectileType<TectonicCaneProjectile>();

    protected override void SafeSetDefaults() {
        Item.SetSize(36, 38);
        Item.SetDefaultToUsable(-1, 30, useSound: SoundID.Item7);
        Item.SetWeaponValues(5, 4f);

        NatureWeaponHandler.SetPotentialDamage(Item, 15);
        NatureWeaponHandler.SetFillingRate(Item, 0.35f);
        //NatureWeaponHandler.SetPotentialUseSpeed(Item, 20);
    }

    public sealed class TectonicCaneBase : BaseRodProjectile {
        protected override bool ShouldWaitUntilProjDespawns() => false;
    }
}

sealed class TectonicCaneProjectile : NatureProjectile {
    public override void Load() {
        On_Collision.TileCollision += On_Collision_TileCollision;
        On_Collision.SlopeCollision += On_Collision_SlopeCollision;
    }

    private static Dictionary<Point, (Projectile, Vector2)> GeneratePosition() {
        Dictionary<Point, (Projectile, Vector2)> tectonicPlatesPositions = [];
        foreach (Projectile projectile in Main.ActiveProjectiles) {
            if (projectile.type == ModContent.ProjectileType<TectonicCaneProjectile>()) {
                List<Vector2> positions = [];
                for (int i = -2; i < 3; i++) {
                    Vector2 position = projectile.position - Vector2.UnitY * 120f * projectile.ai[0];
                    position.X += i * 10f;
                    positions.Add(position);
                }
                foreach (Vector2 position in positions) {
                    tectonicPlatesPositions.TryAdd(position.ToTileCoordinates(), (projectile, position));
                }
            }
        }
        return tectonicPlatesPositions;
    }

    private Vector4 On_Collision_SlopeCollision(On_Collision.orig_SlopeCollision orig, Vector2 Position, Vector2 Velocity, int Width, int Height, float gravity, bool fall) {
        Collision.stair = false;
        Collision.stairFall = false;
        bool[] array = new bool[5];
        float y = Position.Y;
        float y2 = Position.Y;
        Collision.sloping = false;
        Vector2 vector = Position;
        Vector2 vector2 = Position;
        Vector2 vector3 = Velocity;
        int value = (int)(Position.X / 16f) - 1;
        int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
        int value3 = (int)(Position.Y / 16f) - 1;
        int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
        int num = Utils.Clamp(value, 0, Main.maxTilesX - 1);
        value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
        value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 1);
        value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 1);
        Vector2 vector4 = default(Vector2);
        for (int i = num; i < value2; i++) {
            for (int j = value3; j < value4; j++) {
                if (Main.tile[i, j] == null || !Main.tile[i, j].HasTile || Main.tile[i, j].IsActuated || (!Main.tileSolid[Main.tile[i, j].TileType] && (!Main.tileSolidTop[Main.tile[i, j].TileType] || Main.tile[i, j].TileFrameY != 0)))
                    continue;

                vector4.X = i * 16;
                vector4.Y = j * 16;
                int num2 = 16;
                if (Main.tile[i, j].IsHalfBlock) {
                    vector4.Y += 8f;
                    num2 -= 8;
                }

                if (!(Position.X + (float)Width > vector4.X) || !(Position.X < vector4.X + 16f) || !(Position.Y + (float)Height > vector4.Y) || !(Position.Y < vector4.Y + (float)num2))
                    continue;

                bool flag = true;
                if (TileID.Sets.Platforms[Main.tile[i, j].TileType]) {
                    if (Velocity.Y < 0f)
                        flag = false;

                    if (Position.Y + (float)Height < (float)(j * 16) || Position.Y + (float)Height - (1f + Math.Abs(Velocity.X)) > (float)(j * 16 + 16))
                        flag = false;

                    if (((Main.tile[i, j].Slope == (SlopeType)1 && Velocity.X >= 0f) || (Main.tile[i, j].Slope == (SlopeType)2 && Velocity.X <= 0f)) && (Position.Y + (float)Height) / 16f - 1f == (float)j)
                        flag = false;
                }

                if (!flag)
                    continue;

                bool flag2 = false;
                if (fall && TileID.Sets.Platforms[Main.tile[i, j].TileType])
                    flag2 = true;

                int num3 = (int)Main.tile[i, j].Slope;
                vector4.X = i * 16;
                vector4.Y = j * 16;
                if (!(Position.X + (float)Width > vector4.X) || !(Position.X < vector4.X + 16f) || !(Position.Y + (float)Height > vector4.Y) || !(Position.Y < vector4.Y + 16f))
                    continue;

                float num4 = 0f;
                if (num3 == 3 || num3 == 4) {
                    if (num3 == 3)
                        num4 = Position.X - vector4.X;

                    if (num3 == 4)
                        num4 = vector4.X + 16f - (Position.X + (float)Width);

                    if (num4 >= 0f) {
                        if (Position.Y <= vector4.Y + 16f - num4) {
                            float num5 = vector4.Y + 16f - vector.Y - num4;
                            if (Position.Y + num5 > y2) {
                                vector2.Y = Position.Y + num5;
                                y2 = vector2.Y;
                                if (vector3.Y < 0.0101f)
                                    vector3.Y = 0.0101f;

                                array[num3] = true;
                            }
                        }
                    }
                    else if (Position.Y > vector4.Y) {
                        float num6 = vector4.Y + 16f;
                        if (vector2.Y < num6) {
                            vector2.Y = num6;
                            if (vector3.Y < 0.0101f)
                                vector3.Y = 0.0101f;
                        }
                    }
                }

                if (num3 != 1 && num3 != 2)
                    continue;

                if (num3 == 1)
                    num4 = Position.X - vector4.X;

                if (num3 == 2)
                    num4 = vector4.X + 16f - (Position.X + (float)Width);

                if (num4 >= 0f) {
                    if (!(Position.Y + (float)Height >= vector4.Y + num4))
                        continue;

                    float num7 = vector4.Y - (vector.Y + (float)Height) + num4;
                    if (!(Position.Y + num7 < y))
                        continue;

                    if (flag2) {
                        Collision.stairFall = true;
                        continue;
                    }

                    if (TileID.Sets.Platforms[Main.tile[i, j].TileType])
                        Collision.stair = true;
                    else
                        Collision.stair = false;

                    vector2.Y = Position.Y + num7;
                    y = vector2.Y;
                    if (vector3.Y > 0f)
                        vector3.Y = 0f;

                    array[num3] = true;
                    continue;
                }

                if (TileID.Sets.Platforms[Main.tile[i, j].TileType] && !(Position.Y + (float)Height - 4f - Math.Abs(Velocity.X) <= vector4.Y)) {
                    if (flag2)
                        Collision.stairFall = true;

                    continue;
                }

                float num8 = vector4.Y - (float)Height;
                if (!(vector2.Y > num8))
                    continue;

                if (flag2) {
                    Collision.stairFall = true;
                    continue;
                }

                if (TileID.Sets.Platforms[Main.tile[i, j].TileType])
                    Collision.stair = true;
                else
                    Collision.stair = false;

                vector2.Y = num8;
                if (vector3.Y > 0f)
                    vector3.Y = 0f;
            }
        }

        Vector2 position = Position;
        Vector2 velocity = vector2 - Position;
        Vector2 vector5 = Collision.TileCollision(position, velocity, Width, Height);
        if (vector5.Y > velocity.Y) {
            float num9 = velocity.Y - vector5.Y;
            vector2.Y = Position.Y + vector5.Y;
            if (array[1])
                vector2.X = Position.X - num9;

            if (array[2])
                vector2.X = Position.X + num9;

            vector3.X = 0f;
            vector3.Y = 0f;
            Collision.up = false;
        }
        else if (vector5.Y < velocity.Y) {
            float num10 = vector5.Y - velocity.Y;
            vector2.Y = Position.Y + vector5.Y;
            if (array[3])
                vector2.X = Position.X - num10;

            if (array[4])
                vector2.X = Position.X + num10;

            vector3.X = 0f;
            vector3.Y = 0f;
        }

        return new Vector4(vector2, vector3.X, vector3.Y);
    }

    private Vector2 On_Collision_TileCollision(On_Collision.orig_TileCollision orig, Vector2 Position, Vector2 Velocity, int Width, int Height, bool fallThrough, bool fall2, int gravDir) {
        Collision.up = false;
        Collision.down = false;
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
        Dictionary<Point, (Projectile, Vector2)> tectonicPlatesPositions = GeneratePosition();
        float num6 = (value4 + 3) * 16;
        Vector2 vector4 = default(Vector2);
        for (int i = num5; i < value2; i++) {
            for (int j = value3; j < value4; j++) {
                bool flag3 = false;
                if (tectonicPlatesPositions.TryGetValue(new Point(i, j), out (Projectile, Vector2) turple)) {
                    flag3 = true;
                }
                if (!flag3 && (Main.tile[i, j] == null || !Main.tile[i, j].HasTile || Main.tile[i, j].IsActuated || (!Main.tileSolid[Main.tile[i, j].TileType] && (!Main.tileSolidTop[Main.tile[i, j].TileType] || Main.tile[i, j].TileFrameY != 0))))
                    continue;
                vector4.X = i * 16;
                vector4.Y = j * 16;
                if (flag3) {
                    vector4 = turple.Item2;
                    vector4.Y -= 2f;
                }
                int num7 = 16;
                if (Main.tile[i, j].IsHalfBlock) {
                    vector4.Y += 8f;
                    num7 -= 8;
                }

                bool flag4 = !flag3 || turple.Item1.ai[0] == 1f;
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
                    Collision.down = true;
                    if (((!(Main.tileSolidTop[Main.tile[i, j].TileType] && fallThrough)) || !(Velocity.Y <= 1f || fall2)) && num6 > vector4.Y) {
                        num3 = i;
                        num4 = j;
                        if (num7 < 16)
                            num4++;

                        if (num3 != num && !flag) {
                            result.Y = vector4.Y - (vector3.Y + (float)Height) + ((gravDir == -1) ? (-0.01f) : 0f);
                            num6 = vector4.Y;
                            if (!flag4) {
                                result.Y -= 20f;
                            }
                        }
                    }
                }
                else if (vector3.X + (float)Width <= vector4.X && !Main.tileSolidTop[Main.tile[i, j].TileType] && !flag3) {
                    if (i < 1 || (Main.tile[i - 1, j].Slope != (SlopeType)2 && Main.tile[i - 1, j].Slope != (SlopeType)4)) {
                        num = i;
                        num2 = j;
                        if (num2 != num4)
                            result.X = vector4.X - (vector3.X + (float)Width);

                        if (num3 == num)
                            result.Y = vector.Y;
                    }
                }
                else if (vector3.X >= vector4.X + 16f && !Main.tileSolidTop[Main.tile[i, j].TileType] && !flag3) {
                    if (Main.tile[i + 1, j].Slope != (SlopeType)1 && Main.tile[i + 1, j].Slope != (SlopeType)3) {
                        num = i;
                        num2 = j;
                        if (num2 != num4)
                            result.X = vector4.X + 16f - vector3.X;

                        if (num3 == num)
                            result.Y = vector.Y;
                    }
                }
                else if (vector3.Y >= vector4.Y + (float)num7 && !Main.tileSolidTop[Main.tile[i, j].TileType] && !flag3) {
                    Collision.up = true;
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


    protected override void SafeSetDefaults() {
        Projectile.Size = Vector2.One;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.timeLeft = 300;
        Projectile.penetrate = -1;
        Projectile.hide = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;

        Projectile.netImportant = true;
    }

    protected override void SafeOnSpawn(IEntitySource source) {
        if (Projectile.owner != Main.myPlayer) {
            return;
        }

        Projectile.spriteDirection = Main.rand.NextBool().ToDirectionInt();

        EvilBranch.GetPos(Main.player[Projectile.owner], out Point point, out Point point2);
        Projectile.Center = point2.ToWorldCoordinates();

        Projectile.netUpdate = true;
    }

    public override void AI() {
        if (Projectile.ai[0] < 1f) {
            Projectile.ai[0] += TimeSystem.LogicDeltaTime * 2.5f;
        }
        else {
            Projectile.ai[0] = 1f;
        }
        Projectile.ai[1] = Ease.SineOut(Projectile.ai[0]);
        float lerpAmount = 1f - Projectile.ai[0] * 0.5f;
        Projectile.localAI[0] = MathHelper.SmoothStep(Projectile.localAI[0], Main.rand.NextFloatRange(35f) * Main.rand.NextFloatDirection(), lerpAmount);
        Projectile.localAI[1] = MathHelper.SmoothStep(Projectile.localAI[1], Main.rand.NextFloatRange(50f) * Main.rand.NextFloatDirection(), lerpAmount);
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);

    public override bool PreDraw(ref Color lightColor) {
        SpriteFrame frame = new(1, 4);
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Color drawColor = Lighting.GetColor((Projectile.Center - Vector2.UnitY * 20f).ToTileCoordinates());
        SpriteEffects spriteEffects = (SpriteEffects)(Projectile.spriteDirection == 1).ToInt();
        Vector2 position = Projectile.Center - Main.screenPosition;
        Rectangle sourceRectangle = frame.GetSourceRectangle(texture);
        sourceRectangle.Height = Math.Clamp((int)(sourceRectangle.Height * Projectile.ai[1]), 2, sourceRectangle.Height);
        bool flag = Projectile.ai[0] != 1f;
        if (flag) {
            position += new Vector2(Main.rand.NextFloatRange(0.5f) * Projectile.localAI[0], Main.rand.NextFloatRange(1f) * Projectile.localAI[1]) * 0.75f * (1f - Projectile.ai[0]);
        }
        Main.EntitySpriteDraw(texture, position, sourceRectangle, drawColor, Projectile.rotation, sourceRectangle.BottomCenter(), Projectile.scale, spriteEffects);

        return false;
    }
}