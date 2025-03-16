﻿using System.ComponentModel;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

using static RoA.Common.Configs.RoAClientConfig;
using static System.Net.Mime.MediaTypeNames;

namespace RoA.Common.Configs;

sealed class RoAClientConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    public static RoAClientConfig Instance => ModContent.GetInstance<RoAClientConfig>();
    public static bool IsFancy => Instance.WreathDrawingMode == WreathDrawingModes.Fancy || Instance.WreathDrawingMode == WreathDrawingModes.Fancy2;
    public static bool IsBars => Instance.WreathDrawingMode == WreathDrawingModes.Bars || Instance.WreathDrawingMode == WreathDrawingModes.Bars2;

    public enum HighlightModes {
        Normal,
        Always,
        Off
    }

    public enum DamageTooltipOptions {
        Option1,
        Option2,
        Option3,
        Option4,
        Option5
    }

    [Header("Mods.RoA.Configs.GeneralOptionsHeader")]
    [DefaultValue(true)]
    [ReloadRequired]
    public bool VanillaResprites;

    [Header("Mods.RoA.Configs.DruidOptionsHeader")]

    [CustomModConfigItem(typeof(DamageTooltipOptionConfigElement))]
    [DefaultValue(DamageTooltipOptions.Option1)]
    [DrawTicks]
    public DamageTooltipOptions DamageTooltipOption_Internal;

    [CustomModConfigItem(typeof(DamageTooltipOptionConfigElement2))]
    [DefaultValue(DamageTooltipOptions.Option1)]
    [DrawTicks]
    public DamageTooltipOptions DamageTooltipOption;

    [CustomModConfigItem(typeof(BooleanElement2))]
    [DefaultValue(true)]
    public bool DamageScaling;

    [CustomModConfigItem(typeof(BooleanElement))]
    [DefaultValue(true)]
    public bool DruidLeaves;

    [CustomModConfigItem(typeof(DamageTooltipOptionConfigElement3))]
    [DefaultValue(HighlightModes.Normal)]
    [DrawTicks]
    public HighlightModes HighlightMode;

    public enum WreathDrawingModes {
        Normal,
        Normal2,
        Fancy,
        Fancy2,
        Bars,
        Bars2
    }

    [DefaultValue(WreathDrawingModes.Normal)]
    [DrawTicks]
    public WreathDrawingModes WreathDrawingMode;

    public enum WreathPositions {
        Health,
        Player
    }

    [DefaultValue(WreathPositions.Player)]
    [DrawTicks]
    public WreathPositions WreathPosition;
}
