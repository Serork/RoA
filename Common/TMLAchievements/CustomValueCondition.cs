using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
sealed class CustomValueCondition : ModCondition {
    public float Value;

    public readonly float MaxValue;

    public CustomValueCondition(string uniqueName, float maxValue)
        : base(uniqueName) {
        Value = 0f;
        MaxValue = maxValue;
    }

    public override void Register() {
        if (!AchievementLoader.customValueListener.ContainsKey(Name)) {
            AchievementLoader.customValueListener[Name] = new List<CustomValueCondition>();
        }
        AchievementLoader.customValueListener[Name].Add(this);
    }

    public override void Reset() {
        Value = 0f;
    }

    public void AddValue(string eventName, float value) {
        if (Name == eventName) {
            Value += value;
            if (Value >= MaxValue) {
                SetComplete();
                Value = MaxValue;
            }
        }
    }

    public override Tuple<decimal, decimal> GetProgressBar() {
        return Tuple.Create((decimal)Value, (decimal)MaxValue);
    }

    public override void SaveToJson(JObject jsonObject) {
        jsonObject["Value"] = Value;
    }

    public override void LoadFromJson(JObject jsonObject) {
        if (jsonObject.ContainsKey("Value")) {
            Value = (float)jsonObject["Value"];
        }
    }
}
