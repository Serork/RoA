using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class DesertTendril : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(30, 36);
        Item.SetWeaponValues(30, 5f);
        Item.SetUsableValues(-1, 40, autoReuse: true, showItemOnUse: false);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<DesertTendrilVine>());
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }
}
