using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.Enums;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class Macrolepiota : NatureItem {
    public override Color? GetAlpha(Color lightColor) => Color.White;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(28, 40);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsageValues(ItemUseStyleID.None, 30, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);
    }

    public override void HoldItem(Player player) {
        if (!player.IsLocal()) {
            return;
        }

        if (player.HasProjectile<Macrolepiota_HeldProjectile>()) {
            return;
        }

        ProjectileHelper.SpawnPlayerOwnedProjectile<Macrolepiota_HeldProjectile>(new ProjectileHelper.SpawnProjectileArgs(player, player.GetSource_ItemUse(Item)));
    }
}