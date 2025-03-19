using RoA.Common.CustomConditions;
using RoA.Content.Items.Placeable.Decorations;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class LothorEnrageMonolithInPartyGirlShop : GlobalNPC {
    public override void ModifyShop(NPCShop shop) {
        if (shop.NpcType != NPCID.PartyGirl) {
            return;
        }

        shop.InsertAfter(3747, ModContent.ItemType<LothorEnrageMonolith>(), RoAConditions.LothorEnrageMonolith);
    }
}