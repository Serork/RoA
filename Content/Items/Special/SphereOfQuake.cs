using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

namespace RoA.Content.Items.Special;

[AutoloadGlowMask]
sealed class SphereOfQuake : MagicSphere {
    protected override Color? LightingColor => new(73, 170, 104);
}
