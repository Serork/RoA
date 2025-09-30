using System.Collections.Generic;

using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;

namespace RoA.Content.Achievements;

sealed class DefeatEnragedLothor : ModAchievement {
    public CustomFlagCondition KillEnragedLothorCondition { get; private set; }

    public override void SetStaticDefaults() {
        Achievement.SetCategory(AchievementCategory.Challenger);

        KillEnragedLothorCondition = AddCondition();
    }

    public override Position GetDefaultPosition() => new After("MINER_FOR_FIRE");

    public override IEnumerable<Position> GetModdedConstraints() {
        yield return new After(ModContent.GetInstance<DefeatLothor>());
    }
}
