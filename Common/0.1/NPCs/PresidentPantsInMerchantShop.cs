using RoA.Common.CustomConditions;
using RoA.Content.Items.Equipables.Vanity;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class PresidentPantsInMerchantShop : GlobalNPC {
    public override void ModifyShop(NPCShop shop) {
        if (shop.NpcType != NPCID.Merchant) {
            return;
        }

        shop.InsertAfter(ModContent.ItemType<PresidentJacket>(), ModContent.ItemType<PresidentPants>(), RoAConditions.Has05LuckOrMore);
    }
}
