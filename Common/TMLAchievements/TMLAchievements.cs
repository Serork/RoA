using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using log4net;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ReLogic.Content;

using Terraria;
using Terraria.Achievements;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.TMLAchievements;

// This code is taken from Achievement mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2927542027)
// This will NOT run if Achievement mod is installed
// We use Achievement mod instead if possible
public class TMLAchievements : ModSystem {
    public class ModCatagoryButton : UIElement {
        internal static Asset<Texture2D> allicon;

        internal static Asset<Texture2D> modicon;

        private Asset<Texture2D> icon;

        private string text;

        internal static string showAll;

        internal static string showModded;

        internal static LocalizedText showMod;

        public ModCatagoryButton() {
            Update();
            Width.Set(32f, 0f);
            Height.Set(32f, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            Color color = IsMouseHovering ? Color.White : Color.Silver;
            CalculatedStyle s = GetDimensions();
            spriteBatch.Draw(buttonTex.Value, new Rectangle((int)s.X, (int)s.Y, 32, 32), null, color);
            if (icon != null) {
                spriteBatch.Draw(icon.Value, new Rectangle((int)s.X + 1, (int)s.Y + 1, 30, 30), null, color);
            }
            if (IsMouseHovering) {
                Vector2 pos = new Vector2(Main.mouseX, Main.mouseY) + new Vector2(16f);
                Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, text, pos.X, pos.Y, new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), Color.Black, Vector2.Zero);
            }
        }

        public override void LeftClick(UIMouseEvent evt) {
            int num = AchievementLoader.ModsThatAddAchievements.Count;
            ModFilter++;
            if (ModFilter > num + 1) {
                ModFilter = 0;
            }
            if (num == 1 && ModFilter == 1) {
                ModFilter = 2;
            }
            Update();
            base.LeftClick(evt);
        }

        public override void RightClick(UIMouseEvent evt) {
            int num = AchievementLoader.ModsThatAddAchievements.Count;
            ModFilter--;
            if (ModFilter < 0) {
                ModFilter = num + 1;
            }
            if (num == 1 && ModFilter == 1) {
                ModFilter = 0;
            }
            Update();
            base.RightClick(evt);
        }

