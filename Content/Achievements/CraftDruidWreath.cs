using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace RoA.Content.Achievements;

sealed class CraftDruidWreath : ModAchievement {
    public CustomFlagCondition CraftAnyWreathCondition { get; private set; }

    public override void SetStaticDefaults() {
        Achievement.SetCategory(AchievementCategory.Collector);

        CraftAnyWreathCondition = AddCondition();
    }

    public override Position GetDefaultPosition() => new After("EYE_ON_YOU");
}
