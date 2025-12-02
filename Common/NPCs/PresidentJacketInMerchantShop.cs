using RoA.Common.CustomConditions;
using RoA.Content.Items.Equipables.Vanity;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class PresidentJacketInMerchantShop : GlobalNPC {
    public override void ModifyShop(NPCShop shop) {
        if (shop.NpcType != NPCID.Merchant) {
            return;
        }

        shop.InsertAfter(ItemID.MiningHelmet, ModContent.ItemType<PresidentJacket>(), RoAConditions.Has05LuckOrMore);
    }
}
