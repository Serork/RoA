// Decompiled with JetBrains decompiler
// Type: ThoriumMod.Items.Depths.DepthChest_Trapped
// Assembly: ThoriumMod, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 273B2A62-EAD1-4508-B87A-CE706C85A389
// Assembly location: C:\Users\mandr\Documents\My Games\Terraria\tModLoader\ModSources\Sources\tModUnpacker\ThoriumMod\ThoriumMod.dll

using RoA.Content.Items.Placeable.Furniture;
using RoA.Content.Tiles.Miscellaneous;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable;

public class BackwoodsDungeonChest_Trapped : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.TrapSigned[Type] = true;
    }

    public override string Texture => ItemLoader.GetItem(ModContent.ItemType<BackwoodsDungeonChest>()).Texture;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<TrappedChests>(), 0);
        Item.SetShopValues(ItemRarityColor.White0, 500);
        Item.maxStack = Item.CommonMaxStack;
        Item.width = 32;
        Item.height = 32;
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient<BackwoodsDungeonChest>()
            .AddRecipeGroup(ItemID.Wire, 10)
            .AddTile(TileID.HeavyWorkBench)
            .Register();
    }
}
