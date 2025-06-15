using System;

using Terraria.Utilities;

namespace RoA.Core.Utility.Extensions;

static class UnifiedRandomExtensions {
    public static T GetRandomEnumValue<T>(this UnifiedRandom random) where T : Enum {
        Array values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(random.Next(values.Length));
    }
}
