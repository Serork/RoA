using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class WoodpeckerFeather : ModItem {
    public override void SetDefaults() {
        Item.maxStack = Item.CommonMaxStack;
        Item.width = 14;
        Item.height = 34;
        Item.value = Item.sellPrice(0, 2, 50);
        Item.rare = ItemRarityID.Pink;
    }
}
