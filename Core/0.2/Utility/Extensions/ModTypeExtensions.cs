using Terraria.ModLoader;

namespace RoA.Core.Utility.Extensions;

static class ModTypeExtensions {
    public static bool FromRoA(this ModType modType) => modType.Mod == RoA.Instance;
}
