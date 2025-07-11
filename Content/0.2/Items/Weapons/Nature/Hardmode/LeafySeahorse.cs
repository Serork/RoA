using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Projectiles;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class LeafySeahorse : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32, 40);
        Item.SetWeaponValues(40, 2f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 14, autoReuse: true, useSound: SoundID.Item111 with { Pitch = 1f });
        Item.SetShootableValues();
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 80);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);

        Item.staff[Type] = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Vector2 yOffset = Vector2.UnitY.RotatedBy(velocity.ToRotation()) * 6f * player.direction;
        position -= yOffset;
        float randomAngle = 0.075f;
        velocity = velocity.RotatedByRandom(randomAngle);
        TrackedEntitiesSystem.SpawnTrackedProjectile<LeafySeahorse_Bubble>(source, position, velocity, damage, knockback, player.whoAmI);

        return false;
    }
}