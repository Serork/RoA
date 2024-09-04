using RoA.Content.Biomes.Backwoods;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RoA.Common.WorldEvents;

sealed class BackwoodsFogShaderData : ModSceneEffect {
    public static float Opacity { get; private set; }

    private string ShaderName => ShaderLoader.BackwoodsFog;
    private Filter Filter => Filters.Scene[ShaderName];

    private bool ShouldBeActive(Player player) => player.InModBiome<BackwoodsBiome>() && BackwoodsFogHandler.IsFogActive;

    public override bool IsSceneEffectActive(Player player) => ShouldBeActive(player);

    public override void SpecialVisuals(Player player, bool isActive) {
        if (!isActive || !ShouldBeActive(player)) {
            if (Filter.IsActive()) {
                Opacity -= 0.005f;
                Filter.GetShader().UseOpacity(Opacity);
                if (Opacity <= 0f) {
                    Filter.GetShader().UseOpacity(0f);
                    Filter.Deactivate();
                }
            }
            return;
        }

        if (!Filter.IsActive()) {
            Filters.Scene.Activate(ShaderName);
        }
        else {
            if (Opacity < 0.75f) {
                Opacity += 0.0175f;
            }
            Filter.GetShader().UseOpacity(Opacity);
        }
    }
}