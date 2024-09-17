using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

abstract class Rod : ModItem {
    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.DamageType = DamageClass.Magic;

        Item.staff[Type] = true;
    }
}