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
        Item.SetUsableValues(ItemUseStyleID.Shoot, 40, autoReuse: true);
        Item.SetShootableValues((ushort)ModContent.ProjectileType<Bloodly>());
        Item.SetShopValues(ItemRarityColor.Pink5, Item.sellPrice());

        NatureWeaponHandler.SetPotentialDamage(Item, 60);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.2f);
    }

    public override bool? UseItem(Player player) {
        if (player.ItemAnimationJustStarted) {
            player.GetModPlayer<Crimsonest_AttackEncounter>().AttackCount++;
        }

        return base.UseItem(player);
    }
}

sealed class Crimsonest_AttackEncounter : ModPlayer {
    public int AttackCount;
    public bool CanReveal;

    public override void ResetEffects() {
        CanReveal = AttackCount >= Bloodly.AMOUNTNEEDFORATTACK - 1;
        if (AttackCount >= Bloodly.AMOUNTNEEDFORATTACK) {
            AttackCount = 0;
        }
    }

    public override void OnEnterWorld() {
        AttackCount = -1;
    }
}
