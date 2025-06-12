using RoA.Common.CustomConditions;
using RoA.Content.Items.Equipables.Vanity;

using Terraria.ID;

using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class DinoHatInMerchantShop : GlobalNPC {
    public override void ModifyShop(NPCShop shop) {
        if (shop.NpcType != NPCID.Merchant) {
            return;
        }

        shop.InsertAfter(ItemID.MiningHelmet, ModContent.ItemType<DinoHat>(), RoAConditions.HasAnySaddle);
    }
}