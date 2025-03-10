using System.Collections.Generic;

using Terraria.ModLoader;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
sealed class TileMinedCondition : ModCondition {
    public ushort type;

    public TileMinedCondition(ushort tileType)
        : base(GetUniqueName(tileType)) {
        type = tileType;
    }

    public override void Register() {
        if (!AchievementLoader.tileListener.ContainsKey(type)) {
            AchievementLoader.tileListener[type] = new List<TileMinedCondition>();
        }
        AchievementLoader.tileListener[type].Add(this);
    }

    public void TileMined(ushort type) {
        if (this.type == type) {
            SetComplete();
        }
    }

    private static string GetUniqueName(ushort id) {
        ModTile type = TileLoader.GetTile(id);
        if (type == null) {
            return "MineTile_" + id;
        }
        return "MineTile_" + type.Name;
    }
}
