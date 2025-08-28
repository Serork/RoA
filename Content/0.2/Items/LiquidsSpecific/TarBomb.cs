using ModLiquidLib.ModLoader;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.LiquidsSpecific;

sealed class TarBomb : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
        ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
    }

    public override void SetDefaults() {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.shootSpeed = 5f;
        Item.shoot = ModContent.ProjectileType<Projectiles.LiquidsSpecific.TarBomb>();
        Item.width = 22;
        Item.height = 26;
        Item.maxStack = Item.CommonMaxStack;
        Item.UseSound = SoundID.Item1;
        Item.consumable = true;
        Item.useAnimation = 25;
        Item.noUseGraphic = true;
        Item.useTime = 25;
        Item.value = Item.sellPrice(0, 0, 5);
        Item.rare = ItemRarityID.Blue;
    }

    //public override void AddRecipes() {
    //    Recipe recipe = Recipe.Create(ItemID.DryBomb);
    //    recipe.AddIngredient(Type);
    //    recipe.SortAfterFirstRecipesOf(ItemID.DryBomb);
    //    recipe.Register();

    //    Recipe recipe2 = Recipe.Create(Type);
    //    recipe2.AddIngredient(ItemID.DryBomb);
    //    recipe2.AddCondition(LiquidLoader.NearLiquid(LiquidLoader.LiquidType<ExampleLiquid>()));
    //    recipe2.Register();
    //}
}
