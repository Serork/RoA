using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

namespace RoA.Content.Items.Special;

[AutoloadGlowMask]
sealed class SphereOfPyre : MagicSphere {
    protected override Color? LightingColor => new(255, 154, 116);
}
