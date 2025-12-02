using RoA.Content.NPCs.Enemies.Bosses.Lothor;

using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace RoA.Content.Achievements;

sealed class DefeatLothor : ModAchievement {
    public CustomFlagCondition KillLothorCondition { get; private set; } = null!;

    public override void SetStaticDefaults() {
        Achievement.SetCategory(AchievementCategory.Slayer);

        KillLothorCondition = AddCondition();
    }

    public override Position GetDefaultPosition() => new After("MINER_FOR_FIRE");
}
