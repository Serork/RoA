using Microsoft.Xna.Framework;

using RoA.Common.Configs;
using RoA.Common.Druid.Wreath;
using RoA.Common.InterfaceElements;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Concurrent;
using System.Reflection;

using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

using tModPorter;

namespace RoA.Common.Text;

sealed class KeywordSystem : ILoadable {
    internal static float _keywordColorOpacity = 1f, _keywordColorOpacity2;

    public class KeywordTagHandler : ITagHandler {
        TextSnippet ITagHandler.Parse(string text, Color baseColor, string options) {
            if (options is not null) {
                string[] array = options.Split(',');
                for (int i = 0; i < array.Length; i++) {
                    if (array[i].Length == 0) {
                        continue;
                    }
                    switch (array[i][0]) {
                        // nature highlight
                        case 'n':
                            float opacity = Ease.CubeOut(Helper.EaseInOut3(_keywordColorOpacity));
                            baseColor = Color.Lerp(Main.MouseTextColorReal, WreathHandler.GetCurrentColor(Main.LocalPlayer), opacity);
                            break;
                    }
                }
            }

            return new(text, baseColor);
        }
    }

    public void Load(Mod mod) {
        On_Main.DrawInterface_36_Cursor += On_Main_DrawInterface_36_Cursor;
        ChatManager.Register<KeywordTagHandler>(["kw", "keyword"]);
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
                else if ((!Main.HoverItem.IsEmpty() && Main.HoverItem.IsDruidic()) || flag3 || flag || FancyWreathDrawing.IsHoveringUI || WreathDrawing.JustDrawn) {
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
