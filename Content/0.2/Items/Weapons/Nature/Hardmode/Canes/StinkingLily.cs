using RoA.Common.Druid;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Canes;

sealed class StinkingLily : CaneBaseItem<StinkingLily.StinkingLilyBase> {
    protected override ushort ProjectileTypeToCreate() => (ushort)ModContent.ProjectileType<Rafflesia>();

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32);
        Item.SetWeaponValues(30, 4f);
        Item.SetUsableValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);
    }

    public sealed class StinkingLilyBase : CaneBaseProjectile {
        protected override bool ShouldWaitUntilProjDespawns() => false;
    }
}
