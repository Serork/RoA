﻿using Microsoft.Xna.Framework;

using Terraria.ID;

namespace RoA.Content.Tiles.Danger;

sealed class StoneStalactite : StalactiteBase<StoneStalactiteTE, StoneStalactiteProjectile> {
    protected override void SafeSetStaticDefaults() {
        AddMapEntry(new Color(128, 128, 128));
    }
}

sealed class StoneStalactiteTE : StalactiteTE<StoneStalactiteProjectile> { }

sealed class StoneStalactiteProjectile : StalactiteProjectileBase {
    protected override ushort KillDustType() => (ushort)DustID.Stone;
}

