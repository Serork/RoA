using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class Crimsonest : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(30, 36);
        Item.SetWeaponValues(30, 5f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 30, autoReuse: true);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<Bloodly>());
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());


        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }
}
