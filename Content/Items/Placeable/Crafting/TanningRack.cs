using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using RoA.Content.World.Generations;

namespace RoA.Content.Items.Placeable.Crafting;

sealed class TanningRack : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Tanning Rack");
        // Tooltip.SetDefault("Allows leather to be obtained from enemies\nIt must be processed at the rack before spoiling");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 28; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 99;
        Item.useTurn = true;
        Item.autoReuse = true;

        Item.useAnimation = 20;
        Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.Swing;

        Item.consumable = true;
        Item.rare = ItemRarityID.White;
        Item.value = Item.sellPrice(silver: 20);
        Item.createTile = ModContent.TileType<Tiles.Crafting.TanningRack>();
    }

    //public override bool? UseItem(Player player) {
    //    if (player.ItemAnimationJustStarted) {
    //        BackwoodsBiomePass.PlaceBackwoodsCattail(Player.tileTargetX, Player.tileTargetY);
    //    }

    //    return base.UseItem(player);
    //}

    //public override void AddRecipes () {
    //    CreateRecipe()
    //        .AddRecipeGroup(RecipeGroupID.Wood, 15)
    //        .AddRecipeGroup(ItemID.Bunny)
    //        .AddTile(TileID.WorkBenches)
    //       .Register();
    //}
}