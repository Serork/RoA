using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Content.Items.Equipables.Vanity;
using RoA.Content.Items.Materials;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.Items.Placeable.Seeds;
using RoA.Content.Items.Potions;
using RoA.Content.Items.Weapons.Magic;
using RoA.Content.Items.Weapons.Nature.PreHardmode;
using RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;
using RoA.Content.Items.Weapons.Nature.PreHardmode.Claws;
using RoA.Content.Items.Weapons.Summon;

using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class ExtraVanillaCrateItems : ModSystem {
    public override void Load() {
        On_ItemDropDatabase.RegisterCrateDrops += On_ItemDropDatabase_RegisterCrateDrops;
        On_ItemDropDatabase.RegisterLockbox += On_ItemDropDatabase_RegisterLockbox;
        On_ItemDropDatabase.RegisterHerbBag += On_ItemDropDatabase_RegisterHerbBag;
        On_ItemDropDatabase.RegisterObsidianLockbox += On_ItemDropDatabase_RegisterObsidianLockbox;
    }

    private void On_ItemDropDatabase_RegisterObsidianLockbox(On_ItemDropDatabase.orig_RegisterObsidianLockbox orig, ItemDropDatabase self) {
        IItemDropRule ruleFlowerOfFireUnholyTrident = ItemDropRule.ByCondition(new Conditions.NotRemixSeed(), ItemID.FlowerofFire);
        ruleFlowerOfFireUnholyTrident.OnFailedConditions(ItemDropRule.NotScalingWithLuck(ItemID.UnholyTrident), hideLootReport: true);

        IItemDropRule[] obsidianLockBoxList = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.DarkLance),
            ItemDropRule.NotScalingWithLuck(ItemID.Sunfury),
            ruleFlowerOfFireUnholyTrident,
            ItemDropRule.NotScalingWithLuck(ItemID.Flamelash),
            ItemDropRule.NotScalingWithLuck(ItemID.HellwingBow),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<HellfireClaws>())
        };

        self.RegisterToItem(ItemID.ObsidianLockbox, new OneFromRulesRule(1, obsidianLockBoxList));
        self.RegisterToItem(ItemID.ObsidianLockbox, ItemDropRule.NotScalingWithLuck(ItemID.TreasureMagnet, 5));
    }

    private void On_ItemDropDatabase_RegisterHerbBag(On_ItemDropDatabase.orig_RegisterHerbBag orig, ItemDropDatabase self) {
        self.RegisterToItem(ItemID.HerbBag,
            new HerbBagDropsItemDropRule(
                ModContent.ItemType<MiracleMint>(), ItemID.Daybloom, ItemID.Moonglow, ItemID.Blinkroot, ItemID.Waterleaf, ItemID.Deathweed, ItemID.Fireblossom, ItemID.Shiverthorn,
                ModContent.ItemType<MiracleMintSeeds>(), ItemID.DaybloomSeeds, ItemID.MoonglowSeeds, ItemID.BlinkrootSeeds, ItemID.WaterleafSeeds, ItemID.DeathweedSeeds, ItemID.FireblossomSeeds, ItemID.ShiverthornSeeds));
    }

    private void On_ItemDropDatabase_RegisterLockbox(On_ItemDropDatabase.orig_RegisterLockbox orig, ItemDropDatabase self) {
        IItemDropRule ruleAquaScepterBubbleGun = ItemDropRule.ByCondition(new Conditions.NotRemixSeed(), ItemID.AquaScepter);
        ruleAquaScepterBubbleGun.OnFailedConditions(ItemDropRule.NotScalingWithLuck(ItemID.BubbleGun), hideLootReport: true);

        IItemDropRule[] goldenLockBoxList = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.Valor),
            ItemDropRule.NotScalingWithLuck(ItemID.Muramasa),
            ItemDropRule.NotScalingWithLuck(ItemID.CobaltShield),
            ruleAquaScepterBubbleGun,
            ItemDropRule.NotScalingWithLuck(ItemID.BlueMoon),
            ItemDropRule.NotScalingWithLuck(ItemID.MagicMissile),
            ItemDropRule.NotScalingWithLuck(ItemID.Handgun),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<RagingBoots>())
        };

        self.RegisterToItem(ItemID.LockBox, new OneFromRulesRule(1, goldenLockBoxList));
        self.RegisterToItem(ItemID.LockBox, ItemDropRule.NotScalingWithLuck(ItemID.ShadowKey, 3));
    }

    private void On_ItemDropDatabase_RegisterCrateDrops(On_ItemDropDatabase.orig_RegisterCrateDrops orig, ItemDropDatabase self) {
        #region Wooden Crate and Pearlwood Crate
        IItemDropRule[] themed = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.SailfishBoots, 40),
            ItemDropRule.NotScalingWithLuck(ItemID.TsunamiInABottle, 40),
            ItemDropRule.NotScalingWithLuck(ItemID.Extractinator, 50),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<TanningRack>(), 50)
        };
        IItemDropRule[] hardmodeThemed = new IItemDropRule[]
        {
            ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.Sundial, 200),
            ItemDropRule.NotScalingWithLuck(ItemID.SailfishBoots, 40),
            ItemDropRule.NotScalingWithLuck(ItemID.TsunamiInABottle, 40),
            ItemDropRule.NotScalingWithLuck(ItemID.Anchor, 25)
        };
        IItemDropRule[] coin = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 3, 1, 5),
            ItemDropRule.NotScalingWithLuck(ItemID.SilverCoin, 1, 20, 90)
        };
        IItemDropRule[] ores = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 1, 4, 15),
            ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 1, 4, 15),
            ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 1, 4, 15),
            ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 1, 4, 15)
        };
        IItemDropRule[] hardmodeOres = new IItemDropRule[] {
            ItemDropRule.NotScalingWithLuck(ItemID.CobaltOre, 1, 4, 15),
            ItemDropRule.NotScalingWithLuck(ItemID.PalladiumOre, 1, 4, 15)
        };
        IItemDropRule[] bars = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.CopperBar, 1, 2, 5),
            ItemDropRule.NotScalingWithLuck(ItemID.TinBar, 1, 2, 5),
            ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 1, 2, 5),
            ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 1, 2, 5)
        };
        IItemDropRule[] hardmodeBars = new IItemDropRule[] {
            ItemDropRule.NotScalingWithLuck(ItemID.CobaltBar, 1, 2, 3),
            ItemDropRule.NotScalingWithLuck(ItemID.PalladiumBar, 1, 2, 3)
        };
        IItemDropRule[] potions = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 1, 1, 3),
            ItemDropRule.NotScalingWithLuck(ItemID.SwiftnessPotion, 1, 1, 3),
            ItemDropRule.NotScalingWithLuck(ItemID.IronskinPotion, 1, 1, 3),
            ItemDropRule.NotScalingWithLuck(ItemID.NightOwlPotion, 1, 1, 3),
            ItemDropRule.NotScalingWithLuck(ItemID.ShinePotion, 1, 1, 3),
            ItemDropRule.NotScalingWithLuck(ItemID.HunterPotion, 1, 1, 3),
            ItemDropRule.NotScalingWithLuck(ItemID.GillsPotion, 1, 1, 3),
            ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 1, 1, 3),
            ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 1, 1, 3),
            ItemDropRule.NotScalingWithLuck(ItemID.TrapsightPotion, 1, 1, 3) // dangersense
		};
        IItemDropRule[] extraPotions = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.LesserHealingPotion, 1, 5, 15),
            ItemDropRule.NotScalingWithLuck(ItemID.LesserManaPotion, 1, 5, 15)
        };
        IItemDropRule[] extraBait = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.JourneymanBait, 3, 1, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.ApprenticeBait, 1, 1, 4)
        };

        IItemDropRule bc_surfaceLoot = ItemDropRule.OneFromOptionsNotScalingWithLuck(20, ItemID.Aglet, ItemID.ClimbingClaws, ItemID.PortableStool, ItemID.CordageGuide, ItemID.Radar);

        IItemDropRule[] woodenCrateDrop = new IItemDropRule[]
        {
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, themed),
            bc_surfaceLoot,
            ItemDropRule.SequentialRulesNotScalingWithLuck(7, coin),
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, new OneFromRulesRule(7, ores), new OneFromRulesRule(8, bars)),
            new OneFromRulesRule(7, potions),
        };
        IItemDropRule[] pearlwoodCrateDrop = new IItemDropRule[]
        {
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, hardmodeThemed),
            bc_surfaceLoot,
            ItemDropRule.SequentialRulesNotScalingWithLuck(7, coin),
            ItemDropRule.SequentialRulesNotScalingWithLuck(1,
                ItemDropRule.SequentialRulesNotScalingWithLuck(7,
                    new OneFromRulesRule(2, hardmodeOres),
                    new OneFromRulesRule(1, ores)
                ),
                ItemDropRule.SequentialRulesNotScalingWithLuck(8,
                    new OneFromRulesRule(2, hardmodeBars),
                    new OneFromRulesRule(1, bars)
                )
            ),
            new OneFromRulesRule(7, potions),
        };

        self.RegisterToItem(ItemID.WoodenCrate, ItemDropRule.AlwaysAtleastOneSuccess(woodenCrateDrop));
        self.RegisterToItem(ItemID.WoodenCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(pearlwoodCrateDrop));
        self.RegisterToMultipleItems(new OneFromRulesRule(3, extraPotions), ItemID.WoodenCrate, ItemID.WoodenCrateHard);
        self.RegisterToMultipleItems(ItemDropRule.SequentialRulesNotScalingWithLuck(3, extraBait), ItemID.WoodenCrate, ItemID.WoodenCrateHard);
        #endregion

        #region Iron Crate and Mythril Crate
        themed = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.GingerBeard, 25),
            ItemDropRule.NotScalingWithLuck(ItemID.TartarSauce, 20),
            ItemDropRule.NotScalingWithLuck(ItemID.FalconBlade, 15),
            ItemDropRule.NotScalingWithLuck(ItemID.SailfishBoots, 20),
            ItemDropRule.NotScalingWithLuck(ItemID.TsunamiInABottle, 20)
        };
        hardmodeThemed = new IItemDropRule[]
        {
            ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.Sundial, 60),
            ItemDropRule.NotScalingWithLuck(ItemID.GingerBeard, 25),
            ItemDropRule.NotScalingWithLuck(ItemID.TartarSauce, 20),
            ItemDropRule.NotScalingWithLuck(ItemID.FalconBlade, 15),
            ItemDropRule.NotScalingWithLuck(ItemID.SailfishBoots, 20),
            ItemDropRule.NotScalingWithLuck(ItemID.TsunamiInABottle, 20)
        };
        ores = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.CopperOre, 1, 12, 21),
            ItemDropRule.NotScalingWithLuck(ItemID.TinOre, 1, 12, 21),
            ItemDropRule.NotScalingWithLuck(ItemID.IronOre, 1, 12, 21),
            ItemDropRule.NotScalingWithLuck(ItemID.LeadOre, 1, 12, 21),
            ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 1, 12, 21),
            ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 1, 12, 21)
        };
        hardmodeOres = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.CobaltOre, 1, 12, 21),
            ItemDropRule.NotScalingWithLuck(ItemID.PalladiumOre, 1, 12, 21),
            ItemDropRule.NotScalingWithLuck(ItemID.MythrilOre, 1, 12, 21),
            ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumOre, 1, 12, 21)
        };
        bars = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.CopperBar, 1, 4, 7),
            ItemDropRule.NotScalingWithLuck(ItemID.TinBar, 1, 4, 7),
            ItemDropRule.NotScalingWithLuck(ItemID.IronBar, 1, 4, 7),
            ItemDropRule.NotScalingWithLuck(ItemID.LeadBar, 1, 4, 7),
            ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 1, 4, 7),
            ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 1, 4, 7)
        };
        hardmodeBars = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.CobaltBar, 1, 3, 7),
            ItemDropRule.NotScalingWithLuck(ItemID.PalladiumBar, 1, 3, 7),
            ItemDropRule.NotScalingWithLuck(ItemID.MythrilBar, 1, 3, 7),
            ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumBar, 1, 3, 7)
        };
        potions = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.SpelunkerPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.HunterPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.GravitationPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.CalmingPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.FlipperPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<DryadBloodPotion>(), 1, 2, 4)
        };
        extraPotions = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 1, 5, 15),
            ItemDropRule.NotScalingWithLuck(ItemID.ManaPotion, 1, 5, 15)
        };
        extraBait = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.MasterBait, 3, 2, 4),
            ItemDropRule.NotScalingWithLuck(ItemID.JourneymanBait, 1, 2, 4)
        };

        IItemDropRule[] ironCrate = new IItemDropRule[]
        {
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, themed),
            ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 4, 5, 10),
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, new OneFromRulesRule(6, ores), new OneFromRulesRule(4, bars)),
            new OneFromRulesRule(4, potions),
        };
        IItemDropRule[] mythrilCrate = new IItemDropRule[]
        {
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, hardmodeThemed),
            ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 4, 5, 10),
            ItemDropRule.SequentialRulesNotScalingWithLuck(1,
                ItemDropRule.SequentialRulesNotScalingWithLuck(6,
                    new OneFromRulesRule(2, hardmodeOres),
                    new OneFromRulesRule(1, ores)
                ),
                ItemDropRule.SequentialRulesNotScalingWithLuck(4,
                    new OneFromRulesRule(3, 2, hardmodeBars),
                    new OneFromRulesRule(1, bars)
                )
            ),
            new OneFromRulesRule(4, potions),
        };

        self.RegisterToItem(ItemID.IronCrate, ItemDropRule.AlwaysAtleastOneSuccess(ironCrate));
        self.RegisterToItem(ItemID.IronCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(mythrilCrate));
        self.RegisterToMultipleItems(new OneFromRulesRule(2, extraPotions), ItemID.IronCrate, ItemID.IronCrateHard);
        self.RegisterToMultipleItems(ItemDropRule.SequentialRulesNotScalingWithLuck(2, extraBait), ItemID.IronCrate, ItemID.IronCrateHard);
        #endregion

        #region Gold Crate and Titanium Crate
        themed = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.LifeCrystal, 8),
            ItemDropRule.NotScalingWithLuck(ItemID.HardySaddle, 10),
        };
        hardmodeThemed = new IItemDropRule[]
        {
            ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemID.Sundial, 20),
            ItemDropRule.NotScalingWithLuck(ItemID.LifeCrystal, 8),
            ItemDropRule.NotScalingWithLuck(ItemID.HardySaddle, 10),
        };
        ores = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.SilverOre, 1, 25, 34),
            ItemDropRule.NotScalingWithLuck(ItemID.TungstenOre, 1, 25, 34),
            ItemDropRule.NotScalingWithLuck(ItemID.GoldOre, 1, 25, 34),
            ItemDropRule.NotScalingWithLuck(ItemID.PlatinumOre, 1, 25, 34)
        };
        hardmodeOres = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.MythrilOre, 1, 25, 34),
            ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumOre, 1, 25, 34),
            ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteOre, 1, 25, 34),
            ItemDropRule.NotScalingWithLuck(ItemID.TitaniumOre, 1, 25, 34)
        };
        bars = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.SilverBar, 1, 8, 11),
            ItemDropRule.NotScalingWithLuck(ItemID.TungstenBar, 1, 8, 11),
            ItemDropRule.NotScalingWithLuck(ItemID.GoldBar, 1, 8, 11),
            ItemDropRule.NotScalingWithLuck(ItemID.PlatinumBar, 1, 8, 11)
        };
        hardmodeBars = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.MythrilBar, 1, 8, 11),
            ItemDropRule.NotScalingWithLuck(ItemID.OrichalcumBar, 1, 8, 11),
            ItemDropRule.NotScalingWithLuck(ItemID.AdamantiteBar, 1, 8, 11),
            ItemDropRule.NotScalingWithLuck(ItemID.TitaniumBar, 1, 8, 11)
        };
        potions = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.ObsidianSkinPotion, 1, 2, 5),
            ItemDropRule.NotScalingWithLuck(ItemID.SpelunkerPotion, 1, 2, 5),
            ItemDropRule.NotScalingWithLuck(ItemID.GravitationPotion, 1, 2, 5),
            ItemDropRule.NotScalingWithLuck(ItemID.MiningPotion, 1, 2, 5),
            ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 1, 2, 5),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<DeathWardPotion>(), 1, 2, 5)
        };
        extraPotions = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.HealingPotion, 1, 5, 20),
            ItemDropRule.NotScalingWithLuck(ItemID.ManaPotion, 1, 5, 20)
        };

        IItemDropRule[] goldCrate = new IItemDropRule[] {
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, themed),
            ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 3, 8, 20),
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, new OneFromRulesRule(5, ores), new OneFromRulesRule(3, 1, bars)),
            new OneFromRulesRule(3, potions),
            ItemDropRule.NotScalingWithLuck(ItemID.EnchantedSword, 30),
        };
        IItemDropRule[] titaniumCrate = new IItemDropRule[] {
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, hardmodeThemed),
            ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 3, 8, 20),
            ItemDropRule.SequentialRulesNotScalingWithLuck(1,
                ItemDropRule.SequentialRulesNotScalingWithLuck(5,
                    new OneFromRulesRule(2, hardmodeOres),
                    new OneFromRulesRule(1, ores)
                ),
                ItemDropRule.SequentialRulesNotScalingWithLuckWithNumerator(3, 1,
                    new OneFromRulesRule(3, 2, hardmodeBars),
                    new OneFromRulesRule(1, bars)
                )
            ),
            new OneFromRulesRule(3, potions),
            ItemDropRule.NotScalingWithLuck(ItemID.EnchantedSword, 15),
        };

        self.RegisterToItem(ItemID.GoldenCrate, ItemDropRule.AlwaysAtleastOneSuccess(goldCrate));
        self.RegisterToItem(ItemID.GoldenCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(titaniumCrate));
        self.RegisterToMultipleItems(new OneFromRulesRule(2, extraPotions), ItemID.GoldenCrate, ItemID.GoldenCrateHard);
        self.RegisterToMultipleItems(new CommonDrop(ItemID.MasterBait, 3, 3, 7, 2), ItemID.GoldenCrate, ItemID.GoldenCrateHard);
        #endregion

        #region Biome Crates
        #region Biome related
        IItemDropRule[] bc_jungle = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.FlowerBoots, 20),
            ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.AnkletoftheWind, ItemID.Boomstick, ItemID.FeralClaws, ItemID.StaffofRegrowth, ItemID.FiberglassFishingPole),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<HornetSkull>(), 20),
        };
        IItemDropRule bc_bamboo = ItemDropRule.NotScalingWithLuck(ItemID.BambooBlock, 3, 20, 50);
        IItemDropRule bc_seaweed = ItemDropRule.NotScalingWithLuck(ItemID.Seaweed, 20);

        IItemDropRule bc_sky = ItemDropRule.OneFromOptionsNotScalingWithLuck(1,
            ItemID.LuckyHorseshoe, ItemID.CelestialMagnet, ItemID.Starfury, ItemID.ShinyRedBalloon);
        IItemDropRule bc_cloud = ItemDropRule.NotScalingWithLuck(ItemID.Cloud, 2, 50, 100);
        IItemDropRule bc_fledgeWings = ItemDropRule.NotScalingWithLuck(ItemID.CreativeWings, 40, 1, 1);
        IItemDropRule bc_skyPaintings = ItemDropRule.OneFromOptionsNotScalingWithLuck(2, ItemID.HighPitch, ItemID.BlessingfromTheHeavens, ItemID.Constellation,
            ItemID.SeeTheWorldForWhatItIs, ItemID.LoveisintheTrashSlot, ItemID.SunOrnament); // Sun Ornament == Eye of The Sun

        IItemDropRule bc_son = ItemDropRule.NotScalingWithLuck(ItemID.SoulofNight, 2, 2, 5);
        IItemDropRule bc_corrupt = ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.BallOHurt, ItemID.BandofStarpower, ItemID.Musket, ItemID.ShadowOrb,
            /*ItemID.Vilethorn*/ModContent.ItemType<Vilethorn>(),
            ModContent.ItemType<Bookworms>(), ModContent.ItemType<PlanetomaStaff>());
        IItemDropRule bc_crimson = ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.TheUndertaker, ItemID.TheRottedFork,
            /*ItemID.CrimsonRod*/ModContent.ItemType<CrimsonRod>(),
            ItemID.PanicNecklace, ItemID.CrimsonHeart,
            ModContent.ItemType<ArterialSpray>(), ModContent.ItemType<GastroIntestinalMallet>());
        IItemDropRule bc_cursed = ItemDropRule.NotScalingWithLuck(ItemID.CursedFlame, 2, 2, 5);
        IItemDropRule bc_ichor = ItemDropRule.NotScalingWithLuck(ItemID.Ichor, 2, 2, 5);

        IItemDropRule bc_sol = ItemDropRule.NotScalingWithLuck(ItemID.SoulofLight, 2, 2, 5);
        IItemDropRule bc_shard = ItemDropRule.NotScalingWithLuck(ItemID.CrystalShard, 2, 4, 10);

        IItemDropRule bc_lockbox = ItemDropRule.Common(ItemID.LockBox);
        IItemDropRule bc_book = ItemDropRule.NotScalingWithLuck(ItemID.Book, 2, 5, 15);

        IItemDropRule ruleSnowballCannonIceBow = ItemDropRule.ByCondition(new Conditions.NotRemixSeed(), ItemID.SnowballCannon);
        ruleSnowballCannonIceBow.OnFailedConditions(ItemDropRule.NotScalingWithLuck(ItemID.IceBow), hideLootReport: true);

        IItemDropRule[] bc_iceList = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.IceBoomerang),
            ItemDropRule.NotScalingWithLuck(ItemID.IceBlade),
            ItemDropRule.NotScalingWithLuck(ItemID.IceSkates),
            ruleSnowballCannonIceBow,
            ItemDropRule.NotScalingWithLuck(ItemID.BlizzardinaBottle),
            ItemDropRule.NotScalingWithLuck(ItemID.FlurryBoots),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<SpikedIceStaff>()),
        };
        IItemDropRule bc_ice = new OneFromRulesRule(1, bc_iceList);

        IItemDropRule bc_fish = ItemDropRule.NotScalingWithLuck(ItemID.Fish, 20);

        IItemDropRule bc_scarab = ItemDropRule.OneFromOptionsNotScalingWithLuck(1,
            ItemID.AncientChisel, ItemID.ScarabFishingRod, ItemID.SandBoots, ItemID.ThunderSpear, ItemID.ThunderStaff, ItemID.CatBast, ItemID.MysticCoilSnake, ItemID.MagicConch,
            ModContent.ItemType<CactiCaster>());
        IItemDropRule bc_bomb = ItemDropRule.NotScalingWithLuck(ItemID.ScarabBomb, 4, 4, 6);
        IItemDropRule bc_fossil = ItemDropRule.NotScalingWithLuck(ItemID.FossilOre, 4, 10, 16); // sturdy fossil
        IItemDropRule bc_sandstormBottle = ItemDropRule.NotScalingWithLuck(ItemID.SandstorminaBottle, 35);

        IItemDropRule[] bc_lava = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.LavaCharm, 20),
            ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.FlameWakerBoots, ItemID.SuperheatedBlood, ItemID.LavaFishbowl, ItemID.LavaFishingHook, ItemID.VolcanoSmall),
        };
        IItemDropRule bc_pot = ItemDropRule.NotScalingWithLuck(ItemID.PotSuspended, 4, 2, 2);
        IItemDropRule bc_coolshirt = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<StrangerCoat>(), 30);
        IItemDropRule bc_daikatana = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<DullDaikatana>(), 30);
        IItemDropRule bc_gobletOnPain = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<GobletOfPain>(), 30);
        IItemDropRule bc_coolshirt2 = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<StrangerCoat>(), 15);
        IItemDropRule bc_daikatana2 = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<DullDaikatana>(), 15);
        IItemDropRule bc_gobletOnPain2 = ItemDropRule.NotScalingWithLuck(ModContent.ItemType<GobletOfPain>(), 15);
        IItemDropRule bc_obsi = ItemDropRule.Common(ItemID.ObsidianLockbox);
        IItemDropRule bc_wet = ItemDropRule.NotScalingWithLuck(ItemID.WetBomb, 3, 7, 10);
        IItemDropRule bc_plant = ItemDropRule.OneFromOptionsNotScalingWithLuck(2, ItemID.PottedLavaPlantPalm, ItemID.PottedLavaPlantBush, ItemID.PottedLavaPlantBramble, ItemID.PottedLavaPlantBulb, ItemID.PottedLavaPlantTendrils);
        IItemDropRule bc_ornate = ItemDropRule.NotScalingWithLuck(ItemID.OrnateShadowKey, 20);
        IItemDropRule bc_hellcart = ItemDropRule.NotScalingWithLuck(ItemID.HellMinecart, 20); // Demonic Hellcart
        IItemDropRule bc_cake = ItemDropRule.NotScalingWithLuck(ItemID.HellCake, 20);

        IItemDropRule[] bc_sea = new IItemDropRule[]
        {
            ItemDropRule.NotScalingWithLuck(ItemID.WaterWalkingBoots, 10),
            ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemID.BreathingReed, ItemID.FloatingTube, ItemID.Trident, ItemID.Flipper),
        };
        IItemDropRule bc_pile = ItemDropRule.NotScalingWithLuck(ItemID.ShellPileBlock, 3, 20, 50);
        IItemDropRule bc_sharkbait = ItemDropRule.NotScalingWithLuck(ItemID.SharkBait, 10);
        IItemDropRule bc_sand = ItemDropRule.NotScalingWithLuck(ItemID.SandcastleBucket, 10);
        #endregion

        #region Pseudo-global
        IItemDropRule bc_goldCoin = ItemDropRule.NotScalingWithLuck(ItemID.GoldCoin, 4, 5, 12);

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
        hardmodeOres = new IItemDropRule[]
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
        hardmodeBars = new IItemDropRule[]
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
            ItemDropRule.NotScalingWithLuck(ItemID.HeartreachPotion, 1, 2, 4),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<BloodlustPotion>(), 1, 2, 4)
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

        IItemDropRule[] jungle = new IItemDropRule[]
        {
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, bc_jungle),

            bc_goldCoin,
            new OneFromRulesRule(7, ores),
            new OneFromRulesRule(4, bars),
            new OneFromRulesRule(4, potions),

            bc_bamboo,
            bc_seaweed,
        };
        IItemDropRule[] bramble = new IItemDropRule[]
        {
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, bc_jungle),

            bc_goldCoin,
            hardmodeBiomeCrateOres,
            hardmodeBiomeCrateBars,
            new OneFromRulesRule(4, potions),

            bc_bamboo,
            bc_seaweed,
        };
        IItemDropRule[] sky = new IItemDropRule[]
        {
            bc_sky,
            bc_fledgeWings,
            bc_cloud,
            bc_skyPaintings,

            bc_goldCoin,
            new OneFromRulesRule(7, ores),
            new OneFromRulesRule(4, bars),
            new OneFromRulesRule(4, potions),
        };
        IItemDropRule[] azure = new IItemDropRule[]
        {
            bc_sky,
            bc_fledgeWings,
            bc_cloud,
            bc_skyPaintings,

            bc_goldCoin,
            hardmodeBiomeCrateOres,
            hardmodeBiomeCrateBars,
            new OneFromRulesRule(4, potions),
        };
        IItemDropRule[] corrupt = new IItemDropRule[] {
            bc_corrupt,

            bc_goldCoin,
            new OneFromRulesRule(7, ores),
            new OneFromRulesRule(4, bars),
            new OneFromRulesRule(4, potions),
        };
        IItemDropRule[] defiled = new IItemDropRule[] {
            bc_corrupt,

            bc_goldCoin,
            hardmodeBiomeCrateOres,
            hardmodeBiomeCrateBars,
            new OneFromRulesRule(4, potions),

            bc_son,
            bc_cursed,
        };
        IItemDropRule[] crimson = new IItemDropRule[] {
            bc_crimson,

            bc_goldCoin,
            new OneFromRulesRule(7, ores),
            new OneFromRulesRule(4, bars),
            new OneFromRulesRule(4, potions),
        };
        IItemDropRule[] hematic = new IItemDropRule[] {
            bc_crimson,

            bc_goldCoin,
            hardmodeBiomeCrateOres,
            hardmodeBiomeCrateBars,
            new OneFromRulesRule(4, potions),

            bc_son,
            bc_ichor,
        };
        IItemDropRule[] hallowed = new IItemDropRule[] {
            bc_goldCoin,
            new OneFromRulesRule(7, ores),
            new OneFromRulesRule(4, bars),
            new OneFromRulesRule(4, potions),
        };
        IItemDropRule[] divine = new IItemDropRule[] {
            bc_goldCoin,
            hardmodeBiomeCrateOres,
            hardmodeBiomeCrateBars,
            new OneFromRulesRule(4, potions),

            bc_sol,
            bc_shard,
        };
        IItemDropRule[] dungeon = new IItemDropRule[] {
            bc_lockbox,
            bc_book,

            bc_goldCoin,
            new OneFromRulesRule(7, ores),
            new OneFromRulesRule(4, bars),
            new OneFromRulesRule(4, potions),
        };
        IItemDropRule[] stockade = new IItemDropRule[] {
            bc_lockbox,
            bc_book,

            bc_goldCoin,
            hardmodeBiomeCrateOres,
            hardmodeBiomeCrateBars,
            new OneFromRulesRule(4, potions),
        };
        IItemDropRule[] frozen = new IItemDropRule[] {
            bc_ice,

            bc_goldCoin,
            new OneFromRulesRule(7, ores),
            new OneFromRulesRule(4, bars),
            new OneFromRulesRule(4, potions),

            bc_fish,
        };
        IItemDropRule[] boreal = new IItemDropRule[] {
            bc_ice,

            bc_goldCoin,
            hardmodeBiomeCrateOres,
            hardmodeBiomeCrateBars,
            new OneFromRulesRule(4, potions),

            bc_fish,
        };
        IItemDropRule[] oasis = new IItemDropRule[] {
            bc_scarab,
            bc_bomb,
            bc_sandstormBottle,

            bc_goldCoin,
            bc_fossil,
            new OneFromRulesRule(7, ores),
            new OneFromRulesRule(4, bars),
            new OneFromRulesRule(4, potions),
        };
        IItemDropRule[] mirage = new IItemDropRule[] {
            bc_scarab,
            bc_bomb,
            bc_sandstormBottle,

            bc_goldCoin,
            bc_fossil,
            hardmodeBiomeCrateOres,
            hardmodeBiomeCrateBars,
            new OneFromRulesRule(4, potions),
        };
        IItemDropRule[] obsidian = new IItemDropRule[] {
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, bc_lava),
            bc_coolshirt,
            bc_daikatana,
            bc_gobletOnPain,

            bc_pot,
            bc_obsi,
            bc_wet,
            bc_plant,
            bc_hellcart,

            bc_goldCoin,
            new OneFromRulesRule(7, ores),
            new OneFromRulesRule(4, bars),
            new OneFromRulesRule(4, potions),

            bc_ornate,
            bc_cake,
        };
        IItemDropRule[] hellstone = new IItemDropRule[] {
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, bc_lava),
            bc_coolshirt2,
            bc_daikatana2,
            bc_gobletOnPain2,

            bc_pot,
            bc_obsi,
            bc_wet,
            bc_plant,
            bc_hellcart,

            bc_goldCoin,
            hardmodeBiomeCrateOres,
            hardmodeBiomeCrateBars,
            new OneFromRulesRule(4, potions),

            bc_ornate,
            bc_cake,
        };
        IItemDropRule[] ocean = new IItemDropRule[] {
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, bc_sea),
            bc_sharkbait,

            bc_goldCoin,
            new OneFromRulesRule(7, ores),
            new OneFromRulesRule(4, bars),
            new OneFromRulesRule(4, potions),

            bc_pile,
            bc_sand,
        };
        IItemDropRule[] seaside = new IItemDropRule[] {
            ItemDropRule.SequentialRulesNotScalingWithLuck(1, bc_sea),
            bc_sharkbait,

            bc_goldCoin,
            hardmodeBiomeCrateOres,
            hardmodeBiomeCrateBars,
            new OneFromRulesRule(4, potions),

            bc_pile,
            bc_sand,
        };

        self.RegisterToItem(ItemID.JungleFishingCrate, ItemDropRule.AlwaysAtleastOneSuccess(jungle));
        self.RegisterToItem(ItemID.JungleFishingCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(bramble));
        self.RegisterToItem(ItemID.FloatingIslandFishingCrate, ItemDropRule.AlwaysAtleastOneSuccess(sky));
        self.RegisterToItem(ItemID.FloatingIslandFishingCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(azure));
        self.RegisterToItem(ItemID.CorruptFishingCrate, ItemDropRule.AlwaysAtleastOneSuccess(corrupt));
        self.RegisterToItem(ItemID.CorruptFishingCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(defiled));
        self.RegisterToItem(ItemID.CrimsonFishingCrate, ItemDropRule.AlwaysAtleastOneSuccess(crimson));
        self.RegisterToItem(ItemID.CrimsonFishingCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(hematic));
        self.RegisterToItem(ItemID.HallowedFishingCrate, ItemDropRule.AlwaysAtleastOneSuccess(hallowed));
        self.RegisterToItem(ItemID.HallowedFishingCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(divine));
        self.RegisterToItem(ItemID.DungeonFishingCrate, ItemDropRule.AlwaysAtleastOneSuccess(dungeon));
        self.RegisterToItem(ItemID.DungeonFishingCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(stockade));
        self.RegisterToItem(ItemID.FrozenCrate, ItemDropRule.AlwaysAtleastOneSuccess(frozen));
        self.RegisterToItem(ItemID.FrozenCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(boreal));
        self.RegisterToItem(ItemID.OasisCrate, ItemDropRule.AlwaysAtleastOneSuccess(oasis));
        self.RegisterToItem(ItemID.OasisCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(mirage));
        self.RegisterToItem(ItemID.LavaCrate, ItemDropRule.AlwaysAtleastOneSuccess(obsidian));
        self.RegisterToItem(ItemID.LavaCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(hellstone));
        self.RegisterToItem(ItemID.OceanCrate, ItemDropRule.AlwaysAtleastOneSuccess(ocean));
        self.RegisterToItem(ItemID.OceanCrateHard, ItemDropRule.AlwaysAtleastOneSuccess(seaside));

        int[] allCrates = new int[]
        {
            ItemID.JungleFishingCrate, ItemID.JungleFishingCrateHard,
            ItemID.FloatingIslandFishingCrate, ItemID.FloatingIslandFishingCrateHard,
            ItemID.CorruptFishingCrate, ItemID.CorruptFishingCrateHard,
            ItemID.CrimsonFishingCrate, ItemID.CrimsonFishingCrateHard,
            ItemID.HallowedFishingCrate, ItemID.HallowedFishingCrateHard,
            ItemID.DungeonFishingCrate, ItemID.DungeonFishingCrateHard,
            ItemID.FrozenCrate, ItemID.FrozenCrateHard,
            ItemID.OasisCrate, ItemID.OasisCrateHard,
            ItemID.LavaCrate, ItemID.LavaCrateHard,
            ItemID.OceanCrate, ItemID.OceanCrateHard,
        };
        self.RegisterToMultipleItems(new OneFromRulesRule(2, extraPotions), allCrates);
        self.RegisterToMultipleItems(ItemDropRule.SequentialRulesNotScalingWithLuck(2, extraBait), allCrates);
        #endregion
    }
}
