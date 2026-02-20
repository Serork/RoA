using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Tiles.Danger;

sealed class IceStalactite : StalactiteBase<IceStalactiteTE, IceStalactiteProjectile> {
    protected override void SafeSetStaticDefaults() {
        AddMapEntry(new Color(40, 105, 240), CreateMapEntryName());
    }
}

sealed class IceStalactiteTE : StalactiteTE<IceStalactiteProjectile> { }

sealed class IceStalactiteProjectile : StalactiteProjectileBase {
    protected override ushort KillDustType() => (ushort)DustID.Ice;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.Frozen, 120);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(BuffID.Frozen, 120);
    }
}
