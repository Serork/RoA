using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility; 

static partial class Helper {
    public static Vector2 OffsetPerSolidTileSlope_Bottom(Tile tile, bool onlySlope = false) {
        Vector2 result = Vector2.Zero;
        if (!onlySlope && tile.IsHalfBlock) {
            result.Y += 8f;
        }
        if (tile.TopSlope) {
            result.Y += 8f;
        }
        return result;
    }

    public static void SpawnDebugDusts(Vector2 position) {
        Dust.NewDustPerfect(position, ModContent.DustType<Content.Dusts.Torch>(), Vector2.Zero).noGravity = true;
    }

    public static void SpawnDebugDusts(Vector2 position, int type) {
        Dust.NewDustPerfect(position, type, Vector2.Zero).noGravity = true;
    }

    public static Vector2 GetBezierPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t) {
        t = MathHelper.Clamp(t, 0.0f, 1f);
        float num = 1f - t;
        return num * num * num * a + 3f * num * num * t * b + 3f * num * t * t * c + t * t * t * d;
    }

    public static Vector2 GetBezierPoint(Vector2 a, Vector2 b, Vector2 c, float t) {
        t = MathHelper.Clamp(t, 0.0f, 1f);
        float num = 1f - t;
        return num * num * a + 2f * num * t * b + t * t * c;
    }

    public static void PushPath<T>(T[] array, T point) {
        Array.Copy((Array)array, 0, (Array)array, 1, array.Length - 1);
        array[0] = point;
    }

    public static void FillBezier(Vector2[] array, Vector2 a, Vector2 b, Vector2 c, int length = -1) {
        length = length == -1 ? array.Length : length;
        for (int index = 0; index < length; ++index) {
            array[index] = GetBezierPoint(a, b, c, (float)index / (float)(length - 1));
        }
    }

    public static bool IsNearlyZero(this float value, float tolerance = 0.1f) => MathF.Abs(value) < tolerance;

    public static bool SinglePlayerOrServer => Main.netMode != NetmodeID.MultiplayerClient;

    public static SpriteEffects ToSpriteEffects(this bool facedRight) => facedRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
    public static SpriteEffects ToSpriteEffects(this int direction) => direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
    public static SpriteEffects ToSpriteEffects2(this bool facedRight) => facedRight ? SpriteEffects.None : SpriteEffects.FlipVertically;
    public static SpriteEffects ToSpriteEffects2(this int direction) => direction > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

    public static bool OnSurface(Vector2 center, ref Vector2 velocity) => (double)center.Y < Main.worldSurface * 16.0 && Main.tile[(int)center.X / 16, (int)center.Y / 16] != null && Main.tile[(int)center.X / 16, (int)center.Y / 16].WallType == 0 && ((velocity.X > 0f && Main.windSpeedCurrent < 0f) || (velocity.X < 0f && Main.windSpeedCurrent > 0f) || Math.Abs(velocity.X) < Math.Abs(Main.windSpeedCurrent * Main.windPhysicsStrength) * 180f) && Math.Abs(velocity.X) < 16f;

    public static void ApplyWindPhysics(Vector2 center, ref Vector2 velocity) {
        /*if (Main.windPhysics) */ {
            if ((double)center.Y < Main.worldSurface * 16.0 && Main.tile[(int)center.X / 16, (int)center.Y / 16] != null && Main.tile[(int)center.X / 16, (int)center.Y / 16].WallType == 0 && ((velocity.X > 0f && Main.windSpeedCurrent < 0f) || (velocity.X < 0f && Main.windSpeedCurrent > 0f) || Math.Abs(velocity.X) < Math.Abs(Main.windSpeedCurrent * Main.windPhysicsStrength) * 180f) && Math.Abs(velocity.X) < 16f) {
                velocity.X += Main.windSpeedCurrent * Main.windPhysicsStrength;
                MathHelper.Clamp(velocity.X, -16f, 16f);
            }
        }
        //velocityX += GetWindPhysicsVelocity(center);
    }

    public static void ApplyWindPhysicsX(Vector2 center, ref float velocityX) {
        /*if (Main.windPhysics) */
        {
            if ((double)center.Y < Main.worldSurface * 16.0 && Main.tile[(int)center.X / 16, (int)center.Y / 16] != null && Main.tile[(int)center.X / 16, (int)center.Y / 16].WallType == 0 && ((velocityX > 0f && Main.windSpeedCurrent < 0f) || (velocityX < 0f && Main.windSpeedCurrent > 0f) || Math.Abs(velocityX) < Math.Abs(Main.windSpeedCurrent * Main.windPhysicsStrength) * 180f) && Math.Abs(velocityX) < 16f) {
                velocityX += Main.windSpeedCurrent * Main.windPhysicsStrength;
                MathHelper.Clamp(velocityX, -16f, 16f);
            }
        }
        //velocityX += GetWindPhysicsVelocity(center);
    }

    public static bool CanApplyWindPhysics(Vector2 center, float velocityX) => (double)center.Y < Main.worldSurface * 16.0 && Main.tile[(int)center.X / 16, (int)center.Y / 16] != null && Main.tile[(int)center.X / 16, (int)center.Y / 16].WallType == 0 && ((velocityX > 0f && Main.windSpeedCurrent < 0f) || (velocityX < 0f && Main.windSpeedCurrent > 0f) || Math.Abs(velocityX) < Math.Abs(Main.windSpeedCurrent * Main.windPhysicsStrength) * 180f) && Math.Abs(velocityX) < 16f;
}
