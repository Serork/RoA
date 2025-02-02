using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

namespace RoA.Content.Items.Special;

[AutoloadGlowMask]
sealed class SphereOfCondor : MagicSphere {
    protected override Color? LightingColor => new(42, 148, 194);
}
