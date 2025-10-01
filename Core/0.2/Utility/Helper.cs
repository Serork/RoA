using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using Terraria.ID;

namespace RoA.Core.Utility; 

static partial class Helper {
    public static bool IsNearlyZero(this float value, float tolerance = 0.1f) => MathF.Abs(value) < tolerance;

    public static bool SinglePlayerOrServer => Main.netMode != NetmodeID.MultiplayerClient;

    public static SpriteEffects ToSpriteEffects(this int direction) => direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
    
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
