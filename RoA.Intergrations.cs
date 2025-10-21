using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.NPCs;
using RoA.Content.Items;
using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Content.Items.Pets;
using RoA.Content.Items.Special.Lothor;
using RoA.Content.Items.Weapons.Nature.PreHardmode;
using RoA.Content.NPCs.Enemies.Bosses.Lothor;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Achievements;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA;

sealed partial class RoA : Mod {
    private static Asset<Texture2D>? _brilliantBouquetTextureForRecipeBrowser, _fenethsWreathTextureForRecipeBrowser;

    public override void Load() {
        if (!Main.dedServ) {
            Main.RunOnMainThread(() => {
                _brilliantBouquetTextureForRecipeBrowser = DrawUtils.ResizeImage(ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<BrilliantBouquet>()).Texture, AssetRequestMode.ImmediateLoad), 24, 24);
                _fenethsWreathTextureForRecipeBrowser = DrawUtils.ResizeImage(ModContent.Request<Texture2D>(ItemLoader.GetItem(ModContent.ItemType<FenethsBlazingWreath>()).Texture, AssetRequestMode.ImmediateLoad), 24, 24);
            });
        }
    }

    private void DoRecipeBrowserIntergration() {
        if (ModLoader.TryGetMod("RecipeBrowser", out Mod mod) && !Main.dedServ) {
            mod.Call([
                "AddItemCategory",
                "Nature",
                "Weapons",
                _brilliantBouquetTextureForRecipeBrowser,
                (Predicate<Item>)((Item item) => {
                    return item.damage > 0 && item.ModItem is NatureItem;
                })
            ]);

            mod.Call([
                "AddItemCategory",
                "Wreaths",
                "Accessories",
                _fenethsWreathTextureForRecipeBrowser,
                (Predicate<Item>)((Item item) => {
                    return item.accessory && item.ModItem is WreathItem;
                })
            ]);
        }
    }

    private void DoMusicDisplayIntegration() {
        if (!ModLoader.TryGetMod("MusicDisplay", out Mod musicDisplay)) {
            return;
        }

        void addMusicDisplayIntergationFor(string name, string pathToMusic) {
            Mod mod = Instance;
            short slotId = (short)MusicLoader.GetMusicSlot(RoA.MusicMod, pathToMusic);
            LocalizedText author = Language.GetOrRegister($"Mods.RoA.MusicDisplay.{name}.Author");
            LocalizedText displayName = Language.GetOrRegister($"Mods.RoA.MusicDisplay.{name}.DisplayName");
            musicDisplay.Call(
                "AddMusic", slotId, displayName, author, mod.DisplayName);
        }
        addMusicDisplayIntergationFor("Lothor", $"{ResourceManager.SOUNDSPATH}/Music/Lothor");
        addMusicDisplayIntergationFor("Backwoods", $"{ResourceManager.SOUNDSPATH}/Music/ThicketNight");
        addMusicDisplayIntergationFor("BackwoodsFog", $"{ResourceManager.SOUNDSPATH}/Music/Fog");
    }

    private void DoBossChecklistIntegration() {
        if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod)) {
            return;
        }

        if (bossChecklistMod.Version < new Version(1, 6)) {
            return;
        }

        string internalName = nameof(Lothor);

        // Value inferred from boss progression, see the wiki for details
        float weight = 6.5f;

        // Used for tracking checklist progress
        Func<bool> downed = () => DownedBossSystem.DownedLothorBoss;

        // The NPC type of the boss
        int bossType = ModContent.NPCType<Lothor>();

        // "collectibles" like relic, trophy, mask, pet
        List<int> collectibles = new List<int>() {
            ModContent.ItemType<LothorRelic>(),
            ModContent.ItemType<MoonFlower>(),
            ModContent.ItemType<LothorTrophy>(),
            ModContent.ItemType<LothorMask>(),
            ModContent.ItemType<LothorMusicBox>(),
            ModContent.ItemType<BloodCursor>(),
        };

        var customPortrait = (SpriteBatch sb, Rectangle rect, Color color) => {
            Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.Textures + "Lothor_Checklist").Value;
            Vector2 centered = new Vector2(rect.X + (rect.Width / 2) - (texture.Width / 2), rect.Y + (rect.Height / 2) - (texture.Height / 2));
            sb.Draw(texture, centered, color);
        };

        bossChecklistMod.Call(
            "LogBoss",
            Instance,
            internalName,
            weight,
            downed,
            bossType,
            new Dictionary<string, object>() {
                ["spawnInfo"] = Language.GetOrRegister("Mods.RoA.ChecklistLothorSummon"),
                ["collectibles"] = collectibles,
                ["customPortrait"] = customPortrait
                // Other optional arguments as needed are inferred from the wiki
            }
        );
    }

    //private class CompleteAchivementsOnEnteringWorldSystem : ModPlayer {
    //    public override void OnEnterWorld() {
    //        if (!ModLoader.HasMod("TMLAchievements")) {
    //            return;
    //        }

    //        if (Player.whoAmI != Main.myPlayer) {
    //            return;
    //        }

    //        var storage = Player.GetModPlayer<RoAAchievementInGameNotification.RoAAchievementStorage_Player>();
    //        if (storage.DefeatLothor) {
    //            CompleteAchievement("DefeatLothor");
    //        }
    //        if (storage.MineMercuriumNugget) {
    //            CompleteAchievement("MineMercuriumNugget");
    //        }
    //        if (storage.OpenRootboundChest) {
    //            CompleteAchievement("OpenRootboundChest");
    //        }
    //        if (storage.SurviveBackwoodsFog) {
    //            CompleteAchievement("SurviveBackwoodsFog");
    //        }
    //        if (storage.CraftDruidWreath) {
    //            CompleteAchievement("CraftDruidWreath");
    //        }
    //        if (storage.DefeatLothorEnraged) {
    //            CompleteAchievement("DefeatLothorEnraged");
    //        }
    //    }
    //}

    //public static void ShowAchievementNotification(string name) {
    //    if (ModLoader.HasMod("TMLAchievements")) {
    //        return;
    //    }

    //    var storage = Main.LocalPlayer.GetModPlayer<RoAAchievementInGameNotification.RoAAchievementStorage_Player>();
    //    bool flag = false;
    //    switch (name) {
    //        case "DefeatLothor":
    //            flag = storage.DefeatLothor;
    //            break;
    //        case "MineMercuriumNugget":
    //            flag = storage.MineMercuriumNugget;
    //            break;
    //        case "OpenRootboundChest":
    //            flag = storage.OpenRootboundChest;
    //            break;
    //        case "SurviveBackwoodsFog":
    //            flag = storage.SurviveBackwoodsFog;
    //            break;
    //        case "CraftDruidWreath":
    //            flag = storage.CraftDruidWreath;
    //            break;
    //        case "DefeatLothorEnraged":
    //            flag = storage.DefeatLothorEnraged;
    //            break;
    //    }
    //    if (!flag) {
    //        InGameNotificationsTracker.AddNotification(new RoAAchievementInGameNotification(name));
    //        string achievementName = Language.GetTextValue($"Mods.RoA.Achievements.{name}.Name");
    //        Main.NewText(Language.GetTextValue("Achievements.Completed", "[c/ADD8E6:" + achievementName + "]"));
    //        if (SoundEngine.FindActiveSound(in SoundID.AchievementComplete) == null) {
    //            SoundEngine.PlaySound(in SoundID.AchievementComplete);
    //        }
    //    }
    //}

    //public static void CompleteAchievement(string name) {
    //    if (Main.netMode != NetmodeID.Server) {
    //        if (ModLoader.TryGetMod("TMLAchievements", out Mod mod)) {
    //            mod.Call("Event", name);
    //        }

    //        ShowAchievementNotification(name);
    //    }
    //}

    //private void LoadAchievements() {
    //    if (!ModLoader.HasMod("TMLAchievements")) {
    //    }
    //    else {
    //        if (ModLoader.TryGetMod("TMLAchievements", out Mod mod)) {
    //            mod.Call("AddAchievement", Instance,
    //                "DefeatLothor",
    //                AchievementCategory.Slayer,
    //                ResourceManager.AchievementsTextures + "Achievement_DefeatLothor", null,
    //                false, true,
    //                20.5f,
    //                new string[] { "Event_" + "DefeatLothor" });

    //            mod.Call("AddAchievement", Instance,
    //                "MineMercuriumNugget",
    //                AchievementCategory.Explorer,
    //                ResourceManager.AchievementsTextures + "Achievement_MineMercuriumNugget", null,
    //                false, true,
    //                13.75f,
    //                new string[] { "Event_" + "MineMercuriumNugget" });

    //            mod.Call("AddAchievement", Instance,
    //                "OpenRootboundChest",
    //                AchievementCategory.Explorer,
    //                ResourceManager.AchievementsTextures + "Achievement_OpenRootboundChest", null,
    //                false, true,
    //                14.5f,
    //                new string[] { "Event_" + "OpenRootboundChest" });

    //            mod.Call("AddAchievement", Instance,
    //                "SurviveBackwoodsFog",
    //                AchievementCategory.Slayer,
    //                ResourceManager.AchievementsTextures + "Achievement_SurviveBackwoodsFog", null,
    //                false, false,
    //                0f,
    //                new string[] { "Event_" + "SurviveBackwoodsFog" });

    //            mod.Call("AddAchievement", Instance,
    //                "CraftDruidWreath",
    //                AchievementCategory.Collector,
    //                ResourceManager.AchievementsTextures + "Achievement_CraftDruidWreath", null,
    //                false, true,
    //                13.5f,
    //                new string[] { "Event_" + "CraftDruidWreath" });

    //            mod.Call("AddAchievement", Instance,
    //                "DefeatLothorEnraged",
    //                AchievementCategory.Challenger,
    //                ResourceManager.AchievementsTextures + "Achievement_DefeatLothorEnraged", null,
    //                false, true,
    //                20.6f,
    //                new string[] { "Event_" + "DefeatLothorEnraged" });
    //        }
    //    }
    //}
}
