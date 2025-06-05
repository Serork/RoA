using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Special;

[AutoloadGlowMask(shouldApplyItemAlpha: true)]
sealed class SphereOfShock : MagicSphere {
    public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[Type] = (ushort)ModContent.ItemType<SphereOfAspiration>();
    }

    protected override Color? LightingColor => new(86, 173, 177);
}
