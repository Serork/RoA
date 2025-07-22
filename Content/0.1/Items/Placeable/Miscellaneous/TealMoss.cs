using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Solid.Backwoods;

using System.Runtime.CompilerServices;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Miscellaneous;

sealed class TealMoss : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<BackwoodsGreenMoss>());

        int width = 20; int height = 18;
        Item.Size = new Vector2(width, height);
    }

    public override void Load() {
        On_Player.ExtractinatorUse += On_Player_ExtractinatorUse;
    }

    private void On_Player_ExtractinatorUse(On_Player.orig_ExtractinatorUse orig, Player self, int extractType, int extractinatorBlockType) {
        int num = 5000;
        int num2 = 25;
        int num3 = 50;
        int num4 = -1;
        int num5 = -1;
        int num6 = -1;
        int num7 = 1;
        switch (extractType) {
            /*
            case 1:
            */
            case ItemID.DesertFossil:
                num /= 3;
                num2 *= 2;
                num3 = 20;
                num4 = 10;
                break;
            /*
            case 2:
            */
            case ItemID.OldShoe:
                num = -1;
                num2 = -1;
                num3 = -1;
                num4 = -1;
                num5 = 1;
                num7 = -1;
                break;
            /*
            case 3:
            */
            case ItemID.LavaMoss:
                num = -1;
                num2 = -1;
                num3 = -1;
                num4 = -1;
                num5 = -1;
                num7 = -1;
                num6 = 1;
                break;
        }

        int num8 = -1;
        int num9 = 1;
        if (num4 != -1 && Main.rand.Next(num4) == 0) {
            num8 = 3380;
            if (Main.rand.Next(5) == 0)
                num9 += Main.rand.Next(2);

            if (Main.rand.Next(10) == 0)
                num9 += Main.rand.Next(3);

            if (Main.rand.Next(15) == 0)
                num9 += Main.rand.Next(4);
        }
        else if (num7 != -1 && Main.rand.Next(2) == 0) {
            if (Main.rand.Next(12000) == 0) {
                num8 = 74;
                if (Main.rand.Next(14) == 0)
                    num9 += Main.rand.Next(0, 2);

                if (Main.rand.Next(14) == 0)
                    num9 += Main.rand.Next(0, 2);

                if (Main.rand.Next(14) == 0)
                    num9 += Main.rand.Next(0, 2);
            }
            else if (Main.rand.Next(800) == 0) {
                num8 = 73;
                if (Main.rand.Next(6) == 0)
                    num9 += Main.rand.Next(1, 21);

                if (Main.rand.Next(6) == 0)
                    num9 += Main.rand.Next(1, 21);

                if (Main.rand.Next(6) == 0)
                    num9 += Main.rand.Next(1, 21);

                if (Main.rand.Next(6) == 0)
                    num9 += Main.rand.Next(1, 21);

                if (Main.rand.Next(6) == 0)
                    num9 += Main.rand.Next(1, 20);
            }
            else if (Main.rand.Next(60) == 0) {
                num8 = 72;
                if (Main.rand.Next(4) == 0)
                    num9 += Main.rand.Next(5, 26);

                if (Main.rand.Next(4) == 0)
                    num9 += Main.rand.Next(5, 26);

                if (Main.rand.Next(4) == 0)
                    num9 += Main.rand.Next(5, 26);

                if (Main.rand.Next(4) == 0)
                    num9 += Main.rand.Next(5, 25);
            }
            else {
                num8 = 71;
                if (Main.rand.Next(3) == 0)
                    num9 += Main.rand.Next(10, 26);

                if (Main.rand.Next(3) == 0)
                    num9 += Main.rand.Next(10, 26);

                if (Main.rand.Next(3) == 0)
                    num9 += Main.rand.Next(10, 26);

                if (Main.rand.Next(3) == 0)
                    num9 += Main.rand.Next(10, 25);
            }
        }
        else if (num != -1 && Main.rand.Next(num) == 0) {
            num8 = 1242;
        }
        else if (num5 != -1) {
            num8 = ((Main.rand.Next(4) != 1) ? 2674 : ((Main.rand.Next(3) != 1) ? 2006 : ((Main.rand.Next(3) == 1) ? 2675 : 2002)));
        }
        else if (num6 != -1 && extractinatorBlockType == 642) {
            if (Main.rand.Next(10) == 1) {
                switch (Main.rand.Next(5)) {
                    case 0:
                        num8 = 4354;
                        break;
                    case 1:
                        num8 = 4389;
                        break;
                    case 2:
                        num8 = 4377;
                        break;
                    case 3:
                        num8 = 5127;
                        break;
                    default:
                        num8 = 4378;
                        break;
                }
            }
            else {
                switch (Main.rand.Next(6)) {
                    case 0:
                        num8 = 4349;
                        break;
                    case 1:
                        num8 = 4350;
                        break;
                    case 2:
                        num8 = 4351;
                        break;
                    case 3:
                        num8 = 4352;
                        break;
                    case 4:
                        num8 = ModContent.ItemType<TealMoss>();
                        break;
                    default:
                        num8 = 4353;
                        break;
                }
            }
        }
        else if (num6 != -1) {
            switch (Main.rand.Next(5)) {
                case 0:
                    num8 = 4349;
                    break;
                case 1:
                    num8 = 4350;
                    break;
                case 2:
                    num8 = 4351;
                    break;
                case 3:
                    num8 = 4352;
                    break;
                default:
                    num8 = 4353;
                    break;
            }
        }
        else if (num2 != -1 && Main.rand.Next(num2) == 0) {
            switch (Main.rand.Next(6)) {
                case 0:
                    num8 = 181;
                    break;
                case 1:
                    num8 = 180;
                    break;
                case 2:
                    num8 = 177;
                    break;
                case 3:
                    num8 = 179;
                    break;
                case 4:
                    num8 = 178;
                    break;
                default:
                    num8 = 182;
                    break;
            }

            if (Main.rand.Next(20) == 0)
                num9 += Main.rand.Next(0, 2);

            if (Main.rand.Next(30) == 0)
                num9 += Main.rand.Next(0, 3);

            if (Main.rand.Next(40) == 0)
                num9 += Main.rand.Next(0, 4);

            if (Main.rand.Next(50) == 0)
                num9 += Main.rand.Next(0, 5);

            if (Main.rand.Next(60) == 0)
                num9 += Main.rand.Next(0, 6);
        }
        else if (num3 != -1 && Main.rand.Next(num3) == 0) {
            num8 = 999;
            if (Main.rand.Next(20) == 0)
                num9 += Main.rand.Next(0, 2);

            if (Main.rand.Next(30) == 0)
                num9 += Main.rand.Next(0, 3);

            if (Main.rand.Next(40) == 0)
                num9 += Main.rand.Next(0, 4);

            if (Main.rand.Next(50) == 0)
                num9 += Main.rand.Next(0, 5);

            if (Main.rand.Next(60) == 0)
                num9 += Main.rand.Next(0, 6);
        }
        else if (Main.rand.Next(3) == 0) {
            if (Main.rand.Next(5000) == 0) {
                num8 = 74;
                if (Main.rand.Next(10) == 0)
                    num9 += Main.rand.Next(0, 3);

                if (Main.rand.Next(10) == 0)
                    num9 += Main.rand.Next(0, 3);

                if (Main.rand.Next(10) == 0)
                    num9 += Main.rand.Next(0, 3);

                if (Main.rand.Next(10) == 0)
                    num9 += Main.rand.Next(0, 3);

                if (Main.rand.Next(10) == 0)
                    num9 += Main.rand.Next(0, 3);
            }
            else if (Main.rand.Next(400) == 0) {
                num8 = 73;
                if (Main.rand.Next(5) == 0)
                    num9 += Main.rand.Next(1, 21);

                if (Main.rand.Next(5) == 0)
                    num9 += Main.rand.Next(1, 21);

                if (Main.rand.Next(5) == 0)
                    num9 += Main.rand.Next(1, 21);

                if (Main.rand.Next(5) == 0)
                    num9 += Main.rand.Next(1, 21);

                if (Main.rand.Next(5) == 0)
                    num9 += Main.rand.Next(1, 20);
            }
            else if (Main.rand.Next(30) == 0) {
                num8 = 72;
                if (Main.rand.Next(3) == 0)
                    num9 += Main.rand.Next(5, 26);

                if (Main.rand.Next(3) == 0)
                    num9 += Main.rand.Next(5, 26);

                if (Main.rand.Next(3) == 0)
                    num9 += Main.rand.Next(5, 26);

                if (Main.rand.Next(3) == 0)
                    num9 += Main.rand.Next(5, 25);
            }
            else {
                num8 = 71;
                if (Main.rand.Next(2) == 0)
                    num9 += Main.rand.Next(10, 26);

                if (Main.rand.Next(2) == 0)
                    num9 += Main.rand.Next(10, 26);

                if (Main.rand.Next(2) == 0)
                    num9 += Main.rand.Next(10, 26);

                if (Main.rand.Next(2) == 0)
                    num9 += Main.rand.Next(10, 25);
            }
        }
        else if (extractinatorBlockType == 642) {
            switch (Main.rand.Next(14)) {
                case 0:
                    num8 = 12;
                    break;
                case 1:
                    num8 = 11;
                    break;
                case 2:
                    num8 = 14;
                    break;
                case 3:
                    num8 = 13;
                    break;
                case 4:
                    num8 = 699;
                    break;
                case 5:
                    num8 = 700;
                    break;
                case 6:
                    num8 = 701;
                    break;
                case 7:
                    num8 = 702;
                    break;
                case 8:
                    num8 = 364;
                    break;
                case 9:
                    num8 = 1104;
                    break;
                case 10:
                    num8 = 365;
                    break;
                case 11:
                    num8 = 1105;
                    break;
                case 12:
                    num8 = 366;
                    break;
                default:
                    num8 = 1106;
                    break;
            }

            if (Main.rand.Next(20) == 0)
                num9 += Main.rand.Next(0, 2);

            if (Main.rand.Next(30) == 0)
                num9 += Main.rand.Next(0, 3);

            if (Main.rand.Next(40) == 0)
                num9 += Main.rand.Next(0, 4);

            if (Main.rand.Next(50) == 0)
                num9 += Main.rand.Next(0, 5);

            if (Main.rand.Next(60) == 0)
                num9 += Main.rand.Next(0, 6);
        }
        else {
            switch (Main.rand.Next(8)) {
                case 0:
                    num8 = 12;
                    break;
                case 1:
                    num8 = 11;
                    break;
                case 2:
                    num8 = 14;
                    break;
                case 3:
                    num8 = 13;
                    break;
                case 4:
                    num8 = 699;
                    break;
                case 5:
                    num8 = 700;
                    break;
                case 6:
                    num8 = 701;
                    break;
                default:
                    num8 = 702;
                    break;
            }

            if (Main.rand.Next(20) == 0)
                num9 += Main.rand.Next(0, 2);

            if (Main.rand.Next(30) == 0)
                num9 += Main.rand.Next(0, 3);

            if (Main.rand.Next(40) == 0)
                num9 += Main.rand.Next(0, 4);

            if (Main.rand.Next(50) == 0)
                num9 += Main.rand.Next(0, 5);

            if (Main.rand.Next(60) == 0)
                num9 += Main.rand.Next(0, 6);
        }

        ItemLoader.ExtractinatorUse(ref num8, ref num9, extractType, extractinatorBlockType);

        if (num8 > 0)
            Player_DropItemFromExtractinator(self, num8, num9);
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DropItemFromExtractinator")]
    public extern static void Player_DropItemFromExtractinator(Player self, int itemType, int stack);

}
