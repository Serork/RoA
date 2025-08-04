using Terraria;

namespace RoA.Core.Utility.Extensions;

static partial class EntityExtensions {
    public static bool FacedRight(this Entity entity) => entity.direction > 0;
}
