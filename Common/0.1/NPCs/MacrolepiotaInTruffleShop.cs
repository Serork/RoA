using RoA.Common.CustomConditions;
using RoA.Content.Items.Weapons.Nature.Hardmode;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class MacrolepiotaInTruffleShop : GlobalNPC {
    public override void ModifyShop(NPCShop shop) {
        if (shop.NpcType != NPCID.Truffle) {
            return;
        }

        shop.InsertAfter(ItemID.MushroomSpear, ModContent.ItemType<Macrolepiota>(), new Condition($"{RoAConditions.ConditionLang}", () => NPC.downedMechBossAny));
    }
}
