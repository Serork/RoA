using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

abstract class ItemIcon : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ItemsThatShouldNotBeInInventory[Type] = true;
        ItemID.Sets.Deprecated[Type] = true;
    }
}
