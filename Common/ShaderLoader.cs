using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.WorldEvents;
using RoA.Content.Backgrounds;
using RoA.Core;

using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace RoA.Common;

sealed class ShaderLoader : ModSystem {
    public static readonly string BackwoodsSky = RoA.ModName + "Backwoods Sky";
    public static readonly string BackwoodsFog = RoA.ModName + "Backwoods Fog";
    public static readonly string LothorSky = RoA.ModName + "Lothor Sky";
    public static readonly string EnragedLothorSky = RoA.ModName + "Enraged Lothor Sky";
    public static readonly string Vignette = RoA.ModName + "Vignette";

    public static VignetteScreenShaderData VignetteShaderData { get; private set; }
    public static Effect VignetteEffectData { get; private set; }

    public override void OnModLoad() {
        Asset<Effect> vignetteShader = ModContent.Request<Effect>(ResourceManager.Effects + "Vignette", AssetRequestMode.ImmediateLoad);
        VignetteEffectData = vignetteShader.Value;
        VignetteShaderData = new VignetteScreenShaderData(vignetteShader.Value, "MainPS");
        Filters.Scene[Vignette] = new Filter(VignetteShaderData, (EffectPriority)100);

        Filters.Scene[BackwoodsSky] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon").UseColor(0.2f, 0.2f, 0.2f).UseOpacity(0.05f), EffectPriority.High);
        SkyManager.Instance[BackwoodsSky] = new BackwoodsSky();
        Filters.Scene[BackwoodsSky].Load();

        Filters.Scene[BackwoodsFog] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon").UseColor(0.5f, 0.5f, 0.5f).UseOpacity(0.75f), EffectPriority.Medium);
        Filters.Scene[BackwoodsFog].Load();

        SkyManager.Instance[LothorSky] = new LothorShakeSky();
        Filters.Scene[LothorSky] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon"), EffectPriority.High);
        Filters.Scene[LothorSky].Load();

        Filters.Scene[EnragedLothorSky] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon"), EffectPriority.Medium);
        SkyManager.Instance[EnragedLothorSky] = new EnragedLothorSky();
        Filters.Scene[EnragedLothorSky].Load();

        SkyManager.Instance["CustomAmbience"] = new CustomSkyAmbience.CustomAmbientSky();
    }
}
