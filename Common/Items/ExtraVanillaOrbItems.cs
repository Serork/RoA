using Microsoft.Xna.Framework;

using RoA.Common.Configs;
using RoA.Content.Items.Weapons.Magic;
using RoA.Content.Items.Weapons.Nature.PreHardmode;
using RoA.Content.Items.Weapons.Summon;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class ExtraVanillaOrbItems : ILoadable {
    public void Load(Mod mod) {
        On_WorldGen.CheckOrb += On_WorldGen_CheckOrb;
    }

    private void On_WorldGen_CheckOrb(On_WorldGen.orig_CheckOrb orig, int i, int j, int type) {
        //if (Main.tile[i, j] == null)
        //    return;

        short frameX = Main.tile[i, j].TileFrameX;
        bool flag = false;
        if (frameX >= 36)
            flag = true;

        if (WorldGen.destroyObject)
            return;

        int num = i;
        int num2 = j;
        num = ((Main.tile[i, j].TileFrameX != 0 && Main.tile[i, j].TileFrameX != 36) ? (i - 1) : i);
        num2 = ((Main.tile[i, j].TileFrameY != 0) ? (j - 1) : j);
        for (int k = 0; k < 2; k++) {
            for (int l = 0; l < 2; l++) {
                Tile tile = Main.tile[num + k, num2 + l];
                if (tile != null && (!tile.HasUnactuatedTile || tile.TileType != type)) {
                    WorldGen.destroyObject = true;
                    break;
                }
            }

            if (WorldGen.destroyObject)
                break;

            if (type == 12 || type == 639) {
                Tile tile = Main.tile[num + k, num2 + 2];
                if (tile != null && !WorldGen.SolidTileAllowBottomSlope(num + k, num2 + 2)) {
                    WorldGen.destroyObject = true;
                    break;
                }
            }
        }

        if (!WorldGen.destroyObject)
            return;

        bool drop = TileLoader.Drop(i, j, type);
        for (int m = num; m < num + 2; m++) {
            for (int n = num2; n < num2 + 2; n++) {
                if (Main.tile[m, n] != null && Main.tile[m, n].TileType == type)
                    WorldGen.KillTile(m, n);
            }
        }

        var config = ModContent.GetInstance<RoAServerConfig>();
        if (Main.netMode != 1 && !WorldGen.noTileActions) {
            switch (type) {
                case 12:
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 29);
                    break;
                case 639:
                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 109);
                    break;
                case 31:
                    if (flag) {
                        if (config.EvilBiomeExtraItemChance == 0f) {
                            int num3 = Main.rand.Next(7);
                            if (!WorldGen.shadowOrbSmashed)
                                num3 = 0;
                            switch (num3) {
                                case 0: {
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 800, 1, noBroadcast: false, -1);
                                    int stack = WorldGen.genRand.Next(100, 101);
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 97, stack);
                                    break;
                                }
                                case 1:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, /*1256*/ModContent.ItemType<CrimsonRod>(), 1, noBroadcast: false, -1);
                                    break;
                                case 2:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 802, 1, noBroadcast: false, -1);
                                    break;
                                case 3:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 3062, 1, noBroadcast: false, -1);
                                    break;
                                case 4:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 1290, 1, noBroadcast: false, -1);
                                    break;
                                case 5:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, ModContent.ItemType<ArterialSpray>(), 1, noBroadcast: false, -1);
                                    break;
                                case 6:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, ModContent.ItemType<GastroIntestinalMallet>(), 1, noBroadcast: false, -1);
                                    break;
                            }
                        }
                        else {
                            int num4 = Main.rand.Next(7);
                            if (!WorldGen.shadowOrbSmashed)
                                num4 = 0;

                            switch (num4) {
                                case 0: {
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 800, 1, noBroadcast: false, -1);
                                    int stack = WorldGen.genRand.Next(100, 101);
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 97, stack);
                                    break;
                                }
                                case 1:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, /*1256*/ModContent.ItemType<CrimsonRod>(), 1, noBroadcast: false, -1);
                                    break;
                                case 2:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 802, 1, noBroadcast: false, -1);
                                    break;
                                case 3:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 3062, 1, noBroadcast: false, -1);
                                    break;
                                case 4:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 1290, 1, noBroadcast: false, -1);
                                    break;
                                case 5:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, ModContent.ItemType<ArterialSpray>(), 1, noBroadcast: false, -1);
                                    break;
                                case 6:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, ModContent.ItemType<GastroIntestinalMallet>(), 1, noBroadcast: false, -1);
                                    break;
                            }

                            if (Main.rand.NextChance(config.EvilBiomeExtraItemChance)) {
                                int num42 = num4;
                                num4 = Main.rand.Next(7);
                                if (!WorldGen.shadowOrbSmashed)
                                    num4 = 0;

                                while (num4 == num42) {
                                    num4 = Main.rand.Next(7);
                                }

                                switch (num4) {
                                    case 0: {
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 800, 1, noBroadcast: false, -1);
                                        int stack = WorldGen.genRand.Next(100, 101);
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 97, stack);
                                        break;
                                    }
                                    case 1:
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, /*1256*/ModContent.ItemType<CrimsonRod>(), 1, noBroadcast: false, -1);
                                        break;
                                    case 2:
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 802, 1, noBroadcast: false, -1);
                                        break;
                                    case 3:
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 3062, 1, noBroadcast: false, -1);
                                        break;
                                    case 4:
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 1290, 1, noBroadcast: false, -1);
                                        break;
                                    case 5:
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, ModContent.ItemType<ArterialSpray>(), 1, noBroadcast: false, -1);
                                        break;
                                    case 6:
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, ModContent.ItemType<GastroIntestinalMallet>(), 1, noBroadcast: false, -1);
                                        break;
                                }
                            }
                        }
                    }
                    else {
                        if (config.EvilBiomeExtraItemChance == 0f) {
                            int num4 = Main.rand.Next(7);
                            if (!WorldGen.shadowOrbSmashed)
                                num4 = 0;

                            switch (num4) {
                                case 0: {
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 96, 1, noBroadcast: false, -1);
                                    int stack2 = WorldGen.genRand.Next(100, 101);
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 97, stack2);
                                    break;
                                }
                                case 1:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, /*64*/ModContent.ItemType<Vilethorn>(), 1, noBroadcast: false, -1);
                                    break;
                                case 2:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 162, 1, noBroadcast: false, -1);
                                    break;
                                case 3:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 115, 1, noBroadcast: false, -1);
                                    break;
                                case 4:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 111, 1, noBroadcast: false, -1);
                                    break;
                                case 5:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, ModContent.ItemType<Bookworms>(), 1, noBroadcast: false, -1);
                                    break;
                                case 6:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, ModContent.ItemType<PlanetomaStaff>(), 1, noBroadcast: false, -1);
                                    break;
                            }
                        }
                        else {
                            int num4 = Main.rand.Next(7);
                            if (!WorldGen.shadowOrbSmashed)
                                num4 = 0;

                            switch (num4) {
                                case 0: {
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 96, 1, noBroadcast: false, -1);
                                    int stack2 = WorldGen.genRand.Next(100, 101);
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 97, stack2);
                                    break;
                                }
                                case 1:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, /*64*/ModContent.ItemType<Vilethorn>(), 1, noBroadcast: false, -1);
                                    break;
                                case 2:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 162, 1, noBroadcast: false, -1);
                                    break;
                                case 3:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 115, 1, noBroadcast: false, -1);
                                    break;
                                case 4:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 111, 1, noBroadcast: false, -1);
                                    break;
                                case 5:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, ModContent.ItemType<Bookworms>(), 1, noBroadcast: false, -1);
                                    break;
                                case 6:
                                    Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, ModContent.ItemType<PlanetomaStaff>(), 1, noBroadcast: false, -1);
                                    break;
                            }

                            if (Main.rand.NextChance(config.EvilBiomeExtraItemChance)) {
                                int num42 = num4;
                                num4 = Main.rand.Next(7);
                                if (!WorldGen.shadowOrbSmashed)
                                    num4 = 0;

                                while (num4 == num42) {
                                    num4 = Main.rand.Next(7);
                                }

                                switch (num4) {
                                    case 0: {
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 96, 1, noBroadcast: false, -1);
                                        int stack2 = WorldGen.genRand.Next(100, 101);
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 97, stack2);
                                        break;
                                    }
                                    case 1:
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, /*64*/ModContent.ItemType<Vilethorn>(), 1, noBroadcast: false, -1);
                                        break;
                                    case 2:
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 162, 1, noBroadcast: false, -1);
                                        break;
                                    case 3:
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 115, 1, noBroadcast: false, -1);
                                        break;
                                    case 4:
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, 111, 1, noBroadcast: false, -1);
                                        break;
                                    case 5:
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, ModContent.ItemType<Bookworms>(), 1, noBroadcast: false, -1);
                                        break;
                                    case 6:
                                        Item.NewItem(WorldGen.GetItemSource_FromTileBreak(num, num2), num * 16, num2 * 16, 32, 32, ModContent.ItemType<PlanetomaStaff>(), 1, noBroadcast: false, -1);
                                        break;
                                }
                            }
                        }
                    }
                    WorldGen.shadowOrbSmashed = true;
                    WorldGen.shadowOrbCount++;
                    if (WorldGen.shadowOrbCount >= 3) {
                        if (!(NPC.AnyNPCs(266) && flag) && (!NPC.AnyNPCs(13) || flag)) {
                            WorldGen.shadowOrbCount = 0;
                            float num5 = num * 16;
                            float num6 = num2 * 16;
                            float num7 = -1f;
                            int plr = 0;
                            for (int num8 = 0; num8 < 255; num8++) {
                                float num9 = Math.Abs(Main.player[num8].position.X - num5) + Math.Abs(Main.player[num8].position.Y - num6);
                                if (num9 < num7 || num7 == -1f) {
                                    plr = num8;
                                    num7 = num9;
                                }
                            }

                            if (flag)
                                NPC.SpawnOnPlayer(plr, 266);
                            else
                                NPC.SpawnOnPlayer(plr, 13);
                        }
                    }
                    else {
                        LocalizedText localizedText = Lang.misc[10];
                        if (WorldGen.shadowOrbCount == 2)
                            localizedText = Lang.misc[11];

                        if (Main.netMode == 0)
                            Main.NewText(localizedText.ToString(), 50, byte.MaxValue, 130);
                        else if (Main.netMode == 2)
                            ChatHelper.BroadcastChatMessage(NetworkText.FromKey(localizedText.Key), new Color(50, 255, 130));
                    }
                    AchievementsHelper.NotifyProgressionEvent(7);
                    break;
            }
        }

        if (flag)
            SoundEngine.PlaySound(SoundID.NPCDeath1, new Vector2(i * 16, j * 16));
        else
            SoundEngine.PlaySound(SoundID.Shatter, new Vector2(i * 16, j * 16));

        WorldGen.destroyObject = false;
    }

    public void Unload() { }
}
