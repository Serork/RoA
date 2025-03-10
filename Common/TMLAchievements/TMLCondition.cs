using Newtonsoft.Json.Linq;

using Terraria.Achievements;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
sealed class TMLCondition : AchievementCondition {
    private ModCondition condition;

    public TMLCondition(ModCondition condition, string name)
        : base(name) {
        this.condition = condition;
    }

    public override void Clear() {
        base.Clear();
        condition.Reset();
    }

    public void Save(JObject state) {
        condition.SaveToJson(state);
    }

    public override void Load(JObject state) {
        base.Load(state);
        condition.LoadFromJson(state);
    }
}
