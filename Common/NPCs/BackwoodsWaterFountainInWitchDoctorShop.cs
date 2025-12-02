using RoA.Content.Items.Placeable.Decorations;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class BackwoodsWaterFountainInWitchDoctorShop : GlobalNPC {
    public override void ModifyShop(NPCShop shop) {
        if (shop.NpcType != NPCID.WitchDoctor) {
            return;
        }

        shop.InsertAfter(ItemID.JungleWaterFountain, ModContent.ItemType<BackwoodsWaterFountain>());
    }
}
