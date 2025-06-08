using RoA.Content.Items.Weapons.Nature.Rods;

using Terraria.ID;

using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class BloomingDoomInWitchDoctorShopSystem : GlobalNPC {
    public override void ModifyShop(NPCShop shop) {
        if (shop.NpcType != NPCID.WitchDoctor) {
            return;
        }

        shop.InsertAfter(ItemID.Blowgun, ModContent.ItemType<BloomingDoom>());
    }
}
