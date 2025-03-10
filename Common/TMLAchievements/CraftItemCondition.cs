using System.Collections.Generic;

using Terraria.ModLoader;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
sealed class CraftItemCondition : ModCondition {
    public short type;

    public CraftItemCondition(short itemType)
        : base(GetUniqueName(itemType)) {
        type = itemType;
    }

    public override void Register() {
        if (!AchievementLoader.craftListener.ContainsKey(type)) {
            AchievementLoader.craftListener[type] = new List<CraftItemCondition>();
        }
        AchievementLoader.craftListener[type].Add(this);
    }

    public void ItemCrafted(short type, int count) {
        if (this.type == type) {
            SetComplete();
        }
    }

    private static string GetUniqueName(short id) {
        ModItem type = ItemLoader.GetItem(id);
        if (type == null) {
            return "CraftItem_" + id;
        }
        return "CraftItem_" + type.Name;
    }
}
