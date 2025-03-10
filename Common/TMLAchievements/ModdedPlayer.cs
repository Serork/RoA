using Terraria.ModLoader;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
sealed class ModdedPlayer : ModPlayer {
    public override void OnEnterWorld() {
        if (!ModLoader.HasMod("TMLAchievements")) {
            TMLAchievements.SaveLastPlayed();
            TMLAchievements.DoConditionPatch();
            TMLAchievements.LoadAchievements();
        }
    }
}
