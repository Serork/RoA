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
    public static readonly string BackwoodsSky = "Backwoods Sky";
    public static readonly string BackwoodsFog = "Backwoods Fog";
    public static readonly string LothorSky = "Lothor Sky";

    public override void OnModLoad() {
        string name = "Tint";
        Asset<Effect> tintShader = ModContent.Request<Effect>(ResourceManager.Effects + name);
        GameShaders.Misc[$"{RoA.ModName}{name}"] = new MiscShaderData(tintShader, name);

        Filters.Scene[BackwoodsSky] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon").UseColor(0.2f, 0.2f, 0.2f).UseOpacity(0.05f), EffectPriority.High);
        SkyManager.Instance[BackwoodsSky] = new BackwoodsSky();
        Filters.Scene[BackwoodsSky].Load();

        Filters.Scene[BackwoodsFog] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon").UseColor(0.5f, 0.5f, 0.5f).UseOpacity(0.75f), EffectPriority.Medium);
        Filters.Scene[BackwoodsFog].Load();

        SkyManager.Instance[LothorSky] = new LothorShakeSky();
        Filters.Scene[LothorSky] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon"), EffectPriority.High);
        Filters.Scene[LothorSky].Load();

        SkyManager.Instance["CustomAmbience"] = new CustomSkyAmbience.CustomAmbientSky();
    }
}
