using System.Collections.Generic;

using Terraria;
using Terraria.Achievements;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
internal class ModCallAchievement : ModAchievement {
    private string name;

    private AchievementCategory cat;

    private bool bar;

    private bool card;

    private float cardOrder;

    private string tex;

    private string borderTex;

    private string[] addConditions;

    public override string Name => name;

    public override AchievementCategory Catagory => cat;

    public override bool ShowProgressBar => bar;

    public override bool ShowAchievementCard => card;

    public override float AchievementCardOrder => cardOrder;

    public override string Texture => tex;

    public override string CustomBorderTexture => borderTex;

    public override bool IsLoadingEnabled(Mod mod) {
        return false;
    }

    internal void Add(Mod mod, string name, AchievementCategory cat, string texture, string borderTexture, bool showBar, bool showCard, float cardOrder, string[] conditions) {
        if (Main.netMode != NetmodeID.Server && !ModLoader.HasMod("TMLAchievements")) {
            this.name = name;
            this.cat = cat;
            bar = showBar;
            card = showCard;
            this.cardOrder = cardOrder;
            tex = texture;
            borderTex = borderTexture;
            addConditions = conditions;
            typeof(ModType).GetProperty("Mod").SetValue(this, mod);
            Register();
        }
    }

    public override void AddConditions(List<ModCondition> conditions) {
        if (!ModLoader.HasMod("TMLAchievements")) {
            if (addConditions == null) {
                return;
            }
            string[] array = addConditions;
            foreach (string c in array) {
                int i = c.IndexOf('_');
                if (i < 1 || i == c.Length - 1) {
                    continue;
                }
                string type = c.Substring(0, i);
                string value = c.Substring(i + 1);
                switch (type) {
                    case "Collect": {
                        if (short.TryParse(value, out var j)) {
                            conditions.Add(new CollectItemCondition(j));
                        }
                        break;
                    }
                    case "Craft": {
                        if (short.TryParse(value, out var k)) {
                            conditions.Add(new CraftItemCondition(k));
                        }
                        break;
                    }
                    case "Mine": {
                        if (ushort.TryParse(value, out var l)) {
                            conditions.Add(new TileMinedCondition(l));
                        }
                        break;
                    }
                    case "Kill": {
                        if (short.TryParse(value, out var m)) {
                            conditions.Add(new NPCKillCondition(m));
                        }
                        break;
                    }
                    case "Event":
                        conditions.Add(new CustomEventCondition(value));
                        break;
                    case "ValueEvent": {
                        i = value.IndexOf('_');
                        if (i != -1 && i != c.Length - 1 && float.TryParse(value.Substring(i + 1), out var num)) {
                            conditions.Add(new CustomValueCondition(value.Substring(0, i), num));
                        }
                        break;
                    }
                }
            }
        }
    }
}
