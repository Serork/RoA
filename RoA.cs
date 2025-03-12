using MonoMod.RuntimeDetour;

using ReLogic.Content.Sources;

using RoA.Common.Networking;
using RoA.Common.TMLAchievements;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Core;

using System.IO;
using System.Reflection;

using Terraria;
using Terraria.Achievements;
using Terraria.ID;
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
            new ModCallAchievement().Add(Instance,
            "BestialCommunion",
            AchievementCategory.Slayer,
            null, null,
            false, true,
            20.5f,
            ["Kill_" + ModContent.NPCType<Lothor>()]);

            new ModCallAchievement().Add(Instance,
            "WhatsThatSmell",
            AchievementCategory.Explorer,
            ResourceManager.AchievementsTextures + "Achievement_MineMercuriumNugget", null,
            false, true,
            6.5f,
            ["Event_" + TMLAchievements.RoAAchivement.WhatsThatSmell]);

            new ModCallAchievement().Add(Instance,
            "GrootsLoot",
            AchievementCategory.Explorer,
            ResourceManager.AchievementsTextures + "Achievement_OpenRootboundChest", null,
            false, true,
            14.5f,
            ["Event_" + TMLAchievements.RoAAchivement.OpenRootboundChest]);

            new ModCallAchievement().Add(Instance,
            "SilentHills",
            AchievementCategory.Slayer,
            null, null,
            false, false,
            0f,
            ["Event_" + TMLAchievements.RoAAchivement.SilentHills]);

            new ModCallAchievement().Add(Instance,
            "NotPostMortem",
            AchievementCategory.Collector,
            ResourceManager.AchievementsTextures + "Achievement_CraftDruidWreath", null,
            false, true,
            13.5f,
            ["Event_" + TMLAchievements.RoAAchivement.NotPostMortem]);

            new ModCallAchievement().Add(Instance,
            "GutsOfSteel",
            AchievementCategory.Challenger,
            null, null,
            false, true,
            20.6f,
            ["Event_" + TMLAchievements.RoAAchivement.GutsOfSteel]);
        }
        else {
            if (ModLoader.TryGetMod("TMLAchievements", out Mod mod)) {
                mod.Call("AddAchievement", Instance,
                    "BestialCommunion",
                    AchievementCategory.Slayer,
                    null,
                    null, 
                    false, true,
                    20.5f,
                    new string[] { "Kill_" + ModContent.NPCType<Lothor>() });

                mod.Call("AddAchievement", Instance,
                    "WhatsThatSmell",
                    AchievementCategory.Explorer,
                    ResourceManager.AchievementsTextures + "Achievement_MineMercuriumNugget", null,
                    false, true,
                    6.5f,
                    new string[] { "Event_" + TMLAchievements.RoAAchivement.WhatsThatSmell });

                mod.Call("AddAchievement", Instance,
                    "GrootsLoot",
                    AchievementCategory.Explorer,
                    null,
                    null,
                    false, true,
                    14.5f,
                    new string[] { "Event_" + TMLAchievements.RoAAchivement.OpenRootboundChest });

                mod.Call("AddAchievement", Instance,
                    "SilentHills",
                    AchievementCategory.Slayer,
                    null,
                    null,
                    false, false,
                    0f,
                    new string[] { "Event_" + TMLAchievements.RoAAchivement.SilentHills });

                mod.Call("AddAchievement", Instance,
                    "NotPostMortem",
                    AchievementCategory.Collector,
                    null,
                    null,
                    false, true,
                    13.5f,
                    new string[] { "Event_" + TMLAchievements.RoAAchivement.NotPostMortem });

                mod.Call("AddAchievement", Instance,
                    "GutsOfSteel",
                    AchievementCategory.Challenger,
                    null,
                    null,
                    false, true,
                    20.6f,
                    new string[] { "Event_" + TMLAchievements.RoAAchivement.GutsOfSteel });
            }
        }
    }
}
