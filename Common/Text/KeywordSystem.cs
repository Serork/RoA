using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Configs;
using RoA.Common.Druid.Wreath;
using RoA.Common.InterfaceElements;
using RoA.Core;
using RoA.Core.Utility;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

using KeywordInfo = (byte, string);

namespace RoA.Common.Text;

sealed class KeywordSystem : ILoadable {
    internal static float _keywordColorOpacity = 1f, _keywordColorOpacity2;

    private static bool _overResourceBar;

    private sealed class KeywordSystemForVanillaTooltips : GlobalItem {
        private readonly List<KeywordInfo> _keywords_EN = [(0, "mana"), (1, "life"), (1, "health"), (2, "wreath")];
        private readonly List<KeywordInfo> _keywords_RU = [
            (0, "мана"),  (0, "маны"),  (0, "мане"), (0, "ману"), (0, "маной"), (0, "маною"), (0, "мане"),
            (1, "жизнь"), (1, "жизнью"), (1, "жизни"), (1, "жизней"), (1, "жизням"), (1, "жизнями"), (1, "жизнях"),
            (1, "здоровье"), (1, "здоровья"), (1, "здоровью"), (1, "здоровьем"),
            (2, "венок"), (2, "венка"), (2, "венку"), (2, "венком"), (2, "венке"),
        ];

