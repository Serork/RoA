using Humanizer;

using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable;

sealed class BackwoodsCrate : ModItem {
    private class CatchThisCratePlayer : ModPlayer {
        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
            bool inWater = !attempt.inLava && !attempt.inHoney;
            bool inBackwoods = Player.InModBiome<BackwoodsBiome>();
            if (inWater && inBackwoods && attempt.crate) {
                if (!attempt.veryrare && !attempt.legendary && attempt.rare) {
                    itemDrop = ModContent.ItemType<BackwoodsCrate>();
                }
            }
        }
    }

    public override void SetStaticDefaults() {
        ItemID.Sets.IsFishingCrate[Type] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
    }

    public override void SetDefaults() {
        Item.width = 34;
        Item.height = 34;
        Item.rare = 2;
        Item.maxStack = Item.CommonMaxStack;
        Item.createTile = ModContent.TileType<Tiles.Decorations.BackwoodsCrate>();
        Item.useAnimation = 15;
        Item.useTime = 15;
        Item.autoReuse = true;
        Item.useStyle = 1;
        Item.consumable = true;
        Item.value = Item.sellPrice(0, 1);
    }

    public override bool CanRightClick() {
        return true;
    }

    public override void RightClick(Player player) {
        bool flag3 = true;
        IEntitySource source = player.GetSource_OpenItem(Type);
        bool flag = ItemID.Sets.IsFishingCrateHardmode[Type];
        if (Main.rand.NextBool()) {
            bool flag4 = true;
            while (flag4) {
                if (flag && flag4 && Main.rand.Next(60) == 0) {
                    int number24 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, 3064);
                    if (Main.netMode == 1)
                        NetMessage.SendData(21, -1, -1, null, number24, 1f);

                    flag4 = false;
                }

                if (flag4 && Main.rand.Next(25) == 0) {
                    int type12 = 2501;
                    int stack19 = 1;
                    int number25 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type12, stack19);
                    if (Main.netMode == 1)
                        NetMessage.SendData(21, -1, -1, null, number25, 1f);

                    flag4 = false;
                }

                if (flag4 && Main.rand.Next(20) == 0) {
                    int type13 = 2587;
                    int stack20 = 1;
                    int number26 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type13, stack20);
                    if (Main.netMode == 1)
                        NetMessage.SendData(21, -1, -1, null, number26, 1f);

                    flag4 = false;
                }

                if (flag4 && Main.rand.Next(15) == 0) {
                    int type14 = 2608;
                    int stack21 = 1;
                    int number27 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type14, stack21, noBroadcast: false, -1);
                    if (Main.netMode == 1)
                        NetMessage.SendData(21, -1, -1, null, number27, 1f);

                    flag4 = false;
                }

                if (flag4 && Main.rand.Next(20) == 0) {
                    int type15 = 3200;
                    int stack22 = 1;
                    int number28 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type15, stack22, noBroadcast: false, -1);
                    if (Main.netMode == 1)
                        NetMessage.SendData(21, -1, -1, null, number28, 1f);

                    flag4 = false;
                }

                if (flag4 && Main.rand.Next(20) == 0) {
                    int type16 = 3201;
                    int stack23 = 1;
                    int number29 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type16, stack23, noBroadcast: false, -1);
                    if (Main.netMode == 1)
                        NetMessage.SendData(21, -1, -1, null, number29, 1f);

                    flag4 = false;
                }

                if (Main.rand.Next(4) == 0) {
                    int type17 = 73;
                    int stack24 = Main.rand.Next(5, 11);
                    int number30 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type17, stack24);
                    if (Main.netMode == 1)
                        NetMessage.SendData(21, -1, -1, null, number30, 1f);

                    flag4 = false;
                }

                if (Main.rand.Next(6) == 0) {
                    int num9 = Main.rand.Next(6);
                    switch (num9) {
                        case 0:
                            num9 = 12;
                            break;
                        case 1:
                            num9 = 699;
                            break;
                        case 2:
                            num9 = 11;
                            break;
                        case 3:
                            num9 = 700;
                            break;
                        case 4:
                            num9 = 14;
                            break;
                        case 5:
                            num9 = 701;
                            break;
                    }

                    if (Main.rand.Next(2) == 0 && flag) {
                        num9 = Main.rand.Next(4);
                        switch (num9) {
                            case 0:
                                num9 = 364;
                                break;
                            case 1:
                                num9 = 1104;
                                break;
                            case 2:
                                num9 = 365;
                                break;
                            case 3:
                                num9 = 1105;
                                break;
                        }
                    }

                    int stack25 = Main.rand.Next(12, 22);
                    int number31 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, num9, stack25);
                    if (Main.netMode == 1)
                        NetMessage.SendData(21, -1, -1, null, number31, 1f);

                    flag4 = false;
                }
                else if (Main.rand.Next(4) == 0) {
                    int num10 = Main.rand.Next(6);
                    switch (num10) {
                        case 0:
                            num10 = 20;
                            break;
                        case 1:
                            num10 = 703;
                            break;
                        case 2:
                            num10 = 22;
                            break;
                        case 3:
                            num10 = 704;
                            break;
                        case 4:
                            num10 = 21;
                            break;
                        case 5:
                            num10 = 705;
                            break;
                    }

                    int num11 = Main.rand.Next(4, 8);
                    if (Main.rand.Next(3) != 0 && flag) {
                        num10 = Main.rand.Next(4);
                        switch (num10) {
                            case 0:
                                num10 = 381;
                                break;
                            case 1:
                                num10 = 1184;
                                break;
                            case 2:
                                num10 = 382;
                                break;
                            case 3:
                                num10 = 1191;
                                break;
                        }

                        num11 -= Main.rand.Next(2);
                    }

                    int number32 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, num10, num11);
                    if (Main.netMode == 1)
                        NetMessage.SendData(21, -1, -1, null, number32, 1f);

                    flag4 = false;
                }

                if (Main.rand.Next(4) == 0) {
                    int num12 = Main.rand.Next(8);
                    switch (num12) {
                        case 0:
                            num12 = 288;
                            break;
                        case 1:
                            num12 = 296;
                            break;
                        case 2:
                            num12 = 304;
                            break;
                        case 3:
                            num12 = 305;
                            break;
                        case 4:
                            num12 = 2322;
                            break;
                        case 5:
                            num12 = 2323;
                            break;
                        case 6:
                            num12 = 2324;
                            break;
                        case 7:
                            num12 = 2327;
                            break;
                    }

                    int stack26 = Main.rand.Next(2, 5);
                    int number33 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, num12, stack26);
                    if (Main.netMode == 1)
                        NetMessage.SendData(21, -1, -1, null, number33, 1f);

                    flag4 = false;
                }
            }

            if (Main.rand.Next(2) == 0) {
                int type18 = Main.rand.Next(188, 190);
                int stack27 = Main.rand.Next(5, 16);
                int number34 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type18, stack27);
                if (Main.netMode == 1)
                    NetMessage.SendData(21, -1, -1, null, number34, 1f);
            }

            if (Main.rand.Next(2) == 0) {
                int type19 = ((Main.rand.Next(3) != 0) ? 2675 : 2676);
                int stack28 = Main.rand.Next(2, 5);
                int number35 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type19, stack28);
                if (Main.netMode == 1)
                    NetMessage.SendData(21, -1, -1, null, number35, 1f);
            }

            return;
        }
        while (flag3) {
            if (flag && flag3 && Main.rand.Next(20) == 0) {
                int number13 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, 3064);
                if (Main.netMode == 1)
                    NetMessage.SendData(21, -1, -1, null, number13, 1f);

                flag3 = false;
            }

            if (flag3 && Main.rand.Next(8) == 0) {
                int type7 = 29;
                int stack11 = 1;
                int number14 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type7, stack11);
                if (Main.netMode == 1)
                    NetMessage.SendData(21, -1, -1, null, number14, 1f);

                flag3 = false;
            }

            if (flag3 && Main.rand.Next(10) == 0) {
                int type8 = 2491;
                int stack12 = 1;
                int number15 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type8, stack12);
                if (Main.netMode == 1)
                    NetMessage.SendData(21, -1, -1, null, number15, 1f);

                flag3 = false;
            }

            if (Main.rand.Next(3) == 0) {
                int type9 = 73;
                int stack13 = Main.rand.Next(8, 21);
                int number16 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type9, stack13);
                if (Main.netMode == 1)
                    NetMessage.SendData(21, -1, -1, null, number16, 1f);

                flag3 = false;
            }

            if (Main.rand.Next(5) == 0) {
                int num6 = Main.rand.Next(4);
                switch (num6) {
                    case 0:
                        num6 = 14;
                        break;
                    case 1:
                        num6 = 701;
                        break;
                    case 2:
                        num6 = 13;
                        break;
                    case 3:
                        num6 = 702;
                        break;
                }

                if (Main.rand.Next(2) == 0 && flag) {
                    num6 = Main.rand.Next(4);
                    switch (num6) {
                        case 0:
                            num6 = 365;
                            break;
                        case 1:
                            num6 = 1105;
                            break;
                        case 2:
                            num6 = 366;
                            break;
                        case 3:
                            num6 = 1106;
                            break;
                    }
                }

                int stack14 = Main.rand.Next(25, 35);
                int number17 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, num6, stack14);
                if (Main.netMode == 1)
                    NetMessage.SendData(21, -1, -1, null, number17, 1f);

                flag3 = false;
            }
            else {
                if (Main.rand.Next(3) != 0)
                    continue;

                int num7 = Main.rand.Next(4);
                switch (num7) {
                    case 0:
                        num7 = 21;
                        break;
                    case 1:
                        num7 = 19;
                        break;
                    case 2:
                        num7 = 705;
                        break;
                    case 3:
                        num7 = 706;
                        break;
                }

                if (Main.rand.Next(3) != 0 && flag) {
                    num7 = Main.rand.Next(4);
                    switch (num7) {
                        case 0:
                            num7 = 382;
                            break;
                        case 1:
                            num7 = 391;
                            break;
                        case 2:
                            num7 = 1191;
                            break;
                        case 3:
                            num7 = 1198;
                            break;
                    }
                }

                int stack15 = Main.rand.Next(8, 12);
                int number18 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, num7, stack15);
                if (Main.netMode == 1)
                    NetMessage.SendData(21, -1, -1, null, number18, 1f);

                flag3 = false;
            }
        }

        if (Main.rand.Next(3) == 0) {
            int num8 = Main.rand.Next(5);
            switch (num8) {
                case 0:
                    num8 = 288;
                    break;
                case 1:
                    num8 = 296;
                    break;
                case 2:
                    num8 = 305;
                    break;
                case 3:
                    num8 = 2322;
                    break;
                case 4:
                    num8 = 2323;
                    break;
            }

            int stack16 = Main.rand.Next(2, 6);
            int number19 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, num8, stack16);
            if (Main.netMode == 1)
                NetMessage.SendData(21, -1, -1, null, number19, 1f);
        }

        if (Main.rand.Next(2) == 0) {
            int type10 = Main.rand.Next(188, 190);
            int stack17 = Main.rand.Next(5, 21);
            int number20 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type10, stack17);
            if (Main.netMode == 1)
                NetMessage.SendData(21, -1, -1, null, number20, 1f);
        }

        if (Main.rand.Next(3) != 0) {
            int type11 = 2676;
            int stack18 = Main.rand.Next(3, 8);
            int number21 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, type11, stack18);
            if (Main.netMode == 1)
                NetMessage.SendData(21, -1, -1, null, number21, 1f);
        }

        if (Main.rand.Next(30) == 0 && !flag) {
            int number22 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, 989);
            if (Main.netMode == 1)
                NetMessage.SendData(21, -1, -1, null, number22, 1f);
        }

        if (Main.rand.Next(15) == 0 && flag) {
            int number23 = Item.NewItem(source, (int)player.position.X, (int)player.position.Y, player.width, player.height, 989);
            if (Main.netMode == 1)
                NetMessage.SendData(21, -1, -1, null, number23, 1f);
        }

        return;
    }
}