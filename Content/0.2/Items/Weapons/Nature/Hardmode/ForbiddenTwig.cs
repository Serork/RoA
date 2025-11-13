using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class ForbiddenTwig : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 42);
        Item.SetWeaponValues(30, 5f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 40, autoReuse: true, showItemOnUse: false);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<Projectiles.Friendly.Nature.ForbiddenTwig>());
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }
}
