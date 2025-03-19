using MonoMod.RuntimeDetour;

using ReLogic.Content.Sources;

using RoA.Common.Networking;
using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Core;

using System.IO;
using System.Reflection;

using Terraria;
using Terraria.Achievements;
using Terraria.ModLoader;

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

    private void LoadAchievements() {
        if (!ModLoader.HasMod("TMLAchievements")) {
        }
        else {
            if (ModLoader.TryGetMod("TMLAchievements", out Mod mod)) {
                mod.Call("AddAchievement", Instance,
                    "BestialCommunion",
                    AchievementCategory.Slayer,
                    ResourceManager.AchievementsTextures + "Achievement_DefeatLothor", null, 
                    false, true,
                    20.5f,
                    new string[] { "Kill_" + ModContent.NPCType<Lothor>() });

                mod.Call("AddAchievement", Instance,
                    "WhatsThatSmell",
                    AchievementCategory.Explorer,
                    ResourceManager.AchievementsTextures + "Achievement_MineMercuriumNugget", null,
                    false, true,
                    6.5f,
                    new string[] { "Event_" + "WhatsThatSmell" });

                mod.Call("AddAchievement", Instance,
                    "GrootsLoot",
                    AchievementCategory.Explorer,
                    ResourceManager.AchievementsTextures + "Achievement_OpenRootboundChest", null,
                    false, true,
                    14.5f,
                    new string[] { "Event_" + "OpenRootboundChest" });

                mod.Call("AddAchievement", Instance,
                    "SilentHills",
                    AchievementCategory.Slayer,
                    ResourceManager.AchievementsTextures + "Achievement_SurviveBackwoodsFog", null,
                    false, false,
                    0f,
                    new string[] { "Event_" + "SilentHills" });

                mod.Call("AddAchievement", Instance,
                    "NotPostMortem",
                    AchievementCategory.Collector,
                    ResourceManager.AchievementsTextures + "Achievement_CraftDruidWreath", null,
                    false, true,
                    13.5f,
                    new string[] { "Event_" + "NotPostMortem" });

                mod.Call("AddAchievement", Instance,
                    "GutsOfSteel",
                    AchievementCategory.Challenger,
                    ResourceManager.AchievementsTextures + "Achievement_DefeatLothorEnraged", null,
                    false, true,
                    20.6f,
                    new string[] { "Event_" + "GutsOfSteel" });
            }
        }
    }
}
