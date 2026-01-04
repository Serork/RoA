using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Consumables;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Vanity;
using RoA.Content.Items.Miscellaneous;
using RoA.Content.Items.Placeable.Decorations;
using RoA.Content.Items.Weapons.Magic;
using RoA.Content.Items.Weapons.Melee;
using RoA.Content.Items.Weapons.Ranged;
using RoA.Content.Items.Weapons.Summon;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Miscellaneous;

sealed class BackwoodsCrate : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.IsFishingCrate[Type] = true;
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 10;
    }

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Decorations.Crates>(), 0);
        Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(0, 1));
        Item.maxStack = Item.CommonMaxStack;
    }

    public override bool CanRightClick() {
        return true;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Crates;
    }

    public override void ModifyItemLoot(ItemLoot itemLoot) {
        IItemDropRule[] mainItems = [
            ItemDropRule.OneFromOptionsNotScalingWithLuck(1,
            ModContent.ItemType<Bane>(),
            ModContent.ItemType<OvergrownSpear>(),
            ModContent.ItemType<MothStaff>(),
            ModContent.ItemType<DoubleFocusCharm>(),
            ModContent.ItemType<BeastBow>())
        ];
        IItemDropRule[] costume = [
            ItemDropRule.OneFromOptionsNotScalingWithLuck(1,
            ModContent.ItemType<BunnyHat>(),
            ModContent.ItemType<BunnyJacket>(),
            ModContent.ItemType<BunnyPants>())
        ];
        IItemDropRule goldCoin = ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 4, 5, 12);
        IItemDropRule[] slipperyItems = [
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<SlipperyBomb>(), 1, 4, 10),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<SlipperyDynamite>(), 3, 1, 2),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<SlipperyGrenade>(), 1, 10, 21),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<SlipperyGlowstick>(), 1, 12, 25)
        ];
        IItemDropRule[] ores = [
            ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.GoldOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.PlatinumOre, 1, 20, 35),
        ];
        IItemDropRule[] bars = [
            ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.GoldBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.PlatinumBar, 1, 6, 16)
        ];
        IItemDropRule[] potions = [
            ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.SpelunkerPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.HunterPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.GravitationPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 1, 2, 4)
        ];
        IItemDropRule[] extraPotions = [
            ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 1, 5, 17),
            ItemDropRule.NotScalingWithLuck(ItemID.ManaPotion, 1, 5, 17)
        ];
        IItemDropRule[] extraBait = [
            ItemDropRule.NotScalingWithLuck(ItemID.MasterBait, 2, 2, 6),
            ItemDropRule.NotScalingWithLuck(ItemID.JourneymanBait, 1, 2, 6)
        ];
        IItemDropRule[] crateLoot = [
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<ElathaAmulet>(), 20),

            ItemDropRule.SequentialRulesNotScalingWithLuck(1, mainItems),
            ItemDropRule.SequentialRulesNotScalingWithLuck(2, costume),

            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<DryadStatue>(), 6),

            //ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Moss>(), 20),

            goldCoin,
            new OneFromRulesRule(7, ores),
            new OneFromRulesRule(4, bars),
            new OneFromRulesRule(4, potions),

            new OneFromRulesRule(2, extraPotions),
            new OneFromRulesRule(2, extraBait),
        ];

        itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(crateLoot));
    }
}