using System;

using Terraria;

namespace RoA.Core.Utility.Extensions;

static partial class EntityExtensions {
    public static float SpeedX(this Entity entity) => MathF.Abs(entity.velocity.X);
    public static float XDistance(this Entity entity, Entity other) => MathF.Abs(entity.position.X - other.position.X);
    public static bool FacedRight(this Entity entity) => entity.direction > 0;
    public static bool JustChangedDirection(this Entity entity) => entity.direction != entity.oldDirection;
}
