using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Content.Items.Weapons.Druidic;

sealed class PineCone : NatureItem {
    private sealed class GeneratedStorage : ModSystem {
        public static bool PineConeAddedToWorld;

        private static void Reset() {
            PineConeAddedToWorld = false;
        }

        public override void OnWorldLoad() => Reset();
        public override void OnWorldUnload() => Reset();

        public override void PostWorldGen() {
            PineConeAddedToWorld = false;
        }
    }

    protected override void SafeSetDefaults() {
		Item.SetSize(18, 28);
		Item.SetWeaponValues(2, 0.5f);
		Item.SetDefaultToUsable(ItemUseStyleID.HoldUp, 35, false, useSound: SoundID.Item1);
		Item.SetDefaultToShootable((ushort)ModContent.ProjectileType<Projectiles.Friendly.Druidic.PineCone>());
		Item.SetDefaultOthers(Item.sellPrice(silver: 10), ItemRarityID.White);

		NatureWeaponHandler.SetPotentialDamage(Item, 10);
        NatureWeaponHandler.SetFillingRate(Item, 0.8f);
    }

    private static bool IsValidForPineConeDrop(int i, int j) {
        bool result = i % 10 == 0 && j % 3 == 2;
        return result;
    }

    public override void Load() {
        On_WorldGen.GetCommonTreeFoliageData += On_WorldGen_GetCommonTreeFoliageData;
        On_TileDrawing.DrawTrees += On_TileDrawing_DrawTrees;
        On_WorldGen.GetTreeFrame += On_WorldGen_GetTreeFrame;
        On_WorldGen.GrowTree += On_WorldGen_GrowTree;
        On_WorldGen.KillTile_GetTreeDrops += On_WorldGen_KillTile_GetTreeDrops;
    }

    private void On_WorldGen_KillTile_GetTreeDrops(On_WorldGen.orig_KillTile_GetTreeDrops orig, int i, int j, Tile tileCache, ref bool bonusWood, ref int dropItem, ref int secondaryItem) {
        if (tileCache.TileFrameX >= 22 && tileCache.TileFrameY >= 198) {
            if (Main.netMode != 1) {
                if (WorldGen.genRand.Next(2) == 0) {
                    int k;
                    for (k = j; Main.tile[i, k] != null && (!Main.tile[i, k].HasTile || !Main.tileSolid[Main.tile[i, k].TileType] || Main.tileSolidTop[Main.tile[i, k].TileType]); k++) {
                    }

                    if (Main.tile[i, k] != null) {
                        Tile tile = Main.tile[i, k];
                        if (tile.TileType == 2 || tile.TileType == 109 || tile.TileType == 477 || tile.TileType == 492 || tile.TileType == 147 || tile.TileType == 199 || tile.TileType == 23 || tile.TileType == 633) {
                            dropItem = 9;
                            secondaryItem = 27;
                        }
                        else {
                            dropItem = 9;
                        }
                    }
                }
                else {
                    dropItem = 9;
                }
            }
        }
        else {
            dropItem = 9;
        }

        if (dropItem != 9)
            return;

        bool flag = false;
        WorldGen.GetTreeBottom(i, j, out var x, out var y);
        if (Main.tile[x, y].HasTile) {
            switch (Main.tile[x, y].TileType) {
                case 633:
                    dropItem = 5215;
                    break;
                case 23:
                case 661:
                    dropItem = 619;
                    break;
                case 199:
                case 662:
                    dropItem = 911;
                    break;
                case 60:
                    dropItem = 620;
                    break;
                case 109:
                case 492:
                    dropItem = 621;
                    break;
                case 70:
                    if (WorldGen.genRand.Next(2) == 0)
                        dropItem = 183;
                    else
                        dropItem = 0;
                    break;
                case 147:
                    dropItem = 2503;
                    if (IsValidForPineConeDrop(i, j)) {
                        dropItem = ModContent.ItemType<PineCone>();
                        flag = true;
                    }
                    break;
            }
        }

        int num = Player.FindClosest(new Vector2(x * 16, y * 16), 16, 16);
        int axe = Main.player[num].inventory[Main.player[num].selectedItem].axe;
        if (WorldGen.genRand.Next(100) < axe || Main.rand.Next(3) == 0)
            bonusWood = true;
        if (flag && bonusWood) {
            bonusWood = false;
        }
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 2;

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		if (player.IsLocal()) {
			if (Collision.CanHitLine(player.Center, 2, 2, player.GetViableMousePosition(), 2, 2)) {
				Projectile.NewProjectile(player.GetSource_ItemUse(Item), Vector2.Zero, Vector2.Zero, type, damage, knockback, player.whoAmI);
			}
		}

		return false;
	}

