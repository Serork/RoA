using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace RoA.Content.Achievements;

sealed class SurviveBackwoodsFog : ModAchievement {
    public CustomFlagCondition SurviveBackwoodsFogCondition { get; private set; }

    public override void SetStaticDefaults() {
        Achievement.SetCategory(AchievementCategory.Explorer);

        SurviveBackwoodsFogCondition = AddCondition();
    }

    public override Position GetDefaultPosition() => new After("BLOODBATH");
}
