using RoA.Content.Items.Consumables;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Armor.Magic;
using RoA.Content.Items.Equipables.Armor.Nature;
using RoA.Content.Items.Equipables.Armor.Ranged;
using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Content.Items.Equipables.Vanity;
using RoA.Content.Items.Materials;
using RoA.Content.Items.Miscellaneous;
using RoA.Content.Items.Placeable;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.Items.Placeable.Furniture;
using RoA.Content.Items.Potions;
using RoA.Content.Items.Special;
using RoA.Content.Items.Tools;
using RoA.Content.Items.Weapons.Druidic.Rods;
using RoA.Content.Items.Weapons.Magic;
using RoA.Content.Items.Weapons.Melee;
using RoA.Content.Items.Weapons.Ranged;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Recipes;

sealed class RoARecipes : ModSystem {
    public override void AddRecipes() {
        AddElderwoodItems();
        AddDynastyWoodItems();
        AddHellstoneItems(out Recipe starFusion);
        AddMercuriumItems(starFusion);
        AddFlamingFabricItems();
        AddGalipotItems();
        AddHerbItems();
        AddPlanterBoxes();
    }

    private static void AddPlanterBoxes() {
        void addRecipe(short itemType) {
            Recipe result = Recipe.Create(itemType);
            result.AddIngredient(ModContent.ItemType<EmptyPlanterBox>(), 1);
            result.SortBeforeFirstRecipesOf(ItemID.PotSuspendedDaybloom);
            result.Register();
        }
        short[] planterBoxes = [ItemID.BlinkrootPlanterBox, ItemID.CorruptPlanterBox, ItemID.CrimsonPlanterBox, ItemID.DayBloomPlanterBox,
                                ItemID.FireBlossomPlanterBox, ItemID.MoonglowPlanterBox, ItemID.ShiverthornPlanterBox, ItemID.WaterleafPlanterBox];
        foreach (short planterBox in planterBoxes) {
            addRecipe(planterBox);
        }
        for (int i = ItemID.Count; i < ItemLoader.ItemCount; i++) {
            ModItem item = ItemLoader.GetItem(i);
            if (item.Type == ModContent.ItemType<EmptyPlanterBox>()) {
                continue;
            }
            if (item.GetType().ToString().Contains("PlanterBox")) {
                addRecipe((short)item.Type);
            }
        }
    }

    private static void AddHerbItems() {
        Recipe item = Recipe.Create(ModContent.ItemType<Herbarium>());
        item.AddIngredient(ItemID.Book);
        item.AddIngredient(ItemID.Daybloom);
        item.AddIngredient(ItemID.Shiverthorn);
        item.AddIngredient(ItemID.Blinkroot);
        item.AddIngredient(ItemID.Waterleaf);
        item.AddIngredient(ItemID.Deathweed);
        item.AddIngredient(ItemID.Moonglow);
        item.AddIngredient(ModContent.ItemType<Content.Items.Materials.MiracleMint>());
        item.AddIngredient(ModContent.ItemType<Content.Items.Materials.Bonerose>());
        item.AddIngredient(ItemID.Fireblossom);
        item.AddTile(TileID.Bookcases);
        item.SortAfterFirstRecipesOf(ItemID.GarlandHat);
        item.Register();

        // tiles
        item = Recipe.Create(ModContent.ItemType<MiracleMintHangingPot>());
        item.AddIngredient(ItemID.PotSuspended);
        item.AddIngredient(ModContent.ItemType<Content.Items.Materials.MiracleMint>());
        item.SortAfterFirstRecipesOf(ItemID.PotSuspendedFireblossom);
        item.Register();

        // throwable potions
        item = Recipe.Create(ModContent.ItemType<WeaknessPotion>(), 3);
        item.AddIngredient(ItemID.BottledWater, 3);
        item.AddIngredient<Content.Items.Materials.Bonerose>();
        item.AddIngredient<Content.Items.Materials.MiracleMint>();
        item.AddIngredient(ItemID.RottenChunk);
        item.AddTile(TileID.Bottles);
        item.SortAfterFirstRecipesOf(ItemID.StinkPotion);
        item.Register();

        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<WeaknessPotion>(), 3);
        item.AddIngredient(ItemID.BottledWater, 3);
        item.AddIngredient<Content.Items.Materials.Bonerose>();
        item.AddIngredient<Content.Items.Materials.MiracleMint>();
        item.AddIngredient(ItemID.Vertebrae);
        item.AddTile(TileID.Bottles);
        item.SortAfter(temp);
        item.Register();

