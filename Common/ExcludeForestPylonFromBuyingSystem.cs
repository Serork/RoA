using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common;

sealed class ExcludeForestPylonFromBuyingSystem : GlobalNPC {
    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items) {
        int i2 = 0;
        for (int i = 0; i < items.Length; i++) {
            Item item = items[i];
            if (item != null && item.type == ItemID.TeleportationPylonPurity) {
                i2 = i;
                break;
            }
        }
        if (i2 != 0) {
            for (int i = i2; i < items.Length; i++) {
                if (i + 1 < items.Length - 1) {
                    items[i] = items[i + 1];
                }
            }
        }
    }
}
