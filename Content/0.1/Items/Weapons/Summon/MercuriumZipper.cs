using RoA.Content.Buffs;
using RoA.Content.Projectiles.Friendly.Summon;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Summon;

sealed class MercuriumZipper : ModItem {
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MercuriumZipperDebuff.TagDamage);

    public override void SetDefaults() {
        Item.DefaultToWhip(ModContent.ProjectileType<MercuriumZipperProjectile>(), 21, 2, 4);

        Item.useStyle = ItemUseStyleID.HiddenAnimation;

        Item.rare = ItemRarityID.Blue;

        Item.value = Item.sellPrice(0, 0, 40, 0);
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<MercuriumZipper_Effect>()] < 1;

    // Makes the whip receive melee prefixes
    public override bool MeleePrefix() {
        return true;
    }
}
