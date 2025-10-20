using RoA.Content.Tiles.Miscellaneous;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

using static RoA.Common.WorldCommon;

namespace RoA.Common.NPCs;

sealed class GuideHelpTexts : ILoadable {
    void ILoadable.Load(Mod mod) {
        On_Main.HelpText += On_Main_HelpText;
    }

    public static void Update() {
        if (NewGuideHelpTextID.DryadCocoon1Condition() && !WorldCommon.ShownGuideTexts[NewGuideHelpTextID.DryadCocoon1]) {
            WorldCommon.Remind();
            return;
        }
        if (NewGuideHelpTextID.TarCondition() && !WorldCommon.ShownGuideTexts[NewGuideHelpTextID.Tar]) {
            WorldCommon.Remind();
            return;
        }
        if (NewGuideHelpTextID.DryadCocoon2Condition() && !WorldCommon.ShownGuideTexts[NewGuideHelpTextID.DryadCocoon2]) {
            WorldCommon.Remind();
            return;
        }
        if (NewGuideHelpTextID.BackwoodsTreesCondition() && !WorldCommon.ShownGuideTexts[NewGuideHelpTextID.BackwoodsTrees]) {
            WorldCommon.Remind();
            return;
        }

        if (NewGuideHelpTextID.BackwoodsLootRoomsCondition() && !WorldCommon.ShownGuideTexts[NewGuideHelpTextID.BackwoodsLootRooms]) {
            WorldCommon.Remind();
            return;
        }

        if (NewGuideHelpTextID.LothorCondition() && !WorldCommon.ShownGuideTexts[NewGuideHelpTextID.Lothor]) {
            WorldCommon.Remind();
            return;
        }
    }

