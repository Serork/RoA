using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Players;

// spirit
/// <summary>Used to handle if a vignette filter is active for the player, and information related to the application of the shader.</summary>
sealed class VignettePlayer2 : ModPlayer {
    private bool _lastTickVignetteActive;
    private bool _vignetteActive;
    private Vector2 _targetPosition;
    private float _opacity;
    private float _radius;
    private float _fadeDistance;
    private Color _color;

    public override void ResetEffects() {
        _lastTickVignetteActive = _vignetteActive;
        _vignetteActive = false;
    }

    public void SetVignette(float radius, float colorFadeDistance, float opacity) => SetVignette(radius, colorFadeDistance, opacity, Color.Black, Main.screenPosition);

    /// <summary>
    /// Sets a vignette effect for the player with the given radius, distance to fade from the default color to the used color, opacity of the effect, color, and center position.
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="colorFadeDistance"></param>
    /// <param name="opacity"></param>
    /// <param name="color"></param>
    /// <param name="targetPosition"></param>
    public void SetVignette(float radius, float colorFadeDistance, float opacity, Color color, Vector2 targetPosition) {
        _radius = radius;
        _targetPosition = targetPosition;
        _fadeDistance = colorFadeDistance;
        _color = color;
        _opacity = opacity;
        _vignetteActive = true;
    }

    public override void UpdateDead() {
        UpdateVisuals();
    }

    public override void PostUpdate() {
        UpdateVisuals();
    }

    private void UpdateVisuals() {
        if (Main.netMode == NetmodeID.Server) {
            return;
        }

        if (Player.whoAmI != Main.myPlayer) {
            return;
        }

        var vignetteShader = ShaderLoader.VignetteShaderData2;
        vignetteShader.UseColor(_color);
        vignetteShader.UseIntensity(_opacity);
        var vignetteEffect = ShaderLoader.VignetteEffectData2;
        vignetteEffect.Parameters["Radius"].SetValue(_radius);
        vignetteEffect.Parameters["FadeDistance"].SetValue(_fadeDistance);
        Player.ManageSpecialBiomeVisuals(ShaderLoader.Vignette2, _vignetteActive || _lastTickVignetteActive, _targetPosition);
    }
}