        public override bool InstancePerEntity => true;

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            List<KeywordInfo> checkArray = _keywords_EN;
            if (GameCulture.FromCultureName(GameCulture.CultureName.Russian).IsActive) {
                checkArray = _keywords_RU;
            }
            foreach (TooltipLine tooltip in tooltips) {
                if (tooltip.Name == "ItemName") {
                    continue;
                }
                char[] checks = ['m', 'l', 'n'];
                for (int i = 0; i < checkArray.Count; i++) { 
                    char tag = checks[checkArray[i].Item1];
                    string keyword = checkArray[i].Item2;
                    tooltip.Text = Regex.Replace(tooltip.Text, $@"\b{keyword}\b", $"[kw/{tag}:" + keyword + "]");
                    keyword = checkArray[i].Item2.ToUpper();
                    tooltip.Text = Regex.Replace(tooltip.Text, $@"\b{keyword}\b", $"[kw/{tag}:" + keyword + "]");
                }
                foreach (char check in checks) {
                    string pattern = $"[kw/{check}:[kw/{check}:";
                    tooltip.Text = tooltip.Text.Replace(pattern, $"[kw/{check}:");
                    pattern = "]]";
                    tooltip.Text = tooltip.Text.Replace(pattern, "]");
                }
            }
        }
    }

    public class KeywordTagHandler : ITagHandler {
        TextSnippet ITagHandler.Parse(string text, Color baseColor, string options) {
            if (options is not null) {
                string[] array = options.Split(',');
                for (int i = 0; i < array.Length; i++) {
                    if (array[i].Length == 0) {
                        continue;
                    }
                    switch (array[i][0]) {
                        case 'n':
                            float opacity = Ease.CubeOut(Helper.EaseInOut3(_keywordColorOpacity));
                            baseColor = Color.Lerp(Main.MouseTextColorReal, WreathHandler.GetCurrentColor(Main.LocalPlayer), opacity);
                            break;
                        case 'm':
                            opacity = Ease.CubeOut(Helper.EaseInOut3(_keywordColorOpacity));
                            baseColor = Color.Lerp(Main.MouseTextColorReal, new Color(134, 199, 252), opacity);
                            break;
                        case 'l':
                            opacity = Ease.CubeOut(Helper.EaseInOut3(_keywordColorOpacity));
                            baseColor = Color.Lerp(Main.MouseTextColorReal, new Color(252, 145, 134), opacity);
                            break;
                    }
                }
            }

            return new(text, baseColor);
        }
    }

    public void Load(Mod mod) {
        On_CommonResourceBarMethods.DrawLifeMouseOver += On_CommonResourceBarMethods_DrawLifeMouseOver;
        On_CommonResourceBarMethods.DrawManaMouseOver += On_CommonResourceBarMethods_DrawManaMouseOver;

        On_Main.DrawInterface_36_Cursor += On_Main_DrawInterface_36_Cursor;
        ChatManager.Register<KeywordTagHandler>(["kw", "keyword"]);
    }

    private void On_CommonResourceBarMethods_DrawLifeMouseOver(On_CommonResourceBarMethods.orig_DrawLifeMouseOver orig) {
        if (!Main.mouseText) {
            Player localPlayer = Main.LocalPlayer;
            localPlayer.cursorItemIconEnabled = false;
            string text = "[kw/l:" + localPlayer.statLife + "]" + "/" + localPlayer.statLifeMax2;
            Main.instance.MouseTextHackZoom(text);
            Main.mouseText = true;

            _overResourceBar = true;
        }
    }

    private void On_CommonResourceBarMethods_DrawManaMouseOver(On_CommonResourceBarMethods.orig_DrawManaMouseOver orig) {
        if (!Main.mouseText) {
            Player localPlayer = Main.LocalPlayer;
            localPlayer.cursorItemIconEnabled = false;
            string text = "[kw/m:" + localPlayer.statMana + "]" + "/" + localPlayer.statManaMax2;
            Main.instance.MouseTextHackZoom(text);
            _overResourceBar = true;
            Main.mouseText = true;
        }
    }

    internal static void UpdateLogic(bool flag3 = false) {
        var highlightMode = Main.gameMenu || Main.InGameUI.IsVisible ? DamageTooltipOptionConfigElement3.modifying : ModContent.GetInstance<RoAClientConfig>().HighlightMode;
        bool flag2 = highlightMode != RoAClientConfig.HighlightModes.Off;
        if (flag2) {
            if (!Main.gameMenu || flag3) {
                bool flag = false;
                int num2 = 11;
                for (int i = 0; i < Player.MaxBuffs; i++) {
                    if (Main.player[Main.myPlayer].buffType[i] > 0) {
                        _ = Main.player[Main.myPlayer].buffType[i];
                        int x = 32 + i * 38;
                        int num3 = 76;
                        int num4 = i;
                        while (num4 >= num2) {
                            num4 -= num2;
                            x = 32 + num4 * 38;
                            num3 += 50;
                        }
                        int num = Main.player[Main.myPlayer].buffType[i];
                        if (Main.mouseX < x + TextureAssets.Buff[num].Width() && Main.mouseY < num3 + TextureAssets.Buff[num].Height() && Main.mouseX > x && Main.mouseY > num3) {
                            flag = true;
                        }
                    }
                }
                bool flag4 = highlightMode == RoAClientConfig.HighlightModes.Always;
                if (flag4) {
                    _keywordColorOpacity = 1f;
                }
                else if ((!Main.HoverItem.IsEmpty()/* && Main.HoverItem.IsDruidic()*/) || flag3 || flag || FancyWreathDrawing.IsHoveringUI || WreathDrawing.JustDrawn || _overResourceBar) {
                    if (_keywordColorOpacity > 0f) {
                        _keywordColorOpacity -= TimeSystem.LogicDeltaTime * 0.5f;
                    }
                }
                else if (_keywordColorOpacity != 1f) {
                    _keywordColorOpacity = 1f;
                }
                if (!flag4) {
                    if (flag3) {
                        if (_keywordColorOpacity2 <= 0f) {
                            _keywordColorOpacity2 = 180f;
                            _keywordColorOpacity = 1f;
                        }
                        else {
                            _keywordColorOpacity2--;
                        }
                    }
                }
                else {
                    _keywordColorOpacity2 = 0f;
                }
            }
        }
        else {
            _keywordColorOpacity = 0f;
        }

        if (!Main.mouseText && _overResourceBar) {
            _overResourceBar = false;
        }
    }

    private void On_Main_DrawInterface_36_Cursor(On_Main.orig_DrawInterface_36_Cursor orig) {
        UpdateLogic();

        orig();
    }

    public void Unload() {
        var handlerDict = (ConcurrentDictionary<string, ITagHandler>)typeof(ChatManager).GetField("_handlers", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        handlerDict.TryRemove("keyword", out _);
    }
}
