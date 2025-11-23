using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class GraspOfTheWarden : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(40, 46);
        Item.SetWeaponValues(36, 2f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 20, autoReuse: true, useSound: SoundID.Item65);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<WardenHand>(), shootSpeed: 8f);
        Item.SetShopValues(ItemRarityColor.Yellow8, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 50);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }
}
