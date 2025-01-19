using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Content.Tiles.Platforms;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Utilities;

static class Helper {
    public static readonly Color AwakenMessageColor = new(175, 75, 255);
    public static readonly Color EventMessageColor = new(50, 255, 130);

    public static readonly Color GlowMaskColor = new Color(255, 255, 255, 0) * 0.8f;
    public static string ArmorSetBonusKey => Language.GetTextValue(Main.ReversedUpDownArmorSetBonuses ? "Key.UP" : "Key.DOWN");

    public static Vector2 SafeDirectionTo(Entity entity, Vector2 destination, Vector2? fallback = null) {
        if (fallback == null) {
            fallback = new Vector2?(Vector2.Zero);
        }
        return Utils.SafeNormalize(destination - entity.Center, fallback.Value);
    }

    public static int GetGoreType(this string name) => ModContent.Find<ModGore>(RoA.ModName + $"/{name}").Type;

    public static Color FromHexRgb(uint hexRgba) {
        return new Color(
            (byte)(hexRgba >> 16),
            (byte)(hexRgba >> 8),
            (byte)(hexRgba >> 0),
            255
        );
    }

    public static Color FromHexRgba(uint hexRgba) {
        return new Color(
            (byte)(hexRgba >> 24),
            (byte)(hexRgba >> 16),
            (byte)(hexRgba >> 8),
            (byte)(hexRgba >> 0)
        );
    }

    public static bool OnScreenWorld(Vector2 position) {
        return OnScreen(position - Main.screenPosition);
    }
    public static bool OnScreen(Vector2 position) {
        return OnScreen(new Rectangle((int)position.X, (int)position.Y, 1, 1));
    }
    public static bool OnScreenWorld(Rectangle rectangle) {
        return OnScreen(new Rectangle(rectangle.X - (int)Main.screenPosition.X, rectangle.Y - (int)Main.screenPosition.Y, rectangle.Width, rectangle.Height));
    }
    public static bool OnScreen(Rectangle rectangle) {
        return new Rectangle(-20, -20, Main.screenWidth + 20 * 2, Main.screenHeight + 20 * 2).Intersects(rectangle);
    }
    public static bool OnScreenWorld(int x, int y, int width = 1, int height = 1) {
        return OnScreenWorld(new Rectangle(x * 16, y * 16, width * 16, height * 16));
    }

    public static string NamespacePath(this object obj) => NamespacePath(obj.GetType());
    public static string NamespacePath<T>() => NamespacePath(typeof(T));
    public static string NamespacePath(Type t) => t.Namespace.Replace('.', '/');
    public static string GetPath(this object obj) => GetPath(obj.GetType());
    public static string GetPath<T>() => GetPath(typeof(T));
    public static string GetPath(Type t) => $"{NamespacePath(t)}/{t.Name}";

    public static float Approach(float val, float target, float maxMove) => (double)val <= (double)target ? Math.Min(val + maxMove, target) : Math.Max(val - maxMove, target);

    public static Vector2 CircleOffset(this Entity entity, float elapsedTime, float circleRotation, float circleHeight) => ((((float)(MathHelper.TwoPi * (double)elapsedTime + MathHelper.PiOver2)).ToRotationVector2() + new Vector2(0.0f, -1f)) * new Vector2(6 * -entity.direction, circleHeight)).RotatedBy((double)circleRotation);
    public static void CircleMovement(this Entity entity, float counter, float speed = 0.4f, float radius = 14f) {
        Vector2 offset = CircleOffset(entity, counter / 65f, speed, radius);
        entity.velocity = CircleOffset(entity, counter / 65f + 1f / 65f, speed, radius) - offset;
    }
    public static Vector2 CircleMovementVector2(this Entity entity, float counter, float speed = 0.4f, float radius = 14f) {
        Vector2 offset = CircleOffset(entity, counter / 65f, speed, radius);
        return CircleOffset(entity, counter / 65f + 1f / 65f, speed, radius) - offset;
    }

    public static float EaseInOut(float value) {
        float value2 = (float)Math.Pow((double)value, 2.0);
        return value2 / (2.0f * (value2 - value) + 1.0f);
    }

    public static float CappedMeleeScale(this Player player) {
        var item = player.HeldItem;
        return Math.Clamp(player.GetAdjustedItemScale(item), 0.5f * item.scale, 2f * item.scale);
    }

    public static void ScaleUp(Projectile proj) {
        float scale = Main.player[proj.owner].CappedMeleeScale();
        if (scale != 1f) {
            proj.scale *= scale;
            proj.width = (int)(proj.width * proj.scale);
            proj.height = (int)(proj.height * proj.scale);
        }
    }

    public static float EaseInOut2(float value) => (float)Math.Pow((double)value, 2.0) * (3.0f - 2.0f * value);

    public static float EaseInOut3(float value) {
        float value1 = (float)Math.Pow((double)value, 2.0);
        float value2 = 1f - value;
        float value3 = 1f - (float)Math.Pow((double)value2, 2.0);
        return MathHelper.Lerp(value1, value3, value);
    }

