using RoA.Common.CustomConditions;
using RoA.Content.Items.Equipables.Vanity.Developer;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class DevWingsInWizardShop : GlobalNPC {
    public override void ModifyShop(NPCShop shop) {
        if (shop.NpcType != NPCID.Wizard) {
            return;
        }

        shop.InsertAfter(ItemID.EmptyDropper, ModContent.ItemType<PeegeonCape>(), RoAConditions.HasPeegeonSet);
        //shop.InsertAfter(ItemID.EmptyDropper, ModContent.ItemType<NFAWings>(), RoAConditions.HasNFASet);
        shop.InsertAfter(ItemID.EmptyDropper, ModContent.ItemType<EldritchRing>(), RoAConditions.HasHas2rSet);
    }
}
