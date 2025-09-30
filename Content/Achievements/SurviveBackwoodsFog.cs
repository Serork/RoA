using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace RoA.Content.Achievements;

sealed class SurviveBackwoodsFog : ModAchievement {
    public CustomFlagCondition SurviveBackwoodsFogCondition { get; private set; }
    //public CustomFlagCondition SurviveBackwoodsFogCondition_End { get; private set; }

    public override void SetStaticDefaults() {
        Achievement.SetCategory(AchievementCategory.Explorer);

        SurviveBackwoodsFogCondition = AddCondition();
        //SurviveBackwoodsFogCondition_End = AddCondition("Condition_END");
    }

    public override Position GetDefaultPosition() => new After("BLOODBATH");
}
