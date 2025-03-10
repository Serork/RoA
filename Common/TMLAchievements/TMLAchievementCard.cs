using Terraria.UI;


namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
internal class TMLAchievementCard : AchievementAdvisorCard {
    public TMLAchievementCard(ModAchievement achievement)
        : base(achievement.achievement, achievement.AchievementCardOrder) {
        frame = achievement.GetFrame(locked: true);
    }
}
