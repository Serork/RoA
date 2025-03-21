using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Content.Items.Consumables;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.Items.Potions;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace RoA.Content.Tiles.Ambient;

// aequus
sealed class BackwoodsPot : ModTile {
    public override void SetStaticDefaults() {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileCut[Type] = true;

        Main.tileOreFinderPriority[Type] = 100;
        Main.tileSpelunker[Type] = true;

        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleWrapLimit = 3;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.AnchorInvalidTiles = [
            TileID.MagicalIceBlock,
            TileID.Boulder,
            TileID.BouncyBoulder,
            TileID.LifeCrystalBoulder,
            TileID.RollingCactus
        ];
        TileObjectData.addTile(Type);

        HitSound = SoundID.Shatter;

        AddMapEntry(new Color(74, 75, 87), Language.GetText("MapObject.Pot"));
        AddMapEntry(new Color(91, 74, 67), Language.GetText("MapObject.Pot"));

        TileObjectData.newTile.RandomStyleRange = 12;
    }

    public override ushort GetMapOption(int i, int j) {
        var tileFrameY = Main.tile[i, j].TileFrameY;
        if (tileFrameY < 72)
            return 0;
        else
            return 1;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        if (tile.TileFrameY >= 72) {
            type = ModContent.DustType<BackwoodsPotDust2>();
        }
        else {
            type = ModContent.DustType<BackwoodsPotDust1>();
        }

        return base.CreateDust(i, j, ref type);
    }

    public bool PreDropEffects(int i, int j) {
        return true;
    }

    public void ModifyPotionDrop(ref int itemID, ref int stack, UnifiedRandom rng) { }

    public bool ChooseSpecialBiomeTorch(ref int itemID) {
        itemID = ModContent.ItemType<ElderTorch>();

        return false;
    }

    public int ChooseGlowstick(int i, int j) {
        return ModContent.ItemType<SlipperyGlowstick>();
    }

    public int ChooseGrenade(int i, int j) {
        return ModContent.ItemType<SlipperyGrenade>();
    }

    public string GorePath(int i, int j, int frameX, int frameY) {
        return $"{Name}_{frameY}_";
    }