    private bool On_WorldGen_GrowTree(On_WorldGen.orig_GrowTree orig, int i, int y) {
        int j;
        for (j = y; TileID.Sets.TreeSapling[Main.tile[i, j].TileType]; j++) {
        }

        UnifiedRandom genRand = WorldGen.genRand;
        if ((Main.tile[i - 1, j - 1].LiquidAmount != 0 || Main.tile[i, j - 1].LiquidAmount != 0 || Main.tile[i + 1, j - 1].LiquidAmount != 0) && !WorldGen.notTheBees)
            return false;

        if (Main.tile[i, j].HasUnactuatedTile && !Main.tile[i, j].IsHalfBlock && Main.tile[i, j].Slope == 0 && WorldGen.IsTileTypeFitForTree(Main.tile[i, j].TileType) && ((Main.remixWorld && (double)j > Main.worldSurface) || Main.tile[i, j - 1].WallType == 0 || WorldGen.DefaultTreeWallTest(Main.tile[i, j - 1].WallType)) && ((Main.tile[i - 1, j].HasTile && WorldGen.IsTileTypeFitForTree(Main.tile[i - 1, j].TileType)) || (Main.tile[i + 1, j].HasTile && WorldGen.IsTileTypeFitForTree(Main.tile[i + 1, j].TileType)))) {
            TileColorCache cache = Main.tile[i, j].BlockColorAndCoating();
            if (Main.tenthAnniversaryWorld && !WorldGen.gen)
                cache.Color = (byte)genRand.Next(1, 13);

            int num = 2;
            int num2 = genRand.Next(5, 17);
            int num3 = num2 + 4;
            if (Main.tile[i, j].TileType == 60)
                num3 += 5;

            bool flag = false;
            if (Main.tile[i, j].TileType == 70 && WorldGen.EmptyTileCheck(i - num, i + num, j - num3, j - 3, 20) && WorldGen.EmptyTileCheck(i - 1, i + 1, j - 2, j - 1, 20))
                flag = true;

            if (WorldGen.EmptyTileCheck(i - num, i + num, j - num3, j - 1, 20))
                flag = true;

            if (flag) {
                bool flag2 = Main.remixWorld && (double)j < Main.worldSurface;
                bool flag3 = false;
                bool flag4 = false;
                int num4;
                for (int k = j - num2; k < j; k++) {
                    Tile tile = Main.tile[i, k];
                    tile.TileFrameNumber = (byte)genRand.Next(3);
                    tile.HasTile = true;
                    Main.tile[i, k].TileType = 5;
                    Main.tile[i, k].UseBlockColors(cache);
                    num4 = genRand.Next(3);
                    int num5 = genRand.Next(10);
                    if (k == j - 1 || k == j - num2)
                        num5 = 0;

                    while (((num5 == 5 || num5 == 7) && flag3) || ((num5 == 6 || num5 == 7) && flag4)) {
                        num5 = genRand.Next(10);
                    }

                    flag3 = false;
                    flag4 = false;
                    if (num5 == 5 || num5 == 7)
                        flag3 = true;

                    if (num5 == 6 || num5 == 7)
                        flag4 = true;

                    switch (num5) {
                        case 1:
                            if (num4 == 0) {
                                Main.tile[i, k].TileFrameX = 0;
                                Main.tile[i, k].TileFrameY = 66;
                            }
                            if (num4 == 1) {
                                Main.tile[i, k].TileFrameX = 0;
                                Main.tile[i, k].TileFrameY = 88;
                            }
                            if (num4 == 2) {
                                Main.tile[i, k].TileFrameX = 0;
                                Main.tile[i, k].TileFrameY = 110;
                            }
                            break;
                        case 2:
                            if (num4 == 0) {
                                Main.tile[i, k].TileFrameX = 22;
                                Main.tile[i, k].TileFrameY = 0;
                            }
                            if (num4 == 1) {
                                Main.tile[i, k].TileFrameX = 22;
                                Main.tile[i, k].TileFrameY = 22;
                            }
                            if (num4 == 2) {
                                Main.tile[i, k].TileFrameX = 22;
                                Main.tile[i, k].TileFrameY = 44;
                            }
                            break;
                        case 3:
                            if (num4 == 0) {
                                Main.tile[i, k].TileFrameX = 44;
                                Main.tile[i, k].TileFrameY = 66;
                            }
                            if (num4 == 1) {
                                Main.tile[i, k].TileFrameX = 44;
                                Main.tile[i, k].TileFrameY = 88;
                            }
                            if (num4 == 2) {
                                Main.tile[i, k].TileFrameX = 44;
                                Main.tile[i, k].TileFrameY = 110;
                            }
                            break;
                        case 4:
                            if (num4 == 0) {
                                Main.tile[i, k].TileFrameX = 22;
                                Main.tile[i, k].TileFrameY = 66;
                            }
                            if (num4 == 1) {
                                Main.tile[i, k].TileFrameX = 22;
                                Main.tile[i, k].TileFrameY = 88;
                            }
                            if (num4 == 2) {
                                Main.tile[i, k].TileFrameX = 22;
                                Main.tile[i, k].TileFrameY = 110;
                            }
                            break;
                        case 5:
                            if (num4 == 0) {
                                Main.tile[i, k].TileFrameX = 88;
                                Main.tile[i, k].TileFrameY = 0;
                            }
                            if (num4 == 1) {
                                Main.tile[i, k].TileFrameX = 88;
                                Main.tile[i, k].TileFrameY = 22;
                            }
                            if (num4 == 2) {
                                Main.tile[i, k].TileFrameX = 88;
                                Main.tile[i, k].TileFrameY = 44;
                            }
                            break;
                        case 6:
                            if (num4 == 0) {
                                Main.tile[i, k].TileFrameX = 66;
                                Main.tile[i, k].TileFrameY = 66;
                            }
                            if (num4 == 1) {
                                Main.tile[i, k].TileFrameX = 66;
                                Main.tile[i, k].TileFrameY = 88;
                            }
                            if (num4 == 2) {
                                Main.tile[i, k].TileFrameX = 66;
                                Main.tile[i, k].TileFrameY = 110;
                            }
                            break;
                        case 7:
                            if (num4 == 0) {
                                Main.tile[i, k].TileFrameX = 110;
                                Main.tile[i, k].TileFrameY = 66;
                            }
                            if (num4 == 1) {
                                Main.tile[i, k].TileFrameX = 110;
                                Main.tile[i, k].TileFrameY = 88;
                            }
                            if (num4 == 2) {
                                Main.tile[i, k].TileFrameX = 110;
                                Main.tile[i, k].TileFrameY = 110;
                            }
                            break;
                        default:
                            if (num4 == 0) {
                                Main.tile[i, k].TileFrameX = 0;
                                Main.tile[i, k].TileFrameY = 0;
                            }
                            if (num4 == 1) {
                                Main.tile[i, k].TileFrameX = 0;
                                Main.tile[i, k].TileFrameY = 22;
                            }
                            if (num4 == 2) {
                                Main.tile[i, k].TileFrameX = 0;
                                Main.tile[i, k].TileFrameY = 44;
                            }
                            break;
                    }

                    if (num5 == 5 || num5 == 7) {
                        tile = Main.tile[i - 1, k];
                        tile.HasTile = true;
                        Main.tile[i - 1, k].TileType = 5;
                        Main.tile[i - 1, k].UseBlockColors(cache);
                        num4 = genRand.Next(3);
                        if (genRand.Next(3) < 2 && !flag2) {
                            if (Main.tile[i, j].TileType == 147 && (GeneratedStorage.PineConeAddedToWorld || genRand.NextBool(8))) {
                                if (genRand.NextChance(1.0)) {
                                    num4 = 3;
                                }
                                if (num4 == 3) {
                                    Main.tile[i - 1, k].TileFrameX = 44;
                                    Main.tile[i - 1, k].TileFrameY = 264;
                                }
                            }

                            if (num4 == 0) {
                                Main.tile[i - 1, k].TileFrameX = 44;
                                Main.tile[i - 1, k].TileFrameY = 198;
                            }

                            if (num4 == 1) {
                                Main.tile[i - 1, k].TileFrameX = 44;
                                Main.tile[i - 1, k].TileFrameY = 220;
                            }

                            if (num4 == 2) {
                                Main.tile[i - 1, k].TileFrameX = 44;
                                Main.tile[i - 1, k].TileFrameY = 242;
                            }
                        }
                        else {
                            if (num4 == 0) {
                                Main.tile[i - 1, k].TileFrameX = 66;
                                Main.tile[i - 1, k].TileFrameY = 0;
                            }

                            if (num4 == 1) {
                                Main.tile[i - 1, k].TileFrameX = 66;
                                Main.tile[i - 1, k].TileFrameY = 22;
                            }

                            if (num4 == 2) {
                                Main.tile[i - 1, k].TileFrameX = 66;
                                Main.tile[i - 1, k].TileFrameY = 44;
                            }
                        }
                    }

                    if (num5 != 6 && num5 != 7)
                        continue;

                    tile = Main.tile[i + 1, k];
                    tile.HasTile = true;
                    Main.tile[i + 1, k].TileType = 5;
                    Main.tile[i + 1, k].UseBlockColors(cache);
                    num4 = genRand.Next(3);
                    if (genRand.Next(3) < 2 && !flag2) {
                        if (Main.tile[i, j].TileType == 147 && (GeneratedStorage.PineConeAddedToWorld || genRand.NextBool(8))) {
                            if (genRand.NextChance(1.0)) {
                                num4 = 3;
                            }
                            if (num4 == 3) {
                                Main.tile[i + 1, k].TileFrameX = 66;
                                Main.tile[i + 1, k].TileFrameY = 264;
                            }
                        }

                        if (num4 == 0) {
                            Main.tile[i + 1, k].TileFrameX = 66;
                            Main.tile[i + 1, k].TileFrameY = 198;
                        }

                        if (num4 == 1) {
                            Main.tile[i + 1, k].TileFrameX = 66;
                            Main.tile[i + 1, k].TileFrameY = 220;
                        }

                        if (num4 == 2) {
                            Main.tile[i + 1, k].TileFrameX = 66;
                            Main.tile[i + 1, k].TileFrameY = 242;
                        }
                    }
                    else {
                        if (num4 == 0) {
                            Main.tile[i + 1, k].TileFrameX = 88;
                            Main.tile[i + 1, k].TileFrameY = 66;
                        }

                        if (num4 == 1) {
                            Main.tile[i + 1, k].TileFrameX = 88;
                            Main.tile[i + 1, k].TileFrameY = 88;
                        }

                        if (num4 == 2) {
                            Main.tile[i + 1, k].TileFrameX = 88;
                            Main.tile[i + 1, k].TileFrameY = 110;
                        }
                    }
                }

                int num6 = genRand.Next(3);
                bool flag5 = false;
                bool flag6 = false;
                if (Main.tile[i - 1, j].HasUnactuatedTile && !Main.tile[i - 1, j].IsHalfBlock && Main.tile[i - 1, j].Slope == 0 && WorldGen.IsTileTypeFitForTree(Main.tile[i - 1, j].TileType))
                    flag5 = true;

                if (Main.tile[i + 1, j].HasUnactuatedTile && !Main.tile[i + 1, j].IsHalfBlock && Main.tile[i + 1, j].Slope == 0 && WorldGen.IsTileTypeFitForTree(Main.tile[i + 1, j].TileType))
                    flag6 = true;

                if (!flag5) {
                    if (num6 == 0)
                        num6 = 2;

                    if (num6 == 1)
                        num6 = 3;
                }

                if (!flag6) {
                    if (num6 == 0)
                        num6 = 1;

                    if (num6 == 2)
                        num6 = 3;
                }

                if (flag5 && !flag6)
                    num6 = 2;

                if (flag6 && !flag5)
                    num6 = 1;

                if (num6 == 0 || num6 == 1) {
                    Tile tile = Main.tile[i + 1, j - 1];
                    tile.HasTile = true;
                    Main.tile[i + 1, j - 1].TileType = 5;
                    Main.tile[i + 1, j - 1].UseBlockColors(cache);
                    num4 = genRand.Next(3);
                    if (num4 == 0) {
                        Main.tile[i + 1, j - 1].TileFrameX = 22;
                        Main.tile[i + 1, j - 1].TileFrameY = 132;
                    }

                    if (num4 == 1) {
                        Main.tile[i + 1, j - 1].TileFrameX = 22;
                        Main.tile[i + 1, j - 1].TileFrameY = 154;
                    }

                    if (num4 == 2) {
                        Main.tile[i + 1, j - 1].TileFrameX = 22;
                        Main.tile[i + 1, j - 1].TileFrameY = 176;
                    }
                }

                if (num6 == 0 || num6 == 2) {
                    Tile tile = Main.tile[i - 1, j - 1];
                    tile.HasTile = true;
                    Main.tile[i - 1, j - 1].TileType = 5;
                    Main.tile[i - 1, j - 1].UseBlockColors(cache);
                    num4 = genRand.Next(3);
                    if (num4 == 0) {
                        Main.tile[i - 1, j - 1].TileFrameX = 44;
                        Main.tile[i - 1, j - 1].TileFrameY = 132;
                    }

                    if (num4 == 1) {
                        Main.tile[i - 1, j - 1].TileFrameX = 44;
                        Main.tile[i - 1, j - 1].TileFrameY = 154;
                    }

                    if (num4 == 2) {
                        Main.tile[i - 1, j - 1].TileFrameX = 44;
                        Main.tile[i - 1, j - 1].TileFrameY = 176;
                    }
                }

                num4 = genRand.Next(3);
                switch (num6) {
                    case 0:
                        if (num4 == 0) {
                            Main.tile[i, j - 1].TileFrameX = 88;
                            Main.tile[i, j - 1].TileFrameY = 132;
                        }
                        if (num4 == 1) {
                            Main.tile[i, j - 1].TileFrameX = 88;
                            Main.tile[i, j - 1].TileFrameY = 154;
                        }
                        if (num4 == 2) {
                            Main.tile[i, j - 1].TileFrameX = 88;
                            Main.tile[i, j - 1].TileFrameY = 176;
                        }
                        break;
                    case 1:
                        if (num4 == 0) {
                            Main.tile[i, j - 1].TileFrameX = 0;
                            Main.tile[i, j - 1].TileFrameY = 132;
                        }
                        if (num4 == 1) {
                            Main.tile[i, j - 1].TileFrameX = 0;
                            Main.tile[i, j - 1].TileFrameY = 154;
                        }
                        if (num4 == 2) {
                            Main.tile[i, j - 1].TileFrameX = 0;
                            Main.tile[i, j - 1].TileFrameY = 176;
                        }
                        break;
                    case 2:
                        if (num4 == 0) {
                            Main.tile[i, j - 1].TileFrameX = 66;
                            Main.tile[i, j - 1].TileFrameY = 132;
                        }
                        if (num4 == 1) {
                            Main.tile[i, j - 1].TileFrameX = 66;
                            Main.tile[i, j - 1].TileFrameY = 154;
                        }
                        if (num4 == 2) {
                            Main.tile[i, j - 1].TileFrameX = 66;
                            Main.tile[i, j - 1].TileFrameY = 176;
                        }
                        break;
                }

                if (genRand.Next(13) != 0 && !flag2) {
                    num4 = genRand.Next(3);

                    if (num4 == 0) {
                        Main.tile[i, j - num2].TileFrameX = 22;
                        Main.tile[i, j - num2].TileFrameY = 198;
                    }

                    if (num4 == 1) {
                        Main.tile[i, j - num2].TileFrameX = 22;
                        Main.tile[i, j - num2].TileFrameY = 220;
                    }

                    if (num4 == 2) {
                        Main.tile[i, j - num2].TileFrameX = 22;
                        Main.tile[i, j - num2].TileFrameY = 242;
                    }

                }
                else {
                    num4 = genRand.Next(3);

                    if (num4 == 0) {
                        Main.tile[i, j - num2].TileFrameX = 0;
                        Main.tile[i, j - num2].TileFrameY = 198;
                    }

                    if (num4 == 1) {
                        Main.tile[i, j - num2].TileFrameX = 0;
                        Main.tile[i, j - num2].TileFrameY = 220;
                    }

                    if (num4 == 2) {
                        Main.tile[i, j - num2].TileFrameX = 0;
                        Main.tile[i, j - num2].TileFrameY = 242;
                    }
                }

                WorldGen.RangeFrame(i - 2, j - num2 - 1, i + 2, j + 1);
                if (Main.netMode == 2)
                    NetMessage.SendTileSquare(-1, i - 1, j - num2, 3, num2);

                return true;
            }
        }

        return false;
    }

