using Microsoft.Xna.Framework;

using RoA.Common.BackwoodsSystems;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Dusts;
using RoA.Content.Items.Consumables;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.Items.Potions;
using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
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

        //HitSound = SoundID.Shatter;

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
        if (!WorldGen.gen && !Main.dedServ) {
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
        int style = Main.tile[i, j].TileFrameY / 18;
        int num16 = 0;
        while (style > 1) {
            style -= 2;
            num16++;
        }
        num9 -= style;
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

        WorldGen.destroyObject = true;
        //if (num4 >= 7 && num4 <= 9)
        SoundEngine.PlaySound(SoundID.Shatter, new Vector2(i * 16, j * 16));
        //else if (num4 >= 16 && num4 <= 24)
        //    SoundEngine.PlaySound(4, i * 16, j * 16);
        //else
        //    SoundEngine.PlaySound(13, i * 16, j * 16);
        for (int m = num; m < num + 2; m++) {
            for (int n = num9; n < num9 + 2; n++) {
                if (Main.tile[m, n].TileType == ModContent.TileType<BackwoodsPot>() && Main.tile[m, n].HasTile) {
                    WorldGen.KillTile(m, n);
                }
            }
        }
        float num18 = 1f;
        bool flag2 = false;

        if (Main.netMode != NetmodeID.Server)
            DropGores(i, j, frameX, frameY);

        if (Main.netMode != 1)
            SpawnLoot(i, j, num, num9, 0);

        WorldGen.destroyObject = false;
    }

    private void SpawnLoot(int i, int j, int x2, int y2, int style) {
        int firstTileY = BackwoodsVars.FirstTileYAtCenter;
        int centerY = BackwoodsVars.BackwoodsCenterY;
        int sizeY = BackwoodsVars.BackwoodsSizeY;
        int sizeX = BackwoodsVars.BackwoodsHalfSizeX * 2;
        int centerX = BackwoodsVars.BackwoodsCenterX;
        int edgeY = sizeY / 4;

        int worldSurface = firstTileY + edgeY;
        int rockLayer = centerY - edgeY;
        int bottom = centerY + sizeY / 2 - edgeY * 2;
        bool flag = (double)j < rockLayer;
        bool flag2 = j < Main.UnderworldLayer;
        //if (Main.remixWorld) {
        //    flag = (double)j > Main.rockLayer && j < Main.UnderworldLayer;
        //    flag2 = (double)j > Main.worldSurface && (double)j < Main.rockLayer;
        //}

        UnifiedRandom genRand = WorldGen.genRand;

        float num = 1f;
        bool flag3 = style >= 34 && style <= 36;
        //switch (style) {
        //    case 4:
        //    case 5:
        //    case 6:
        //        num = 1.25f;
        //        break;
        //    default:
        //        if (style >= 7 && style <= 9) {
        //            num = 1.75f;
        //        }
        //        else if (style >= 10 && style <= 12) {
        //            num = 1.9f;
        //        }
        //        else if (style >= 13 && style <= 15) {
        //            num = 2.1f;
        //        }
        //        else if (style >= 16 && style <= 18) {
        //            num = 1.6f;
        //        }
        //        else if (style >= 19 && style <= 21) {
        //            num = 3.5f;
        //        }
        //        else if (style >= 22 && style <= 24) {
        //            num = 1.6f;
        //        }
        //        else if (style >= 25 && style <= 27) {
        //            num = 10f;
        //        }
        //        else if (style >= 28 && style <= 30) {
        //            if (Main.hardMode)
        //                num = 4f;
        //        }
        //        else if (style >= 31 && style <= 33) {
        //            num = 2f;
        //        }
        //        else if (style >= 34 && style <= 36) {
        //            num = 1.25f;
        //        }
        //        break;
        //    case 0:
        //    case 1:
        //    case 2:
        //    case 3:
        //        break;
        //}
        num = 1.75f;

        num = (num * 2f + 1f) / 3f;
        int range = (int)(500f / ((num + 1f) / 2f));
        if (WorldGen.gen)
            return;

        if (Player.GetClosestRollLuck(i, j, range) == 0f) {
            if (Main.netMode != 1)
                Projectile.NewProjectile(new EntitySource_TileBreak(i, j), i * 16 + 16, j * 16 + 16, 0f, -12f, 518, 0, 0f, Main.myPlayer);

            return;
        }

        if (genRand.Next(35) == 0 && Main.wallDungeon[Main.tile[i, j].WallType] && (double)j > worldSurface) {
            Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 327);
            return;
        }

        if (Main.getGoodWorld && genRand.Next(6) == 0) {
            Projectile.NewProjectile(new EntitySource_TileBreak(i, j), i * 16 + 16, j * 16 + 8, (float)Main.rand.Next(-100, 101) * 0.002f, 0f, 28, 0, 0f, Main.myPlayer, 16f, 16f);
            return;
        }

        if (Main.remixWorld && Main.netMode != 1 && genRand.Next(5) == 0) {
            Player player = Main.player[Player.FindClosest(new Vector2(i * 16, j * 16), 16, 16)];
            if (Main.rand.Next(2) == 0) {
                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 75);
            }
            else if (player.ZoneJungle) {
                int num2 = -1;
                num2 = NPC.NewNPC(new EntitySource_SpawnNPC(), x2 * 16 + 16, y2 * 16 + 32, -10);
                if (num2 > -1) {
                    Main.npc[num2].ai[1] = 75f;
                    Main.npc[num2].netUpdate = true;
                }
            }
            else if ((double)j > rockLayer && j < Main.maxTilesY - 350) {
                int num3 = -1;
                num3 = ((Main.rand.Next(9) == 0) ? NPC.NewNPC(new EntitySource_SpawnNPC(), x2 * 16 + 16, y2 * 16 + 32, -7) : ((Main.rand.Next(7) == 0) ? NPC.NewNPC(new EntitySource_SpawnNPC(), x2 * 16 + 16, y2 * 16 + 32, -8) : ((Main.rand.Next(6) == 0) ? NPC.NewNPC(new EntitySource_SpawnNPC(), x2 * 16 + 16, y2 * 16 + 32, -9) : ((Main.rand.Next(3) != 0) ? NPC.NewNPC(new EntitySource_SpawnNPC(), x2 * 16 + 16, y2 * 16 + 32, 1) : NPC.NewNPC(new EntitySource_SpawnNPC(), x2 * 16 + 16, y2 * 16 + 32, -3)))));
                if (num3 > -1) {
                    Main.npc[num3].ai[1] = 75f;
                    Main.npc[num3].netUpdate = true;
                }
            }
            else if ((double)j > worldSurface && (double)j <= rockLayer) {
                int num4 = -1;
                num4 = NPC.NewNPC(new EntitySource_SpawnNPC(), x2 * 16 + 16, y2 * 16 + 32, -6);
                if (num4 > -1) {
                    Main.npc[num4].ai[1] = 75f;
                    Main.npc[num4].netUpdate = true;
                }
            }
            else {
                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 75);
            }

            return;
        }

        if (Main.remixWorld && (double)i > (double)Main.maxTilesX * 0.37 && (double)i < (double)Main.maxTilesX * 0.63 && j > Main.maxTilesY - 220) {
            int stack = Main.rand.Next(20, 41);
            Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 965, stack);
            return;
        }

        if (genRand.Next(45) == 0 || (Main.rand.Next(45) == 0 && Main.expertMode)) {
            if ((double)j < worldSurface) {
                int num5 = genRand.Next(10);
                if (num5 == 0)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 292);

                if (num5 == 1)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 298);

                if (num5 == 2)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 299);

                if (num5 == 3)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 290);

                if (num5 == 4)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2322);

                if (num5 == 5)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2324);

                if (num5 == 6)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2325);

                if (num5 >= 7)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2350, genRand.Next(1, 3));
            }
            else if (flag) {
                int num6 = genRand.Next(12);
                if (num6 == 0)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 289);

                if (num6 == 1)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 298);

                if (num6 == 2)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 299);

                if (num6 == 3)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 290);

                if (num6 == 4)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 303);

                if (num6 == 5)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 291);

                if (num6 == 6)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 304);

                if (num6 == 7)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2322);

                if (num6 == 8)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2329);

                if (num6 == 9)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<WillpowerPotion>());

                if (num6 >= 7)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2350, genRand.Next(1, 3));
            }
            else if (j < bottom) {
                int num7 = genRand.Next(19);
                if (num7 == 0)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 296);

                if (num7 == 1)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 295);

                if (num7 == 2)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 299);

                if (num7 == 3)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 302);

                if (num7 == 4)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 303);

                if (num7 == 5)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 305);

                if (num7 == 6)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 301);

                if (num7 == 7)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 302);

                if (num7 == 8)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 297);

                if (num7 == 9)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 304);

                if (num7 == 10)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2322);

                if (num7 == 11)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2323);

                if (num7 == 12)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2327);

                if (num7 == 13)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2329);

                if (num7 == 14)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<WillpowerPotion>());

                if (num7 == 15)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<ResiliencePotion>());

                if (num7 == 16)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<DryadBloodPotion>());

                if (num7 == 17)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<WeightPotion>());

                if (num7 >= 7)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2350, genRand.Next(1, 3));
            }
            else {
                int num8 = genRand.Next(19);
                if (num8 == 0)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 296);

                if (num8 == 1)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 295);

                if (num8 == 2)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 293);

                if (num8 == 3)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 288);

                if (num8 == 4)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 294);

                if (num8 == 5)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 297);

                if (num8 == 6)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 304);

                if (num8 == 7)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 305);

                if (num8 == 8)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 301);

                if (num8 == 9)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 302);

                if (num8 == 10)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 288);

                if (num8 == 11)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 300);

                if (num8 == 12)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2323);

                if (num8 == 13)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2326);

                if (num8 == 14)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<DeathWardPotion>());

                if (num8 == 15)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<BloodlustPotion>());

                if (num8 == 16)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<BrightstonePotion>());

                if (num8 == 17)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<ResiliencePotion>());

                if (num8 == 18)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<DryadBloodPotion>());

                if (genRand.Next(5) == 0)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 4870);
            }

            return;
        }

        if (Main.netMode == 2 && Main.rand.Next(30) == 0) {
            Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 2997);
            return;
        }

        int num9 = Main.rand.Next(7);
        if (Main.expertMode)
            num9--;

        Player player2 = Main.player[Player.FindClosest(new Vector2(i * 16, j * 16), 16, 16)];
        int num10 = 0;
        int num11 = 20;
        for (int k = 0; k < 50; k++) {
            Item item = player2.inventory[k];
            if (!item.IsAir && item.createTile == 4) {
                num10 += item.stack;
                if (num10 >= num11)
                    break;
            }
        }

        bool flag4 = num10 < num11;
        if (num9 == 0 && player2.statLife < player2.statLifeMax2) {
            Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 58);
            if (Main.rand.Next(2) == 0)
                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 58);

            if (Main.expertMode) {
                if (Main.rand.Next(2) == 0)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 58);

                if (Main.rand.Next(2) == 0)
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 58);
            }

            return;
        }

        if (num9 == 1 || (num9 == 0 && flag4)) {
            int num12 = Main.rand.Next(2, 7);
            if (Main.expertMode)
                num12 += Main.rand.Next(1, 7);

            int type = 8;
            int type2 = 282;
            if (player2.InModBiome<BackwoodsBiome>()) {
                num12 += Main.rand.Next(2, 7);
                type = ModContent.ItemType<ElderTorch>();
                //num12 = (int)((float)num12 * 1.5f);
            }
            else if (player2.ZoneHallow) {
                num12 += Main.rand.Next(2, 7);
                type = 4387;
            }
            else if ((style >= 22 && style <= 24) || player2.ZoneCrimson) {
                num12 += Main.rand.Next(2, 7);
                type = 4386;
            }
            else if ((style >= 16 && style <= 18) || player2.ZoneCorrupt) {
                num12 += Main.rand.Next(2, 7);
                type = 4385;
            }
            else if (style >= 7 && style <= 9) {
                num12 += Main.rand.Next(2, 7);
                num12 = (int)((float)num12 * 1.5f);
                type = 4388;
            }
            else if (style >= 4 && style <= 6) {
                type = 974;
                type2 = 286;
            }
            else if (style >= 34 && style <= 36) {
                num12 += Main.rand.Next(2, 7);
                type = 4383;
            }
            else if (player2.ZoneGlowshroom) {
                num12 += Main.rand.Next(2, 7);
                type = 5293;
            }

            if (Main.tile[i, j].LiquidAmount > 0)
                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, type2, num12);
            else
                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, type, num12);

            return;
        }

        switch (num9) {
            case 2: {
                int stack2 = Main.rand.Next(10, 21);
                int type4 = 40;
                if (flag && genRand.Next(2) == 0)
                    type4 = ((!Main.hardMode) ? 42 : 168);

                if (j > Main.UnderworldLayer)
                    type4 = 265;
                else if (Main.hardMode)
                    type4 = ((Main.rand.Next(2) != 0) ? 47 : ((WorldGen.SavedOreTiers.Silver != 168) ? 278 : 4915));

                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, type4, stack2);
                return;
            }
            case 3: {
                int type5 = 28;
                if (j > Main.UnderworldLayer || Main.hardMode)
                    type5 = 188;

                int num14 = 1;
                if (Main.expertMode && Main.rand.Next(3) != 0)
                    num14++;

                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, type5, num14);
                return;
            }
            case 4:
                if (flag3 || flag2) {
                    int type3 = 166;
                    if (flag3)
                        type3 = 4423;

                    int num13 = Main.rand.Next(4) + 1;
                    if (Main.expertMode)
                        num13 += Main.rand.Next(4);

                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, type3, num13);
                    return;
                }
                break;
        }

        if ((num9 == 4 || num9 == 5) && j < Main.UnderworldLayer && !Main.hardMode) {
            int stack3 = Main.rand.Next(20, 41);
            Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 965, stack3);
            return;
        }

        float num15 = 200 + genRand.Next(-100, 101);
        if ((double)j < worldSurface)
            num15 *= 0.5f;
        else if (flag)
            num15 *= 0.75f;
        else if (j > bottom)
            num15 *= 1.25f;

        num15 *= 1f + (float)Main.rand.Next(-20, 21) * 0.01f;
        if (Main.rand.Next(4) == 0)
            num15 *= 1f + (float)Main.rand.Next(5, 11) * 0.01f;

        if (Main.rand.Next(8) == 0)
            num15 *= 1f + (float)Main.rand.Next(10, 21) * 0.01f;

        if (Main.rand.Next(12) == 0)
            num15 *= 1f + (float)Main.rand.Next(20, 41) * 0.01f;

        if (Main.rand.Next(16) == 0)
            num15 *= 1f + (float)Main.rand.Next(40, 81) * 0.01f;

        if (Main.rand.Next(20) == 0)
            num15 *= 1f + (float)Main.rand.Next(50, 101) * 0.01f;

        if (Main.expertMode)
            num15 *= 2.5f;

        if (Main.expertMode && Main.rand.Next(2) == 0)
            num15 *= 1.25f;

        if (Main.expertMode && Main.rand.Next(3) == 0)
            num15 *= 1.5f;

        if (Main.expertMode && Main.rand.Next(4) == 0)
            num15 *= 1.75f;

        num15 *= num;
        if (NPC.downedBoss1)
            num15 *= 1.1f;

        if (NPC.downedBoss2)
            num15 *= 1.1f;

        if (NPC.downedBoss3)
            num15 *= 1.1f;

        if (NPC.downedMechBoss1)
            num15 *= 1.1f;

        if (NPC.downedMechBoss2)
            num15 *= 1.1f;

        if (NPC.downedMechBoss3)
            num15 *= 1.1f;

        if (NPC.downedPlantBoss)
            num15 *= 1.1f;

        if (NPC.downedQueenBee)
            num15 *= 1.1f;

        if (NPC.downedGolemBoss)
            num15 *= 1.1f;

        if (NPC.downedPirates)
            num15 *= 1.1f;

        if (NPC.downedGoblins)
            num15 *= 1.1f;

        if (NPC.downedFrost)
            num15 *= 1.1f;

        while ((int)num15 > 0) {
            if (num15 > 1000000f) {
                int num16 = (int)(num15 / 1000000f);
                if (num16 > 50 && Main.rand.Next(2) == 0)
                    num16 /= Main.rand.Next(3) + 1;

                if (Main.rand.Next(2) == 0)
                    num16 /= Main.rand.Next(3) + 1;

                num15 -= (float)(1000000 * num16);
                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 74, num16);
                continue;
            }

            if (num15 > 10000f) {
                int num17 = (int)(num15 / 10000f);
                if (num17 > 50 && Main.rand.Next(2) == 0)
                    num17 /= Main.rand.Next(3) + 1;

                if (Main.rand.Next(2) == 0)
                    num17 /= Main.rand.Next(3) + 1;

                num15 -= (float)(10000 * num17);
                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 73, num17);
                continue;
            }

            if (num15 > 100f) {
                int num18 = (int)(num15 / 100f);
                if (num18 > 50 && Main.rand.Next(2) == 0)
                    num18 /= Main.rand.Next(3) + 1;

                if (Main.rand.Next(2) == 0)
                    num18 /= Main.rand.Next(3) + 1;

                num15 -= (float)(100 * num18);
                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 72, num18);
                continue;
            }

            int num19 = (int)num15;
            if (num19 > 50 && Main.rand.Next(2) == 0)
                num19 /= Main.rand.Next(3) + 1;

            if (Main.rand.Next(2) == 0)
                num19 /= Main.rand.Next(4) + 1;

            if (num19 < 1)
                num19 = 1;

            num15 -= (float)num19;
            Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, 71, num19);
        }
    }
}