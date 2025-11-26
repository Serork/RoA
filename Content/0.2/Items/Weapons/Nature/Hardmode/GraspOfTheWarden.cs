using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Content.NPCs.Enemies.Backwoods.Hardmode;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

[AutoloadGlowMask(210, 210, 210, 210, shouldApplyItemAlpha: true)]
sealed class GraspOfTheWarden : NatureItem {
    public override void SetStaticDefaults() {
        Item.staff[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(40, 46);
        Item.SetWeaponValues(36, 2f);
        Item.SetUsableValues(ItemUseStyleID.Shoot, 20, autoReuse: true, useSound: WardenOfTheWoods.HitSound);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<WardenHand>(), shootSpeed: 8f);
        Item.SetShopValues(ItemRarityColor.Yellow8, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 50);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }
}
