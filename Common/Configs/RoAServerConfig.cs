using System.ComponentModel;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace RoA.Common.Configs;

sealed class RoAServerConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ServerSide;

    public static RoAServerConfig Instance => ModContent.GetInstance<RoAServerConfig>();

    [Header("Mods.RoA.Configs.GeneralOptionsHeader2")]
    [DefaultValue(true)]
    [ReloadRequired]
    public bool ChangeVanillaRecipes;

    [DefaultValue(true)]
    public bool DropDevSets;

    [Range(0f, 1f)]
    [Increment(0.5f)]
    //[DrawTicks]
    [DefaultValue(0f)]
    public float EvilBiomeExtraItemChance;

    [Header("Mods.RoA.Configs.GenerationOptionHeader")]
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
}
