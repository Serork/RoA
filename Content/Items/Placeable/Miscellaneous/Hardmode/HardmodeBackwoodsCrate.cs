using RoA.Content.Items.Consumables;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Vanity;
using RoA.Content.Items.Miscellaneous;
using RoA.Content.Items.Placeable.Decorations;
using RoA.Content.Items.Weapons.Magic;
using RoA.Content.Items.Weapons.Melee;
using RoA.Content.Items.Weapons.Ranged;
using RoA.Content.Items.Weapons.Summon;
using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Miscellaneous.Hardmode;

sealed class HardmodeBackwoodsCrate : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Decorations.Crates>(), 1);
        Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(0, 1));
        Item.maxStack = Item.CommonMaxStack;
    }

    public override bool CanRightClick() => true;

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

        #region Pseudo-global
        ores = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.GoldOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.PlatinumOre, 1, 20, 35)
        };
        IItemDropRule[] hardmodeOres = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.CobaltOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.PalladiumOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.MythrilOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteOre, 1, 20, 35),
            ItemDropRule.NotScalingWithLuck(ItemID.TitaniumOre, 1, 20, 35)
        };
        bars = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.GoldBar, 1, 6, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.PlatinumBar, 1, 6, 16)
        };
        IItemDropRule[] hardmodeBars = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.CobaltBar, 1, 5, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.PalladiumBar, 1, 5, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.MythrilBar, 1, 5, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumBar, 1, 5, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteBar, 1, 5, 16),
            ItemDropRule.NotScalingWithLuck(ItemID.TitaniumBar, 1, 5, 16)
        };
        potions = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.SpelunkerPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.HunterPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.GravitationPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 1, 2, 4)
        };
        extraPotions = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 1, 5, 17),
            ItemDropRule.NotScalingWithLuck(ItemID.ManaPotion, 1, 5, 17)
        };
        extraBait = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.MasterBait, 2, 2, 6),
            ItemDropRule.NotScalingWithLuck(ItemID.JourneymanBait, 1, 2, 6)
        };
        #endregion

        IItemDropRule hardmodeBiomeCrateOres = ItemDropRule.SequentialRulesNotScalingWithLuck(7,
            new OneFromRulesRule(2, hardmodeOres),
            new OneFromRulesRule(1, ores)
        );
        IItemDropRule hardmodeBiomeCrateBars = ItemDropRule.SequentialRulesNotScalingWithLuck(4,
            new OneFromRulesRule(3, 2, hardmodeBars),
            new OneFromRulesRule(1, bars)
        );
        IItemDropRule[] crateLoot = [
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<ElathaAmulet>(), 20),

            ItemDropRule.SequentialRulesNotScalingWithLuck(1, mainItems),
            ItemDropRule.SequentialRulesNotScalingWithLuck(2, costume),

            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<DryadStatue>(), 6),

            //ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Moss>(), 20),

            goldCoin,
            hardmodeBiomeCrateOres,
            hardmodeBiomeCrateBars,
            new OneFromRulesRule(4, potions),

            new OneFromRulesRule(2, extraPotions),
            new OneFromRulesRule(2, extraBait),
        ];

        itemLoot.Add(ItemDropRule.AlwaysAtleastOneSuccess(crateLoot));
    }
}
