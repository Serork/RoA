using RoA.Content.Items.Equipables.Wreaths;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class TwigWreathInDryadShopSystem : GlobalNPC {
    public override void ModifyShop(NPCShop shop) {
        if (shop.NpcType != NPCID.Dryad) {
            return;
        }

        shop.InsertAfter(ItemID.DirtRod, ModContent.ItemType<TwigWreath>());
    }
}
