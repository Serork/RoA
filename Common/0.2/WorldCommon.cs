using RoA.Common.NPCs;
using RoA.Content.Tiles.Miscellaneous;

using System;

using Terraria;
using Terraria.ID;
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

    public static bool[] ShownGuideTexts { get; private set; } = null!;

    public static bool ShouldRemindOfNewGuideText;

    public static bool HasNewGuideTextToShow { get; private set; }

    public string GetKeyText(int index) => RoA.ModName + "newguidetexts" + nameof(ShownGuideTexts) + Utils.Clamp(index, 0, NEWGUIDETEXTCOUNT);

    public static void Remind() {
        if (!ShouldRemindOfNewGuideText) {
            return;
        }

        HasNewGuideTextToShow = true;
    }

    public override void ClearWorld() {
        ShownGuideTexts = new bool[NEWGUIDETEXTCOUNT];
    }

    public override void SaveWorldData(TagCompound tag) {
        for (int i = 0; i < NEWGUIDETEXTCOUNT; i++) {
            tag[GetKeyText(i)] = ShownGuideTexts[i];
        }
        if (ShouldRemindOfNewGuideText) {
            tag[RoA.ModName + nameof(ShouldRemindOfNewGuideText)] = true;
        }
    }

    public override void LoadWorldData(TagCompound tag) {
        for (int i = 0; i < NEWGUIDETEXTCOUNT; i++) {
            ShownGuideTexts[i] = tag.GetBool(GetKeyText(i));
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
        GuideHelpTexts.Update();
    }
}
