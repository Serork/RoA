using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core;

using Terraria;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
static class AchievementLoader {
    internal static readonly List<ModAchievement> modAchievements = new List<ModAchievement>();

    internal static readonly List<AchievementAdvisorCard> modCards = new List<AchievementAdvisorCard>();

    public static Asset<Texture2D>[] AchievementTexture;

    public static Dictionary<int, Asset<Texture2D>> AchievementBorderTextures;

    public static readonly List<Mod> ModsThatAddAchievements = new List<Mod>();

    internal static FieldInfo Name = typeof(Achievement).GetField("FriendlyName", BindingFlags.Instance | BindingFlags.Public);

    internal static FieldInfo Desc = typeof(Achievement).GetField("Description", BindingFlags.Instance | BindingFlags.Public);

    internal static FieldInfo Cards = typeof(AchievementAdvisor).GetField("_cards", BindingFlags.Instance | BindingFlags.NonPublic);

    internal static Dictionary<short, List<CollectItemCondition>> itemListener = new Dictionary<short, List<CollectItemCondition>>();

    internal static Dictionary<short, List<CraftItemCondition>> craftListener = new Dictionary<short, List<CraftItemCondition>>();

    internal static Dictionary<ushort, List<TileMinedCondition>> tileListener = new Dictionary<ushort, List<TileMinedCondition>>();

    internal static Dictionary<short, List<NPCKillCondition>> npcKilled = new Dictionary<short, List<NPCKillCondition>>();

    internal static Dictionary<string, List<CustomEventCondition>> customEventListener = new Dictionary<string, List<CustomEventCondition>>();

    internal static Dictionary<string, List<CustomValueCondition>> customValueListener = new Dictionary<string, List<CustomValueCondition>>();

    public static int Count => modAchievements.Count;

    internal static void Add(ModAchievement achievement) {
        List<ModCondition> conditions = new List<ModCondition>();
        achievement.AddConditions(conditions);
        achievement.conditions = conditions;
        modAchievements.Add(achievement);
        TMLAchievement a = achievement.achievement = new TMLAchievement(achievement, achievement.Mod.Name + "-" + achievement.Name);
        foreach (ModCondition c in conditions) {
            c.Register();
            achievement.achievement.AddCondition(c.condition);
        }
        Main.Achievements.Register(a);
        Main.Achievements.RegisterAchievementCategory(a.Name, achievement.Catagory);
        if (!ModsThatAddAchievements.Contains(achievement.Mod)) {
            ModsThatAddAchievements.Add(achievement.Mod);
        }
    }

    internal static void Load() {
        if (!ModLoader.HasMod("TMLAchievements")) {
            AchievementsHelper.OnItemPickup += OnItemPickup;
            AchievementsHelper.OnItemCraft += OnItemCrafted;
            AchievementsHelper.OnTileDestroyed += OnTileMined;
            AchievementsHelper.OnNPCKilled += OnNPCKilled;
        }
    }

    internal static void Unload() {
        if (!ModLoader.HasMod("TMLAchievements")) {
            AchievementsHelper.OnItemPickup -= OnItemPickup;
            AchievementsHelper.OnItemCraft -= OnItemCrafted;
            AchievementsHelper.OnTileDestroyed -= OnTileMined;
            AchievementsHelper.OnNPCKilled -= OnNPCKilled;
            Dictionary<string, Achievement> dict = (Dictionary<string, Achievement>)TMLAchievements.AchievementsField.GetValue(Main.Achievements);
            foreach (ModAchievement a in modAchievements) {
                dict.Remove(a.achievement.Name);
            }
            modAchievements.Clear();
            typeof(Achievement).GetField("_totalAchievements", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, dict.Count);
            List<AchievementAdvisorCard> cards = (List<AchievementAdvisorCard>)Cards.GetValue(Main.AchievementAdvisor);
            foreach (AchievementAdvisorCard i in modCards) {
                cards.Remove(i);
            }
        }
    }

    internal static void PostSetup() {
        if (!ModLoader.HasMod("TMLAchievements")) {
            SetupAchievementTextures();
            SetupAchievementCards();
            TMLAchievements.LoadLastPlayed();
            TMLAchievements.LoadAchievements();
        }
    }

