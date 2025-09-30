using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace RoA.Content.Achievements;

sealed class OpenRootboundChest : ModAchievement {
    public CustomFlagCondition OpenRootboundChestCondition { get; private set; }

    public override void SetStaticDefaults() {
        Achievement.SetCategory(AchievementCategory.Explorer);

        OpenRootboundChestCondition = AddCondition();
    }

    public override Position GetDefaultPosition() => new Before("WHERES_MY_HONEY");
}
