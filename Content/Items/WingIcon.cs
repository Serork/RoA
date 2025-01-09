using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items;

sealed class WingIcon : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ItemsThatShouldNotBeInInventory[Type] = true;
        ItemID.Sets.Deprecated[Type] = true;
    }
}
