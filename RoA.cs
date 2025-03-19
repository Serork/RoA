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

            Main.NewText(RoAAchievementInGameNotification.RoAAchievementStorage.MineMercuriumNugget);

            if (RoAAchievementInGameNotification.RoAAchievementStorage.DefeatLothor) {
                CompleteAchievement("DefeatLothor");
            }
            if (RoAAchievementInGameNotification.RoAAchievementStorage.MineMercuriumNugget) {
                CompleteAchievement("MineMercuriumNugget");
            }
            if (RoAAchievementInGameNotification.RoAAchievementStorage.OpenRootboundChest) {
                CompleteAchievement("OpenRootboundChest");
            }
            if (RoAAchievementInGameNotification.RoAAchievementStorage.SurviveBackwoodsFog) {
                CompleteAchievement("SurviveBackwoodsFog");
            }
            if (RoAAchievementInGameNotification.RoAAchievementStorage.CraftDruidWreath) {
                CompleteAchievement("CraftDruidWreath");
            }
            if (RoAAchievementInGameNotification.RoAAchievementStorage.DefeatLothorEnraged) {
                CompleteAchievement("DefeatLothorEnraged");
            }
        }
    }

    public static void ShowAchievementNotification(string name) {
        if (ModLoader.HasMod("TMLAchievements")) {
            return;
        }

        bool flag = false;
        switch (name) {
            case "DefeatLothor":
                flag = RoAAchievementInGameNotification.RoAAchievementStorage.DefeatLothor;
                break;
            case "MineMercuriumNugget":
                flag = RoAAchievementInGameNotification.RoAAchievementStorage.MineMercuriumNugget;
                break;
            case "OpenRootboundChest":
                flag = RoAAchievementInGameNotification.RoAAchievementStorage.OpenRootboundChest;
                break;
            case "SurviveBackwoodsFog":
                flag = RoAAchievementInGameNotification.RoAAchievementStorage.SurviveBackwoodsFog;
                break;
            case "CraftDruidWreath":
                flag = RoAAchievementInGameNotification.RoAAchievementStorage.CraftDruidWreath;
                break;
            case "DefeatLothorEnraged":
                flag = RoAAchievementInGameNotification.RoAAchievementStorage.DefeatLothorEnraged;
                break;
        }
        if (!flag) {
            InGameNotificationsTracker.AddNotification(new RoAAchievementInGameNotification(name));
            Main.NewText(Language.GetTextValue("Achievements.Completed", Language.GetTextValue($"Mods.RoA.Achievements.{name}.Name")));
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
                    new string[] { "Kill_" + ModContent.NPCType<Lothor>() });

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
