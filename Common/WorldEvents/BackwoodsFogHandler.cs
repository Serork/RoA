using Terraria.ModLoader;

namespace RoA.Common.WorldEvents;

sealed class BackwoodsFogHandler : ModSystem {
    public static bool IsFogActive { get; private set; } = true;
}
