using RoA.Content.Achievements;
using RoA.Content.Items.Consumables;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Armor.Magic;
using RoA.Content.Items.Equipables.Armor.Nature;
using RoA.Content.Items.Equipables.Armor.Ranged;
using RoA.Content.Items.Equipables.Armor.Summon;
using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Content.Items.Equipables.Vanity;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Content.Items.Food;
using RoA.Content.Items.Materials;
using RoA.Content.Items.Miscellaneous;
using RoA.Content.Items.Placeable;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.Items.Placeable.Decorations;
using RoA.Content.Items.Placeable.Furniture;
using RoA.Content.Items.Placeable.Walls;
using RoA.Content.Items.Potions;
using RoA.Content.Items.Special;
using RoA.Content.Items.Tools;
using RoA.Content.Items.Weapons.Druidic;
using RoA.Content.Items.Weapons.Druidic.Claws;
using RoA.Content.Items.Weapons.Druidic.Rods;
using RoA.Content.Items.Weapons.Magic;
using RoA.Content.Items.Weapons.Melee;
using RoA.Content.Items.Weapons.Ranged;
using RoA.Content.Items.Weapons.Ranged.Ammo;
using RoA.Content.Items.Weapons.Summon;
using RoA.Content.Tiles.Station;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Recipes;

sealed class RoARecipes : ModSystem {
    public override void AddRecipes() {
        AddElderwoodItems();
        AddDynastyWoodItems();
        AddHellstoneItems(out Recipe daikatana);
        AddMercuriumItems(daikatana);
        AddFlamingFabricItems();
        AddGalipotItems();
        AddHerbItems();
        AddPlanterBoxes();
        AddOtherSawmill();
        AddWreaths(out Recipe lastWreath);
        AddDruidItems(lastWreath);
        AddCopperSets();
        AddBoneHarpySet();
        AddCorruptionRelatedStuff();
        AddCrimsonRelatedStuff();
        AddTinkererWorkShopItems();
        AddFood();
        AddTerrarium();
        AddIceBiomeItems();
        AddTrappedChests();
        AddTorch();
        AddWalls();
        AddCampfire();
        AddOther();
    }

