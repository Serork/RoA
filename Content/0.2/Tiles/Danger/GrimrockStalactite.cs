using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
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

    protected override float FallSpeedModifier => 1.5f;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Bleeding, 90);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(BuffID.Bleeding, 90);
    }
}