        private void Update() {
            if (ModFilter == 0) {
                text = showAll;
                icon = allicon;
                return;
            }
            if (ModFilter == 1) {
                text = showModded;
                icon = modicon;
                return;
            }
            Mod i = AchievementLoader.ModsThatAddAchievements[ModFilter - 2];
            text = showMod.Format(i.DisplayName);
            if (i.HasAsset("icon_small")) {
                icon = i.Assets.Request<Texture2D>("icon_small", (AssetRequestMode)2);
            }
            else {
                icon = null;
            }
        }
    }

    public class LoadVanillaButton : UIElement {
        internal static Asset<Texture2D> tex;

        internal static string text;

        public LoadVanillaButton() {
            Width.Set(32f, 0f);
            Height.Set(32f, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            Color color = IsMouseHovering ? Color.White : Color.Silver;
            CalculatedStyle s = GetDimensions();
            spriteBatch.Draw(tex.Value, new Rectangle((int)s.X, (int)s.Y, 32, 32), null, color);
            if (IsMouseHovering) {
                Vector2 pos = new Vector2(Main.mouseX, Main.mouseY) + new Vector2(16f);
                Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, text, pos.X, pos.Y, new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), Color.Black, Vector2.Zero);
            }
        }

        public override void LeftClick(UIMouseEvent evt) {
            VanillaAchievements.LoadVanillaAchievements();
        }
    }

    public class JsonAchievement {
        [JsonProperty]
        public Dictionary<string, JObject> Conditions;
    }

    public static ILog Log = null;

    public static int ModFilter = 0;

    private static bool DoPatch = true;

    internal static string lastPlayer = null;

    internal static string lastWorld = null;

    internal static FieldInfo AchievementsField = typeof(AchievementManager).GetField("_achievements", BindingFlags.Instance | BindingFlags.NonPublic);

    internal static FieldInfo ConditionIsComplete = typeof(AchievementCondition).GetField("_isCompleted", BindingFlags.Instance | BindingFlags.NonPublic);

    internal static FieldInfo Conditions = typeof(Achievement).GetField("_conditions", BindingFlags.Instance | BindingFlags.NonPublic);

    private static FieldInfo UIAchievementsField = typeof(UIAchievementsMenu).GetField("_achievementsList", BindingFlags.Instance | BindingFlags.NonPublic);

    private static FieldInfo MenuElementsField = typeof(UIAchievementsMenu).GetField("_achievementElements", BindingFlags.Instance | BindingFlags.NonPublic);

    private static FieldInfo MenuButtonsField = typeof(UIAchievementsMenu).GetField("_categoryButtons", BindingFlags.Instance | BindingFlags.NonPublic);

    private static MethodInfo MenuFilterMethod = typeof(UIAchievementsMenu).GetMethod("FilterList", BindingFlags.Instance | BindingFlags.NonPublic);

    private static FieldInfo HoveredCardField = typeof(AchievementAdvisor).GetField("_hoveredCard", BindingFlags.Instance | BindingFlags.NonPublic);

    private static MethodInfo BestCardMethod = typeof(AchievementAdvisor).GetMethod("GetBestCards", BindingFlags.Instance | BindingFlags.NonPublic);

    private static MethodInfo DrawCardMethod = typeof(AchievementAdvisor).GetMethod("DrawCard", BindingFlags.Instance | BindingFlags.NonPublic);

    private static FieldInfo UIIcon = typeof(UIAchievementListItem).GetField("_achievementIcon", BindingFlags.Instance | BindingFlags.NonPublic);

    private static FieldInfo UIIconBorder = typeof(UIAchievementListItem).GetField("_achievementIconBorders", BindingFlags.Instance | BindingFlags.NonPublic);

    private static FieldInfo FrameUnlocked = typeof(UIAchievementListItem).GetField("_iconFrameUnlocked", BindingFlags.Instance | BindingFlags.NonPublic);

    private static FieldInfo FrameLocked = typeof(UIAchievementListItem).GetField("_iconFrameLocked", BindingFlags.Instance | BindingFlags.NonPublic);

    private static Asset<Texture2D> achievementsTex;

    private static Asset<Texture2D> achievementsBorderTex;

    private static Asset<Texture2D> achievementsBorderHoverFatTex;

    private static Asset<Texture2D> achievementsBorderHoverThinTex;

    internal static Asset<Texture2D> buttonTex;

    public static string Savepath => Main.SavePath + Path.DirectorySeparatorChar + "AchievementData_RoA";

    public static bool PerPlayerAchievements => true;

    public static bool PerWorldAchievements {
        get {
            return false;
        }
    }

    public override void Load() {
        Log = RoA.Instance.Logger;

        if (!ModLoader.HasMod("TMLAchievements")) {
            if (Main.netMode != 2) {
                On_AchievementManager.Save += On_AchievementManager_Save;
                On_AchievementManager.Load += On_AchievementManager_Load;
                On_AchievementManager.Load_string_bool += LoadRealPatch;
                On_AchievementManager.CreateAchievementsList += CreateListPatch;
                On_AchievementAdvisorCard.IsAchievableInWorld += PossibleInWorldPatch;
                On_UIAchievementsMenu.InitializePage += AchievementMenuInitPatch;
                On_UIAchievementsMenu.FilterList += FilterPatch;
                On_AchievementsHelper.HandleNurseService += HandleNursePatch;
                On_AchievementsHelper.HandleRunning += HandleRunningPatch;
                On_AchievementsHelper.HandleMining += HandleMiningPatch;
                On_UIAchievementsMenu.InitializePage += PagePatch;
                On_UIAchievementListItem.ctor += CreateUIAchievementListItem;
                On_UIAchievementListItem.GetTrackerValues += TrackerPatch;
                On_InGamePopups.AchievementUnlockedPopup.ctor += CreateAchievementPopup;
                On_AchievementAdvisor.DrawOneAchievement += DrawCardInventoryPatch;
                On_AchievementAdvisor.DrawCard += DrawCardPatch;
                achievementsTex = (Asset<Texture2D>)typeof(AchievementAdvisor).GetField("_achievementsTexture", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Main.AchievementAdvisor);
                achievementsBorderTex = (Asset<Texture2D>)typeof(AchievementAdvisor).GetField("_achievementsBorderTexture", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Main.AchievementAdvisor);
                achievementsBorderHoverFatTex = (Asset<Texture2D>)typeof(AchievementAdvisor).GetField("_achievementsBorderMouseHoverFatTexture", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Main.AchievementAdvisor);
                achievementsBorderHoverThinTex = (Asset<Texture2D>)typeof(AchievementAdvisor).GetField("_achievementsBorderMouseHoverThinTexture", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Main.AchievementAdvisor);
                AchievementLoader.Load();
            }
        }
    }

    private void On_AchievementManager_Load(On_AchievementManager.orig_Load orig, AchievementManager self) {
        LoadAchievementsFromFile(Savepath);
    }

    private void On_AchievementManager_Save(On_AchievementManager.orig_Save orig, AchievementManager self) {
        SaveAchievements();
        orig.Invoke(self);
    }

    public override void Unload() {
        if (Main.netMode != 2) {
            AchievementLoader.Unload();
        }
    }

    internal static void LoadLang() {
        if (!ModLoader.HasMod("TMLAchievements")) {
            ModCatagoryButton.showAll = LanguageManager.Instance.GetTextValue("Mods.TMLAchievements.UI.ShowAll");
            ModCatagoryButton.showModded = LanguageManager.Instance.GetTextValue("Mods.TMLAchievements.UI.ShowModded");
            ModCatagoryButton.showMod = LanguageManager.Instance.GetText("Mods.TMLAchievements.UI.ShowMod");
            LoadVanillaButton.text = LanguageManager.Instance.GetTextValue("Mods.TMLAchievements.UI.LoadVanilla");
        }
    }

    public static string GetCurrentSavePath() {
        string path = Savepath;
        if (PerPlayerAchievements) {
            path = path + Path.DirectorySeparatorChar + "Player";
            if (PerWorldAchievements) {
                path = path + Path.DirectorySeparatorChar + lastPlayer;
            }
        }
        else if (PerWorldAchievements) {
            path = path + Path.DirectorySeparatorChar + "World";
        }
        return path;
    }

    public static string GetCurrentFileName() {
        if (PerWorldAchievements) {
            return lastWorld;
        }
        if (PerPlayerAchievements) {
            return lastPlayer;
        }
        return "achievements";
    }

    public static void SaveAchievements() {
        SaveAchievementsToFile(GetCurrentSavePath(), GetCurrentFileName());
    }

    public static void LoadAchievements() {
        ClearAchievements();
        if ((!PerPlayerAchievements || lastPlayer != null) && (!PerWorldAchievements || lastWorld != null)) {
            LoadAchievementsFromFile(GetCurrentSavePath(), GetCurrentFileName());
        }
    }

    public static void SaveLastPlayed() {
        try {
            if (Main.ActivePlayerFileData != null && Main.ActivePlayerFileData.Path != null) {
                lastPlayer = Main.ActivePlayerFileData.GetFileName(includeExtension: false);
            }
            if (Main.ActiveWorldFileData != null && Main.ActiveWorldFileData.Path != null) {
                lastWorld = Main.ActiveWorldFileData.GetFileName(includeExtension: false);
            }
            Directory.CreateDirectory(Savepath);
            using StreamWriter f = File.CreateText(Savepath + Path.DirectorySeparatorChar + "lastPlayed.dat");
            if (lastPlayer != null) {
                f.WriteLine(lastPlayer);
            }
            else {
                f.WriteLine();
            }
            if (lastWorld != null) {
                f.Write(lastWorld);
            }
        }
        catch {
            Log.Error("An error occured while saving the last played info");
        }
    }

    public static void LoadLastPlayed() {
        string path = Savepath + Path.DirectorySeparatorChar + "lastPlayed.dat";
        if (!File.Exists(path)) {
            return;
        }
        try {
            string player;
            string world;
            using (StreamReader r = new StreamReader(path)) {
                player = r.ReadLine();
                world = r.ReadLine();
            }
            if (player != null && player != "") {
                lastPlayer = player;
            }
            if (world != null && world != "") {
                lastWorld = world;
            }
        }
        catch {
        }
    }

    public static void OnAchievementCompleted(Achievement achievement) {
        Main.NewText(Language.GetTextValue("Achievements.Completed", Language.GetTextValue(achievement.FriendlyName.Key)));
        if (SoundEngine.FindActiveSound(in SoundID.AchievementComplete) == null) {
            SoundEngine.PlaySound(in SoundID.AchievementComplete);
        }
    }

    public static void SaveAchievementsToFile(string path, string name = "achievements") {
        Directory.CreateDirectory(path);
        string file = path + Path.DirectorySeparatorChar + name + ".json";
        AchievementManager self = Main.Achievements;
        try {
            Dictionary<string, JsonAchievement> old = null;
            try {
                if (File.Exists(file)) {
                    using StreamReader r = new StreamReader(file);
                    old = (Dictionary<string, JsonAchievement>)JsonSerializer.Create((JsonSerializerSettings)typeof(AchievementManager).GetField("_serializerSettings", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self)).Deserialize(r, typeof(Dictionary<string, JsonAchievement>));
                }
            }
            catch {
            }
            Dictionary<string, Achievement> obj2 = (Dictionary<string, Achievement>)AchievementsField.GetValue(self);
            JObject json = new JObject();
            foreach (KeyValuePair<string, Achievement> item2 in obj2) {
                JObject conditions = new JObject();
                foreach (KeyValuePair<string, AchievementCondition> item3 in (Dictionary<string, AchievementCondition>)Conditions.GetValue(item2.Value)) {
                    JObject o = (JObject)JToken.FromObject(item3.Value);
                    if (item3.Value is TMLCondition tml) {
                        tml.Save(o);
                    }
                    conditions.Add(new JProperty(item3.Key, o));
                }
                old?.Remove(item2.Key);
                json.Add(new JProperty(item2.Value.Name, new JObject(new JProperty("Conditions", conditions))));
            }
            if (old != null) {
                foreach (KeyValuePair<string, JsonAchievement> item in old) {
                    json.Add(new JProperty(item.Key, JToken.FromObject(item.Value)));
                }
            }
            using StreamWriter f = File.CreateText(file);
            f.Write(json.ToString());
        }
        catch {
            Log.Error("An error occurred while saving achievement data.");
        }
    }

    public static void LoadAchievementsFromFile(string path, string name = "achievements") {
        string file = path + Path.DirectorySeparatorChar + name + ".json";
        if (!File.Exists(file)) {
            return;
        }
        AchievementManager self = Main.Achievements;
        try {
            object s = typeof(AchievementManager).GetField("_serializerSettings", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self);
            Dictionary<string, Achievement> achievements = (Dictionary<string, Achievement>)AchievementsField.GetValue(self);
            Dictionary<string, JsonAchievement> a = null;
            using (StreamReader r = new StreamReader(file)) {
                a = (Dictionary<string, JsonAchievement>)JsonSerializer.Create((JsonSerializerSettings)s).Deserialize(r, typeof(Dictionary<string, JsonAchievement>));
            }
            if (a == null) {
                throw new Exception();
            }
            foreach (KeyValuePair<string, JsonAchievement> item in a) {
                if (achievements.ContainsKey(item.Key)) {
                    achievements[item.Key].Load(item.Value.Conditions);
                }
            }
        }
        catch {
            Log.Error("An error occurred while loading achievement data.");
            ClearAchievements();
        }
    }

    public static void ClearAchievements() {
        foreach (KeyValuePair<string, Achievement> item in (Dictionary<string, Achievement>)AchievementsField.GetValue(Main.Achievements)) {
            item.Value.ClearProgress();
        }
    }

    public static bool FilterActive(Mod mod) {
        if (ModFilter != 1) {
            return AchievementLoader.ModsThatAddAchievements[ModFilter - 2] == mod;
        }
        return true;
    }

    private void PagePatch(On_UIAchievementsMenu.orig_InitializePage orig, UIAchievementsMenu self) {
        orig.Invoke(self);
    }

    private List<Achievement> CreateListPatch(On_AchievementManager.orig_CreateAchievementsList orig, AchievementManager self) {
        /*if (Config.EnableVanillaAchievements) */{
            return orig.Invoke(self);
        }
        List<Achievement> i = new List<Achievement>();
        foreach (ModAchievement a in AchievementLoader.modAchievements) {
            i.Add(a.achievement);
        }
        return i;
    }

    internal static void DoConditionPatch() {
        if (!DoPatch) {
            return;
        }
        DoPatch = false;
        try {
            On_AchievementCondition.Complete += ModContent.GetInstance<TMLAchievements>().ConditionCompletePatch;
        }
        catch {
            Log.Error("Could not apply AchievementCondition Complete patch");
        }
    }

    private void ConditionCompletePatch(On_AchievementCondition.orig_Complete orig, AchievementCondition self) {
        if (!self.IsCompleted) {
            orig.Invoke(self);
        }
    }

    public enum RoAAchivement {
        BestialCommunion,
        WhatsThatSmell,
        OpenRootboundChest,
        SilentHills,
        NotPostMortem,
        GutsOfSteel
    }

    public static void CompleteEventAchievement(RoAAchivement roaAchivement) {
        if (Main.netMode != 2) {
            if (ModLoader.TryGetMod("TMLAchievements", out Mod mod)) {
                mod.Call("Event", roaAchivement.ToString());

                return;
            }

            AchievementLoader.OnEvent(roaAchivement.ToString());
        }
    }

    private bool PossibleInWorldPatch(On_AchievementAdvisorCard.orig_IsAchievableInWorld orig, AchievementAdvisorCard self) {
        //if (!Config.EnableVanillaAchievements && !(self is TMLAchievementCard)) {
        //    return false;
        //}
        return orig.Invoke(self);
    }

    private void AchievementMenuInitPatch(On_UIAchievementsMenu.orig_InitializePage orig, UIAchievementsMenu self) {
        orig.Invoke(self);
        //if (AchievementLoader.ModsThatAddAchievements.Count == 0) {
        //    return;
        //}
        //try {
        //    UIElement e = (UIList)UIAchievementsField.GetValue(self);
        //    if (e == null || e.Parent == null) {
        //        return;
        //    }
        //    e = e.Parent;
        //    foreach (UIElement i in e.Children) {
        //        if (i.Top.Pixels == 10f && i.Top.Percent == 0f) {
        //            UIElement button = new ModCatagoryButton();
        //            button.Left.Set(152f, 0f);
        //            UIElement.MouseEvent j = (UIElement.MouseEvent)MenuFilterMethod.CreateDelegate(typeof(UIElement.MouseEvent), self);
        //            button.OnLeftClick += j;
        //            button.OnRightClick += j;
        //            i.Append(button);
        //            break;
        //        }
        //    }
        //    if (ModFilter != 0) {
        //        MenuFilterMethod.Invoke(self, new object[2]);
        //    }
        //}
        //catch {
        //}
    }

    private void FilterPatch(On_UIAchievementsMenu.orig_FilterList orig, UIAchievementsMenu self, UIMouseEvent evt, UIElement listeningElement) {
        if (ModFilter == 0) {
            orig.Invoke(self, evt, listeningElement);
            return;
        }
        SoundEngine.PlaySound(in SoundID.MenuTick);
        UIList list = (UIList)UIAchievementsField.GetValue(self);
        list.Clear();
        List<UIToggleImage> buttons = (List<UIToggleImage>)MenuButtonsField.GetValue(self);
        foreach (UIAchievementListItem achievementElement in (List<UIAchievementListItem>)MenuElementsField.GetValue(self)) {
            Achievement a = achievementElement.GetAchievement();
            if (buttons[(int)a.Category].IsOn && a is TMLAchievement t && FilterActive(t.ModAchievement.Mod)) {
                list.Add(achievementElement);
            }
        }
        self.Recalculate();
    }

    private void HandleNursePatch(On_AchievementsHelper.orig_HandleNurseService orig, int coinsSpent) {
        /*if (Config.EnableVanillaAchievements)*/ {
            orig.Invoke(coinsSpent);
        }
    }

    private void HandleRunningPatch(On_AchievementsHelper.orig_HandleRunning orig, float pixelsMoved) {
        /*if (Config.EnableVanillaAchievements) */{
            orig.Invoke(pixelsMoved);
        }
    }

    private void HandleMiningPatch(On_AchievementsHelper.orig_HandleMining orig) {
        /*if (Config.EnableVanillaAchievements)*/ {
            orig.Invoke();
        }
    }

    private void CreateUIAchievementListItem(On_UIAchievementListItem.orig_ctor orig, UIAchievementListItem self, Achievement achievement, bool largeForOtherLanguages) {
        orig.Invoke(self, achievement, largeForOtherLanguages);
        if (achievement is TMLAchievement a) {
            FrameUnlocked.SetValue(self, a.ModAchievement.GetFrame());
            FrameLocked.SetValue(self, a.ModAchievement.GetFrame(locked: true));
            ((UIImageFramed)UIIcon.GetValue(self)).SetImage(a.texture, a.ModAchievement.GetFrame());
            if (a.borderTex != null) {
                ((UIImage)UIIconBorder.GetValue(self)).SetImage(a.borderTex);
            }
        }
    }

    private Tuple<decimal, decimal> TrackerPatch(On_UIAchievementListItem.orig_GetTrackerValues orig, UIAchievementListItem self) {
        if (self.GetAchievement() is TMLAchievement a) {
            return a.ModAchievement.GetProgressBar();
        }
        return orig.Invoke(self);
    }

    private void CreateAchievementPopup(On_InGamePopups.AchievementUnlockedPopup.orig_ctor orig, InGamePopups.AchievementUnlockedPopup self, Achievement achievement) {
        orig.Invoke(self, achievement);
        if (achievement is TMLAchievement a) {
            typeof(InGamePopups.AchievementUnlockedPopup).GetField("_achievementTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(self, a.texture);
            typeof(InGamePopups.AchievementUnlockedPopup).GetField("_achievementIconFrame", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(self, a.ModAchievement.GetFrame());
            if (a.borderTex != null) {
                typeof(InGamePopups.AchievementUnlockedPopup).GetField("_achievementBorderTexture", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(self, a.borderTex);
            }
        }
    }

    private void DrawCardInventoryPatch(On_AchievementAdvisor.orig_DrawOneAchievement orig, AchievementAdvisor self, SpriteBatch spriteBatch, Vector2 position, bool large) {
        if (false) {
            orig.Invoke(self, spriteBatch, position, large);
            /*if (Config.ShowAchievementCard)*/ {
                DrawAchievementCard(self, spriteBatch, position + new Vector2(65f, -2f), large);
            }
        }
        else /*if (Config.ShowAchievementCard) */{
            orig.Invoke(self, spriteBatch, position, large);
        }
    }

    private void DrawAchievementCard(AchievementAdvisor self, SpriteBatch spriteBatch, Vector2 position, bool large) {
        List<AchievementAdvisorCard> bestCards = (List<AchievementAdvisorCard>)BestCardMethod.Invoke(self, new object[1] { 1 });
        if (bestCards.Count < 1) {
            return;
        }
        AchievementAdvisorCard hoveredCard = bestCards[0];
        float num = 0.35f;
        if (large) {
            num = 0.75f;
        }
        HoveredCardField.SetValue(self, null);
        object[] param = new object[5]
        {
            bestCards[0],
            spriteBatch,
            position + new Vector2(8f) * num,
            num,
            null
        };
        DrawCardMethod.Invoke(self, param);
        if (!(bool)param[4]) {
            return;
        }
        HoveredCardField.SetValue(self, hoveredCard);
        if (!PlayerInput.IgnoreMouseInterface) {
            Main.player[Main.myPlayer].mouseInterface = true;
            if (Main.mouseLeft && Main.mouseLeftRelease) {
                Main.ingameOptionsWindow = false;
                IngameFancyUI.OpenAchievementsAndGoto(hoveredCard.achievement);
            }
        }
    }

    private void DrawCardPatch(On_AchievementAdvisor.orig_DrawCard orig, AchievementAdvisor self, AchievementAdvisorCard card, SpriteBatch spriteBatch, Vector2 position, float scale, out bool hovered) {
        hovered = false;
        if (Main.MouseScreen.Between(position, position + card.frame.Size() * scale)) {
            Main.LocalPlayer.mouseInterface = true;
            hovered = true;
        }
        Color color = Color.White;
        if (!hovered) {
            color = new Color(220, 220, 220, 220);
        }
        Vector2 value = new Vector2(-4f) * scale;
        Vector2 value2 = new Vector2(-8f) * scale;
        Texture2D hoverTex = achievementsBorderHoverFatTex.Value;
        if (scale > 0.5f) {
            hoverTex = achievementsBorderHoverThinTex.Value;
            value2 = new Vector2(-5f) * scale;
        }
        Rectangle frame = card.frame;
        Texture2D borderTex = achievementsBorderTex.Value;
        Texture2D tex;
        if (card.achievement is TMLAchievement a) {
            tex = a.texture.Value;
            if (a.borderTex != null) {
                borderTex = a.borderTex.Value;
            }
        }
        else {
            tex = achievementsTex.Value;
            frame.X += 528;
        }
        spriteBatch.Draw(tex, position, (Rectangle?)frame, color, 0f, Vector2.Zero, scale, 0, 0f);
        spriteBatch.Draw(borderTex, position + value, null, color, 0f, Vector2.Zero, scale, 0, 0f);
        if (hovered) {
            spriteBatch.Draw(hoverTex, position + value2, null, Main.OurFavoriteColor, 0f, Vector2.Zero, scale, 0, 0f);
        }
    }

    private void LoadRealPatch(On_AchievementManager.orig_Load_string_bool orig, AchievementManager self, string path, bool cloud) {
    }
}
