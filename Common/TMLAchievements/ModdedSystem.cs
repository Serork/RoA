using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
sealed class ModdedSystem : ModSystem {
    public override void ModifyGameTipVisibility(IReadOnlyList<GameTipData> gameTips) {
        if (!ModLoader.HasMod("TMLAchievements")) {
            if (Main.netMode != 2) {
                TMLAchievements.LoadLang();
                AchievementLoader.PostSetup();
                AchievementLoader.SetupAchievementNames();
            }
        }
    }
}
