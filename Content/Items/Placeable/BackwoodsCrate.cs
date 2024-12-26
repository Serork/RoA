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