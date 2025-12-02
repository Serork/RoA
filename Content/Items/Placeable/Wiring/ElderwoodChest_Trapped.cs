using RoA.Content.Items.Placeable.Furniture;
using RoA.Content.Tiles.Miscellaneous;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Wiring;

public class ElderwoodChest_Trapped : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.TrapSigned[Type] = true;
    }

    public override string Texture => ItemLoader.GetItem(ModContent.ItemType<ElderwoodChest>()).Texture;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<TrappedChests>(), 1);
        Item.SetShopValues(ItemRarityColor.White0, 500);
        Item.maxStack = Item.CommonMaxStack;
        Item.width = 32;
        Item.height = 28;
    }
}
