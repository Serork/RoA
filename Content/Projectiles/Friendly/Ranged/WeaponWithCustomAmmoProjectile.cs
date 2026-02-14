using RoA.Common.Players;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

abstract class WeaponWithCustomAmmoProjectile : ModProjectile {
    public Item AttachedWeapon { get; private set; } = null!;

    protected virtual bool ShouldAttachWeapon => true;

    public override void OnSpawn(IEntitySource source) {
        Player player = Projectile.GetOwnerAsPlayer();
        // TODO: sync
        if (!ShouldAttachWeapon) {
            AttachedWeapon = player.GetModPlayer<RangedArmorSetPlayer>().UsedRangedWeaponWithCustomAmmo;
        }
        else {
            AttachedWeapon = player.GetSelectedItem();
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        Player player = Projectile.GetOwnerAsPlayer();
        if (player.GetCommon().IsBadgeOfHonorEffectActive && target.life <= 0 && target.CanActivateOnHitEffect()) {
            var handler = player.GetModPlayer<RangedArmorSetPlayer>();
            if (!handler.CanReceiveCustomAmminition) {
                return;
            }
            handler.ReceiveCustomAmmunition(AttachedWeapon);
            handler.CanReceiveCustomAmminition = false;
        }
    }
}
