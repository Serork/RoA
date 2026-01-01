using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class EmeraldOpaleye : ModItem {
    public override void SetDefaults() {
        Item.maxStack = Item.CommonMaxStack;
        Item.width = 36;
        Item.height = 36;
        Item.value = Item.sellPrice(0, 0, 5);
    }
}