    private void On_Main_HelpText(On_Main.orig_HelpText orig) {
        //orig();

        if (NewGuideHelpTextID.DryadCocoon1Condition() && !WorldCommon.ShownGuideTexts[NewGuideHelpTextID.DryadCocoon1]) {
            Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{1}");
            WorldCommon.ShownGuideTexts[NewGuideHelpTextID.DryadCocoon1] = true;
            return;
        }
        if (NewGuideHelpTextID.TarCondition() && !WorldCommon.ShownGuideTexts[NewGuideHelpTextID.Tar]) {
            Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{1}");
            WorldCommon.ShownGuideTexts[NewGuideHelpTextID.Tar] = true;
            return;
        }
        if (NewGuideHelpTextID.DryadCocoon2Condition() && !WorldCommon.ShownGuideTexts[NewGuideHelpTextID.DryadCocoon2]) {
            Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{2}");
            WorldCommon.ShownGuideTexts[NewGuideHelpTextID.DryadCocoon2] = true;
            return;
        }
        if (NewGuideHelpTextID.BackwoodsTreesCondition() && !WorldCommon.ShownGuideTexts[NewGuideHelpTextID.BackwoodsTrees]) {
            Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{3}");
            WorldCommon.ShownGuideTexts[NewGuideHelpTextID.BackwoodsTrees] = true;
            return;
        }

        if (NewGuideHelpTextID.BackwoodsLootRoomsCondition() && !WorldCommon.ShownGuideTexts[NewGuideHelpTextID.BackwoodsLootRooms]) {
            Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{4}");
            WorldCommon.ShownGuideTexts[NewGuideHelpTextID.BackwoodsLootRooms] = true;
            return;
        }

        if (NewGuideHelpTextID.LothorCondition() && !WorldCommon.ShownGuideTexts[NewGuideHelpTextID.Lothor]) {
            Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{5}");
            WorldCommon.ShownGuideTexts[NewGuideHelpTextID.Lothor] = true;
            return;
        }

        bool flag = false;
        /*
		if (Main.LocalPlayer.statLifeMax > 100)
		*/
        if (Main.LocalPlayer.ConsumedLifeCrystals > 0)
            flag = true;

        bool flag2 = false;
        /*
		if (Main.LocalPlayer.statManaMax > 20)
		*/
        if (Main.LocalPlayer.ConsumedManaCrystals > 0)
            flag2 = true;

        bool flag3 = true;
        bool flag4 = false;
        bool flag5 = false;
        bool flag6 = false;
        bool flag7 = false;
        bool flag8 = false;
        bool flag9 = false;
        bool flag10 = false;
        bool flag11 = false;
        bool flag12 = false;
        bool flag13 = false;
        for (int i = 0; i < 58; i++) {
            if (Main.LocalPlayer.inventory[i].pick > 0 && Main.LocalPlayer.inventory[i].Name != "Copper Pickaxe")
                flag3 = false;
            if (Main.LocalPlayer.inventory[i].axe > 0 && Main.LocalPlayer.inventory[i].Name != "Copper Axe")
                flag3 = false;

            if (Main.LocalPlayer.inventory[i].hammer > 0)
                flag3 = false;
            if (Main.LocalPlayer.inventory[i].type == 11 || Main.LocalPlayer.inventory[i].type == 12 || Main.LocalPlayer.inventory[i].type == 13 || Main.LocalPlayer.inventory[i].type == 14 || Main.LocalPlayer.inventory[i].type == 699 || Main.LocalPlayer.inventory[i].type == 700 || Main.LocalPlayer.inventory[i].type == 701 || Main.LocalPlayer.inventory[i].type == 702)
                flag4 = true;
            if (Main.LocalPlayer.inventory[i].type == 19 || Main.LocalPlayer.inventory[i].type == 20 || Main.LocalPlayer.inventory[i].type == 21 || Main.LocalPlayer.inventory[i].type == 22 || Main.LocalPlayer.inventory[i].type == 703 || Main.LocalPlayer.inventory[i].type == 704 || Main.LocalPlayer.inventory[i].type == 705 || Main.LocalPlayer.inventory[i].type == 706)
                flag5 = true;
            if (Main.LocalPlayer.inventory[i].type == 75)
                flag6 = true;
            if (Main.LocalPlayer.inventory[i].type == 38)
                flag7 = true;
            if (Main.LocalPlayer.inventory[i].type == 68 || Main.LocalPlayer.inventory[i].type == 70 || Main.LocalPlayer.inventory[i].type == 1330 || Main.LocalPlayer.inventory[i].type == 1331 || Main.LocalPlayer.inventory[i].type == 67 || Main.LocalPlayer.inventory[i].type == 2886)
                flag8 = true;
            if (Main.LocalPlayer.inventory[i].type == 84 || Main.LocalPlayer.inventory[i].type == 1236 || Main.LocalPlayer.inventory[i].type == 1237 || Main.LocalPlayer.inventory[i].type == 1238 || Main.LocalPlayer.inventory[i].type == 1239 || Main.LocalPlayer.inventory[i].type == 1240 || Main.LocalPlayer.inventory[i].type == 1241 || Main.LocalPlayer.inventory[i].type == 939 || Main.LocalPlayer.inventory[i].type == 1273 || Main.LocalPlayer.inventory[i].type == 2585 || Main.LocalPlayer.inventory[i].type == 2360 || Main.LocalPlayer.inventory[i].type == 185 || Main.LocalPlayer.inventory[i].type == 1800 || Main.LocalPlayer.inventory[i].type == 1915)
                flag9 = true;
            if (Main.LocalPlayer.inventory[i].type == 3347)
                flag10 = true;
            if (Main.LocalPlayer.inventory[i].type == 174)
                flag11 = true;
            if (Main.LocalPlayer.inventory[i].type == 1141)
                flag12 = true;
            if (Main.LocalPlayer.inventory[i].type == 1533 || Main.LocalPlayer.inventory[i].type == 1534 || Main.LocalPlayer.inventory[i].type == 1535 || Main.LocalPlayer.inventory[i].type == 1536 || Main.LocalPlayer.inventory[i].type == 1537 || Main.LocalPlayer.inventory[i].type == 4714)
                flag13 = true;
        }
        bool flag14 = false;
        bool flag15 = false;
        bool flag16 = false;
        bool flag17 = false;
        bool flag18 = false;
        bool flag19 = false;
        bool flag20 = false;
        bool flag21 = false;
        bool flag22 = false;
        bool flag23 = false;
        bool flag24 = false;
        bool flag25 = false;
        bool flag26 = false;
        bool flag27 = false;
        bool flag28 = false;
        bool flag29 = false;
        bool flag30 = false;
        bool flag31 = false;
        bool flag32 = false;
        bool flag33 = false;
        bool flag34 = false;
        bool flag35 = false;
        bool flag36 = false;
        bool flag37 = false;
        bool flag38 = false;
        int num = 0;
        for (int j = 0; j < 200; j++) {
            if (Main.npc[j].active) {
                if (Main.npc[j].townNPC && Main.npc[j].type != 37)
                    num++;
                if (Main.npc[j].type == 17)
                    flag14 = true;
                if (Main.npc[j].type == 18)
                    flag15 = true;
                if (Main.npc[j].type == 19)
                    flag17 = true;
                if (Main.npc[j].type == NPCID.Dryad)
                    flag16 = true;
                if (Main.npc[j].type == 54)
                    flag22 = true;
                if (Main.npc[j].type == 124)
                    flag19 = true;
                if (Main.npc[j].type == 38)
                    flag18 = true;
                if (Main.npc[j].type == 108)
                    flag20 = true;
                if (Main.npc[j].type == 107)
                    flag21 = true;
                if (Main.npc[j].type == 228)
                    flag23 = true;
                if (Main.npc[j].type == 178)
                    flag24 = true;
                if (Main.npc[j].type == 209)
                    flag25 = true;
                if (Main.npc[j].type == 353)
                    flag26 = true;
                if (Main.npc[j].type == 633)
                    flag38 = true;
                if (Main.npc[j].type == 369)
                    flag27 = true;
                if (Main.npc[j].type == 441)
                    flag28 = true;
                if (Main.npc[j].type == 229)
                    flag29 = true;
                if (Main.npc[j].type == 207)
                    flag30 = true;
                if (Main.npc[j].type == 160)
                    flag31 = true;
                if (Main.npc[j].type == 588)
                    flag32 = true;
                if (Main.npc[j].type == 227)
                    flag33 = true;
                if (Main.npc[j].type == 208)
                    flag34 = true;
                if (Main.npc[j].type == 550)
                    flag35 = true;
                if (Main.npc[j].type == 368)
                    flag36 = true;
                if (Main.npc[j].type == 453)
                    flag37 = true;
            }
        }
        object obj = Lang.CreateDialogSubstitutionObject();
        while (true) {
            Main.helpText++;
            if (Language.Exists("GuideHelpText.Help_" + Main.helpText)) {
                LocalizedText text = Language.GetText("GuideHelpText.Help_" + Main.helpText);
                if (text.CanFormatWith(obj)) {
                    Main.npcChatText = text.FormatWith(obj);
                    return;
                }
            }
            if (flag3) {
                if (Main.helpText == 1) {
                    Main.npcChatText = Lang.dialog(177);
                    return;
                }
                if (Main.helpText == 2) {
                    Main.npcChatText = Lang.dialog(178);
                    return;
                }
                if (Main.helpText == 3) {
                    Main.npcChatText = Lang.dialog(179);
                    return;
                }
                if (Main.helpText == 4) {
                    Main.npcChatText = Lang.dialog(180);
                    return;
                }
                if (Main.helpText == 5) {
                    Main.npcChatText = Lang.dialog(181);
                    return;
                }
                if (Main.helpText == 6) {
                    Main.npcChatText = Lang.dialog(182);
                    return;
                }
            }
            if (flag3 && !flag4 && !flag5 && Main.helpText == 11) {
                Main.npcChatText = Lang.dialog(183);
                return;
            }
            if (flag3 && flag4 && !flag5) {
                if (Main.helpText == 21) {
                    Main.npcChatText = Lang.dialog(184);
                    return;
                }
                if (Main.helpText == 22) {
                    Main.npcChatText = Lang.dialog(185);
                    return;
                }
            }
            if (flag3 && flag5) {
                if (Main.helpText == 31) {
                    Main.npcChatText = Lang.dialog(186);
                    return;
                }
                if (Main.helpText == 32) {
                    Main.npcChatText = Lang.dialog(187);
                    return;
                }
            }
            if (!flag && Main.helpText == 41) {
                Main.npcChatText = Lang.dialog(188);
                return;
            }
            if (!flag2 && Main.helpText == 42) {
                Main.npcChatText = Lang.dialog(189);
                return;
            }
            if (!flag2 && !flag6 && Main.helpText == 43) {
                Main.npcChatText = Lang.dialog(190);
                return;
            }
            if (!flag14 && !flag15) {
                if (Main.helpText == 51) {
                    Main.npcChatText = Lang.dialog(191);
                    return;
                }
                if (Main.helpText == 52) {
                    Main.npcChatText = Lang.dialog(192);
                    return;
                }
                if (Main.helpText == 53) {
                    Main.npcChatText = Lang.dialog(193);
                    return;
                }
                if (Main.helpText == 54) {
                    Main.npcChatText = Lang.dialog(194);
                    return;
                }
                if (Main.helpText == 55) {
                    Main.npcChatText = Language.GetTextValue("GuideHelpText.Help_1065");
                    return;
                }
            }
            if (!flag14 && Main.helpText == 61) {
                Main.npcChatText = Lang.dialog(195);
                return;
            }
            if (!flag15 && Main.helpText == 62) {
                Main.npcChatText = Lang.dialog(196);
                return;
            }
            if (!flag17 && Main.helpText == 63) {
                Main.npcChatText = Lang.dialog(197);
                return;
            }
            //if (!flag16 && Main.helpText == 64) {
            //    Main.npcChatText = Lang.dialog(198);
            //    return;
            //}
            if (NewGuideHelpTextID.DryadCocoon1Condition() && Main.helpText == 64) {
                Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{1}");
                return;
            }
            if (Main.helpText == 65) {
                Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{6}");
                return;
            }
            if (NewGuideHelpTextID.DryadCocoon2Condition() && Main.helpText == 66) {
                Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{2}");
                return;
            }
            int num5 = 2;
            if (!flag19 && Main.helpText == 65 + num5 && NPC.downedBoss3) {
                Main.npcChatText = Lang.dialog(199);
                return;
            }
            if (!flag22 && Main.helpText == 66 + num5 && NPC.downedBoss3) {
                Main.npcChatText = Lang.dialog(200);
                return;
            }
            if (!flag18 && Main.helpText == 67 + num5) {
                Main.npcChatText = Lang.dialog(201);
                return;
            }
            if (!flag21 && NPC.downedBoss2 && Main.helpText == 68 + num5) {
                Main.npcChatText = Lang.dialog(202);
                return;
            }
            if (!flag20 && Main.hardMode && Main.helpText == 69 + num5) {
                Main.npcChatText = Lang.dialog(203);
                return;
            }
            if (!flag23 && Main.helpText == 70 + num5 && NPC.downedBoss2) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1100");
                return;
            }

            if (!flag24 && Main.helpText == 71 + num5 && Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1101");
                return;
            }

            if (!flag25 && Main.helpText == 72 + num5 && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1102");
                return;
            }

            if (!flag26 && Main.helpText == 73 + num5) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1103");
                return;
            }

