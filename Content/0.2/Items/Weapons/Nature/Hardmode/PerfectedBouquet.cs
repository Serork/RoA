using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

[AutoloadGlowMask]
sealed class PerfectedBouquet : NatureItem {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32, 36);
        Item.SetWeaponValues(36, 2f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 20, autoReuse: true, useSound: SoundID.Item65);
        //Item.SetShootableValues((ushort)ModContent.ProjectileType<AcalyphaTulip>(), 8f);
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 50);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);

        Item.staff[Type] = true;
    }
}
