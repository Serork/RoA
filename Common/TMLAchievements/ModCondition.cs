using System;

using Newtonsoft.Json.Linq;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
abstract class ModCondition {
    internal TMLCondition condition;

    public string Name => condition.Name;

    public virtual bool Completed => condition.IsCompleted;

    protected ModCondition(string uniqueName) {
        condition = new TMLCondition(this, uniqueName);
    }

    public abstract void Register();

    public virtual void SetComplete() {
        condition.Complete();
    }

    public virtual void SetUncomplete() {
        condition.Clear();
    }

    public virtual void Reset() {
    }

    public virtual Tuple<decimal, decimal> GetProgressBar() {
        return null;
    }

    public virtual void SaveToJson(JObject json) {
    }

    public virtual void LoadFromJson(JObject json) {
    }
}
