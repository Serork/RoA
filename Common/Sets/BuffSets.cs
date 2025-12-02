using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Sets;

sealed class BuffSets : ModSystem {
    public static bool[] Debuffs = BuffID.Sets.Factory.CreateBoolSet();

    public override void Load() {
        On_Player.AddBuff_DetermineBuffTimeToAdd += On_Player_AddBuff_DetermineBuffTimeToAdd;
    }

    private int On_Player_AddBuff_DetermineBuffTimeToAdd(On_Player.orig_AddBuff_DetermineBuffTimeToAdd orig, Player self, int type, int time1) {
        if (Debuffs[type]) {
            int num = time1;
            if (Main.expertMode && self.whoAmI == Main.myPlayer) {
                float debuffTimeMultiplier = Main.GameModeInfo.DebuffTimeMultiplier;
                if (Main.GameModeInfo.IsJourneyMode) {
                    if (Main.masterMode)
                        debuffTimeMultiplier = Main.RegisteredGameModes[2].DebuffTimeMultiplier;
                    else if (Main.expertMode)
                        debuffTimeMultiplier = Main.RegisteredGameModes[1].DebuffTimeMultiplier;
                }

                num = (int)(debuffTimeMultiplier * (float)num);

                return num;
            }
        }
        return orig(self, type, time1);
    }
}
