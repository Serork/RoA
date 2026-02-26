using RoA.Common.Druid;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class FilamentYarn : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(46, 48);
        Item.SetWeaponValues(100, 5f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 20, autoReuse: false);
        Item.SetShootableValues(0);
        Item.SetShopValues(ItemRarityColor.StrongRed10, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 200);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }
}
