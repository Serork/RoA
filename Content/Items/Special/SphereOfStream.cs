using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

namespace RoA.Content.Items.Special;

[AutoloadGlowMask]
sealed class SphereOfStream : MagicSphere {
    protected override Color? LightingColor => new(57, 136, 232);
}
