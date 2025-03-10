using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Xna.Framework;

using Terraria.Achievements;
using Terraria.ModLoader;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
abstract class ModAchievement : ModTexturedType {
    private static FieldInfo count = typeof(Achievement).GetField("_completedCount", BindingFlags.Instance | BindingFlags.NonPublic);

    public TMLAchievement achievement;

    public List<ModCondition> conditions;

    public int Id;

    public virtual AchievementCategory Catagory => AchievementCategory.Collector;

    public virtual bool ShowProgressBar => false;

    public virtual bool ShowAchievementCard => false;

    public virtual float AchievementCardOrder => 2.5f;

    public virtual string CustomBorderTexture => null;

    protected sealed override void Register() {
        if (!ModLoader.HasMod("TMLAchievements")) {
            ModTypeLookup<ModAchievement>.Register(this);
            AchievementLoader.Add(this);
            Id = AchievementLoader.Count - 1;
        }
    }

    public sealed override void SetupContent() {
        if (!ModLoader.HasMod("TMLAchievements")) {
            SetStaticDefaults();
        }
    }

    public virtual void AddConditions(List<ModCondition> conditions) {
    }

    public virtual Rectangle GetFrame(bool locked = false) {
        return new Rectangle(locked ? 64 : 0, 0, 64, 64);
    }

    public int GetCompletedCount() {
        return (int)count.GetValue(achievement);
    }

    public int GetConditionCount() {
        return conditions.Count;
    }

    public virtual Tuple<decimal, decimal> GetProgressBar() {
        if (ShowProgressBar) {
            return Tuple.Create((decimal)GetCompletedCount(), (decimal)GetConditionCount());
        }
        foreach (ModCondition condition in conditions) {
            Tuple<decimal, decimal> t = condition.GetProgressBar();
            if (t != null) {
                return t;
            }
        }
        return Tuple.Create(0m, 0m);
    }
}
