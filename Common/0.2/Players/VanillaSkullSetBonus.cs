using RoA.Content.Projectiles.Friendly.Miscellaneous;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

using static RoA.Content.Projectiles.Friendly.Miscellaneous.BoneSpinner;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer {
    public bool ApplyVanillaSkullSetBonus;

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
        if (!ApplyVanillaSkullSetBonus) {
            return;
        }

        if (proj.DamageType != DamageClass.Ranged) {
            return;
        }

        if (proj.GetGlobalProjectile<BoneSpinner_MakeAmmoHoming>().IsEffectActive) {
            return;
        }

        if (!Main.rand.NextBool(4) || proj.GetOwnerAsPlayer().HasProjectile<BoneSpinner>(3)) {
            return;
        }

        ProjectileUtils.SpawnPlayerOwnedProjectile<BoneSpinner>(new ProjectileUtils.SpawnProjectileArgs(Player, Player.GetSource_OnHit(target)) {
            Position = target.Center,
            Velocity = proj.velocity
        });
    }
}
