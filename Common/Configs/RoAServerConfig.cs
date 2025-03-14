using System.ComponentModel;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RoA.Common.Configs;

sealed class RoAServerConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ServerSide;

    public static RoAServerConfig Instance => ModContent.GetInstance<RoAServerConfig>();

    [DefaultValue(false)]
    [ReloadRequired]
    public bool VanillaRecipes;

    [DefaultValue(true)]
    public bool DropDevSets;

    [Range(0.5f, 1.5f)]
    [Increment(0.05f)]
    //[DrawTicks]
    [DefaultValue(1f)]
    public float BackwoodsWidthMultiplier;

    [Range(0.5f, 1.5f)]
    [Increment(0.05f)]
    //[DrawTicks]
    [DefaultValue(1f)]
    public float BackwoodsHeightMultiplier;

    [Range(0f, 1f)]
    [Increment(0.5f)]
    //[DrawTicks]
    [DefaultValue(0f)]
    public float EvilBiomeExtraItemChance;
}