    public static float EaseInOut4(float value) => (double)value < 1.0 ? 1f - (float)Math.Pow(2.0, -10.0 * (double)value) : 1f;

    public static float Wave(float minimum, float maximum, float speed = 1f, float offset = 0f) => Wave((float)TimeSystem.TimeForVisualEffects, minimum, maximum, speed, offset);
    public static float Wave(float step, float minimum, float maximum, float speed = 1f, float offset = 0f) => minimum + ((float)Math.Sin(step * (double)speed + (double)offset) + 1f) / 2f * (maximum - minimum);

    public static void AddClamp(ref int value, int add, int min, int max) => AddClamp(ref value, add, min, max);
    public static void AddClamp(ref float value, float add, float min = 0f, float max = 1f) => value = Math.Clamp(value + add, min, max);

    public static Vector2 VelocityToPoint(Vector2 a, Vector2 b, float speed) {
        Vector2 vector2 = b - a;
        Vector2 velocity = vector2 * (speed / vector2.Length());
        return !velocity.HasNaNs() ? velocity : Vector2.Zero;
    }

    public static float VelocityAngle(Vector2 velocity) => (float)Math.Atan2(velocity.Y, velocity.X) + (float)Math.PI / 2f;

    public static void InertiaMoveTowards(ref Vector2 velocity, Vector2 position, Vector2 destination, float inertia = 15f, float speed = 5f, float minDistance = 10f, bool max = false) {
        Vector2 direction = destination - position;
        bool flag = (max && velocity.Length() >= 1f) || !max;
        if (direction.Length() > minDistance) {
            direction.Normalize();
            velocity = (velocity * inertia + direction * speed) / (inertia + 1f);
        }
        else if (flag) {
            velocity *= (float)Math.Pow(0.97, inertia * 2.0 / inertia);
        }
    }

    public static void SmoothClamp(ref float value, float min, float max, float lerpValue) {
        if (value < min) {
            value = MathHelper.Lerp(value, min, lerpValue);
        }
        if (value > max) {
            value = MathHelper.Lerp(value, max, lerpValue);
        }
    }

    public static int GetDirection(this float value) {
        if (float.IsNaN(value)) {
            return 1;
        }

        int result = Math.Sign(value);
        if (result != 0) {
            return result;
        }

        return 1;
    }

    public static Vector2 GetLimitedPosition(Vector2 startPosition, Vector2 endPosition, float maxLength, float minLength = 0f) {
        Vector2 dif = endPosition - startPosition;
        Vector2 result = startPosition + dif.SafeNormalize(Vector2.UnitY) * MathHelper.Clamp(dif.Length(), minLength, maxLength);
        return result;
    }

    public static float SearchForNearestTile<T>(this Entity entity, out Point tile, out Point? searchTile, Predicate<Point>? condition = null, int maxDist = 2) where T : ModTile {
        searchTile = null;
        tile = Point.Zero;
        Point center = entity.Center.ToTileCoordinates();
        bool foundTile = false;
        for (int i = center.X - maxDist; i < center.X + maxDist; i++) {
            for (int j = center.Y - maxDist; j < center.Y + maxDist; j++) {
                Tile searchedTile = WorldGenHelper.GetTileSafely(i, j);
                if (searchedTile.HasTile) {
                    bool closer = Vector2.DistanceSquared(tile.ToVector2(), center.ToVector2()) > Vector2.DistanceSquared(new Vector2(i, j), center.ToVector2());
                    if (WorldGen.SolidTile(searchedTile) && closer) {
                        tile = new Point(i, j);
                        foundTile = true;
                    }

                    Point tilePointPosition = new(i, j);
                    bool isTileValid = searchedTile.TileType == ModContent.TileType<T>() && Vector2.DistanceSquared((searchTile ?? Point.Zero).ToWorldCoordinates(), entity.Center) > Vector2.DistanceSquared(new Vector2(i, j) * 16, entity.Center);
                    if (condition != null && !condition(tilePointPosition)) {
                        isTileValid = false;
                    }
                    if (isTileValid) {
                        searchTile = tilePointPosition;
                    }
                }
            }
        }

        if (!foundTile) {
            return -1f;
        }
        return Vector2.DistanceSquared(tile.ToWorldCoordinates(), center.ToWorldCoordinates());
    }

