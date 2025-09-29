using RoA.Core.Data;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Core.Utility.Extensions;

static partial class PlayerExtensions {
    public static bool HasProjectile<T>(this Player player) where T : ModProjectile => player.ownedProjectileCounts[ModContent.ProjectileType<T>()] >= 1;

    public static bool IsAliveAndFree(this Player player) => player.IsAlive() && !player.CCed;
    public static bool IsAlive(this Player player) => player.active && !player.dead;

    public static bool IsHolding<T>(this Player player) where T : ModItem => player.HeldItem.type == ModContent.ItemType<T>();

    public static void UseBodyFrame(this Player player, PlayerFrame frameToUse) => player.bodyFrame.Y = 56 * (int)frameToUse;
}
