using System.ComponentModel;

using Terraria.ModLoader.Config;

namespace RoA.Common.Configs;

sealed class RoAConfig : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    public enum HighlightModes {
        Normal,
        Always,
        Off
    }

    [DefaultValue(false)]
    public bool HideDruidLeaves;

    [DefaultValue(HighlightModes.Normal)]
    [DrawTicks]
    public HighlightModes HighlightMode;

    public enum WreathDrawingModes {
        Normal,
        Fancy,
        Bars
    }

    [DefaultValue(WreathDrawingModes.Normal)]
    [DrawTicks]
    public WreathDrawingModes WreathDrawingMode;
}