    private static void AddWalls() {
        Recipe item = Recipe.Create(ModContent.ItemType<ElderwoodFence>(), 4);
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>();
        item.AddTile(TileID.WorkBenches);
        item.SortAfterFirstRecipesOf(ItemID.AshWoodFence);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<ElderwoodWall>(), 4);
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>();
        item.AddTile(TileID.WorkBenches);
        item.SortAfterFirstRecipesOf(ItemID.AshWoodWall);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<GrimstoneBrickWall>(), 4);
        item.AddIngredient<Content.Items.Placeable.Crafting.GrimstoneBrick>();
        item.AddTile(TileID.WorkBenches);
        item.SortAfterFirstRecipesOf(ItemID.CrimtaneBrickWall);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<GrimstoneWall>(), 4);
        item.AddIngredient<Content.Items.Placeable.Crafting.Grimstone>();
        item.AddTile(TileID.WorkBenches);
        item.SortAfterFirstRecipesOf(ItemID.StoneWall);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<LivingBackwoodsLeavesWall>(), 4);
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>();
        item.AddTile(TileID.LivingLoom);
        item.SortAfterFirstRecipesOf(ItemID.LivingLeafWall);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<TealMossWall>(), 4);
        item.AddIngredient<TealMoss>();
        item.AddTile(TileID.WorkBenches);
        item.AddCondition(Condition.InGraveyard);
        item.SortAfterFirstRecipesOf(4500);
        item.Register();
        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<TealMoss>(), 1);
        item.AddIngredient<TealMossWall>(4);
        item.AddTile(TileID.WorkBenches);
        item.SortAfterFirstRecipesOf(ItemID.RedMoss);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<BackwoodsRootWall>(), 4);
        item.AddIngredient<Elderwood>();
        item.AddTile(TileID.WorkBenches);
        item.AddCondition(Condition.InGraveyard);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<Elderwood>(), 1);
        item.AddIngredient<BackwoodsRootWall>(4);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();
    }

    private static void AddOther() {
        Recipe item = Recipe.Create(ModContent.ItemType<Grimstone>());
        item.AddIngredient(ItemID.StoneBlock);
        item.AddCondition(Condition.NearWater);
        Recipe mudBlock = Main.recipe.FirstOrDefault(x => x.createItem.type == ItemID.MudBlock && x.requiredItem.Any(x2 => x2.type == ItemID.DirtBlock));
        item.SortAfter(mudBlock);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<GrimstoneBrick>());
        item.AddIngredient<Grimstone>(2);
        item.AddTile(TileID.Furnaces);
        item.SortAfterFirstRecipesOf(ItemID.CrimtaneBrick);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<BackwoodsStoneChest>());
        item.AddIngredient<GrimstoneBrick>(8);
        item.AddRecipeGroup(RecipeGroupID.IronBar, 2);
        item.AddTile(TileID.WorkBenches);
        item.SortAfterFirstRecipesOf(ItemID.DesertChest);
        item.Register();

        // accessories
        item = Recipe.Create(ModContent.ItemType<RoyalQualityHoney>());
        item.AddIngredient(ItemID.BottledHoney, 1);
        item.AddIngredient(ItemID.LifeCrystal, 1);
        item.AddIngredient(ItemID.BeeWax, 8);
        item.AddTile(TileID.Furnaces);
        item.SortAfterFirstRecipesOf(ItemID.HornetStaff);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<JewellersBelt>());
        item.AddIngredient(ItemID.Leather, 8);
        item.AddIngredient(ItemID.Chain, 2);
        item.AddTile(TileID.WorkBenches);
        item.SortBeforeFirstRecipesOf(ItemID.HeartLantern);
        item.Register();
    }

    private static void AddCampfire() {
        Recipe item = Recipe.Create(ModContent.ItemType<BackwoodsCampfire>());
        item.AddRecipeGroup(RecipeGroupID.Wood, 10);
        item.AddIngredient<ElderTorch>(5);
        item.SortAfterFirstRecipesOf(ItemID.JungleCampfire);
        item.Register();
    }

    private static void AddTorch() {
        Recipe item = Recipe.Create(ModContent.ItemType<ElderTorch>());
        item.AddIngredient(ItemID.Torch, 3);
        item.AddIngredient<Grimstone>();
        item.SortAfterFirstRecipesOf(ItemID.JungleTorch);
        item.Register();
    }

    private static void AddTrappedChests() {
        Recipe item = Recipe.Create(ModContent.ItemType<BackwoodsDungeonChest_Trapped>());
        item.AddIngredient<BackwoodsDungeonChest>(1);
        item.AddIngredient(ItemID.Wire, 10);
        item.AddTile(TileID.HeavyWorkBench);
        item.SortAfterFirstRecipesOf(ItemID.Fake_DungeonDesertChest);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<BackwoodsStoneChest_Trapped>());
        item.AddIngredient<BackwoodsStoneChest>(1);
        item.AddIngredient(ItemID.Wire, 10);
        item.AddTile(TileID.HeavyWorkBench);
        item.SortAfterFirstRecipesOf(ItemID.Fake_DesertChest);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<ElderwoodChest_Trapped>());
        item.AddIngredient<ElderwoodChest>(1);
        item.AddIngredient(ItemID.Wire, 10);
        item.AddTile(TileID.HeavyWorkBench);
        item.SortAfterFirstRecipesOf(ItemID.Fake_AshWoodChest);
        item.Register();
        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodChest2_Trapped>());
        item.AddIngredient<ElderwoodChest2>(1);
        item.AddIngredient(ItemID.Wire, 10);
        item.AddTile(TileID.HeavyWorkBench);
        item.SortAfter(temp);
        item.Register();
    }

    private static void AddIceBiomeItems() {
        Recipe item = Recipe.Create(ModContent.ItemType<FlinxFurUshanka>());
        item.AddIngredient(ItemID.Silk, 4);
        item.AddIngredient(ItemID.FlinxFur, 4);
        item.AddTile(TileID.Loom);
        item.SortBeforeFirstRecipesOf(ItemID.FlinxFurCoat);
        item.Register();
    }

    private static void AddTerrarium() {
        Recipe item = Recipe.Create(ModContent.ItemType<HedgehogTerrarium>());
        item.AddIngredient(ItemID.Terrarium);
        item.AddIngredient<Hedgehog>(1);
        item.SortAfterFirstRecipesOf(ItemID.OwlCage);
        item.Register();
    }

    private static void AddFood() {
        Recipe item = Recipe.Create(ModContent.ItemType<AlmondMilk>());
        item.AddIngredient<Almond>(1);
        item.AddIngredient(ItemID.Bottle);
        item.AddTile(TileID.CookingPots);
        item.SortAfterFirstRecipesOf(ItemID.PeachSangria);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<SherwoodShake>());
        item.AddIngredient<Pistachio>(1);
        item.AddIngredient(ItemID.Cherry);
        item.AddIngredient(ItemID.Bottle);
        item.AddTile(TileID.CookingPots);
        item.SortAfterFirstRecipesOf(ItemID.SmoothieofDarkness);
        item.Register();
    }

    private static void CompleteAchievement(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack) {
        ModContent.GetInstance<CraftDruidWreath>().CraftAnyWreathCondition.Complete();
        //RoA.CompleteAchievement("CraftDruidWreath");
        //Main.LocalPlayer.GetModPlayer<RoAAchievementInGameNotification.RoAAchievementStorage_Player>().CraftDruidWreath = true;
    }

    private static void AddWreaths(out Recipe lastWreath) {
        Recipe item = Recipe.Create(ModContent.ItemType<ForestWreath>());
        item.AddIngredient<TwigWreath>(1);
        item.AddIngredient(ItemID.Daybloom, 5);
        item.SortBeforeFirstRecipesOf(ItemID.IceTorch);
        item.AddOnCraftCallback(CompleteAchievement);
        item.Register();
        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<ForestWreath2>());
        item.AddIngredient<ForestWreath>(1);
        item.AddIngredient(ItemID.Sunflower, 1);
        item.AddIngredient<NaturesHeart>(1);
        item.AddTile<Content.Tiles.Ambient.OvergrownAltar>();
        item.SortAfter(temp);
        item.AddOnCraftCallback(CompleteAchievement);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<JungleWreath>());
        item.AddIngredient<TwigWreath>(1);
        item.AddIngredient(ItemID.Moonglow, 5);
        item.SortAfter(temp);
        item.Register();
        item.AddOnCraftCallback(CompleteAchievement);
        temp = item;
        item = Recipe.Create(ModContent.ItemType<JungleWreath2>());
        item.AddIngredient<JungleWreath>(1);
        item.AddIngredient(ItemID.JungleRose, 1);
        item.AddIngredient<NaturesHeart>(1);
        item.AddTile<Content.Tiles.Ambient.OvergrownAltar>();
        item.SortAfter(temp);
        item.AddOnCraftCallback(CompleteAchievement);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<BeachWreath>());
        item.AddIngredient<TwigWreath>(1);
        item.AddIngredient(ItemID.Waterleaf, 5);
        item.SortAfter(temp);
        item.AddOnCraftCallback(CompleteAchievement);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<BeachWreath2>());
        item.AddIngredient<BeachWreath>(1);
        item.AddIngredient(ItemID.Coral, 1);
        item.AddIngredient<NaturesHeart>(1);
        item.AddTile<Content.Tiles.Ambient.OvergrownAltar>();
        item.SortAfter(temp);
        item.AddOnCraftCallback(CompleteAchievement);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<SnowWreath>());
        item.AddIngredient<TwigWreath>(1);
        item.AddIngredient(ItemID.Shiverthorn, 5);
        item.SortAfter(temp);
        item.AddOnCraftCallback(CompleteAchievement);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<SnowWreath2>());
        item.AddIngredient<SnowWreath>(1);
        item.AddIngredient<Cloudberry>(1);
        item.AddIngredient<NaturesHeart>(1);
        item.AddTile<Content.Tiles.Ambient.OvergrownAltar>();
        item.SortAfter(temp);
        item.AddOnCraftCallback(CompleteAchievement);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<FenethsBlazingWreath>());
        item.AddIngredient<TwigWreath>(1);
        item.AddIngredient(ItemID.Fireblossom, 10);
        item.AddTile<FenethStatue>();
        item.SortAfter(temp);
        item.AddOnCraftCallback(CompleteAchievement);
        item.Register();
        lastWreath = item;
    }

    private static void AddTinkererWorkShopItems() {
        Recipe item = Recipe.Create(ModContent.ItemType<AlchemicalSkull>());
        item.AddIngredient(ItemID.ObsidianSkull);
        item.AddIngredient(ItemID.Bezoar);
        item.AddIngredient<NaturesHeart>(1);
        item.AddTile(TileID.TinkerersWorkbench);
        item.SortAfterFirstRecipesOf(ItemID.ObsidianShield);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<FeathersInABalloon>());
        item.AddIngredient<FeathersInABottle>(1);
        item.AddIngredient(ItemID.ShinyRedBalloon);
        item.AddTile(TileID.TinkerersWorkbench);
        item.SortAfterFirstRecipesOf(ItemID.FartInABalloon);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<BandOfPurity>());
        item.AddIngredient<BandOfNature>(1);
        item.AddIngredient(ItemID.BandofRegeneration);
        item.AddTile(TileID.TinkerersWorkbench);
        item.SortAfterFirstRecipesOf(ItemID.ManaRegenerationBand);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<FeathersInABottle>());
        item.AddIngredient(ItemID.CloudinaBottle);
        item.AddIngredient(ItemID.Feather, 5);
        item.AddTile(TileID.TinkerersWorkbench);
        item.SortAfterFirstRecipesOf(ItemID.FartinaJar);
        item.Register();
    }

    private static void AddCrimsonRelatedStuff() {
        Recipe item = Recipe.Create(ModContent.ItemType<DreadheartCrimsonHelmet>());
        item.AddIngredient(ItemID.TissueSample, 10);
        item.AddIngredient<NaturesHeart>();
        item.AddTile<Content.Tiles.Ambient.OvergrownAltar>();
        item.SortAfterFirstRecipesOf(ItemID.CrimsonGreaves);
        item.Register();
        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<DreadheartCrimsonChestplate>());
        item.AddIngredient(ItemID.TissueSample, 20);
        item.AddIngredient<NaturesHeart>();
        item.AddTile<Content.Tiles.Ambient.OvergrownAltar>();
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<DreadheartCrimsonLeggings>());
        item.AddIngredient(ItemID.TissueSample, 15);
        item.AddIngredient<NaturesHeart>();
        item.AddTile<Content.Tiles.Ambient.OvergrownAltar>();
        item.SortAfter(temp);
        item.Register();

        // weapons
        item = Recipe.Create(ModContent.ItemType<GutwrenchingHooks>());
        item.AddIngredient(ItemID.CrimtaneBar, 10);
        item.AddIngredient(ItemID.TissueSample, 5);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.CrimsonYoyo);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<ShadewoodStaff>());
        item.AddIngredient(ItemID.CrimtaneBar, 8);
        item.AddIngredient(ItemID.TissueSample, 5);
        item.AddIngredient(ItemID.Shadewood, 8);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.TheMeatball);
        item.Register();
    }

    private static void AddCorruptionRelatedStuff() {
        Recipe item = Recipe.Create(ModContent.ItemType<DreadheartCorruptionHelmet>());
        item.AddIngredient(ItemID.ShadowScale, 10);
        item.AddIngredient<NaturesHeart>();
        item.AddTile<Content.Tiles.Ambient.OvergrownAltar>();
        item.SortAfterFirstRecipesOf(ItemID.ShadowGreaves);
        item.Register();
        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<DreadheartCorruptionChestplate>());
        item.AddIngredient(ItemID.ShadowScale, 20);
        item.AddIngredient<NaturesHeart>();
        item.AddTile<Content.Tiles.Ambient.OvergrownAltar>();
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<DreadheartCorruptionLeggings>());
        item.AddIngredient(ItemID.ShadowScale, 15);
        item.AddIngredient<NaturesHeart>();
        item.AddTile<Content.Tiles.Ambient.OvergrownAltar>();
        item.SortAfter(temp);
        item.Register();

        // weapons
        item = Recipe.Create(ModContent.ItemType<HorrorPincers>());
        item.AddIngredient(ItemID.DemoniteBar, 10);
        item.AddIngredient(ItemID.ShadowScale, 5);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.CorruptYoyo);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<EbonwoodStaff>());
        item.AddIngredient(ItemID.DemoniteBar, 8);
        item.AddIngredient(ItemID.ShadowScale, 5);
        item.AddIngredient(ItemID.Ebonwood, 8);
        item.AddTile(TileID.Anvils);
        item.SortBeforeFirstRecipesOf(ItemID.CorruptYoyo);
        item.Register();
    }

    private static void AddBoneHarpySet() {
        Recipe item = Recipe.Create(ModContent.ItemType<WorshipperBonehelm>());
        item.AddIngredient(ItemID.Bone, 40);
        item.AddIngredient(ItemID.Leather, 5);
        item.AddTile(TileID.WorkBenches);
        item.SortAfterFirstRecipesOf(ItemID.NecroGreaves);
        item.Register();
        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<WorshipperMantle>());
        item.AddIngredient(ItemID.Bone, 50);
        item.AddIngredient(ItemID.Leather, 5);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<WorshipperGarb>());
        item.AddIngredient(ItemID.Bone, 40);
        item.AddIngredient(ItemID.Leather, 5);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();
    }

    private static void AddCopperSets() {
        Recipe item = Recipe.Create(ModContent.ItemType<CopperAcolyteHat>());
        item.AddIngredient(ItemID.CopperBar, 8);
        item.AddIngredient(ItemID.Leather, 5);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.CopperGreaves);
        item.Register();
        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<CopperAcolyteJacket>());
        item.AddIngredient(ItemID.CopperBar, 10);
        item.AddIngredient(ItemID.Leather, 8);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<CopperAcolyteLeggings>());
        item.AddIngredient(ItemID.CopperBar, 6);
        item.AddIngredient(ItemID.Leather, 5);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<TinAcolyteHat>());
        item.AddIngredient(ItemID.TinBar, 8);
        item.AddIngredient(ItemID.Leather, 5);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.TinGreaves);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<TinAcolyteJacket>());
        item.AddIngredient(ItemID.TinBar, 10);
        item.AddIngredient(ItemID.Leather, 8);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<TinAcolyteLeggings>());
        item.AddIngredient(ItemID.TinBar, 6);
        item.AddIngredient(ItemID.Leather, 5);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();
    }

    private static void AddDruidItems(Recipe lastWreath) {
        Recipe item = Recipe.Create(ModContent.ItemType<PastoralRod>());
        item.AddRecipeGroup(RecipeGroupID.Wood, 5);
        item.AddIngredient(ItemID.Rope, 10);
        item.AddTile(TileID.WorkBenches);
        item.SortAfterFirstRecipesOf(ItemID.WoodYoyo);
        item.Register();

        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<SapStream>());
        item.AddRecipeGroup(RecipeGroupID.Wood, 10);
        item.AddIngredient(ModContent.ItemType<Galipot>(), 5);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<BrilliantBouquet>());
        item.AddIngredient(ModContent.ItemType<ExoticTulip>());
        item.AddIngredient(ModContent.ItemType<SweetTulip>());
        item.AddIngredient(ModContent.ItemType<WeepingTulip>());
        item.SortAfter(lastWreath);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<ThornyClaws>());
        item.AddIngredient(ItemID.RichMahogany, 10);
        item.AddIngredient(ItemID.Stinger, 8);
        item.AddIngredient(ItemID.JungleSpores, 6);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.ThornWhip);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<RipePumpkin>());
        item.AddIngredient(ItemID.Pumpkin, 15);
        item.AddTile(TileID.WorkBenches);
        item.SortAfterFirstRecipesOf(ItemID.PumpkinLeggings);
        item.Register();
    }

    private static void AddOtherSawmill() {
        Recipe item = Recipe.Create(ModContent.ItemType<Tapper>());
        item.AddRecipeGroup(RecipeGroupID.Wood, 10);
        item.AddRecipeGroup(RecipeGroupID.IronBar, 2);
        item.AddTile(TileID.Sawmill);
        item.SortBeforeFirstRecipesOf(ItemID.Trapdoor);
        item.Register();
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
        Recipe item = Recipe.Create(ModContent.ItemType<LuminousFlowerHat>());
        item.AddIngredient(ModContent.ItemType<LuminousFlower>());
        item.AddIngredient(ItemID.Sunflower);
        item.SortBeforeFirstRecipesOf(ItemID.GarlandHat);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<Herbarium>());
        item.AddIngredient(ItemID.Book);
        item.AddIngredient(ItemID.Daybloom);
        item.AddIngredient(ItemID.Moonglow);
        item.AddIngredient(ItemID.Blinkroot);
        item.AddIngredient(ItemID.Waterleaf);
        item.AddIngredient(ItemID.Deathweed);
        item.AddIngredient(ItemID.Shiverthorn);
        item.AddIngredient(ItemID.Fireblossom);
        item.AddIngredient(ModContent.ItemType<Content.Items.Materials.MiracleMint>());
        //item.AddIngredient(ModContent.ItemType<Content.Items.Materials.Bonerose>());
        item.AddTile(TileID.Bookcases);
        item.SortAfterFirstRecipesOf(ItemID.GarlandHat);
        item.Register();

        // tiles
        item = Recipe.Create(ModContent.ItemType<MiracleMintHangingPot>());
        item.AddIngredient(ItemID.PotSuspended);
        item.AddIngredient(ModContent.ItemType<Content.Items.Materials.MiracleMint>());
        item.SortAfterFirstRecipesOf(ItemID.PotSuspendedFireblossom);
        item.Register();

        Recipe temp = item;
        item = Recipe.Create(ModContent.ItemType<BoneroseHangingPot>());
        item.AddIngredient(ItemID.PotSuspended);
        item.AddIngredient(ModContent.ItemType<Content.Items.Materials.Bonerose>());
        item.SortAfter(temp);
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

        temp = item;
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

        // armor
        item = Recipe.Create(ModContent.ItemType<LivingWoodHelmet>());
        item.AddIngredient(ItemID.Wood, 10);
        item.AddIngredient<Content.Items.Materials.Galipot>(3);
        item.AddTile(TileID.LivingLoom);
        item.SortAfterFirstRecipesOf(ItemID.WoodGreaves);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<LivingWoodChestplate>());
        item.AddIngredient(ItemID.Wood, 20);
        item.AddIngredient<Content.Items.Materials.Galipot>(5);
        item.AddTile(TileID.LivingLoom);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<LivingWoodGreaves>());
        item.AddIngredient(ItemID.Wood, 15);
        item.AddIngredient<Content.Items.Materials.Galipot>(2);
        item.AddTile(TileID.LivingLoom);
        item.SortAfter(temp);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<LivingBorealWoodHelmet>());
        item.AddIngredient(ItemID.BorealWood, 10);
        item.AddIngredient<Content.Items.Materials.Galipot>(3);
        item.AddTile(TileID.LivingLoom);
        item.SortAfterFirstRecipesOf(ItemID.BorealWoodGreaves);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<LivingBorealWoodChestplate>());
        item.AddIngredient(ItemID.BorealWood, 20);
        item.AddIngredient<Content.Items.Materials.Galipot>(5);
        item.AddTile(TileID.LivingLoom);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<LivingBorealWoodGreaves>());
        item.AddIngredient(ItemID.BorealWood, 15);
        item.AddIngredient<Content.Items.Materials.Galipot>(2);
        item.AddTile(TileID.LivingLoom);
        item.SortAfter(temp);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<LivingPalmHelmet>());
        item.AddIngredient(ItemID.PalmWood, 10);
        item.AddIngredient<Content.Items.Materials.Galipot>(3);
        item.AddTile(TileID.LivingLoom);
        item.SortAfterFirstRecipesOf(ItemID.PalmWoodGreaves);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<LivingPalmChestplate>());
        item.AddIngredient(ItemID.PalmWood, 20);
        item.AddIngredient<Content.Items.Materials.Galipot>(5);
        item.AddTile(TileID.LivingLoom);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<LivingPalmGreaves>());
        item.AddIngredient(ItemID.PalmWood, 15);
        item.AddIngredient<Content.Items.Materials.Galipot>(2);
        item.AddTile(TileID.LivingLoom);
        item.SortAfter(temp);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<LivingMahoganyHelmet>());
        item.AddIngredient(ItemID.RichMahogany, 10);
        item.AddIngredient<Content.Items.Materials.Galipot>(3);
        item.AddTile(TileID.LivingLoom);
        item.SortAfterFirstRecipesOf(ItemID.RichMahoganyGreaves);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<LivingMahoganyChestplate>());
        item.AddIngredient(ItemID.RichMahogany, 20);
        item.AddIngredient<Content.Items.Materials.Galipot>(5);
        item.AddTile(TileID.LivingLoom);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<LivingMahoganyGreaves>());
        item.AddIngredient(ItemID.RichMahogany, 15);
        item.AddIngredient<Content.Items.Materials.Galipot>(2);
        item.AddTile(TileID.LivingLoom);
        item.SortAfter(temp);
        item.Register();

        // other
        item = Recipe.Create(ModContent.ItemType<GalipotArrow>(), 30);
        item.AddIngredient(ItemID.WoodenArrow, 30);
        item.AddIngredient<Content.Items.Materials.Galipot>(1);
        item.SortAfterFirstRecipesOf(ItemID.FrostburnArrow);
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

    private static void AddHellstoneItems(out Recipe daikatana) {
        Recipe item = Recipe.Create(ModContent.ItemType<StarFusion>());
        item.AddIngredient(ItemID.Starfury, 1);
        item.AddIngredient(ItemID.MeteoriteBar, 10);
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(10);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.OrangePhaseblade);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<DiabolicDaikatana>());
        item.AddIngredient<Content.Items.Materials.DullDaikatana>(1);
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(10);
        item.AddIngredient(ItemID.Bone, 25);
        item.AddTile(TileID.DemonAltar);
        item.SortAfterFirstRecipesOf(ItemID.FireproofBugNet);
        item.Register();
        daikatana = item;

        item = Recipe.Create(ModContent.ItemType<TectonicCane>());
        item.AddIngredient(ItemID.HellstoneBar, 6);
        item.AddIngredient<Content.Items.Materials.FlamingFabric>(10);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.ImpStaff);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<FlamingFabric>());
        item.AddIngredient(ItemID.Hellstone, 1);
        item.AddIngredient(ItemID.Cobweb, 5);
        item.AddTile(TileID.Loom);
        item.SortAfterFirstRecipesOf(ItemID.HellstoneBar);
        item.Register();
    }

    private static void AddMercuriumItems(Recipe starFusion) {
        Recipe item = Recipe.Create(ModContent.ItemType<MercuriumBolt>(), 100);
        item.AddIngredient(ItemID.WoodenArrow, 100);
        item.AddIngredient<MercuriumNugget>(1);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.UnholyArrow);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<MercuriumBullet>(), 70);
        item.AddIngredient(ItemID.MusketBall, 70);
        item.AddIngredient<MercuriumNugget>(1);
        item.AddTile(TileID.Anvils);
        item.SortAfterFirstRecipesOf(ItemID.TungstenBullet);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<MercuriumNugget>());
        item.AddIngredient<Content.Items.Placeable.Crafting.MercuriumOre>(4);
        item.AddIngredient(ItemID.EbonstoneBlock, 2);
        item.AddTile(TileID.DemonAltar);
        item.SortAfterFirstRecipesOf(ItemID.DemoniteBar);
        item.Register();
        item = Recipe.Create(ModContent.ItemType<MercuriumNugget>());
        item.AddIngredient<Content.Items.Placeable.Crafting.MercuriumOre>(4);
        item.AddIngredient(ItemID.CrimstoneBlock, 2);
        item.AddTile(TileID.DemonAltar);
        item.SortAfterFirstRecipesOf(ItemID.CrimtaneBar);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<MercuriumBolter>());
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
        item = Recipe.Create(ModContent.ItemType<MercuriumZipper>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(12);
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
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(8);
        item.AddIngredient(ItemID.Lens, 1);
        item.AddIngredient(ItemID.Wire, 5);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        // accessories
        temp = item;
        item = Recipe.Create(ModContent.ItemType<MercuriumCenser>());
        item.AddIngredient<Content.Items.Materials.MercuriumNugget>(12);
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
        Recipe item = Recipe.Create(ModContent.ItemType<Elderwood>());
        item.AddIngredient<Content.Items.Placeable.Furniture.ElderwoodPlatform>(2);
        Recipe ashWoodFromPlatform = Main.recipe.FirstOrDefault(x => x.requiredItem.Any(x2 => x2.type == ItemID.AshWoodPlatform));
        item.SortAfter(ashWoodFromPlatform);
        item.Register();
        item = Recipe.Create(ModContent.ItemType<Elderwood>());
        item.AddIngredient<Content.Items.Placeable.Walls.ElderwoodWall>(4);
        item.AddTile(TileID.WorkBenches);
        Recipe ashWoodFromWall = Main.recipe.FirstOrDefault(x => x.requiredItem.Any(x2 => x2.type == ItemID.AshWoodWall));
        item.SortAfter(ashWoodFromWall);
        item.Register();
        item = Recipe.Create(ModContent.ItemType<Elderwood>());
        item.AddIngredient<Content.Items.Placeable.Walls.ElderwoodFence>(4);
        item.AddTile(TileID.WorkBenches);
        Recipe ashWoodFromFence = Main.recipe.FirstOrDefault(x => x.requiredItem.Any(x2 => x2.type == ItemID.AshWoodFence));
        item.SortAfter(ashWoodFromFence);
        item.Register();

        item = Recipe.Create(ModContent.ItemType<ElderwoodHelmet>());
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

        item = Recipe.Create(ModContent.ItemType<LivingElderwoodCrown>());
        item.AddIngredient<Elderwood>(10);
        item.AddIngredient<Content.Items.Materials.Galipot>(3);
        item.AddTile(TileID.LivingLoom);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<LivingElderwoodBreastplate>());
        item.AddIngredient<Elderwood>(20);
        item.AddIngredient<Content.Items.Materials.Galipot>(5);
        item.AddTile(TileID.LivingLoom);
        item.SortAfter(temp);
        item.Register();
        temp = item;
        item = Recipe.Create(ModContent.ItemType<LivingElderwoodGreaves>());
        item.AddIngredient<Elderwood>(15);
        item.AddIngredient<Content.Items.Materials.Galipot>(2);
        item.AddTile(TileID.LivingLoom);
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

        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodClaws>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(10);
        item.AddRecipeGroup(RecipeGroupID.IronBar, 5);
        item.AddTile(TileID.Anvils);
        item.SortAfter(temp);
        item.Register();

        temp = item;
        item = Recipe.Create(ModContent.ItemType<Woodbinder>());
        item.AddIngredient<Content.Items.Placeable.Crafting.Elderwood>(16);
        item.AddIngredient<NaturesHeart>(1);
        item.AddTile(TileID.WorkBenches);
        item.SortAfter(temp);
        item.Register();

        // furniture
        temp = item;
        item = Recipe.Create(ModContent.ItemType<ElderwoodBeam>(), 2);
        item.AddIngredient(ModContent.ItemType<Elderwood>(), 1);
        item.AddTile(TileID.Sawmill);
        item.SortAfter(temp);
        item.Register();

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
