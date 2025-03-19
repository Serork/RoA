using Microsoft.Xna.Framework;

using MonoMod.RuntimeDetour;

using ReLogic.Content.Sources;

using RoA.Common;
using RoA.Common.Networking;
using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Core;

using System.IO;
using System.Reflection;

using Terraria;
using Terraria.Achievements;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace RoA;

sealed class RoA : Mod {
    public static readonly string ModSourcePath = Path.Combine(Program.SavePathShared, "ModSources");

    private static RoA? _instance;

    public RoA() {
        _instance = this;
    }

    public static RoA Instance => _instance ??= ModContent.GetInstance<RoA>();

    public static string ModName => Instance.Name;

    public override IContentSource CreateDefaultContentSource() => new CustomContentSource(base.CreateDefaultContentSource());

    public override void HandlePacket(BinaryReader reader, int sender) {
        MultiplayerSystem.HandlePacket(reader, sender);
    }

    public override void PostSetupContent() {
        foreach (IPostSetupContent type in GetContent<IPostSetupContent>()) {
            type.PostSetupContent();
        }

        LoadAchievements();
    }

    public static Hook Detour(MethodInfo source, MethodInfo target) {
        Hook hook = new(source, target);
        hook.Apply();

        return hook;
    }

    private class CompleteAchivementsOnEnteringWorldSystem : ModPlayer {
        public override void OnEnterWorld() {
            if (!ModLoader.HasMod("TMLAchievements")) {
                return;
            }

            if (Player.whoAmI != Main.myPlayer) {
                return;
            }

            var storage = Player.GetModPlayer<RoAAchievementInGameNotification.RoAAchievementStorage_Player>();
            if (storage.DefeatLothor) {
                CompleteAchievement("DefeatLothor");
            }
            if (storage.MineMercuriumNugget) {
                CompleteAchievement("MineMercuriumNugget");
            }
            if (storage.OpenRootboundChest) {
                CompleteAchievement("OpenRootboundChest");
            }
            if (storage.SurviveBackwoodsFog) {
                CompleteAchievement("SurviveBackwoodsFog");
            }
            if (storage.CraftDruidWreath) {
                CompleteAchievement("CraftDruidWreath");
            }
            if (storage.DefeatLothorEnraged) {
                CompleteAchievement("DefeatLothorEnraged");
            }
        }
    }

    public static void ShowAchievementNotification(string name) {
        if (ModLoader.HasMod("TMLAchievements")) {
            return;
        }

        var storage = Main.LocalPlayer.GetModPlayer<RoAAchievementInGameNotification.RoAAchievementStorage_Player>();
        bool flag = false;
        switch (name) {
            case "DefeatLothor":
                flag = storage.DefeatLothor;
                break;
            case "MineMercuriumNugget":
                flag = storage.MineMercuriumNugget;
                break;
            case "OpenRootboundChest":
                flag = storage.OpenRootboundChest;
                break;
            case "SurviveBackwoodsFog":
                flag = storage.SurviveBackwoodsFog;
                break;
            case "CraftDruidWreath":
                flag = storage.CraftDruidWreath;
                break;
            case "DefeatLothorEnraged":
                flag = storage.DefeatLothorEnraged;
                break;
        }
        if (!flag) {
            InGameNotificationsTracker.AddNotification(new RoAAchievementInGameNotification(name));
            string achievementName = Language.GetTextValue($"Mods.RoA.Achievements.{name}.Name");
            Main.NewText(Language.GetTextValue("Achievements.Completed", "[c/ADD8E6:" + achievementName + "]"));
            if (SoundEngine.FindActiveSound(in SoundID.AchievementComplete) == null) {
                SoundEngine.PlaySound(in SoundID.AchievementComplete);
            }
        }
    }

    public static void CompleteAchievement(string name) {
        if (Main.netMode != NetmodeID.Server) {
            if (ModLoader.TryGetMod("TMLAchievements", out Mod mod)) {
                mod.Call("Event", name);
            }

            ShowAchievementNotification(name);
        }
    }

    private void LoadAchievements() {
        if (!ModLoader.HasMod("TMLAchievements")) {
        }
        else {
            if (ModLoader.TryGetMod("TMLAchievements", out Mod mod)) {
                mod.Call("AddAchievement", Instance,
                    "DefeatLothor",
                    AchievementCategory.Slayer,
                    ResourceManager.AchievementsTextures + "Achievement_DefeatLothor", null, 
                    false, true,
                    20.5f,
                    new string[] { "Event_" + "DefeatLothor" });

                mod.Call("AddAchievement", Instance,
                    "MineMercuriumNugget",
                    AchievementCategory.Explorer,
                    ResourceManager.AchievementsTextures + "Achievement_MineMercuriumNugget", null,
                    false, true,
                    6.5f,
                    new string[] { "Event_" + "MineMercuriumNugget" });

                mod.Call("AddAchievement", Instance,
                    "OpenRootboundChest",
                    AchievementCategory.Explorer,
                    ResourceManager.AchievementsTextures + "Achievement_OpenRootboundChest", null,
                    false, true,
                    14.5f,
                    new string[] { "Event_" + "OpenRootboundChest" });

                mod.Call("AddAchievement", Instance,
                    "SurviveBackwoodsFog",
                    AchievementCategory.Slayer,
                    ResourceManager.AchievementsTextures + "Achievement_SurviveBackwoodsFog", null,
                    false, false,
                    0f,
                    new string[] { "Event_" + "SurviveBackwoodsFog" });

                mod.Call("AddAchievement", Instance,
                    "CraftDruidWreath",
                    AchievementCategory.Collector,
                    ResourceManager.AchievementsTextures + "Achievement_CraftDruidWreath", null,
                    false, true,
                    13.5f,
                    new string[] { "Event_" + "CraftDruidWreath" });

                mod.Call("AddAchievement", Instance,
                    "DefeatLothorEnraged",
                    AchievementCategory.Challenger,
                    ResourceManager.AchievementsTextures + "Achievement_DefeatLothorEnraged", null,
                    false, true,
                    20.6f,
                    new string[] { "Event_" + "DefeatLothorEnraged" });
            }
        }
    }
}
