using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace RoA.Content.Achievements;

sealed class MineMercuriumNugget : ModAchievement {
    public CustomFlagCondition MineMercuriumAndIgnoreItsEffectCondition { get; private set; } = null!;

    public override void SetStaticDefaults() {
        Achievement.SetCategory(AchievementCategory.Challenger);

        MineMercuriumAndIgnoreItsEffectCondition = AddCondition();
    }

    public override Position GetDefaultPosition() => new Before("BULLDOZER");
}
