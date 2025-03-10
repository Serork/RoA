using System.Collections.Generic;

using Terraria.ModLoader;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
sealed class CollectItemCondition : ModCondition {
    public short type;

    public CollectItemCondition(short itemType)
        : base(GetUniqueName(itemType)) {
        type = itemType;
    }

    public override void Register() {
        if (!AchievementLoader.itemListener.ContainsKey(type)) {
            AchievementLoader.itemListener[type] = new List<CollectItemCondition>();
        }
        AchievementLoader.itemListener[type].Add(this);
    }

    public void ItemCollected(short type, int count) {
        if (this.type == type) {
            SetComplete();
        }
    }

    private static string GetUniqueName(short id) {
        ModItem type = ItemLoader.GetItem(id);
        if (type == null) {
            return "CollectItem_" + id;
        }
        return "CollectItem_" + type.Name;
    }
}