        // other potions
        item = Recipe.Create(ModContent.ItemType<BloodlustPotion>());
        item.AddIngredient(ItemID.BottledWater);
        item.AddIngredient(ItemID.Deathweed);
        item.AddIngredient<Content.Items.Materials.Bonerose>();
        item.AddIngredient(ItemID.Vine);
        item.AddTile(TileID.Bottles);
        item.SortAfterFirstRecipesOf(ItemID.LifeforcePotion);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<DeathWardPotion>());
        item.AddIngredient(ItemID.BottledWater);
        item.AddIngredient<Content.Items.Materials.MiracleMint>();
        item.AddIngredient(ItemID.Fireblossom);
        item.AddIngredient(ItemID.Worm);
        item.AddTile(TileID.Bottles);
        item.SortAfter(temp);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<DryadBloodPotion>());
        item.AddIngredient(ItemID.BottledWater);
        item.AddIngredient<Content.Items.Materials.MiracleMint>();
        item.AddIngredient(ItemID.Waterleaf);
        item.AddIngredient<Content.Items.Materials.Galipot>();
        item.AddTile(TileID.Bottles);
        item.SortAfterFirstRecipesOf(ItemID.WarmthPotion);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<PrismaticPotion>());
        item.AddIngredient(ItemID.BottledWater);
        item.AddIngredient(ItemID.Prismite);
        item.AddIngredient(ItemID.ButterflyDust);
        item.AddIngredient(ItemID.Deathweed);
        item.AddIngredient<Content.Items.Materials.MiracleMint>();
        item.AddTile(TileID.Bottles);
        item.SortAfterFirstRecipesOf(ItemID.WrathPotion);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<WeightPotion>());
        item.AddIngredient(ItemID.BottledWater);
        item.AddIngredient(ItemID.Daybloom);
        item.AddIngredient(ItemID.SiltBlock);
        item.AddTile(TileID.Bottles);
        item.SortAfterFirstRecipesOf(ItemID.FeatherfallPotion);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<ResiliencePotion>());
        item.AddIngredient(ItemID.BottledWater);
        item.AddIngredient<Content.Items.Materials.MiracleMint>();
        item.AddIngredient(ItemID.Moonglow);
        item.AddTile(TileID.Bottles);
        item.SortAfterFirstRecipesOf(ItemID.MagicPowerPotion);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<WillpowerPotion>());
        item.AddIngredient(ItemID.BottledWater);
        item.AddIngredient(ItemID.Blinkroot);
        item.AddIngredient<Content.Items.Materials.Bonerose>();
        item.AddIngredient<Content.Items.Materials.Galipot>();
        item.AddTile(TileID.Bottles);
        item.SortAfter(temp);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<BrightstonePotion>());
        item.AddIngredient(ItemID.ShinePotion);
        item.AddIngredient<Content.Items.Materials.Bonerose>();
        item.AddTile(TileID.Bottles);
        item.SortAfterFirstRecipesOf(ItemID.ShinePotion);
        item.Register();
    }

    private static void AddGalipotItems() {
        // slippery items
        Recipe item = Recipe.Create(ModContent.ItemType<SlipperyGrenade>(), 2);
        item.AddIngredient(ItemID.Grenade, 2);
        item.AddIngredient<Content.Items.Materials.Galipot>(1);
        item.SortAfterFirstRecipesOf(ItemID.WoodYoyo);
        item.Register();

        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<SlipperyBomb>());
        item.AddIngredient(ItemID.Bomb);
        item.AddIngredient<Content.Items.Materials.Galipot>(1);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<SlipperyDynamite>());
        item.AddIngredient(ItemID.Dynamite);
        item.AddIngredient<Content.Items.Materials.Galipot>(1);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<SlipperyGlowstick>(), 5);
        item.AddIngredient(ItemID.Glowstick, 5);
        item.AddIngredient<Content.Items.Materials.Galipot>(1);
        item.SortAfter(temp);
        item.Register();
    }

    private static void AddFlamingFabricItems() {
        // armor
        Recipe item = Recipe.Create(ModContent.ItemType<AshwalkerHood>());
        item.AddIngredient<Content.Items.Materials.FlamingFabric>(15);
        item.AddTile(TileID.Loom);
        item.SortAfterFirstRecipesOf(ItemID.FireproofBugNet);
        item.Register();

        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<AshwalkerRobe>());
        item.AddIngredient<Content.Items.Materials.FlamingFabric>(25);
        item.AddTile(TileID.Loom);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<AshwalkerLeggings>());
        item.AddIngredient<Content.Items.Materials.FlamingFabric>(20);
        item.AddTile(TileID.Loom);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<FlametrackerHat>());
        item.AddIngredient<Content.Items.Materials.FlamingFabric>(12);
        item.AddIngredient(ItemID.Leather, 10);
        item.AddTile(TileID.Loom);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<FlametrackerJacket>());
        item.AddIngredient<Content.Items.Materials.FlamingFabric>(20);
        item.AddIngredient(ItemID.Leather, 15);
        item.AddTile(TileID.Loom);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<FlametrackerPants>());
        item.AddIngredient<Content.Items.Materials.FlamingFabric>(15);
        item.AddIngredient(ItemID.Leather, 10);
        item.AddTile(TileID.Loom);
        item.SortAfter(temp);
        item.Register();

        // vanity
        temp = item;
        item = Recipe.Create(ModContent.ItemType<DevilHunterCloak>());
        item.AddIngredient<Content.Items.Materials.FlamingFabric>(15);
        item.AddIngredient(ItemID.RedDye, 1);
        item.AddTile(TileID.Loom);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<DevilHunterPants>());
        item.AddIngredient<Content.Items.Materials.FlamingFabric>(10);
        item.AddIngredient(ItemID.RedDye, 1);
        item.AddTile(TileID.Loom);
        item.SortAfter(temp);
        item.Register();

        // accessories
        temp = item;
        item = Recipe.Create(ModContent.ItemType<CosmicHat>());
        item.AddIngredient(ItemID.WizardHat, 1);
        item.AddIngredient<Content.Items.Materials.FlamingFabric>(3);
        item.AddIngredient(ItemID.ManaCrystal, 1);
        item.AddTile(TileID.Loom);
        item.SortAfter(temp);
        item.Register();
    }

    private static void AddHellstoneItems(out Recipe starFusion) {
        Recipe item = Recipe.Create(ModContent.ItemType<StarFusion>());
        item.AddIngredient(ItemID.Starfury, 1);
        item.AddIngredient(ItemID.HellstoneBar, 10);
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(10);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.FireproofBugNet);
        item.Register();
        starFusion = item;

        item = Recipe.Create(ModContent.ItemType<TectonicCane>());
        item.AddIngredient(ItemID.HellstoneBar, 6);
        item.AddIngredient<Content.Items.Materials.FlamingFabric>(10);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.ImpStaff);
        item.Register();
    }

    private static void AddMercuriumItems(Recipe starFusion) {
        Recipe item = Recipe.Create(ModContent.ItemType<MercuriumBolter>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(8);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.Magiluminescence);
        item.Register();

        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<MercuriumPickaxe>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(18);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<MercuriumAxe>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(14);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<MercuriumHammer>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(14);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<MercuriumSword>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(10);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<MercuriumStaff>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(10);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<SentinelHelmet>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(10);
        item.AddIngredient(ItemID.Leather, 8);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<SentinelBreastplate>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(10);
        item.AddIngredient(ItemID.Leather, 12);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<SentinelLeggings>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(8);
        item.AddIngredient(ItemID.Leather, 8);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<Beacon>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(10);
        item.AddIngredient(ItemID.Lens, 1);
        item.AddIngredient(ItemID.Wire, 5);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        // staves
        temp = starFusion;
        item = Recipe.Create(ModContent.ItemType<RodOfTheDragonfire>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(15);
        item.AddIngredient(ItemID.GoldBar, 10);
        item.AddIngredient<SphereOfPyre>(1);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<RodOfTheDragonfire>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(15);
        item.AddIngredient(ItemID.PlatinumBar, 10);
        item.AddIngredient<SphereOfPyre>(1);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<RodOfTheShock>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(15);
        item.AddIngredient(ItemID.GoldBar, 10);
        item.AddIngredient<SphereOfShock>(1);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<RodOfTheShock>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(15);
        item.AddIngredient(ItemID.PlatinumBar, 10);
        item.AddIngredient<SphereOfShock>(1);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<RodOfTheStream>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(15);
        item.AddIngredient(ItemID.GoldBar, 10);
        item.AddIngredient<SphereOfStream>(1);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<RodOfTheStream>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(15);
        item.AddIngredient(ItemID.PlatinumBar, 10);
        item.AddIngredient<SphereOfStream>(1);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<RodOfTheTerra>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(15);
        item.AddIngredient(ItemID.GoldBar, 10);
        item.AddIngredient<SphereOfQuake>(1);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<RodOfTheTerra>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(15);
        item.AddIngredient(ItemID.PlatinumBar, 10);
        item.AddIngredient<SphereOfQuake>(1);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<RodOfTheCondor>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(15);
        item.AddIngredient(ItemID.GoldBar, 10);
        item.AddIngredient<SphereOfCondor>(1);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<RodOfTheCondor>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(15);
        item.AddIngredient(ItemID.PlatinumBar, 10);
        item.AddIngredient<SphereOfCondor>(1);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();
    }

    private static void AddDynastyWoodItems() {
        Recipe item = Recipe.Create(ModContent.ItemType<DynastyWoodHelmet>());
        item.AddIngredient(ItemID.DynastyWood, 20);
        item.AddTile(TileID.WorkBenches);
        item.SortAfterFirstRecipesOf(ItemID.DynastyCup);
        item.Register();

        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<DynastyWoodBreastplate>());
        item.AddIngredient(ItemID.DynastyWood, 30);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<DynastyWoodLeggings>());
        item.AddIngredient(ItemID.DynastyWood, 25);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<DynastyWoodSword>());
        item.AddIngredient(ItemID.DynastyWood, 7);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<DynastyWoodHammer>());
        item.AddIngredient(ItemID.DynastyWood, 8);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<DynastyWoodBow>());
        item.AddIngredient(ItemID.DynastyWood, 10);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();
    }

    private static void AddElderwoodItems() {
        Recipe item = Recipe.Create(ModContent.ItemType<ElderwoodHelmet>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(20);
        item.AddTile(TileID.WorkBenches);
        item.SortAfterFirstRecipesOf(ItemID.AshWoodToilet);
        item.Register();

        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodChestplate>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(30);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodLeggings>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(25);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodSword>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(7);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodHammer>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(8);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodBow>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(10);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        // furniture
        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodBathtub>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(14);
        item.AddTile(TileID.Sawmill);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodBed>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(15);
        item.AddIngredient(ItemID.Silk, 5);
        item.AddTile(TileID.Sawmill);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodBookcase>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(20);
        item.AddIngredient(ItemID.Book, 10);
        item.AddTile(TileID.Sawmill);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodDresser>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(16);
        item.AddTile(TileID.Sawmill);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodCandelabra>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(5);
        item.AddIngredient(ItemID.Torch, 3);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodCandle>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(4);
        item.AddIngredient(ItemID.Torch, 1);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodChair>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(4);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodChandelier>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(4);
        item.AddIngredient(ItemID.Torch, 4);
        item.AddIngredient(ItemID.Chain, 1);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodChest>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(8);
        item.AddRecipeGroup(RecipeGroupID.IronBar, 2);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodClock>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(10);
        item.AddRecipeGroup(RecipeGroupID.IronBar, 3);
        item.AddIngredient(ItemID.Glass, 6);
        item.AddTile(TileID.Sawmill);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodDoor>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(6);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodLamp>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(3);
        item.AddIngredient(ItemID.Torch, 1);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodLantern>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(6);
        item.AddIngredient(ItemID.Torch, 1);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodPiano>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(15);
        item.AddIngredient(ItemID.Bone, 4);
        item.AddIngredient(ItemID.Book, 1);
        item.AddTile(TileID.Sawmill);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodSink>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(6);
        item.AddIngredient(ItemID.WaterBucket, 1);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodSofa>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(5);
        item.AddIngredient(ItemID.Silk, 2);
        item.AddTile(TileID.Sawmill);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodTable>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(8);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodWorkbench>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(10);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodToilet>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(6);
        item.AddTile(TileID.Sawmill);
        item.SortAfter(temp);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<ElderwoodPlatform>(), 2);
        item.AddIngredient(ModContent.ItemType<Elderwood>(), 1);
        item.SortAfterFirstRecipesOf(ItemID.AshWoodPlatform);
        item.Register();
    }
}
