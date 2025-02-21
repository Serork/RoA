using Microsoft.Xna.Framework;

using RoA.Content.Items.Placeable.Crafting;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged;

sealed class ElderwoodBow : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        int width = 16; int height = 32;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 29;
        Item.autoReuse = false;

        Item.DamageType = DamageClass.Ranged;
        Item.damage = 10;
        Item.knockBack = 1;

        Item.value = Item.sellPrice(copper: 20);
        Item.rare = ItemRarityID.White;
        Item.UseSound = SoundID.Item5;

        Item.shoot = ProjectileID.PurificationPowder;
        Item.useAmmo = AmmoID.Arrow;
        Item.shootSpeed = 11f;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<Elderwood>(10)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