    public static void BasicFlier(this Entity entity, float moveIntervalX = 0.1f, float moveIntervalY = 0.04f, float maxSpeedX = 4f, float maxSpeedY = 1.5f) {
        if (entity.direction == -1 && entity.velocity.X > -maxSpeedX) {
            entity.velocity.X -= moveIntervalX;
            if (entity.velocity.X > maxSpeedX) {
                entity.velocity.X -= moveIntervalX;
            }
            else if (entity.velocity.X > 0f) {
                entity.velocity.X += moveIntervalX * 0.5f;
            }
            if (entity.velocity.X < -maxSpeedX) {
                entity.velocity.X = -maxSpeedX;
            }
        }
        else if (entity.direction == 1 && entity.velocity.X < maxSpeedX) {
            entity.velocity.X += moveIntervalX;
            if (entity.velocity.X < -maxSpeedX) {
                entity.velocity.X += moveIntervalX;
            }
            else if (entity.velocity.X < 0f) {
                entity.velocity.X -= moveIntervalX * 0.5f;
            }
            if (entity.velocity.X > maxSpeedX) {
                entity.velocity.X = maxSpeedX;
            }
        }
        if (entity is NPC npc) {
            if (npc.directionY == -1 && (double)npc.velocity.Y > -maxSpeedY) {
                npc.velocity.Y -= moveIntervalY;
                if ((double)npc.velocity.Y > maxSpeedY) {
                    npc.velocity.Y -= moveIntervalY;
                }
                else if (npc.velocity.Y > 0f) {
                    npc.velocity.Y += moveIntervalY * 0.5f;
                }
                if ((double)npc.velocity.Y < -maxSpeedY) {
                    npc.velocity.Y = -maxSpeedY;
                }
            }
            else if (npc.directionY == 1 && (double)npc.velocity.Y < maxSpeedY) {
                npc.velocity.Y += moveIntervalY;
                if ((double)npc.velocity.Y < -maxSpeedY) {
                    npc.velocity.Y += moveIntervalY;
                }
                else if (npc.velocity.Y < 0f) {
                    npc.velocity.Y -= moveIntervalY * 0.5f;
                }
                if ((double)npc.velocity.Y > maxSpeedY) {
                    npc.velocity.Y = maxSpeedY;
                }
            }
            if (npc.collideX) {
                npc.direction = npc.oldVelocity.X > 0f ? -1 : 1;
                npc.velocity.X = -npc.velocity.X * 3f;
            }
            if (npc.collideY) {
                if (npc.oldVelocity.Y > 0f) {
                    npc.directionY = -1;
                }
                else {
                    npc.directionY = 1;
                }
                npc.velocity.Y = -npc.velocity.Y * 3f;
            }
        }
    }

    public static void LookAtPlayer(this Entity entity, Player player) => entity.direction = -Math.Sign(entity.Center.X - player.Center.X);

    public static void LookAt(this Entity entity, Vector2 position) => entity.direction = -Math.Sign(entity.position.X - position.X);

    public static bool LookingAt(this Entity entity, Vector2 position) => entity.position.X < position.X ? entity.direction == 1 : entity.direction != 1;

    public static void SlightlyMoveTo(this Entity entity, Vector2 position, float speed = 10f, float inertia = 15f) {
        Vector2 movement = position - entity.position;
        Vector2 movement2 = movement * (speed / movement.Length());
        entity.velocity += (movement2 - entity.velocity) / inertia;
    }

    public static void NewMessage(string text, Color color) {
        if (Main.netMode == NetmodeID.SinglePlayer) {
            Main.NewText(text, color);
        }
        else {
            ChatHelper.BroadcastChatMessage(NetworkText.FromKey(text), color);
        }
    }

    public static string AddSpace(this string text) => text.PadRight(text.Length + 1);
    public static string AddSpace2(this string text) => text.PadLeft(text.Length + 1);

    // aequus
    public static bool DeathrayHitbox(Vector2 center, Rectangle targetHitbox, float rotation, float length, float size, float startLength = 0f) {
        return DeathrayHitbox(center, targetHitbox, rotation.ToRotationVector2(), length, size, startLength);
    }

    public static bool DeathrayHitbox(Vector2 center, Rectangle targetHitbox, Vector2 normal, float length, float size, float startLength = 0f) {
        return DeathrayHitbox(center + normal * startLength, center + normal * startLength + normal * length, targetHitbox, size);
    }

    public static bool DeathrayHitbox(Vector2 from, Vector2 to, Rectangle targetHitbox, float size) {
        float _ = float.NaN;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), from, to, size, ref _);
    }

    // terraria overhaul
    public static float Damp(float source, float destination, float smoothing, float dt) => MathHelper.Lerp(source, destination, 1f - MathF.Pow(smoothing, dt));

    public static float SmoothAngleLerp(this float curAngle, float targetAngle, float amount) {
        float angle;
        if (targetAngle < curAngle) {
            float num = targetAngle + (float)Math.PI * 2f;
            angle = ((num - curAngle > curAngle - targetAngle) ? MathHelper.SmoothStep(curAngle, targetAngle, amount) : MathHelper.SmoothStep(curAngle, num, amount));
        }
        else {
            if (!(targetAngle > curAngle))
                return curAngle;

            float num = targetAngle - (float)Math.PI * 2f;
            angle = ((targetAngle - curAngle > curAngle - num) ? MathHelper.SmoothStep(curAngle, num, amount) : MathHelper.SmoothStep(curAngle, targetAngle, amount));
        }

        return MathHelper.WrapAngle(angle);
    }
}
