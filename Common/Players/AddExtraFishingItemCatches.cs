using ModLiquidLib.ModLoader;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Materials;
using RoA.Content.Items.Miscellaneous.QuestFish;
using RoA.Content.Items.Weapons.Nature.Hardmode;
using RoA.Content.Liquids;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

using static Terraria.ID.ContentSamples.CreativeHelper;

namespace RoA.Common.Players;

sealed class AddExtraFishingItemCatches : ModPlayer {
    public override void Load() {
        On_Projectile.FishingCheck_RollItemDrop += On_Projectile_FishingCheck_RollItemDrop;
    }

    private void On_Projectile_FishingCheck_RollItemDrop(On_Projectile.orig_FishingCheck_RollItemDrop orig, Projectile self, ref FishingAttempt fisher) {
        int owner = self.owner;
        bool inBackwoods = Main.player[owner].InModBiome<BackwoodsBiome>();

        bool flag = Main.player[owner].ZoneCorrupt;
        bool flag2 = Main.player[owner].ZoneCrimson;
        bool flag3 = Main.player[owner].ZoneJungle;
        bool flag4 = Main.player[owner].ZoneSnow;
        bool flag5 = Main.player[owner].ZoneDungeon;
        if (!NPC.downedBoss3)
            flag5 = false;

        if (Main.notTheBeesWorld && !Main.remixWorld && Main.rand.Next(2) == 0)
            flag3 = false;

        if (Main.remixWorld && fisher.heightLevel == 0) {
            flag = false;
            flag2 = false;
        }
        else if (flag && flag2) {
            if (Main.rand.Next(2) == 0)
                flag2 = false;
            else
                flag = false;
        }

        if (fisher.rolledEnemySpawn > 0)
            return;

        if (fisher.inLava) {
            if (fisher.CanFishInLava) {
                if (fisher.crate && Main.rand.Next(6) == 0) {
                    fisher.rolledItemDrop = (Main.hardMode ? 4878 : 4877);
                }
                else if (fisher.legendary && Main.hardMode && Main.rand.Next(3) == 0) {
                    fisher.rolledItemDrop = Main.rand.NextFromList(new short[4] {
                        4819,
                        4820,
                        4872,
                        2331
                    });
                }
                else if (fisher.legendary && !Main.hardMode && Main.rand.Next(3) == 0) {
                    fisher.rolledItemDrop = Main.rand.NextFromList(new short[3] {
                        4819,
                        4820,
                        4872
                    });
                }
                else if (fisher.veryrare) {
                    fisher.rolledItemDrop = 2312;
                }
                else if (fisher.rare) {
                    fisher.rolledItemDrop = 2315;
                }
            }

            return;
        }

        Tile tile = Main.tile[fisher.X, fisher.Y];
        if (tile.LiquidType == LiquidLoader.LiquidType<Tar>()) {
            if (fisher.questFish == ModContent.ItemType<BoneBroth>() && fisher.uncommon) {
                fisher.rolledItemDrop = ModContent.ItemType<BoneBroth>();
            }
            else if (fisher.uncommon) {
                fisher.rolledItemDrop = ModContent.ItemType<BlotchedFish>();
            }
            return;
        }

        if (fisher.inHoney) {
            if (fisher.rare || (fisher.uncommon && Main.rand.Next(2) == 0))
                fisher.rolledItemDrop = 2314;
            else if (fisher.uncommon && fisher.questFish == 2451)
                fisher.rolledItemDrop = 2451;

            return;
        }

        if (fisher.inHoney) {
            if (fisher.rare || (fisher.uncommon && Main.rand.Next(2) == 0))
                fisher.rolledItemDrop = 2314;
            else if (fisher.uncommon && fisher.questFish == 2451)
                fisher.rolledItemDrop = 2451;

            return;
        }

        if (Main.rand.Next(50) > fisher.fishingLevel && Main.rand.Next(50) > fisher.fishingLevel && fisher.waterTilesCount < fisher.waterNeededToFish) {
            fisher.rolledItemDrop = Main.rand.Next(2337, 2340);
            if (Main.rand.Next(8) == 0)
                fisher.rolledItemDrop = 5275;

            return;
        }

        if (fisher.crate) {
            bool hardMode = Main.hardMode;
            if (fisher.rare && flag5)
                fisher.rolledItemDrop = (hardMode ? 3984 : 3205);
            else if (fisher.rare && (Main.player[owner].ZoneBeach || (Main.remixWorld && fisher.heightLevel == 1 && (double)fisher.Y >= Main.rockLayer && Main.rand.Next(2) == 0)))
                fisher.rolledItemDrop = (hardMode ? 5003 : 5002);
            else if (fisher.rare && flag)
                fisher.rolledItemDrop = (hardMode ? 3982 : 3203);
            else if (fisher.rare && flag2)
                fisher.rolledItemDrop = (hardMode ? 3983 : 3204);
            else if (fisher.rare && Main.player[owner].ZoneHallow)
                fisher.rolledItemDrop = (hardMode ? 3986 : 3207);
            else if (fisher.rare && flag3)
                fisher.rolledItemDrop = (hardMode ? 3987 : 3208);
            else if (fisher.rare && Main.player[owner].ZoneSnow)
                fisher.rolledItemDrop = (hardMode ? 4406 : 4405);
            else if (fisher.rare && Main.player[owner].ZoneDesert)
                fisher.rolledItemDrop = (hardMode ? 4408 : 4407);
            else if (fisher.rare && fisher.heightLevel == 0)
                fisher.rolledItemDrop = (hardMode ? 3985 : 3206);
            else if (fisher.veryrare || fisher.legendary)
                fisher.rolledItemDrop = (hardMode ? 3981 : 2336);
            else if (fisher.uncommon)
                fisher.rolledItemDrop = (hardMode ? 3980 : 2335);
            else
                fisher.rolledItemDrop = (hardMode ? 3979 : 2334);

            return;
        }

        if (!NPC.combatBookWasUsed && Main.bloodMoon && fisher.legendary && Main.rand.Next(3) == 0) {
            fisher.rolledItemDrop = 4382;
            return;
        }

        if (Main.bloodMoon && fisher.legendary && Main.rand.Next(2) == 0) {
            fisher.rolledItemDrop = 5240;
            return;
        }

        if (fisher.legendary && Main.rand.Next(5) == 0) {
            fisher.rolledItemDrop = 2423;
            return;
        }

        if (fisher.legendary && Main.rand.Next(5) == 0) {
            fisher.rolledItemDrop = 3225;
            return;
        }

        if (fisher.legendary && Main.rand.Next(10) == 0) {
            fisher.rolledItemDrop = 2420;
            return;
        }

        if (!fisher.legendary && !fisher.veryrare && fisher.uncommon && Main.rand.Next(5) == 0) {
            fisher.rolledItemDrop = 3196;
            return;
        }

        bool flag6 = Main.player[owner].ZoneDesert;
        if (flag5) {
            flag6 = false;
            if (fisher.rolledItemDrop == 0 && fisher.veryrare && Main.rand.Next(7) == 0)
                fisher.rolledItemDrop = 3000;
        }
        else {
            if (inBackwoods) {
                if (fisher.legendary && Main.hardMode && Main.rand.Next(2) == 0) {
                    fisher.rolledItemDrop = ModContent.ItemType<LeafySeahorse>();
                }
                else if (fisher.questFish == ModContent.ItemType<Druidfish>() && fisher.heightLevel <= 1 && fisher.uncommon) {
                    fisher.rolledItemDrop = ModContent.ItemType<Druidfish>();
                }
                else if (fisher.questFish == ModContent.ItemType<SwimmingStone>() && fisher.heightLevel > 1 && fisher.uncommon) {
                    fisher.rolledItemDrop = ModContent.ItemType<SwimmingStone>();
                }
                else if (fisher.rare) {
                    fisher.rolledItemDrop = ModContent.ItemType<EmeraldOpaleye>();
                }
            }
            else if (flag) {
                if (fisher.legendary && Main.hardMode && Main.player[owner].ZoneSnow && fisher.heightLevel == 3 && Main.rand.Next(3) != 0)
                    fisher.rolledItemDrop = 2429;
                else if (fisher.legendary && Main.hardMode && Main.rand.Next(2) == 0)
                    fisher.rolledItemDrop = 3210;
                else if (fisher.rare)
                    fisher.rolledItemDrop = 2330;
                else if (fisher.uncommon && fisher.questFish == 2454)
                    fisher.rolledItemDrop = 2454;
                else if (fisher.uncommon && fisher.questFish == 2485)
                    fisher.rolledItemDrop = 2485;
                else if (fisher.uncommon && fisher.questFish == 2457)
                    fisher.rolledItemDrop = 2457;
                else if (fisher.uncommon)
                    fisher.rolledItemDrop = 2318;
            }
            else if (flag2) {
                if (fisher.legendary && Main.hardMode && Main.player[owner].ZoneSnow && fisher.heightLevel == 3 && Main.rand.Next(3) != 0)
                    fisher.rolledItemDrop = 2429;
                else if (fisher.legendary && Main.hardMode && Main.rand.Next(2) == 0)
                    fisher.rolledItemDrop = 3211;
                else if (fisher.uncommon && fisher.questFish == 2477)
                    fisher.rolledItemDrop = 2477;
                else if (fisher.uncommon && fisher.questFish == 2463)
                    fisher.rolledItemDrop = 2463;
                else if (fisher.uncommon)
                    fisher.rolledItemDrop = 2319;
                else if (fisher.common)
                    fisher.rolledItemDrop = 2305;
            }
            else if (Main.player[owner].ZoneHallow) {
                if (flag6 && Main.rand.Next(2) == 0) {
                    if (fisher.uncommon && fisher.questFish == 4393)
                        fisher.rolledItemDrop = 4393;
                    else if (fisher.uncommon && fisher.questFish == 4394)
                        fisher.rolledItemDrop = 4394;
                    else if (fisher.uncommon)
                        fisher.rolledItemDrop = 4410;
                    else if (Main.rand.Next(3) == 0)
                        fisher.rolledItemDrop = 4402;
                    else
                        fisher.rolledItemDrop = 4401;
                }
                else if (fisher.legendary && Main.hardMode && Main.player[owner].ZoneSnow && fisher.heightLevel == 3 && Main.rand.Next(3) != 0) {
                    fisher.rolledItemDrop = 2429;
                }
                else if (fisher.legendary && Main.hardMode && Main.rand.Next(2) == 0) {
                    fisher.rolledItemDrop = 3209;
                }
                else if (fisher.legendary && Main.hardMode && Main.rand.Next(3) != 0) {
                    fisher.rolledItemDrop = 5274;
                }
                else if (fisher.heightLevel > 1 && fisher.veryrare) {
                    fisher.rolledItemDrop = 2317;
                }
                else if (fisher.heightLevel > 1 && fisher.uncommon && fisher.questFish == 2465) {
                    fisher.rolledItemDrop = 2465;
                }
                else if (fisher.heightLevel < 2 && fisher.uncommon && fisher.questFish == 2468) {
                    fisher.rolledItemDrop = 2468;
                }
                else if (fisher.rare) {
                    fisher.rolledItemDrop = 2310;
                }
                else if (fisher.uncommon && fisher.questFish == 2471) {
                    fisher.rolledItemDrop = 2471;
                }
                else if (fisher.uncommon) {
                    fisher.rolledItemDrop = 2307;
                }
            }

            if (fisher.rolledItemDrop == 0 && Main.player[owner].ZoneGlowshroom && fisher.uncommon && fisher.questFish == 2475)
                fisher.rolledItemDrop = 2475;

            if (flag4 && flag3 && Main.rand.Next(2) == 0)
                flag4 = false;

            if (fisher.rolledItemDrop == 0 && flag4) {
                if (fisher.heightLevel < 2 && fisher.uncommon && fisher.questFish == 2467)
                    fisher.rolledItemDrop = 2467;
                else if (fisher.heightLevel == 1 && fisher.uncommon && fisher.questFish == 2470)
                    fisher.rolledItemDrop = 2470;
                else if (fisher.heightLevel >= 2 && fisher.uncommon && fisher.questFish == 2484)
                    fisher.rolledItemDrop = 2484;
                else if (fisher.heightLevel > 1 && fisher.uncommon && fisher.questFish == 2466)
                    fisher.rolledItemDrop = 2466;
                else if ((fisher.common && Main.rand.Next(12) == 0) || (fisher.uncommon && Main.rand.Next(6) == 0))
                    fisher.rolledItemDrop = 3197;
                else if (fisher.uncommon)
                    fisher.rolledItemDrop = 2306;
                else if (fisher.common)
                    fisher.rolledItemDrop = 2299;
                else if (fisher.heightLevel > 1 && Main.rand.Next(3) == 0)
                    fisher.rolledItemDrop = 2309;
            }

            if (fisher.rolledItemDrop == 0 && flag3) {
                if (fisher.heightLevel == 1 && fisher.uncommon && fisher.questFish == 2452)
                    fisher.rolledItemDrop = 2452;
                else if (fisher.heightLevel == 1 && fisher.uncommon && fisher.questFish == 2483)
                    fisher.rolledItemDrop = 2483;
                else if (fisher.heightLevel == 1 && fisher.uncommon && fisher.questFish == 2488)
                    fisher.rolledItemDrop = 2488;
                else if (fisher.heightLevel >= 1 && fisher.uncommon && fisher.questFish == 2486)
                    fisher.rolledItemDrop = 2486;
                else if (fisher.heightLevel > 1 && fisher.uncommon)
                    fisher.rolledItemDrop = 2311;
                else if (fisher.uncommon)
                    fisher.rolledItemDrop = 2313;
                else if (fisher.common)
                    fisher.rolledItemDrop = 2302;
            }
        }

        if (fisher.rolledItemDrop == 0) {
            if ((Main.remixWorld && fisher.heightLevel == 1 && (double)fisher.Y >= Main.rockLayer && Main.rand.Next(3) == 0) || (fisher.heightLevel <= 1 && (fisher.X < 380 || fisher.X > Main.maxTilesX - 380) && fisher.waterTilesCount > 1000)) {
                if (fisher.veryrare && Main.rand.Next(2) == 0)
                    fisher.rolledItemDrop = 2341;
                else if (fisher.veryrare)
                    fisher.rolledItemDrop = 2342;
                else if (fisher.rare && Main.rand.Next(5) == 0)
                    fisher.rolledItemDrop = 2438;
                else if (fisher.rare && Main.rand.Next(3) == 0)
                    fisher.rolledItemDrop = 2332;
                else if (fisher.uncommon && fisher.questFish == 2480)
                    fisher.rolledItemDrop = 2480;
                else if (fisher.uncommon && fisher.questFish == 2481)
                    fisher.rolledItemDrop = 2481;
                else if (fisher.uncommon)
                    fisher.rolledItemDrop = 2316;
                else if (fisher.common && Main.rand.Next(2) == 0)
                    fisher.rolledItemDrop = 2301;
                else if (fisher.common)
                    fisher.rolledItemDrop = 2300;
                else
                    fisher.rolledItemDrop = 2297;
            }
            else if (flag6) {
                if (fisher.uncommon && fisher.questFish == 4393)
                    fisher.rolledItemDrop = 4393;
                else if (fisher.uncommon && fisher.questFish == 4394)
                    fisher.rolledItemDrop = 4394;
                else if (fisher.uncommon)
                    fisher.rolledItemDrop = 4410;
                else if (Main.rand.Next(3) == 0)
                    fisher.rolledItemDrop = 4402;
                else
                    fisher.rolledItemDrop = 4401;
            }
        }

        if (fisher.rolledItemDrop != 0)
            return;

        if (fisher.heightLevel < 2 && fisher.uncommon && fisher.questFish == 2461) {
            fisher.rolledItemDrop = 2461;
        }
        else if (fisher.heightLevel == 0 && fisher.uncommon && fisher.questFish == 2453) {
            fisher.rolledItemDrop = 2453;
        }
        else if (fisher.heightLevel == 0 && fisher.uncommon && fisher.questFish == 2473) {
            fisher.rolledItemDrop = 2473;
        }
        else if (fisher.heightLevel == 0 && fisher.uncommon && fisher.questFish == 2476) {
            fisher.rolledItemDrop = 2476;
        }
        else if (fisher.heightLevel < 2 && fisher.uncommon && fisher.questFish == 2458) {
            fisher.rolledItemDrop = 2458;
        }
        else if (fisher.heightLevel < 2 && fisher.uncommon && fisher.questFish == 2459) {
            fisher.rolledItemDrop = 2459;
        }
        else if (fisher.heightLevel == 0 && fisher.uncommon) {
            fisher.rolledItemDrop = 2304;
        }
        else if (fisher.heightLevel > 0 && fisher.heightLevel < 3 && fisher.uncommon && fisher.questFish == 2455) {
            fisher.rolledItemDrop = 2455;
        }
        else if (fisher.heightLevel == 1 && fisher.uncommon && fisher.questFish == 2479) {
            fisher.rolledItemDrop = 2479;
        }
        else if (fisher.heightLevel == 1 && fisher.uncommon && fisher.questFish == 2456) {
            fisher.rolledItemDrop = 2456;
        }
        else if (fisher.heightLevel == 1 && fisher.uncommon && fisher.questFish == 2474) {
            fisher.rolledItemDrop = 2474;
        }
        else if (fisher.heightLevel > 1 && fisher.rare && Main.rand.Next(5) == 0) {
            if (Main.hardMode && Main.rand.Next(2) == 0)
                fisher.rolledItemDrop = 2437;
            else
                fisher.rolledItemDrop = 2436;
        }
        else if (fisher.heightLevel > 1 && fisher.legendary && Main.rand.Next(3) != 0) {
            fisher.rolledItemDrop = 2308;
        }
        else if (fisher.heightLevel > 1 && fisher.veryrare && Main.rand.Next(2) == 0) {
            fisher.rolledItemDrop = 2320;
        }
        else if (fisher.heightLevel > 1 && fisher.rare) {
            fisher.rolledItemDrop = 2321;
        }
        else if (fisher.heightLevel > 1 && fisher.uncommon && fisher.questFish == 2478) {
            fisher.rolledItemDrop = 2478;
        }
        else if (fisher.heightLevel > 1 && fisher.uncommon && fisher.questFish == 2450) {
            fisher.rolledItemDrop = 2450;
        }
        else if (fisher.heightLevel > 1 && fisher.uncommon && fisher.questFish == 2464) {
            fisher.rolledItemDrop = 2464;
        }
        else if (fisher.heightLevel > 1 && fisher.uncommon && fisher.questFish == 2469) {
            fisher.rolledItemDrop = 2469;
        }
        else if (fisher.heightLevel > 2 && fisher.uncommon && fisher.questFish == 2462) {
            fisher.rolledItemDrop = 2462;
        }
        else if (fisher.heightLevel > 2 && fisher.uncommon && fisher.questFish == 2482) {
            fisher.rolledItemDrop = 2482;
        }
        else if (fisher.heightLevel > 2 && fisher.uncommon && fisher.questFish == 2472) {
            fisher.rolledItemDrop = 2472;
        }
        else if (fisher.heightLevel > 2 && fisher.uncommon && fisher.questFish == 2460) {
            fisher.rolledItemDrop = 2460;
        }
        else if (fisher.heightLevel > 1 && fisher.uncommon && Main.rand.Next(4) != 0) {
            fisher.rolledItemDrop = 2303;
        }
        else if (fisher.heightLevel > 1 && (fisher.uncommon || fisher.common || Main.rand.Next(4) == 0)) {
            if (Main.rand.Next(4) == 0)
                fisher.rolledItemDrop = 2303;
            else
                fisher.rolledItemDrop = 2309;
        }
        else if (fisher.uncommon && fisher.questFish == 2487) {
            fisher.rolledItemDrop = 2487;
        }
        else if (fisher.waterTilesCount > 1000 && fisher.common) {
            fisher.rolledItemDrop = 2298;
        }
        else {
            fisher.rolledItemDrop = 2290;
        }
    }
}
