using RoA.Content.Backgrounds;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common;

sealed class ShaderLoader : ModSystem {
    public static readonly string BackwoodsSky = "Backwoods Sky";
    public static readonly string BackwoodsFog = "Backwoods Fog";

    public override void OnModLoad() {
        if (Main.netMode == NetmodeID.Server) {
            return;
        }

        Filters.Scene[BackwoodsSky] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon").UseColor(0.2f, 0.2f, 0.2f).UseOpacity(0.05f), EffectPriority.High);
        SkyManager.Instance[BackwoodsSky] = new BackwoodsSky();
        Filters.Scene[BackwoodsSky].Load();

        Filters.Scene[BackwoodsFog] = new Filter(new BackwoodsScreenShaderData("FilterBloodMoon").UseColor(0.5f, 0.5f, 0.5f).UseOpacity(0.75f), EffectPriority.Medium);
        Filters.Scene[BackwoodsFog].Load();
    }
}
