using Terraria;
using Terraria.Utilities;

namespace RoA.Core.Utility;

static class RandomExtensions {
    public static bool NextChance(this UnifiedRandom rand, double chance) => rand.NextDouble() <= chance;

    public static float NextFloatRange(this UnifiedRandom rand, float range) => Utils.NextFloat(rand, -range, range);
}
