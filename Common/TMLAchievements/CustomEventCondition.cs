using System.Collections.Generic;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
sealed class CustomEventCondition : ModCondition {
    public CustomEventCondition(string uniqueName)
        : base(uniqueName) {
    }

    public override void Register() {
        if (!AchievementLoader.customEventListener.ContainsKey(Name)) {
            AchievementLoader.customEventListener[Name] = new List<CustomEventCondition>();
        }
        AchievementLoader.customEventListener[Name].Add(this);
    }

    public void CheckEvent(string name) {
        if (Name == name) {
            SetComplete();
        }
    }
}
