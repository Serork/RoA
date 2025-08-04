using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;
 
sealed class SacredAcalypha : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(36, 34);
        Item.SetWeaponValues(36, 2f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 25, autoReuse: true, useSound: SoundID.Item65);
        Item.SetShootableValues(shootSpeed: 8f);
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 50);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        float randomAngle = 0.1f;
        velocity = velocity.RotatedByRandom(randomAngle);
        int max = 3;
        int count = (int)(max * MathUtils.Clamp01(WreathHandler.GetWreathChargeProgress(player) + 1f / max));
        for (int i = 0; i < count; i++) {
            ProjectileUtils.SpawnPlayerOwnedProjectile<AcalyphaTulip>(new ProjectileUtils.SpawnProjectileArgs(player, source) {
                Position = position,
                Velocity = velocity,
                Damage = damage,
                KnockBack = knockback,
            });
        }

        return false;
    }
}
