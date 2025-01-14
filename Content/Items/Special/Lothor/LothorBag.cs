using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Special.Lothor;

sealed class LothorBag : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.BossBag[Type] = true;
        ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;

        Item.ResearchUnlockCount = 3;
    }

    public override void SetDefaults() {
        int width = 34; int height = width;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;

        Item.rare = ItemRarityID.LightRed;
        Item.expert = true;
    }

    public override bool CanRightClick() => true;

    public override void ModifyItemLoot(ItemLoot itemLoot) {
        //itemLoot.Add(new OneFromRulesRule(1, ItemDropRule.Common(ModContent.ItemType<Weapons.Magic.Bane>()),
        //									 ItemDropRule.Common(ModContent.ItemType<Weapons.Ranged.ChemicalPrisoner>()),
        //									 ItemDropRule.Common(ModContent.ItemType<Weapons.Summon.FlederStaff>()),
        //									 ItemDropRule.Common(ModContent.ItemType<Weapons.Melee.BloodshedAxe>()),
        //									 ItemDropRule.Common(ModContent.ItemType<Equipables.Accessories.SoulOfTheWoods>())));
        //itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Pets.MoonFlower>(), 10));
        //itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<LothorMask>(), 8));
        //itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Equipables.Accessories.Expert.BloodbathLocket>()));

        //itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<NPCs.Enemies.Bosses.Lothor.Lothor>()));
    }
}