using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

sealed class CottonCane : CaneBaseItem<CottonCane.CottonCaneBase> {
    protected override void SafeSetDefaults() {
        Item.SetSizeValues(38, 38);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);

        Item.autoReuse = true;
    }

    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<CottonBoll>();

    public sealed class CottonCaneBase : CaneBaseProjectile {
        protected override bool ShouldWaitUntilProjDespawn() => false;
    }
}
