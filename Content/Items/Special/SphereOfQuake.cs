﻿using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Special;

[AutoloadGlowMask]
sealed class SphereOfQuake : MagicSphere {
    public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[Type] = (ushort)ModContent.ItemType<SphereOfAspiration>();
    }

    protected override Color? LightingColor => new(73, 170, 104);
}
