using Microsoft.Xna.Framework;

using Terraria.ModLoader;

namespace RoA.Content.Tiles.Danger;

sealed class GrimrockStalactite : StalactiteBase<GrimrockStalactiteTE, GrimrockStalactiteProjectile> {
    protected override void SafeSetStaticDefaults() {
        AddMapEntry(new Color(74, 75, 87));

        MineResist = 1.25f;
    }
}

sealed class GrimrockStalactiteTE : StalactiteTE<GrimrockStalactiteProjectile> { }

sealed class GrimrockStalactiteProjectile : StalactiteProjectileBase {
    protected override ushort KillDustType() => (ushort)ModContent.DustType<Dusts.Backwoods.Stone>();
}
