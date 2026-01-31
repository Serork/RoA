using RoA.Content.Projectiles.Friendly.Ranged;
using RoA.Core.Defaults;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged.Hardmode;

sealed class SatchelCharge : ModItem {
    public override void SetDefaults() {
        Item.SetSizeValues(22, 34);
        Item.DefaultToRangedWeapon(ModContent.ProjectileType<SatchelChargeProjectile>(), AmmoID.None, 10, 5f);
        Item.knockBack = 6.5f;
        Item.UseSound = null;
        Item.damage = 14;
        Item.value = Item.buyPrice(0, 35);
        Item.rare = 3;
    }
}
