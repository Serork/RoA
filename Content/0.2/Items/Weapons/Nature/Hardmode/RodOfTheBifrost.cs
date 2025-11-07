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

sealed class RodOfTheBifrost : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(28, 40);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 20, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.Yellow8, Item.sellPrice());
        Item.SetShootableValues();

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        //player.GetCommon().UseRodOfTheBifrostPattern();
        Projectile magicalBlock = ProjectileUtils.SpawnPlayerOwnedProjectile<MagicalBifrostBlock>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_ItemUse(Item)) {
            Damage = damage,
            KnockBack = knockback,
            AI1 = player.miscCounterNormalized * 12f % 1f
        });

        return false;
    }
}