            if (!flag27 && Main.helpText == 74 + num5) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1104");
                return;
            }

            if (!flag28 && Main.helpText == 75 + num5 && Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1105");
                return;
            }

            if (!flag29 && Main.helpText == 76 + num5 && Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1106");
                return;
            }

            if (!flag30 && Main.helpText == 77 + num5) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1107");
                return;
            }

            if (!flag31 && Main.helpText == 78 + num5 && Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1108");
                return;
            }

            if (!flag32 && Main.helpText == 79 + num5) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1109");
                return;
            }

            if (!flag33 && Main.helpText == 80 + num5 && num >= 5) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1110");
                return;
            }

            if (!flag34 && Main.helpText == 81 + num5 && num >= 11) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1111");
                return;
            }

            if (!flag35 && NPC.downedBoss2 && Main.helpText == 82 + num5) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1112");
                return;
            }

            if (!flag36 && Main.helpText == 83 + num5 && flag14) {
                Main.npcChatText = Language.GetTextValueWith("GuideHelpTextSpecific.Help_1113", obj);
                return;
            }

            if (!flag37 && Main.helpText == 84 + num5 && !Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1114");
                return;
            }

            if (!flag38 && Main.helpText == 85 + num5 && !Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1115");
                return;
            }

            if (flag7 && !WorldGen.crimson && Main.helpText == 100) {
                Main.npcChatText = Lang.dialog(204);
                return;
            }

            if (flag8 && Main.helpText == 101) {
                Main.npcChatText = Lang.dialog(WorldGen.crimson ? 403 : 205);
                return;
            }

            if ((flag7 || flag8) && Main.helpText == 102) {
                Main.npcChatText = Lang.dialog(WorldGen.crimson ? 402 : 206);
                return;
            }

            if (flag7 && WorldGen.crimson && Main.helpText == 103) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1159");
                return;
            }

            int num6 = -1;
            if (!flag9 && Main.LocalPlayer.miscEquips[4].IsAir && Main.helpText == 201 + num6 && !Main.hardMode && !NPC.downedBoss3 && !NPC.downedBoss2) {
                Main.npcChatText = Lang.dialog(207);
                return;
            }

            /*
			if (Main.helpText == 202 && !hardMode && Main.LocalPlayer.statLifeMax >= 140) {
			*/
            if (Main.helpText == 202 + num6 && !Main.hardMode && Main.LocalPlayer.ConsumedLifeCrystals >= 2) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1120");
                return;
            }

            if (Main.helpText == 203 + num6 && Main.hardMode && NPC.downedMechBossAny) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1121");
                return;
            }

            /*
			if (Main.helpText == 204 && !NPC.downedGoblins && Main.LocalPlayer.statLifeMax >= 200 && WorldGen.shadowOrbSmashed) {
			*/
            if (Main.helpText == 204 + num6 && !NPC.downedGoblins && Main.LocalPlayer.ConsumedLifeCrystals >= 5 && WorldGen.shadowOrbSmashed) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1122");
                return;
            }

            /*
			if (Main.helpText == 205 && hardMode && !NPC.downedPirates && Main.LocalPlayer.statLifeMax >= 200) {
			*/
            if (Main.helpText == 205 + num6 && Main.hardMode && !NPC.downedPirates && Main.LocalPlayer.ConsumedLifeCrystals >= 5) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1123");
                return;
            }

            if (Main.helpText == 206 + num6 && Main.hardMode && NPC.downedGolemBoss && !NPC.downedMartians) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1124");
                return;
            }

            if (Main.helpText == 206 && NewGuideHelpTextID.BackwoodsTreesCondition()) {
                Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{3}");
                return;
            }

            if (Main.helpText == 207 && NewGuideHelpTextID.BackwoodsLootRoomsCondition()) {
                Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{4}");
                return;
            }

            num6 = 1;
            if (Main.helpText == 207 + num6 && (NPC.downedBoss1 || NPC.downedBoss2 || NPC.downedBoss3)) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1125");
                return;
            }

            if (Main.helpText == 208 + num6 && NewGuideHelpTextID.LothorCondition()) {
                Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{5}");
                return;
            }

            num6 += 1;

            if (Main.helpText == 208 + num6 && !Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1130");
                return;
            }

            if (Main.helpText == 209 + num6 && !Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1131");
                return;
            }

            if (Main.helpText == 210 + num6 && !Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1132");
                return;
            }

            if (Main.helpText == 211 + num6 && !Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1133");
                return;
            }

            if (Main.helpText == 212 + num6 && !Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1134");
                return;
            }

            if (Main.helpText == 213 + num6 && !Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1135");
                return;
            }

            if (Main.helpText == 214 + num6 && !Main.hardMode && (flag4 || flag5)) {
                Main.npcChatText = Language.GetTextValueWith("GuideHelpTextSpecific.Help_1136", obj);
                return;
            }

            if (Main.helpText == 215 + num6 && Main.LocalPlayer.anglerQuestsFinished < 1) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1137");
                return;
            }

            if (Main.helpText == 216 + num6 && !Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1138");
                return;
            }

            if (Main.helpText == 1000 && !NPC.downedBoss1 && !NPC.downedBoss2) {
                Main.npcChatText = Lang.dialog(208);
                return;
            }

            if (Main.helpText == 1001 && !NPC.downedBoss1 && !NPC.downedBoss2) {
                Main.npcChatText = Lang.dialog(209);
                return;
            }

            if (Main.helpText == 1002 && !NPC.downedBoss2) {
                if (WorldGen.crimson)
                    Main.npcChatText = Lang.dialog(331);
                else
                    Main.npcChatText = Lang.dialog(210);

                return;
            }

            /*
			if (Main.helpText == 1050 && !NPC.downedBoss1 && Main.LocalPlayer.statLifeMax < 200) {
			*/
            if (Main.helpText == 1050 && !NPC.downedBoss1 && Main.LocalPlayer.ConsumedLifeCrystals < 5) {
                Main.npcChatText = Lang.dialog(211);
                return;
            }

            if (Main.helpText == 1051 && !NPC.downedBoss1 && Main.LocalPlayer.statDefense <= 10) {
                Main.npcChatText = Lang.dialog(212);
                return;
            }

            /*
			if (Main.helpText == 1052 && !NPC.downedBoss1 && Main.LocalPlayer.statLifeMax >= 200 && Main.LocalPlayer.statDefense > 10) {
			*/
            if (Main.helpText == 1052 && !NPC.downedBoss1 && Main.LocalPlayer.ConsumedLifeCrystals >= 5 && Main.LocalPlayer.statDefense > 10) {
                Main.npcChatText = Lang.dialog(WorldGen.crimson ? 404 : 213);
                return;
            }

            /*
			if (Main.helpText == 1053 && NPC.downedBoss1 && !NPC.downedBoss2 && Main.LocalPlayer.statLifeMax < 300) {
			*/
            if (Main.helpText == 1053 && NPC.downedBoss1 && !NPC.downedBoss2 && Main.LocalPlayer.ConsumedLifeCrystals < 10) {
                Main.npcChatText = Lang.dialog(214);
                return;
            }

            /*
			if (Main.helpText == 1054 && NPC.downedBoss1 && !NPC.downedBoss2 && !WorldGen.crimson && Main.LocalPlayer.statLifeMax >= 300) {
			*/
            if (Main.helpText == 1054 && NPC.downedBoss1 && !NPC.downedBoss2 && !WorldGen.crimson && Main.LocalPlayer.ConsumedLifeCrystals >= 10) {
                Main.npcChatText = Lang.dialog(215);
                return;
            }

            /*
			if (Main.helpText == 1055 && NPC.downedBoss1 && !NPC.downedBoss2 && !WorldGen.crimson && Main.LocalPlayer.statLifeMax >= 300) {
			*/
            if (Main.helpText == 1055 && NPC.downedBoss1 && !NPC.downedBoss2 && !WorldGen.crimson && Main.LocalPlayer.ConsumedLifeCrystals >= 10) {
                Main.npcChatText = Lang.dialog(216);
                return;
            }

            if (Main.helpText == 1056 && NPC.downedBoss1 && NPC.downedBoss2 && !NPC.downedBoss3) {
                Main.npcChatText = Lang.dialog(217);
                return;
            }

            /*
			if (Main.helpText == 1057 && NPC.downedBoss1 && NPC.downedBoss2 && NPC.downedBoss3 && !hardMode && Main.LocalPlayer.statLifeMax < 400) {
			*/
            if (Main.helpText == 1057 && NPC.downedBoss1 && NPC.downedBoss2 && NPC.downedBoss3 && !Main.hardMode && Main.LocalPlayer.ConsumedLifeCrystals < Player.LifeCrystalMax) {
                Main.npcChatText = Lang.dialog(218);
                return;
            }

            /*
			if (Main.helpText == 1058 && NPC.downedBoss1 && NPC.downedBoss2 && NPC.downedBoss3 && !hardMode && Main.LocalPlayer.statLifeMax >= 400) {
			*/
            if (Main.helpText == 1058 && NPC.downedBoss1 && NPC.downedBoss2 && NPC.downedBoss3 && !Main.hardMode && Main.LocalPlayer.ConsumedLifeCrystals == Player.LifeCrystalMax) {
                Main.npcChatText = Lang.dialog(219);
                return;
            }

            /*
			if (Main.helpText == 1059 && NPC.downedBoss1 && NPC.downedBoss2 && NPC.downedBoss3 && !hardMode && Main.LocalPlayer.statLifeMax >= 400) {
			*/
            if (Main.helpText == 1059 && NPC.downedBoss1 && NPC.downedBoss2 && NPC.downedBoss3 && !Main.hardMode && Main.LocalPlayer.ConsumedLifeCrystals == Player.LifeCrystalMax) {
                Main.npcChatText = Lang.dialog(220);
                return;
            }

            /*
			if (Main.helpText == 1060 && NPC.downedBoss1 && NPC.downedBoss2 && NPC.downedBoss3 && !hardMode && Main.LocalPlayer.statLifeMax >= 400) {
			*/
            if (Main.helpText == 1060 && NPC.downedBoss1 && NPC.downedBoss2 && NPC.downedBoss3 && !Main.hardMode && Main.LocalPlayer.ConsumedLifeCrystals == Player.LifeCrystalMax) {
                Main.npcChatText = Lang.dialog(221);
                return;
            }

            if (Main.helpText == 1061 && Main.hardMode && !NPC.downedPlantBoss) {
                Main.npcChatText = Lang.dialog(WorldGen.crimson ? 401 : 222);
                return;
            }

            if (Main.helpText == 1062 && Main.hardMode && !NPC.downedPlantBoss) {
                Main.npcChatText = Lang.dialog(223);
                return;
            }

            /*
			if (Main.helpText == 1140 && NPC.downedBoss1 && !NPC.downedBoss2 && WorldGen.crimson && Main.LocalPlayer.statLifeMax >= 300) {
			*/
            if (Main.helpText == 1140 && NPC.downedBoss1 && !NPC.downedBoss2 && WorldGen.crimson && Main.LocalPlayer.ConsumedLifeCrystals >= 10) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1140");
                return;
            }

            /*
			if (Main.helpText == 1141 && NPC.downedBoss1 && !NPC.downedBoss2 && WorldGen.crimson && Main.LocalPlayer.statLifeMax >= 300) {
			*/
            if (Main.helpText == 1141 && NPC.downedBoss1 && !NPC.downedBoss2 && WorldGen.crimson && Main.LocalPlayer.ConsumedLifeCrystals >= 10) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1141");
                return;
            }

            if (Main.helpText == 1142 && NPC.downedBoss2 && !Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1142");
                return;
            }

            /*
			if (Main.helpText == 1143 && NPC.downedBoss2 && !NPC.downedQueenBee && Main.LocalPlayer.statLifeMax >= 300) {
			*/
            if (Main.helpText == 1143 && NPC.downedBoss2 && !NPC.downedQueenBee && Main.LocalPlayer.ConsumedLifeCrystals >= 10) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1143");
                return;
            }

            if (Main.helpText == 1144 && flag10) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1144");
                return;
            }

            if (Main.helpText == 1145 && flag11 && !Main.hardMode) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1145");
                return;
            }

            if (Main.helpText == 1146 && Main.hardMode && Main.LocalPlayer.wingsLogic == 0 && !Main.LocalPlayer.mount.Active && !NPC.downedPlantBoss) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1146");
                return;
            }

            if (Main.helpText == 1147 && Main.hardMode && WorldGen.SavedOreTiers.Adamantite == 111 && !NPC.downedMechBossAny) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1147");
                return;
            }

            if (Main.helpText == 1148 && Main.hardMode && WorldGen.SavedOreTiers.Adamantite == 223 && !NPC.downedMechBossAny) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1148");
                return;
            }

            /*
			if (Main.helpText == 1149 && hardMode && NPC.downedMechBossAny && Main.LocalPlayer.statLifeMax < 500) {
			*/
            if (Main.helpText == 1149 && Main.hardMode && NPC.downedMechBossAny && Main.LocalPlayer.ConsumedLifeCrystals == Player.LifeCrystalMax && Main.LocalPlayer.ConsumedLifeFruit < Player.LifeFruitMax) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1149");
                return;
            }

            if (Main.helpText == 1150 && Main.hardMode && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && !NPC.downedPlantBoss) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1150");
                return;
            }

            if (Main.helpText == 1151 && Main.hardMode && NPC.downedPlantBoss && !NPC.downedGolemBoss && flag12) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1151");
                return;
            }

            if (Main.helpText == 1152 && Main.hardMode && NPC.downedPlantBoss && !NPC.downedGolemBoss && !flag12) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1152");
                return;
            }

            if (Main.helpText == 1153 && Main.hardMode && flag13) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1153");
                return;
            }

            if (Main.helpText == 1154 && Main.hardMode && !NPC.downedFishron) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1154");
                return;
            }

            if (Main.helpText == 1155 && Main.hardMode && NPC.downedGolemBoss && !NPC.downedHalloweenTree && !NPC.downedHalloweenKing) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1155");
                return;
            }

            if (Main.helpText == 1156 && Main.hardMode && NPC.downedGolemBoss && !NPC.downedChristmasIceQueen && !NPC.downedChristmasTree && !NPC.downedChristmasSantank) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1156");
                return;
            }

            if (Main.helpText == 1157 && Main.hardMode && NPC.downedGolemBoss && NPC.AnyNPCs(437) && !NPC.downedMoonlord) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1157");
                return;
            }

            if (Main.helpText == 1158 && Main.hardMode && NPC.LunarApocalypseIsUp && !NPC.downedMoonlord) {
                Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1158");
                return;
            }

            if (Main.helpText == 1159 && NPC.downedBoss1 && NPC.downedBoss2 && !NPC.downedDeerclops)
                break;

            if (Main.helpText > 1200)
                Main.helpText = 0;
        }

        Main.npcChatText = Language.GetTextValue("GuideHelpTextSpecific.Help_1160");
    }

    void ILoadable.Unload() { }
}
