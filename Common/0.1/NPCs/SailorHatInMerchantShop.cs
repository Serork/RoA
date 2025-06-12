using RoA.Common.CustomConditions;
using RoA.Content.Items.Equipables.Vanity;

using Terraria.ID;

using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class SailorHatInMerchantShop : GlobalNPC {
    public override void ModifyShop(NPCShop shop) {
        if (shop.NpcType != NPCID.Pirate) {
            return;
        }

        shop.InsertAfter(ItemID.PiratePants, ModContent.ItemType<SailorHat>(), RoAConditions.SailorHatCondition);
    }
}