    public void DropGores(int i, int j, int frameX, int frameY) {
        string path = GorePath(i, j, frameX / 36, frameY / 36);
        if (!Main.dedServ) {
            for (int k = 0; k < 3; k++) {
                Gore.NewGore(new EntitySource_TileBreak(i, j), new Vector2(i * 16, j * 16), default(Vector2),
                    ModContent.Find<ModGore>(RoA.ModName + "/" + path + k).Type);
            }
        }
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        bool flag = false;
        int num = 0;
        int num9 = j;
        for (num += Main.tile[i, j].TileFrameX / 18; num > 1; num -= 2) {
        }
        num *= -1;
        num += i;
        int num15 = Main.tile[i, j].TileFrameY / 18;
        int num16 = 0;
        while (num15 > 1) {
            num15 -= 2;
            num16++;
        }
        num9 -= num15;
        for (int k = num; k < num + 2; k++) {
            for (int l = num9; l < num9 + 2; l++) {
                int num17;
                for (num17 = Main.tile[k, l].TileFrameX / 18; num17 > 1; num17 -= 2) {
                }
                if (!Main.tile[k, l].HasTile || Main.tile[k, l].TileType != Type || num17 != k - num || Main.tile[k, l].TileFrameY != (l - num9) * 18 + num16 * 36) {
                    flag = true;
                }
            }
            if (!WorldGen.SolidTile2(k, num9 + 2)) {
                flag = true;
            }
        }
        if (!flag) {
            return;
        }
        if (PreDropEffects(i, j)) {
            WorldGen.destroyObject = true;
            for (int m = num; m < num + 2; m++) {
                for (int n = num9; n < num9 + 2; n++) {
                    if (Main.tile[m, n].TileType == Type && Main.tile[m, n].HasTile) {
                        WorldGen.KillTile(m, n);
                    }
                }
            }
            float num18 = 1f;
            bool flag2 = false;

            if (Main.netMode != NetmodeID.Server)
                DropGores(i, j, frameX, frameY);

            num18 = (num18 * 2f + 1f) / 3f;
            int range = (int)(500f / ((num18 + 1f) / 2f));
            List<int> rockLayerRoAPotions = [];
            rockLayerRoAPotions.Add(ModContent.ItemType<DeathWardPotion>());
            rockLayerRoAPotions.Add(ModContent.ItemType<BloodlustPotion>());
            rockLayerRoAPotions.Add(ModContent.ItemType<BrightstonePotion>());
            rockLayerRoAPotions.Add(ModContent.ItemType<DryadBloodPotion>());
            rockLayerRoAPotions.Add(ModContent.ItemType<ResiliencePotion>());
            rockLayerRoAPotions.Add(ModContent.ItemType<WeightPotion>());
            rockLayerRoAPotions.Add(ModContent.ItemType<WillpowerPotion>());
            if (!WorldGen.gen) {
                var source = new EntitySource_TileBreak(i, j);
                if (Player.GetClosestRollLuck(i, j, range) == 0f) {
                    if (Main.netMode != NetmodeID.MultiplayerClient) {
                        Projectile.NewProjectile(source, i * 16 + 16, j * 16 + 16, 0f, -12f, ProjectileID.CoinPortal, 0, 0f, Main.myPlayer);
                    }
                }
                else if (WorldGen.genRand.NextBool(35) && Main.wallDungeon[Main.tile[i, j].WallType] && j > Main.worldSurface) {
                    Item.NewItem(source, i * 16, j * 16, 16, 16, ItemID.GoldenKey);
                }
                else if (Main.getGoodWorld && WorldGen.genRand.NextBool(4)) {
                    Projectile.NewProjectile(source, i * 16 + 16, j * 16 + 8, Main.rand.Next(-100, 101) * 0.002f, 0f, ProjectileID.Bomb, 0, 0f, Player.FindClosest(new Vector2(i * 16, j * 16), 16, 16));
                }
                else if (WorldGen.genRand.NextBool(45) || Main.rand.NextBool(45) && Main.expertMode) {
                    if (j < Main.worldSurface) {
                        int pot;
                        int stack = 1;
                        if (WorldGen.genRand.NextChance(0.75)) {
                            pot = WorldGen.genRand.NextFromCollection(rockLayerRoAPotions);
                        }
                        else {
                            switch (WorldGen.genRand.Next(10)) {
                                case 0:
                                    pot = ItemID.IronskinPotion;
                                    break;
                                case 1:
                                    pot = ItemID.ShinePotion;
                                    break;
                                case 2:
                                    pot = ItemID.NightOwlPotion;
                                    break;
                                case 3:
                                    pot = ItemID.BattlePotion;
                                    break;
                                case 4:
                                    pot = ItemID.MiningPotion;
                                    break;
                                case 5:
                                    pot = ItemID.MiningPotion;
                                    break;
                                case 6:
                                    pot = ItemID.CalmingPotion;
                                    break;
                                case 7:
                                    pot = ItemID.HeartreachPotion;
                                    break;
                                default:
                                    pot = ItemID.RecallPotion;
                                    stack = WorldGen.genRand.Next(3) + 1;
                                    break;
                            }
                        }

                        ModifyPotionDrop(ref pot, ref stack, WorldGen.genRand);
                        Item.NewItem(source, i * 16, j * 16, 16, 16, pot, stack);
                    }
                    else if (j < Main.rockLayer) {
                        int random = WorldGen.genRand.Next(11);
                        int potItem = 0;
                        if (WorldGen.genRand.NextChance(0.75)) {
                            potItem = WorldGen.genRand.NextFromCollection(rockLayerRoAPotions);
                        }
                        else {
                            switch (random) {
                                case 0:
                                    potItem = ItemID.RegenerationPotion;
                                    break;
                                case 1:
                                    potItem = ItemID.ShinePotion;
                                    break;
                                case 2:
                                    potItem = ItemID.NightOwlPotion;
                                    break;
                                case 3:
                                    potItem = ItemID.SwiftnessPotion;
                                    break;
                                case 4:
                                    potItem = ItemID.ArcheryPotion;
                                    break;
                                case 5:
                                    potItem = ItemID.GillsPotion;
                                    break;
                                case 6:
                                    potItem = ItemID.HunterPotion;
                                    break;
                                case 7:
                                    potItem = ItemID.MiningPotion;
                                    break;
                                case 8:
                                    potItem = ItemID.TrapsightPotion;
                                    break;
                            }
                        }

                        if (potItem != 0) {
                            int stack = 1;
                            ModifyPotionDrop(ref potItem, ref stack, WorldGen.genRand);
                            Item.NewItem(source, i * 16, j * 16, 16, 16, potItem, 1);
                        }

                        if (random >= 7) {
                            int recallPotion = ItemID.RecallPotion;
                            int stack = WorldGen.genRand.Next(1, 3);
                            ModifyPotionDrop(ref potItem, ref stack, WorldGen.genRand);

                            Item.NewItem(source, i * 16, j * 16, 16, 16, recallPotion, stack);
                        }
                    }
                    else if (j < Main.UnderworldLayer) {
                        int num23 = WorldGen.genRand.Next(15);
                        if (WorldGen.genRand.NextChance(0.75)) {
                            int pot = WorldGen.genRand.NextFromCollection(rockLayerRoAPotions);
                            Item.NewItem(source, i * 16, j * 16, 16, 16, pot, 1);
                        }
                        else {
                            if (num23 == 0) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 296);
                            }
                            if (num23 == 1) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 295);
                            }
                            if (num23 == 2) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 299);
                            }
                            if (num23 == 3) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 302);
                            }
                            if (num23 == 4) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 303);
                            }
                            if (num23 == 5) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 305);
                            }
                            if (num23 == 6) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 301);
                            }
                            if (num23 == 7) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 302);
                            }
                            if (num23 == 8) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 297);
                            }
                            if (num23 == 9) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 304);
                            }
                            if (num23 == 10) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 2322);
                            }
                            if (num23 == 11) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 2323);
                            }
                            if (num23 == 12) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 2327);
                            }
                            if (num23 == 13) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 2329);
                            }
                            if (num23 >= 7) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 2350, WorldGen.genRand.Next(1, 3));
                            }
                        }
                    }
                    else {
                        int num24 = WorldGen.genRand.Next(14);
                        if (WorldGen.genRand.NextChance(0.75)) {
                            int pot = WorldGen.genRand.NextFromCollection(rockLayerRoAPotions);
                            Item.NewItem(source, i * 16, j * 16, 16, 16, pot, 1);
                        }
                        else {
                            if (num24 == 0) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 296);
                            }
                            if (num24 == 1) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 295);
                            }
                            if (num24 == 2) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 293);
                            }
                            if (num24 == 3) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 288);
                            }
                            if (num24 == 4) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 294);
                            }
                            if (num24 == 5) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 297);
                            }
                            if (num24 == 6) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 304);
                            }
                            if (num24 == 7) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 305);
                            }
                            if (num24 == 8) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 301);
                            }
                            if (num24 == 9) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 302);
                            }
                            if (num24 == 10) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 288);
                            }
                            if (num24 == 11) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 300);
                            }
                            if (num24 == 12) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 2323);
                            }
                            if (num24 == 13) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 2326);
                            }
                            if (WorldGen.genRand.NextBool(5)) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 4870);
                            }
                        }
                    }
                }
                else if (Main.netMode == NetmodeID.Server && Main.rand.NextBool(30)) {
                    Item.NewItem(source, i * 16, j * 16, 16, 16, ItemID.WormholePotion);
                }
                else {
                    int num2 = Main.rand.Next(7);
                    if (Main.expertMode) {
                        num2--;
                    }
                    Player player = Main.player[Player.FindClosest(new Vector2(i * 16, j * 16), 16, 16)];
                    int num3 = 0;
                    int num4 = 20;
                    for (int num5 = 0; num5 < 50; num5++) {
                        Item item = player.inventory[num5];
                        if (!item.IsAir && item.createTile == TileID.Torches) {
                            num3 += item.stack;
                            if (num3 >= num4) {
                                break;
                            }
                        }
                    }
                    bool flag3 = num3 < num4;
                    if (num2 == 0 && player.statLife < player.statLifeMax2) {
                        Item.NewItem(source, i * 16, j * 16, 16, 16, ItemID.Heart);
                        if (Main.rand.NextBool(2)) {
                            Item.NewItem(source, i * 16, j * 16, 16, 16, ItemID.Heart);
                        }
                        if (Main.expertMode) {
                            if (Main.rand.NextBool(2)) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, ItemID.Heart);
                            }
                            if (Main.rand.NextBool(2)) {
                                Item.NewItem(source, i * 16, j * 16, 16, 16, ItemID.Heart);
                            }
                        }
                    }
                    else if (num2 == 1 || num2 == 0 && flag3) {
                        int stack = Main.rand.Next(2, 7);
                        if (Main.expertMode) {
                            stack += Main.rand.Next(1, 7);
                        }
                        int type2 = 8;
                        int type3 = ChooseGlowstick(i, j);

                        if (player.ZoneHallow) {
                            stack += Main.rand.Next(2, 7);
                            type2 = 4387;
                        }
                        else if (player.ZoneCrimson) {
                            stack += Main.rand.Next(2, 7);
                            type2 = 4386;
                        }
                        else if (player.ZoneCorrupt) {
                            stack += Main.rand.Next(2, 7);
                            type2 = 4385;
                        }
                        else if (ChooseSpecialBiomeTorch(ref type2)) {
                            stack += Main.rand.Next(2, 7);
                        }

                        if (Main.tile[i, j].LiquidAmount > 0) {
                            Item.NewItem(source, i * 16, j * 16, 16, 16, type3, stack);
                        }
                        else {
                            Item.NewItem(source, i * 16, j * 16, 16, 16, type2, stack);
                        }
                    }
                    else if (num2 == 2) {
                        int stack = Main.rand.Next(10, 21);
                        int type4 = 40;
                        if (j < Main.rockLayer && WorldGen.genRand.NextBool(2)) {
                            type4 = !Main.hardMode ? ItemID.Shuriken : ChooseGrenade(i, j);
                        }
                        if (j > Main.UnderworldLayer) {
                            type4 = ItemID.HellfireArrow;
                        }
                        else if (Main.hardMode) {
                            type4 = !Main.rand.NextBool(2) ? ItemID.UnholyArrow : WorldGen.SavedOreTiers.Silver != TileID.Tungsten ? ItemID.SilverBullet : ItemID.TungstenBullet;
                        }
                        Item.NewItem(source, i * 16, j * 16, 16, 16, type4, stack);
                    }
                    else if (num2 == 3) {
                        int type5 = 28;
                        if (j > Main.UnderworldLayer || Main.hardMode) {
                            type5 = 188;
                        }
                        int num7 = 1;
                        if (Main.expertMode && !Main.rand.NextBool(3)) {
                            num7++;
                        }
                        Item.NewItem(source, i * 16, j * 16, 16, 16, type5, num7);
                    }
                    else if (num2 == 4 && (flag2 || j > Main.rockLayer)) {
                        int type6 = ModContent.ItemType<SlipperyBomb>();
                        if (flag2) {
                            type6 = 4423;
                        }
                        int num8 = Main.rand.Next(4) + 1;
                        if (Main.expertMode) {
                            num8 += Main.rand.Next(4);
                        }
                        Item.NewItem(source, i * 16, j * 16, 16, 16, type6, num8);
                    }
                    else if ((num2 == 4 || num2 == 5) && j < Main.UnderworldLayer && !Main.hardMode) {
                        int stack2 = Main.rand.Next(20, 41);
                        Item.NewItem(source, i * 16, j * 16, 16, 16, 965, stack2);
                    }
                    else {
                        float num10 = 200 + WorldGen.genRand.Next(-100, 101);
                        if (j < Main.worldSurface) {
                            num10 *= 0.5f;
                        }
                        else if (j < Main.rockLayer) {
                            num10 *= 0.75f;
                        }
                        else if (j > Main.maxTilesY - 250) {
                            num10 *= 1.25f;
                        }
                        num10 *= 1f + Main.rand.Next(-20, 21) * 0.01f;
                        if (Main.rand.NextBool(4)) {
                            num10 *= 1f + Main.rand.Next(5, 11) * 0.01f;
                        }
                        if (Main.rand.NextBool(8)) {
                            num10 *= 1f + Main.rand.Next(10, 21) * 0.01f;
                        }
                        if (Main.rand.NextBool(12)) {
                            num10 *= 1f + Main.rand.Next(20, 41) * 0.01f;
                        }
                        if (Main.rand.NextBool(16)) {
                            num10 *= 1f + Main.rand.Next(40, 81) * 0.01f;
                        }
                        if (Main.rand.NextBool(20)) {
                            num10 *= 1f + Main.rand.Next(50, 101) * 0.01f;
                        }
                        if (Main.expertMode) {
                            num10 *= 2.5f;
                        }
                        if (Main.expertMode && Main.rand.NextBool(2)) {
                            num10 *= 1.25f;
                        }
                        if (Main.expertMode && Main.rand.NextBool(3)) {
                            num10 *= 1.5f;
                        }
                        if (Main.expertMode && Main.rand.NextBool(4)) {
                            num10 *= 1.75f;
                        }
                        num10 *= num18;
                        if (NPC.downedBoss1) {
                            num10 *= 1.1f;
                        }
                        if (NPC.downedBoss2) {
                            num10 *= 1.1f;
                        }
                        if (NPC.downedBoss3) {
                            num10 *= 1.1f;
                        }
                        if (NPC.downedMechBoss1) {
                            num10 *= 1.1f;
                        }
                        if (NPC.downedMechBoss2) {
                            num10 *= 1.1f;
                        }
                        if (NPC.downedMechBoss3) {
                            num10 *= 1.1f;
                        }
                        if (NPC.downedPlantBoss) {
                            num10 *= 1.1f;
                        }
                        if (NPC.downedQueenBee) {
                            num10 *= 1.1f;
                        }
                        if (NPC.downedGolemBoss) {
                            num10 *= 1.1f;
                        }
                        if (NPC.downedPirates) {
                            num10 *= 1.1f;
                        }
                        if (NPC.downedGoblins) {
                            num10 *= 1.1f;
                        }
                        if (NPC.downedFrost) {
                            num10 *= 1.1f;
                        }
                        while ((int)num10 > 0) {
                            if (num10 > 1000000f) {
                                int num11 = (int)(num10 / 1000000f);
                                if (num11 > 50 && Main.rand.NextBool(2)) {
                                    num11 /= Main.rand.Next(3) + 1;
                                }
                                if (Main.rand.NextBool(2)) {
                                    num11 /= Main.rand.Next(3) + 1;
                                }
                                num10 -= 1000000 * num11;
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 74, num11);
                                continue;
                            }
                            if (num10 > 10000f) {
                                int num12 = (int)(num10 / 10000f);
                                if (num12 > 50 && Main.rand.NextBool(2)) {
                                    num12 /= Main.rand.Next(3) + 1;
                                }
                                if (Main.rand.NextBool(2)) {
                                    num12 /= Main.rand.Next(3) + 1;
                                }
                                num10 -= 10000 * num12;
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 73, num12);
                                continue;
                            }
                            if (num10 > 100f) {
                                int num13 = (int)(num10 / 100f);
                                if (num13 > 50 && Main.rand.NextBool(2)) {
                                    num13 /= Main.rand.Next(3) + 1;
                                }
                                if (Main.rand.NextBool(2)) {
                                    num13 /= Main.rand.Next(3) + 1;
                                }
                                num10 -= 100 * num13;
                                Item.NewItem(source, i * 16, j * 16, 16, 16, 72, num13);
                                continue;
                            }
                            int num14 = (int)num10;
                            if (num14 > 50 && Main.rand.NextBool(2)) {
                                num14 /= Main.rand.Next(3) + 1;
                            }
                            if (Main.rand.NextBool(2)) {
                                num14 /= Main.rand.Next(4) + 1;
                            }
                            if (num14 < 1) {
                                num14 = 1;
                            }
                            num10 -= num14;
                            Item.NewItem(source, i * 16, j * 16, 16, 16, 71, num14);
                        }
                    }
                }
            }
        }
        WorldGen.destroyObject = false;
    }
}