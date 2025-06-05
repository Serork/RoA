using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged;

sealed class DynastyWoodBow : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        int width = 16; int height = 32;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 27;
        Item.autoReuse = false;

        Item.DamageType = DamageClass.Ranged;
        Item.damage = 9;

        Item.value = Item.sellPrice(copper: 40);
        Item.rare = ItemRarityID.White;
        Item.UseSound = SoundID.Item5;

        Item.shoot = ProjectileID.PurificationPowder;
        Item.useAmmo = AmmoID.Arrow;
        Item.shootSpeed = 6.4f;
    }
}
