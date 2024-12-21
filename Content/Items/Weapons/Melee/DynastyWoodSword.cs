using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Melee;

sealed class DynastyWoodSword : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults() {
        int width = 32; int height = width;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 24;
        Item.autoReuse = false;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 7;
        Item.knockBack = 4f;

        Item.value = Item.sellPrice(copper: 20);
        Item.rare = ItemRarityID.White;

        Item.UseSound = SoundID.Item1;
    }

    //public override void AddRecipes() {
    //	CreateRecipe()
    //		.AddIngredient(ItemID.DynastyWood, 7)
    //		.AddTile(TileID.WorkBenches)
    //		.Register();
    //}
}