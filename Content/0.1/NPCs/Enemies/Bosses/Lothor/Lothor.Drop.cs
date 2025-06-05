using RoA.Common.CustomConditions;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Content.Items.Pets;
using RoA.Content.Items.Special;
using RoA.Content.Items.Special.Lothor;
using RoA.Content.Items.Weapons.Melee;
using RoA.Content.Items.Weapons.Ranged;
using RoA.Content.Items.Weapons.Summon;

using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class Lothor : ModNPC {
    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LothorTrophy>(), 10));

        LeadingConditionRule notExpertRule = new(new Conditions.NotExpert());
        notExpertRule.OnSuccess(ItemDropRule.OneFromOptions(1,
                                                            ModContent.ItemType<SphereOfAspiration>(),
                                                            ModContent.ItemType<ChemicalPrisoner>(),
                                                            ModContent.ItemType<FlederStaff>(),
                                                            ModContent.ItemType<BloodshedAxe>(),
                                                            ModContent.ItemType<SoulOfTheWoods>()));
        notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<LothorMask>(), 7));
        notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<BloodCursor>(), 20));
        npcLoot.Add(notExpertRule);

        npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<LothorBag>()));

        npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<LothorRelic>()));
        npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<MoonFlower>(), 4));

        npcLoot.Add(RoAConditions.ShouldDropFlederSlayer);
    }
}
