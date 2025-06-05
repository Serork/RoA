using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Special;

[AutoloadGlowMask(shouldApplyItemAlpha: true)]
sealed class SphereOfCondor : MagicSphere {
    public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[Type] = (ushort)ModContent.ItemType<SphereOfAspiration>();
    }

    protected override Color? LightingColor => new(42, 148, 194);
}
