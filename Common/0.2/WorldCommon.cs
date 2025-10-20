using RoA.Common.NPCs;
using RoA.Content.Tiles.Miscellaneous;

using System;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common;

sealed class WorldCommon : ModSystem {
    public static class NewGuideHelpTextID {
        public const byte DryadCocoon1 = 0;
        public const byte DryadCocoon2 = 1;
        public const byte BackwoodsTrees = 2;
        public const byte BackwoodsLootRooms = 3;
        public const byte Lothor = 4;
        public const byte Tar = 5;
        public const byte Count = 6;

        public static Func<bool> DryadCocoon1Condition => () => {
            return !TreeDryad.AbleToBeDestroyed;
        };
        public static Func<bool> DryadCocoon2Condition => () => {
            return TreeDryad.AbleToBeDestroyed;
        };
        public static Func<bool> BackwoodsTreesCondition => () => NPC.downedBoss2;
        public static Func<bool> BackwoodsLootRoomsCondition => () => NPC.downedBoss2;
        public static Func<bool> LothorCondition => () => NPC.downedBoss3 && !DownedBossSystem.DownedLothorBoss;
        public static Func<bool> TarCondition => () => true;
    }

    public static byte NEWGUIDETEXTCOUNT => NewGuideHelpTextID.Count;


    private static bool[] _shownGuideTexts = null!;


    public static bool ShouldRemindOfNewGuideText;

    public static bool HasNewGuideTextToShow { get; private set; }

    public static int GetClampedIndex(int index) => Utils.Clamp(index, 0, NEWGUIDETEXTCOUNT);
    public static string GetKeyText(int index) => RoA.ModName + "newguidetexts" + nameof(_shownGuideTexts) + GetClampedIndex(index);

    public static void TryToRemind() {
        if (NewGuideHelpTextID.DryadCocoon1Condition()) {
            if (Remind(NewGuideHelpTextID.DryadCocoon1)) {
                return;
            }
        }
        if (NewGuideHelpTextID.TarCondition()) {
            if (Remind(NewGuideHelpTextID.Tar)) {
                return;
            }
        }
        if (NewGuideHelpTextID.DryadCocoon2Condition()) {
            if (Remind(NewGuideHelpTextID.DryadCocoon2)) {
                return;
            }
        }
        if (NewGuideHelpTextID.BackwoodsTreesCondition()) {
            if (Remind(NewGuideHelpTextID.BackwoodsTrees)) {
                return;
            }
        }

        if (NewGuideHelpTextID.BackwoodsLootRoomsCondition()) {
            if (Remind(NewGuideHelpTextID.BackwoodsLootRooms)) {
                return;
            }
        }

        if (NewGuideHelpTextID.LothorCondition()) {
            if (Remind(NewGuideHelpTextID.Lothor)) {
                return;
            }
        }
    }

    public static bool Remind(int index) {
        index = GetClampedIndex(index);

        if (!ShouldRemindOfNewGuideText || _shownGuideTexts[index]) {
            return false;
        }

        HasNewGuideTextToShow = true;

        return true;
    }

    public static bool ShowMessage(int index) {
        if (!HasNewGuideTextToShow) {
            return false;
        }

        index = GetClampedIndex(index);
        if (!_shownGuideTexts[index]) {
            Main.npcChatText = Language.GetTextValue($"Mods.RoA.NPCs.Town.Guide.HelpText{index}");

            _shownGuideTexts[index] = true;

            ShouldRemindOfNewGuideText = false;

            return true;
        }

        return false;
    }

    public override void ClearWorld() {
        _shownGuideTexts = new bool[NEWGUIDETEXTCOUNT];
    }

    public override void SaveWorldData(TagCompound tag) {
        for (int i = 0; i < NEWGUIDETEXTCOUNT; i++) {
            tag[GetKeyText(i)] = _shownGuideTexts[i];
        }
        if (ShouldRemindOfNewGuideText) {
            tag[RoA.ModName + nameof(ShouldRemindOfNewGuideText)] = true;
        }
    }

    public override void LoadWorldData(TagCompound tag) {
        for (int i = 0; i < NEWGUIDETEXTCOUNT; i++) {
            _shownGuideTexts[i] = tag.GetBool(GetKeyText(i));
        }
        ShouldRemindOfNewGuideText = tag.GetBool(RoA.ModName + nameof(ShouldRemindOfNewGuideText));
    }

    public override void PostUpdatePlayers() {
        if (!ShouldRemindOfNewGuideText) {
            if (Main.dayTime) {
                if (Main.time < 1 || (Main.IsFastForwardingTime() && Main.time < 61)) {
                    ShouldRemindOfNewGuideText = true;
                }
            }
        }

        HasNewGuideTextToShow = false;
        TryToRemind();
    }
}