    internal static void SetupAchievementTextures() {
        AchievementTexture = new Asset<Texture2D>[Count];
        AchievementBorderTextures = new Dictionary<int, Asset<Texture2D>>();
        Asset<Texture2D> baseTex = ModContent.Request<Texture2D>(ResourceManager.AchievementsTextures + "BaseAchievement", (AssetRequestMode)2);
        foreach (ModAchievement i in modAchievements) {
            try {
                if (i.Texture != null && ModContent.HasAsset(i.Texture)) {
                    Asset<Texture2D> tex2 = ModContent.Request<Texture2D>(i.Texture, (AssetRequestMode)2);
                    AchievementTexture[i.Id] = tex2;
                    i.achievement.texture = tex2;
                }
                else {
                    AchievementTexture[i.Id] = baseTex;
                    i.achievement.texture = baseTex;
                }
                if (i.CustomBorderTexture != null && ModContent.HasAsset(i.CustomBorderTexture)) {
                    Asset<Texture2D> tex = ModContent.Request<Texture2D>(i.CustomBorderTexture, (AssetRequestMode)2);
                    AchievementBorderTextures[i.Id] = tex;
                    i.achievement.borderTex = tex;
                }
            }
            catch {
                AchievementTexture[i.Id] = baseTex;
                i.achievement.texture = baseTex;
            }
        }
        TMLAchievements.buttonTex = ModContent.Request<Texture2D>(ResourceManager.AchievementsTextures + "EmptyIcon", (AssetRequestMode)2);
        TMLAchievements.LoadVanillaButton.tex = ModContent.Request<Texture2D>(ResourceManager.AchievementsTextures + "LoadVanillaIcon", (AssetRequestMode)2);
        TMLAchievements.ModCatagoryButton.allicon = ModContent.Request<Texture2D>(ResourceManager.AchievementsTextures + "AllIcon", (AssetRequestMode)2);
        TMLAchievements.ModCatagoryButton.modicon = ModContent.Request<Texture2D>(ResourceManager.AchievementsTextures + "ModdedIcon", (AssetRequestMode)2);
    }

    internal static void SetupAchievementNames() {
        foreach (ModAchievement i in modAchievements) {
            Achievement a = i.achievement;
            string key = "Mods." + i.Mod.Name + ".Achievements." + i.Name;
            Name.SetValue(a, Language.GetText(key + ".Name"));
            Desc.SetValue(a, Language.GetText(key + ".Description"));
        }
    }

    internal static void SetupAchievementCards() {
        List<AchievementAdvisorCard> cards = (List<AchievementAdvisorCard>)Cards.GetValue(Main.AchievementAdvisor);
        bool flag = false;
        foreach (ModAchievement a in modAchievements) {
            if (a.ShowAchievementCard) {
                flag = true;
                AchievementAdvisorCard card = new TMLAchievementCard(a);
                cards.Add(card);
                modCards.Add(card);
            }
        }
        if (flag) {
            Cards.SetValue(Main.AchievementAdvisor, cards.OrderBy((c) => c.order).ToList());
        }
        foreach (var ach in cards.OrderBy((c) => c.order).ToList()) {
            Console.WriteLine(ach.achievement.Name + " " + ach.order);
        }
    }

    public static void OnItemPickup(Player player, short type, int count) {
        if (player.whoAmI != Main.myPlayer || !itemListener.ContainsKey(type)) {
            return;
        }
        foreach (CollectItemCondition c in itemListener[type]) {
            if (!c.Completed) {
                c.ItemCollected(type, count);
            }
        }
    }

    public static void OnItemCrafted(short type, int count) {
        if (!craftListener.ContainsKey(type)) {
            return;
        }
        foreach (CraftItemCondition c in craftListener[type]) {
            if (!c.Completed) {
                c.ItemCrafted(type, count);
            }
        }
    }

    public static void OnTileMined(Player player, ushort type) {
        if (player.whoAmI != Main.myPlayer || !tileListener.ContainsKey(type)) {
            return;
        }
        foreach (TileMinedCondition c in tileListener[type]) {
            if (!c.Completed) {
                c.TileMined(type);
            }
        }
    }

    public static void OnNPCKilled(Player player, short type) {
        if (player.whoAmI != Main.myPlayer || !npcKilled.ContainsKey(type)) {
            return;
        }
        foreach (NPCKillCondition c in npcKilled[type]) {
            if (!c.Completed) {
                c.NPCKilled(type);
            }
        }
    }

    public static void OnEvent(string eventName) {
        foreach (CustomEventCondition c in customEventListener[eventName]) {
            if (!c.Completed) {
                c.CheckEvent(eventName);
            }
        }
    }

    public static void CustomValueEvent(string eventName, float value) {
        foreach (CustomValueCondition c in customValueListener[eventName]) {
            if (!c.Completed) {
                c.AddValue(eventName, value);
            }
        }
    }
}
