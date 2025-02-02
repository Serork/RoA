using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

namespace RoA.Content.Items.Special;

[AutoloadGlowMask]
sealed class SphereOfShock : MagicSphere {
    protected override Color? LightingColor => new(86, 173, 177);
}
