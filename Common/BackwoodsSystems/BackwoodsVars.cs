using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Platforms;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Trees;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace RoA.Common.BackwoodsSystems;

sealed class BackwoodsVars : ModSystem {
    private static float _preDownedBossTimer;
    private static bool _backwoodsAwake;

    private sealed class RemoveUnusedTreeCords2 : GlobalTile {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
            if (!Main.hardMode) {
                if (!fail && !effectOnly && !noItem) {
                    Point position = new(i, j);
                    if (Main.tile[i, j].TileType == TileID.Trees && AllTreesWorldPositions.Contains(position)) {
                        AllTreesWorldPositions.Remove(position);
                        BackwoodsTreeCountInWorld--;
                    }
                }
            }
        }
    }

    public static int BackwoodsTreeCountInWorld { get; internal set; }
    public static List<Point> AllTreesWorldPositions { get; internal set; } = [];

    public static void AddBackwoodsTree(int i, int j) {
        Point position = new(i, j);
        if (!AllTreesWorldPositions.Contains(position)) {
            AllTreesWorldPositions.Add(position);
            BackwoodsTreeCountInWorld++;
        }
    }

    public static int FirstTileYAtCenter { get; internal set; }
    public static int BackwoodsTileForBackground { get; internal set; }
    public static int BackwoodsStartX { get; internal set; }
    public static int BackwoodsHalfSizeX { get; internal set; }

    public static HashSet<ushort> BackwoodsTileTypes { get; } = [(ushort)ModContent.TileType<LivingElderwood>(), (ushort)ModContent.TileType<LivingElderwoodlLeaves>(), (ushort)ModContent.TileType<TreeBranch>(), (ushort)ModContent.TileType<BackwoodsGrass>(), (ushort)ModContent.TileType<BackwoodsGreenMoss>(), (ushort)ModContent.TileType<BackwoodsStone>(), TileID.Dirt];

    public override void Unload() {
        AllTreesWorldPositions.Clear();
        AllTreesWorldPositions = null;
    }

    public override void ClearWorld() => ResetAllFlags();

    public override void SaveWorldData(TagCompound tag) {
        tag[nameof(FirstTileYAtCenter)] = FirstTileYAtCenter;
        tag[nameof(BackwoodsTileForBackground)] = BackwoodsTileForBackground;
        tag[nameof(_preDownedBossTimer)] = _preDownedBossTimer;
        tag[nameof(_backwoodsAwake)] = _backwoodsAwake;
        tag[nameof(BackwoodsStartX)] = BackwoodsStartX;
        tag[nameof(BackwoodsHalfSizeX)] = BackwoodsHalfSizeX;

        if (!Main.hardMode) {
            tag[nameof(BackwoodsTreeCountInWorld)] = BackwoodsTreeCountInWorld;
            for (int i = 0; i < BackwoodsTreeCountInWorld; i++) {
                tag[$"backwoodstreepositionX{i}"] = AllTreesWorldPositions[i].X;
                tag[$"backwoodstreepositionY{i}"] = AllTreesWorldPositions[i].Y;
            }
        }
    }

    public override void LoadWorldData(TagCompound tag) {
        FirstTileYAtCenter = tag.GetInt(nameof(FirstTileYAtCenter));
        BackwoodsTileForBackground = tag.GetInt(nameof(BackwoodsTileForBackground));
        _preDownedBossTimer = tag.GetFloat(nameof(_preDownedBossTimer));
        _backwoodsAwake = tag.GetBool(nameof(_backwoodsAwake));
        BackwoodsStartX = tag.GetInt(nameof(BackwoodsStartX));
        BackwoodsHalfSizeX = tag.GetInt(nameof(BackwoodsHalfSizeX));

        if (!Main.hardMode) {
            BackwoodsTreeCountInWorld = tag.GetInt(nameof(BackwoodsTreeCountInWorld));
            for (int i = 0; i < BackwoodsTreeCountInWorld; i++) {
                int x = tag.GetInt($"backwoodstreepositionX{i}");
                int y = tag.GetInt($"backwoodstreepositionY{i}");
                AllTreesWorldPositions.Add(new Point(x, y));
            }
        }
    }

    private static void ResetAllFlags() {
        BackwoodsHalfSizeX = BackwoodsStartX = FirstTileYAtCenter = BackwoodsTileForBackground = 0;
        _preDownedBossTimer = 0f;
        _backwoodsAwake = false;

        if (!Main.hardMode) {
            BackwoodsTreeCountInWorld = 0;
            AllTreesWorldPositions.Clear();
            AllTreesWorldPositions = [];
        }
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(FirstTileYAtCenter);
        writer.Write(BackwoodsTileForBackground);
        writer.Write(_preDownedBossTimer);
        writer.Write(_backwoodsAwake);
        writer.Write(BackwoodsStartX);
        writer.Write(BackwoodsHalfSizeX);

        if (!Main.hardMode) {
            writer.Write(BackwoodsTreeCountInWorld);
            for (int i = 0; i < BackwoodsTreeCountInWorld; i++) {
                writer.Write(BackwoodsStartX);
                writer.Write(AllTreesWorldPositions[i].X);
                writer.Write(AllTreesWorldPositions[i].Y);
            }
        }
    }

    public override void NetReceive(BinaryReader reader) {
        FirstTileYAtCenter = reader.ReadInt32();
        BackwoodsTileForBackground = reader.ReadInt32();
        _preDownedBossTimer = reader.ReadSingle();
        _backwoodsAwake = reader.ReadBoolean();
        BackwoodsStartX = reader.ReadInt32();
        BackwoodsHalfSizeX = reader.ReadInt32();

        if (!Main.hardMode) {
            BackwoodsStartX = reader.ReadInt16();
            AllTreesWorldPositions.Clear();
            for (int i = 0; i < BackwoodsTreeCountInWorld; i++) {
                AllTreesWorldPositions.Add(new Point(reader.ReadInt16(), reader.ReadInt16()));
            }
        }
    }

    public override void Load() {
        On_WorldGen.GrowTree += On_WorldGen_GrowTree;
    }

    private bool On_WorldGen_GrowTree(On_WorldGen.orig_GrowTree orig, int i, int y) {
        int j;
        for (j = y; TileID.Sets.TreeSapling[Main.tile[i, j].TileType]; j++) {
        }

        if ((Main.tile[i - 1, j - 1].LiquidAmount != 0 || Main.tile[i, j - 1].LiquidAmount != 0 || Main.tile[i + 1, j - 1].LiquidAmount != 0) && !WorldGen.notTheBees)
            return false;

        UnifiedRandom genRand = WorldGen.genRand;
        if (Main.tile[i, j].HasUnactuatedTile && !Main.tile[i, j].IsHalfBlock && Main.tile[i, j].Slope == 0 && WorldGen.IsTileTypeFitForTree(Main.tile[i, j].TileType) && ((Main.remixWorld && (double)j > Main.worldSurface) || Main.tile[i, j - 1].WallType == 0 || WorldGen.DefaultTreeWallTest(Main.tile[i, j - 1].WallType)) && ((Main.tile[i - 1, j].HasTile && WorldGen.IsTileTypeFitForTree(Main.tile[i - 1, j].TileType)) || (Main.tile[i + 1, j].HasActuator && WorldGen.IsTileTypeFitForTree(Main.tile[i + 1, j].TileType)))) {
            TileColorCache cache = Main.tile[i, j].BlockColorAndCoating();
            if (Main.tenthAnniversaryWorld && !WorldGen.gen)
                cache.Color = (byte)genRand.Next(1, 13);

            bool isPrimordialTree = false;
            if (Main.tile[i, j].TileType == ModContent.TileType<BackwoodsGrass>()) {
                isPrimordialTree = true;
            }
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
                    tile.TileFrameNumber = ((byte)genRand.Next(3));
                    tile.HasTile = true;
                    Main.tile[i, k].TileType = 5;
                    Main.tile[i, k].UseBlockColors(cache);
                    if (isPrimordialTree) {
                        BackwoodsVars.AddBackwoodsTree(i, k);
                    }
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
                        Tile tile2 = Main.tile[i - 1, k];
                        tile2.HasTile = true;
                        Main.tile[i - 1, k].TileType = 5;
                        Main.tile[i - 1, k].UseBlockColors(cache);
                        if (isPrimordialTree) {
                            BackwoodsVars.AddBackwoodsTree(i - 1, k);
                        }
                        num4 = genRand.Next(3);
                        if (genRand.Next(3) < 2 && !flag2) {
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
                    if (isPrimordialTree) {
                        BackwoodsVars.AddBackwoodsTree(i + 1, k);
                    }
                    num4 = genRand.Next(3);
                    if (genRand.Next(3) < 2 && !flag2) {
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
                    if (isPrimordialTree) {
                        BackwoodsVars.AddBackwoodsTree(i + 1, j - 1);
                    }
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
                    if (isPrimordialTree) {
                        BackwoodsVars.AddBackwoodsTree(i - 1, j - 1);
                    }
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

    public override void PostUpdateNPCs() {
        if (!_backwoodsAwake) {
            if (NPC.downedBoss2) {
                _backwoodsAwake = true;
            }

            return;
        }

        if (_preDownedBossTimer == -1f || Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }
        _preDownedBossTimer += TimeSystem.LogicDeltaTime;
        if (_preDownedBossTimer >= 5f) {
            _preDownedBossTimer = -1f;

            Color color = Color.LightGreen;
            string text = Language.GetText("Mods.RoA.World.BackwoodsFree" + (WorldGen.crimson ? "1" : "2")).ToString();
            Helper.NewMessage(text, color);

            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendData(MessageID.WorldData);
            }
        }
    }
}
