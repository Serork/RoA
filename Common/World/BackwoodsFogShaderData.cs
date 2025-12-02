using RoA.Content.Biomes.Backwoods;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RoA.Common.World;

sealed class BackwoodsFogShaderData : ModSceneEffect {
    private string ShaderName => ShaderLoader.BackwoodsFog;
    private Filter Filter => Filters.Scene[ShaderName];

    private bool ShouldBeActive(Player player) => player.InModBiome<BackwoodsBiome>() && BackwoodsFogHandler.IsFogActive;

    public override bool IsSceneEffectActive(Player player) => ShouldBeActive(player);

    public override void SpecialVisuals(Player player, bool isActive) {
        if (!isActive || !ShouldBeActive(player)) {
            if (Filter.IsActive()) {
                Filter.GetShader().UseOpacity(BackwoodsFogHandler.Opacity);
                if (BackwoodsFogHandler.Opacity <= 0f) {
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
            Filter.GetShader().UseOpacity(BackwoodsFogHandler.Opacity);
        }
    }
}