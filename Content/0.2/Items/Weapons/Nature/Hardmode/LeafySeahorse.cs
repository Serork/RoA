using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class LeafySeahorse : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32, 40);
        Item.SetWeaponValues(40, 2f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 14, autoReuse: true, useSound: SoundID.Item111 with { Pitch = 1f });
        Item.SetShootableValues();
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 80);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Vector2 yOffset = Vector2.UnitY.RotatedBy(velocity.ToRotation()) * 11.5f * player.direction * player.gravDir;
        position -= yOffset;
        float randomAngle = 0.075f;
        velocity = velocity.RotatedByRandom(randomAngle);
        ProjectileUtils.SpawnPlayerOwnedProjectile<LeafySeahorse_Bubble>(new ProjectileUtils.SpawnProjectileArgs(player, source) {
            Position = position,
            Velocity = velocity,
            Damage = damage,
            KnockBack = knockback,
        }, centered: true);

        return false;
    }
}