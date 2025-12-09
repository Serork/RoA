using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Players;

// spirit
/// <summary>Used to handle if a vignette filter is active for the player, and information related to the application of the shader.</summary>
sealed class VignettePlayer : ModPlayer {
    private bool _lastTickVignetteActive;
    private bool _vignetteActive;
    private Vector2 _targetPosition;
    private float _opacity;
    private float _radius;
    private float _fadeDistance;
    private Color _color;
    private float _noiseProgress;
    private bool _useNoise;

    public override void ResetEffects() {
        _lastTickVignetteActive = _vignetteActive;
        _vignetteActive = false;
    }

    public void SetVignette(float radius, float colorFadeDistance, float opacity, bool useNoise = false) => SetVignette(radius, colorFadeDistance, opacity, Color.Black, Main.screenPosition, useNoise);

    /// <summary>
    /// Sets a vignette effect for the player with the given radius, distance to fade from the default color to the used color, opacity of the effect, color, and center position.
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="colorFadeDistance"></param>
    /// <param name="opacity"></param>
    /// <param name="color"></param>
    /// <param name="targetPosition"></param>
    public void SetVignette(float radius, float colorFadeDistance, float opacity, Color color, Vector2 targetPosition, bool useNoise = false) {
        _radius = radius;
        _targetPosition = targetPosition;
        _fadeDistance = colorFadeDistance;
        _color = color;
        _opacity = opacity;
        _vignetteActive = true;
        _useNoise = useNoise;
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

        var vignetteShader = ShaderLoader.FogVignetteShaderData;
        vignetteShader.UseColor(_color);
        vignetteShader.UseOpacity(_opacity);

        if (_useNoise) {
            vignetteShader.UseImage(ResourceManager.Noise, 0);
            vignetteShader.UseImage(ResourceManager.Noise, 1);
            vignetteShader.UseTargetPosition(Main.screenPosition + (Vector2.UnitY * Player.gfxOffY));
            vignetteShader.UseProgress(_noiseProgress += Main.windSpeedCurrent * Main.windPhysicsStrength * -0.0375f);
            vignetteShader.UseIntensity(1f - 0.0625f * MathUtils.Clamp01(_opacity * 2f));
            vignetteShader.UseImageScale(new Vector2(Main.screenWidth, Main.screenHeight), 0);
            vignetteShader.UseImageScale(new Vector2(1024, 1024), 1);
        }

        vignetteShader.Apply();

        var vignetteEffect = ShaderLoader.FogVignetteEffectData;
        vignetteEffect.Parameters["Radius"].SetValue(_radius);
        vignetteEffect.Parameters["FadeDistance"].SetValue(_fadeDistance);
        Player.ManageSpecialBiomeVisuals(ShaderLoader.FogVignette, _vignetteActive || _lastTickVignetteActive, _targetPosition);
    }
}
