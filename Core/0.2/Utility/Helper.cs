using Microsoft.Xna.Framework;

using System;

using Terraria;
using Terraria.ID;

namespace RoA.Core.Utility; 

static partial class Helper {
    public static bool SinglePlayerOrServer => Main.netMode != NetmodeID.MultiplayerClient;
    
    public static void ApplyWindPhysics(Vector2 center, ref Vector2 velocity) {
        ///*if (Main.windPhysics) */{
        //    if ((double)center.Y < Main.worldSurface * 16.0 && Main.tile[(int)center.X / 16, (int)center.Y / 16] != null && Main.tile[(int)center.X / 16, (int)center.Y / 16].WallType == 0 && ((velocity.X > 0f && Main.windSpeedCurrent < 0f) || (velocity.X < 0f && Main.windSpeedCurrent > 0f) || Math.Abs(velocity.X) < Math.Abs(Main.windSpeedCurrent * Main.windPhysicsStrength) * 180f) && Math.Abs(velocity.X) < 16f) {
        //        velocity.X += Main.windSpeedCurrent * Main.windPhysicsStrength;
        //        MathHelper.Clamp(velocity.X, -16f, 16f);
        //    }
        //}
        velocity += GetWindPhysicsVelocity(center);
    }

    public static Vector2 GetWindPhysicsVelocity(Vector2 center) {
        Vector2 velocity = Vector2.Zero;
        if ((double)center.Y < Main.worldSurface * 16.0 && Main.tile[(int)center.X / 16, (int)center.Y / 16] != null && Main.tile[(int)center.X / 16, (int)center.Y / 16].WallType == 0 && ((velocity.X > 0f && Main.windSpeedCurrent < 0f) || (velocity.X < 0f && Main.windSpeedCurrent > 0f) || Math.Abs(velocity.X) < Math.Abs(Main.windSpeedCurrent * Main.windPhysicsStrength) * 180f) && Math.Abs(velocity.X) < 16f) {
            velocity.X += Main.windSpeedCurrent * Main.windPhysicsStrength;
            MathHelper.Clamp(velocity.X, -16f, 16f);
        }

        return velocity;
    }
}