    private int On_WorldGen_GetTreeFrame(On_WorldGen.orig_GetTreeFrame orig, Tile t) {
        if (t.TileFrameY == 220)
            return 1;

        if (t.TileFrameY == 242)
            return 2;

        if (t.TileFrameY == 264)
            return 3;

        return 0;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_treeWindCounter")]
    public extern static ref double TileDrawing_treeWindCounter(TileDrawing self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_rand")]
    public extern static ref UnifiedRandom TileDrawing_rand(TileDrawing self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_specialsCount")]
    public extern static ref int[] TileDrawing_specialsCount(TileDrawing self);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_specialPositions")]
    public extern static ref Point[][] TileDrawing_specialPositions(TileDrawing self);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "EmitTreeLeaves")]
    public extern static void TileDrawing_EmitTreeLeaves(TileDrawing self, int tilePosX, int tilePosY, int grassPosX, int grassPosY);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetWindCycle")]
    public extern static float TileDrawing_GetWindCycle(TileDrawing self, int x, int y, double windCounter);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetTreeTopTexture")]
    public extern static Texture2D TileDrawing_GetTreeTopTexture(TileDrawing self, int treeTextureIndex, int treeTextureStyle, byte tileColor);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetTreeBranchTexture")]
    public extern static Texture2D TileDrawing_GetTreeBranchTexture(TileDrawing self, int treeTextureIndex, int treeTextureStyle, byte tileColor);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetPalmTreeBiome")]
    public extern static int TileDrawing_GetPalmTreeBiome(TileDrawing self, int tileX, int tileY);

    private void On_TileDrawing_DrawTrees(On_TileDrawing.orig_DrawTrees orig, TileDrawing self) {
        Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
        Vector2 zero = Vector2.Zero;
        int num = 0;
        int num2 = TileDrawing_specialsCount(self)[num];
        float num3 = 0.08f;
        float num4 = 0.06f;
        for (int i = 0; i < num2; i++) {
            Point point = TileDrawing_specialPositions(self)[num][i];
            int x = point.X;
            int y = point.Y;
            Tile tile = Main.tile[x, y];
            if (tile == null || !tile.HasTile)
                continue;

            ushort type = tile.TileType;
            short frameX = tile.TileFrameX;
            short frameY = tile.TileFrameY;
            bool flag = tile.WallType > 0;
            WorldGen.GetTreeFoliageDataMethod getTreeFoliageDataMethod = null;
            try {
                bool flag2 = false;
                switch (type) {
                    case 5:
                        flag2 = true;
                        getTreeFoliageDataMethod = WorldGen.GetCommonTreeFoliageData;
                        break;
                    case 583:
                    case 584:
                    case 585:
                    case 586:
                    case 587:
                    case 588:
                    case 589:
                        flag2 = true;
                        getTreeFoliageDataMethod = WorldGen.GetGemTreeFoliageData;
                        break;
                    case 596:
                    case 616:
                        flag2 = true;
                        getTreeFoliageDataMethod = WorldGen.GetVanityTreeFoliageData;
                        break;
                    case 634:
                        flag2 = true;
                        getTreeFoliageDataMethod = WorldGen.GetAshTreeFoliageData;
                        break;
                }

                if (flag2 && frameY >= 198 && frameX >= 22) {
                    int treeFrame = WorldGen.GetTreeFrame(tile);
                    switch (frameX) {
                        case 22: {
                            // top
                            int treeStyle3 = 0;
                            int topTextureFrameWidth3 = 80;
                            int topTextureFrameHeight3 = 80;
                            int num13 = 0;
                            int grassPosX = x + num13;
                            int floorY3 = y;
                            if (!getTreeFoliageDataMethod(x, y, num13, ref treeFrame, ref treeStyle3, out floorY3, out topTextureFrameWidth3, out topTextureFrameHeight3))
                                continue;

                            TileDrawing_EmitTreeLeaves(self, x, y, grassPosX, floorY3);
                            if (treeStyle3 == 14) {
                                float num14 = (float)TileDrawing_rand(self).Next(28, 42) * 0.005f;
                                num14 += (float)(270 - Main.mouseTextColor) / 1000f;
                                if (tile.TileColor == 0) {
                                    Lighting.AddLight(x, y, 0.1f, 0.2f + num14 / 2f, 0.7f + num14);
                                }
                                else {
                                    Color color5 = WorldGen.paintColor(tile.TileColor);
                                    float r3 = (float)(int)color5.R / 255f;
                                    float g3 = (float)(int)color5.G / 255f;
                                    float b3 = (float)(int)color5.B / 255f;
                                    Lighting.AddLight(x, y, r3, g3, b3);
                                }
                            }

                            byte tileColor3 = tile.TileColor;
                            Texture2D treeTopTexture = TileDrawing_GetTreeTopTexture(self, treeStyle3, 0, tileColor3);
                            Vector2 vector = (vector = new Vector2(x * 16 - (int)unscaledPosition.X + 8, y * 16 - (int)unscaledPosition.Y + 16) + zero);
                            float num15 = 0f;
                            if (!flag)
                                num15 = TileDrawing_GetWindCycle(self, x, y, TileDrawing_treeWindCounter(self));

                            vector.X += num15 * 2f;
                            vector.Y += Math.Abs(num15) * 2f;
                            Color color6 = Lighting.GetColor(x, y);
                            if (tile.IsTileFullbright)
                                color6 = Color.White;

                            Main.spriteBatch.Draw(treeTopTexture, vector, new Rectangle(treeFrame * (topTextureFrameWidth3 + 2), 0, topTextureFrameWidth3, topTextureFrameHeight3), color6, num15 * num3, new Vector2(topTextureFrameWidth3 / 2, topTextureFrameHeight3), 1f, SpriteEffects.None, 0f);
                            if (type == 634) {
                                Texture2D value3 = TextureAssets.GlowMask[316].Value;
                                Color white3 = Color.White;
                                Main.spriteBatch.Draw(value3, vector, new Rectangle(treeFrame * (topTextureFrameWidth3 + 2), 0, topTextureFrameWidth3, topTextureFrameHeight3), white3, num15 * num3, new Vector2(topTextureFrameWidth3 / 2, topTextureFrameHeight3), 1f, SpriteEffects.None, 0f);
                            }

                            break;
                        }
                        case 44: {
                            // left
                            int treeStyle2 = 0;
                            int num9 = x;
                            int floorY2 = y;
                            int num10 = 1;
                            if (!getTreeFoliageDataMethod(x, y, num10, ref treeFrame, ref treeStyle2, out floorY2, out var _, out var _))
                                continue;

                            TileDrawing_EmitTreeLeaves(self, x, y, num9 + num10, floorY2);
                            if (treeStyle2 == 14) {
                                float num11 = (float)TileDrawing_rand(self).Next(28, 42) * 0.005f;
                                num11 += (float)(270 - Main.mouseTextColor) / 1000f;
                                if (tile.TileColor == 0) {
                                    Lighting.AddLight(x, y, 0.1f, 0.2f + num11 / 2f, 0.7f + num11);
                                }
                                else {
                                    Color color3 = WorldGen.paintColor(tile.TileColor);
                                    float r2 = (float)(int)color3.R / 255f;
                                    float g2 = (float)(int)color3.G / 255f;
                                    float b2 = (float)(int)color3.B / 255f;
                                    Lighting.AddLight(x, y, r2, g2, b2);
                                }
                            }

                            byte tileColor2 = tile.TileColor;
                            Texture2D treeBranchTexture2 = TileDrawing_GetTreeBranchTexture(self, treeStyle2, 0, tileColor2);
                            Vector2 position2 = new Vector2(x * 16, y * 16) - unscaledPosition.Floor() + zero + new Vector2(16f, 12f);
                            float num12 = 0f;
                            if (!flag)
                                num12 = TileDrawing_GetWindCycle(self, x, y, TileDrawing_treeWindCounter(self));

                            if (num12 > 0f)
                                position2.X += num12;

                            position2.X += Math.Abs(num12) * 2f;
                            Color color4 = Lighting.GetColor(x, y);
                            if (tile.IsTileFullbright)
                                color4 = Color.White;

                            Main.spriteBatch.Draw(treeBranchTexture2, position2, new Rectangle(0, treeFrame * 42, 40, 40), color4, num12 * num4, new Vector2(40f, 24f), 1f, SpriteEffects.None, 0f);
                            if (type == 634) {
                                Texture2D value2 = TextureAssets.GlowMask[317].Value;
                                Color white2 = Color.White;
                                Main.spriteBatch.Draw(value2, position2, new Rectangle(0, treeFrame * 42, 40, 40), white2, num12 * num4, new Vector2(40f, 24f), 1f, SpriteEffects.None, 0f);
                            }

                            break;
                        }
                        case 66: {
                            // right
                            int treeStyle = 0;
                            int num5 = x;
                            int floorY = y;
                            int num6 = -1;
                            if (!getTreeFoliageDataMethod(x, y, num6, ref treeFrame, ref treeStyle, out floorY, out var _, out var _))
                                continue;

                            TileDrawing_EmitTreeLeaves(self, x, y, num5 + num6, floorY);
                            if (treeStyle == 14) {
                                float num7 = (float)TileDrawing_rand(self).Next(28, 42) * 0.005f;
                                num7 += (float)(270 - Main.mouseTextColor) / 1000f;
                                if (tile.TileColor == 0) {
                                    Lighting.AddLight(x, y, 0.1f, 0.2f + num7 / 2f, 0.7f + num7);
                                }
                                else {
                                    Color color = WorldGen.paintColor(tile.TileColor);
                                    float r = (float)(int)color.R / 255f;
                                    float g = (float)(int)color.G / 255f;
                                    float b = (float)(int)color.B / 255f;
                                    Lighting.AddLight(x, y, r, g, b);
                                }
                            }

                            byte tileColor = tile.TileColor;
                            Texture2D treeBranchTexture = TileDrawing_GetTreeBranchTexture(self, treeStyle, 0, tileColor);
                            Vector2 position = new Vector2(x * 16, y * 16) - unscaledPosition.Floor() + zero + new Vector2(0f, 18f);
                            float num8 = 0f;
                            if (!flag)
                                num8 = TileDrawing_GetWindCycle(self, x, y, TileDrawing_treeWindCounter(self));

                            if (num8 < 0f)
                                position.X += num8;

                            position.X -= Math.Abs(num8) * 2f;
                            Color color2 = Lighting.GetColor(x, y);
                            if (tile.IsTileFullbright)
                                color2 = Color.White;

                            Main.spriteBatch.Draw(treeBranchTexture, position, new Rectangle(42, treeFrame * 42, 40, 40), color2, num8 * num4, new Vector2(0f, 30f), 1f, SpriteEffects.None, 0f);
                            if (type == 634) {
                                Texture2D value = TextureAssets.GlowMask[317].Value;
                                Color white = Color.White;
                                Main.spriteBatch.Draw(value, position, new Rectangle(42, treeFrame * 42, 40, 40), white, num8 * num4, new Vector2(0f, 30f), 1f, SpriteEffects.None, 0f);
                            }

                            break;
                        }
                    }
                }

                if (type == 323 && frameX >= 88 && frameX <= 132) {
                    int num16 = 0;
                    switch (frameX) {
                        case 110:
                            num16 = 1;
                            break;
                        case 132:
                            num16 = 2;
                            break;
                    }

                    int treeTextureIndex = 15;
                    int num17 = 80;
                    int num18 = 80;
                    int num19 = 32;
                    int num20 = 0;
                    int palmTreeBiome = TileDrawing_GetPalmTreeBiome(self, x, y);
                    int y2 = palmTreeBiome * 82;
                    if (palmTreeBiome >= 4 && palmTreeBiome <= 7) {
                        treeTextureIndex = 21;
                        num17 = 114;
                        num18 = 98;
                        y2 = (palmTreeBiome - 4) * 98;
                        num19 = 48;
                        num20 = 2;
                    }

                    int frameY2 = Main.tile[x, y].TileFrameY;
                    byte tileColor4 = tile.TileColor;
                    Texture2D treeTopTexture2 = TileDrawing_GetTreeTopTexture(self, treeTextureIndex, palmTreeBiome, tileColor4);
                    Vector2 position3 = new Vector2(x * 16 - (int)unscaledPosition.X - num19 + frameY2 + num17 / 2, y * 16 - (int)unscaledPosition.Y + 16 + num20) + zero;
                    float num21 = 0f;
                    if (!flag)
                        num21 = TileDrawing_GetWindCycle(self, x, y, TileDrawing_treeWindCounter(self));

                    position3.X += num21 * 2f;
                    position3.Y += Math.Abs(num21) * 2f;
                    Color color7 = Lighting.GetColor(x, y);
                    if (tile.IsTileFullbright)
                        color7 = Color.White;

                    Main.spriteBatch.Draw(treeTopTexture2, position3, new Rectangle(num16 * (num17 + 2), y2, num17, num18), color7, num21 * num3, new Vector2(num17 / 2, num18), 1f, SpriteEffects.None, 0f);
                }
            }
            catch {
            }
        }
    }

    private bool On_WorldGen_GetCommonTreeFoliageData(On_WorldGen.orig_GetCommonTreeFoliageData orig, int i, int j, int xoffset, ref int treeFrame, ref int treeStyle, out int floorY, out int topTextureFrameWidth, out int topTextureFrameHeight) {
        _ = Main.tile[i, j];
        int num = i + xoffset;
        topTextureFrameWidth = 80;
        topTextureFrameHeight = 80;
        floorY = j;
        int num2 = 0;
        for (int k = 0; k < 100; k++) {
            floorY = j + k;
            Tile tile = Main.tile[num, floorY];
            if (tile == null)
                return false;

            switch (tile.TileType) {
                case 2:
                case 477: {
                    int num4 = 0;
                    num4 = ((num <= Main.treeX[0]) ? WorldGen.TreeTops.GetTreeStyle(0) : ((num <= Main.treeX[1]) ? WorldGen.TreeTops.GetTreeStyle(1) : ((num > Main.treeX[2]) ? WorldGen.TreeTops.GetTreeStyle(3) : WorldGen.TreeTops.GetTreeStyle(2))));
                    switch (num4) {
                        case 0:
                            treeStyle = 0;
                            break;
                        case 5:
                            treeStyle = 10;
                            break;
                        default:
                            treeStyle = 5 + num4;
                            break;
                    }

                    return true;
                }
                case 23:
                case 661:
                    treeStyle = 1;
                    return true;
                case 70:
                    treeStyle = 14;
                    return true;
                case 199:
                case 662:
                    treeStyle = 5;
                    return true;
                case 60:
                    topTextureFrameHeight = 96;
                    topTextureFrameWidth = 114;
                    treeStyle = 2;
                    num2 = WorldGen.TreeTops.GetTreeStyle(5);
                    if (num2 == 1) {
                        treeStyle = 11;
                        topTextureFrameWidth = 116;
                    }
                    if ((double)floorY > Main.worldSurface) {
                        treeStyle = 13;
                        topTextureFrameWidth = 116;
                    }
                    return true;
                case 147:
                    treeStyle = 4;
                    num2 = WorldGen.TreeTops.GetTreeStyle(6);
                    if (num2 == 0) {
                        treeStyle = 12;
                        if (i % 10 == 0)
                            treeStyle = 18;
                    }
                    if (num2 == 2 || num2 == 3 || num2 == 32 || num2 == 4 || num2 == 42 || num2 == 5 || num2 == 7) {
                        if (num2 % 2 == 0) {
                            if (i < Main.maxTilesX / 2)
                                treeStyle = 16;
                            else
                                treeStyle = 17;
                        }
                        else if (i > Main.maxTilesX / 2) {
                            treeStyle = 16;
                        }
                        else {
                            treeStyle = 17;
                        }
                    }
                    return true;
                case 109:
                case 492: {
                    topTextureFrameHeight = 140;
                    int num3 = (treeStyle = WorldGen.GetHollowTreeFoliageStyle());
                    if (num3 == 19)
                        topTextureFrameWidth = 120;

                    if (num3 == 20) {
                        treeStyle = 20;
                        if (i % 6 == 1)
                            treeFrame += 3;
                        else if (i % 6 == 2)
                            treeFrame += 6;
                        else if (i % 6 == 3)
                            treeFrame += 9;
                        else if (i % 6 == 4)
                            treeFrame += 12;
                        else if (i % 6 == 5)
                            treeFrame += 15;
                    }
                    else if (i % 3 == 1) {
                        treeFrame += 3;
                    }
                    // Extra patch context.
                    else if (i % 3 == 2) {
                        treeFrame += 6;
                    }

                    return true;
                }
                default:
                    var tree = PlantLoader.Get<ModTree>(TileID.Trees, tile.TileType);
                    if (tree != null) {
                        tree.SetTreeFoliageSettings(tile, ref xoffset, ref treeFrame, ref floorY, ref topTextureFrameWidth, ref topTextureFrameHeight);
                        treeStyle = tile.TileType + ModTree.VanillaTopTextureCount;
                        return true;
                    }

                    break;

            }
        }

        return false;
    }

}