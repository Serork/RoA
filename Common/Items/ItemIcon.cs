using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

abstract class ItemIcon : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ItemsThatShouldNotBeInInventory[Item.type] = true;
        //ItemID.Sets.Deprecated[Item.type] = true;
    }
}
