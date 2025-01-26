using RoA.Content.Biomes.Backwoods;

using Terraria;

namespace RoA.Common.CustomConditions;

static class RoAConditions {
    public static Condition InBackwoods = new("Mods.RoA.Conditions.BackwoodsBiome",
        () => Main.LocalPlayer.InModBiome<BackwoodsBiome>());
}
