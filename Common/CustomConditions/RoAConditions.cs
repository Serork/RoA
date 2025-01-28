using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Weapons.Melee;

using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
namespace RoA.Common.CustomConditions;

static class RoAConditions {
    public static Condition InBackwoods = new("Mods.RoA.Conditions.BackwoodsBiome", () => Main.LocalPlayer.InModBiome<BackwoodsBiome>());

    public static readonly IItemDropRule ShouldDropFlederSlayer = ItemDropRule.ByCondition(new FlederSlayerDropCondition(), ModContent.ItemType<FlederSlayer>());
}
