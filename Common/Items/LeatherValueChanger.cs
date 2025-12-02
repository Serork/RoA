using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class LeatherValueChanger : GlobalItem {
    public override void SetDefaults(Item entity) {
        if (entity.type == ItemID.Leather) {
            entity.value = Item.sellPrice(0, 0, 1, 0);
        }
    }
}
