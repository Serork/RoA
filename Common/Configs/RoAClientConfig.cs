using System.ComponentModel;

using Terraria.ModLoader;
using Terraria.ModLoader.Config;

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
    [Header("Mods.RoA.Configs.GeneralOptionsHeader")]
    [DefaultValue(true)]
    [ReloadRequired]
    public bool VanillaResprites;

    [Header("Mods.RoA.Configs.DruidOptionsHeader")]
    [DefaultValue(true)]
    public bool DruidLeaves;

    [DefaultValue(HighlightModes.Normal)]
    //[DrawTicks]
    public HighlightModes HighlightMode;

    public enum WreathDrawingModes {
        Normal,
        Fancy,
        Fancy2,
        Bars,
        Bars2
    }

    [DefaultValue(WreathDrawingModes.Normal)]
    //[DrawTicks]
    public WreathDrawingModes WreathDrawingMode;
}
