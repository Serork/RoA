using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Text;
using RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace RoA.Common.Configs;

sealed class BooleanElement2 : ConfigElement<bool> {
    private Asset<Texture2D> _toggleTexture;

    internal static bool Value2;

    // TODO. Display status string? (right now only on/off texture, but True/False, Yes/No, Enabled/Disabled options)
    public override void OnBind() {
        base.OnBind();
        _toggleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");

        OnLeftClick += (ev, v) => Value2 = !Value2;

        Value2 = Value;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);

        if (Value != Value2) {
            Value = Value2;
        }

        CalculatedStyle dimensions = base.GetDimensions();
        // "Yes" and "No" since no "True" and "False" translation available
        Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, Value2 ? Lang.menu[126].Value : Lang.menu[124].Value, new Vector2(dimensions.X + dimensions.Width - 60, dimensions.Y + 8f), Color.White, 0f, Vector2.Zero, new Vector2(0.8f));
        Rectangle sourceRectangle = new Rectangle(Value2 ? ((_toggleTexture.Width() - 2) / 2 + 2) : 0, 0, (_toggleTexture.Width() - 2) / 2, _toggleTexture.Height());
        Vector2 drawPosition = new Vector2(dimensions.X + dimensions.Width - sourceRectangle.Width - 10f, dimensions.Y + 8f);
        spriteBatch.Draw(_toggleTexture.Value, drawPosition, sourceRectangle, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
    }
}

sealed class BooleanElement : ConfigElement<bool> {
    private Asset<Texture2D> _toggleTexture;

    internal static bool Value2;

    // TODO. Display status string? (right now only on/off texture, but True/False, Yes/No, Enabled/Disabled options)
    public override void OnBind() {
        base.OnBind();
        _toggleTexture = Main.Assets.Request<Texture2D>("Images/UI/Settings_Toggle");

        OnLeftClick += (ev, v) => Value2 = !Value2;

        Value2 = Value;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);

        if (Value != Value2) {
            Value = Value2;
        }

        CalculatedStyle dimensions = base.GetDimensions();
        // "Yes" and "No" since no "True" and "False" translation available
        Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, Value2 ? Lang.menu[126].Value : Lang.menu[124].Value, new Vector2(dimensions.X + dimensions.Width - 60, dimensions.Y + 8f), Color.White, 0f, Vector2.Zero, new Vector2(0.8f));
        Rectangle sourceRectangle = new Rectangle(Value2 ? ((_toggleTexture.Width() - 2) / 2 + 2) : 0, 0, (_toggleTexture.Width() - 2) / 2, _toggleTexture.Height());
        Vector2 drawPosition = new Vector2(dimensions.X + dimensions.Width - sourceRectangle.Width - 10f, dimensions.Y + 8f);
        spriteBatch.Draw(_toggleTexture.Value, drawPosition, sourceRectangle, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
    }
}


sealed class DamageTooltipOptionConfigElement : ConfigElement {
    private static int _lastHeight = 150;

    private int max;
    private string[] valueStrings;
    private Func<object> _getValue;
    private Func<object> _getValueString;
    private Func<int> _getIndex;
    private Action<int> _setValue;
    private Item _hoverItem;

    private EnumElement _lock;

    protected Color SliderColor { get; set; } = Color.White;
    protected Utils.ColorLerpMethod ColorMethod { get; set; }

    protected float Proportion {
        get => _getIndex() / (float)(max - 1);
        set => _setValue((int)(Math.Round(value * (max - 1))));
    }

    //private ref RoAClientConfig.DamageTooltipOptions modifying => ref ModContent.GetInstance<RoAClientConfig>().DamageTooltipOption;
    private ref RoAClientConfig.DamageTooltipOptions modifying => ref ModContent.GetInstance<RoAClientConfig>().DamageTooltipOption;

    public DamageTooltipOptionConfigElement() {
        Width.Set(0, 1f);
        float ratio = Main.screenHeight / (float)Main.screenWidth;
        Height.Set(145 + 20 + 8, 0f);

        valueStrings = Enum.GetNames(typeof(RoAClientConfig.DamageTooltipOptions));
        for (int i = 0; i < valueStrings.Length; i++) {
            var enumFieldFieldInfo = modifying;
            string name = Language.GetTextValue($"Mods.RoA.Configs.DamageTooltipOptions.Option{i + 1}.Label");
            valueStrings[i] = name;
        }

        max = valueStrings.Length;

        ColorMethod = new Utils.ColorLerpMethod((percent) => Color.Lerp(Color.Black, SliderColor, percent));

        _getValue = () => DefaultGetValue();
        _getValueString = () => DefaultGetStringValue();
        _getIndex = () => DefaultGetIndex();
        _setValue = (int value) => DefaultSetValue(value);

        _lock = new EnumElement();

        Recalculate();
    }

    private void DefaultSetValue(int index) {
        if (!MemberInfo.CanWrite)
            return;

        modifying = (RoAClientConfig.DamageTooltipOptions)Enum.GetValues(typeof(RoAClientConfig.DamageTooltipOptions)).GetValue(index);
        SetObject(modifying);
    }

    private object DefaultGetValue() {
        return GetObject();
    }

    private int DefaultGetIndex() {
        return Array.IndexOf(Enum.GetValues(typeof(RoAClientConfig.DamageTooltipOptions)), _getValue());
    }

    private string DefaultGetStringValue() {
        int index = _getIndex();
        if (index < 0) // User manually entered invalid enum number into json or loading future Enum value saved as int.
            return Language.GetTextValue("tModLoader.ModConfigUnknownEnum");
        return valueStrings[index];
    }

    public float DrawValueBar(SpriteBatch sb, float scale, float perc, int lockState = 0, Utils.ColorLerpMethod colorMethod = null) {
        perc = Utils.Clamp(perc, -.05f, 1.05f);

        if (colorMethod == null)
            colorMethod = new Utils.ColorLerpMethod(Utils.ColorLerp_BlackToWhite);

        Texture2D colorBarTexture = TextureAssets.ColorBar.Value;
        Vector2 vector = new Vector2((float)colorBarTexture.Width, (float)colorBarTexture.Height) * scale;
        IngameOptions.valuePosition.X -= (float)((int)vector.X);
        Rectangle rectangle = new Rectangle((int)IngameOptions.valuePosition.X, (int)IngameOptions.valuePosition.Y - (int)vector.Y / 2, (int)vector.X, (int)vector.Y);
        Rectangle destinationRectangle = rectangle;
        int num = 167;
        float num2 = rectangle.X + 5f * scale;
        float num3 = rectangle.Y + 4f * scale;

        if (true) {
            int numTicks = valueStrings.Length;
            if (numTicks > 1) {
                for (int tick = 0; tick < numTicks; tick++) {
                    float percent = tick * 1f / (valueStrings.Length - 1);

                    if (percent <= 1f)
                        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(num2 + num * percent * scale), rectangle.Y - 2, 2, rectangle.Height + 4), Color.White);
                }
            }
        }

        sb.Draw(colorBarTexture, rectangle, Color.White);

        for (float num4 = 0f; num4 < (float)num; num4 += 1f) {
            float percent = num4 / (float)num;
            sb.Draw(TextureAssets.ColorBlip.Value, new Vector2(num2 + num4 * scale, num3), null, colorMethod(percent), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        rectangle.Inflate((int)(-5f * scale), 2);

        //rectangle.X = (int)num2;
        //rectangle.Y = (int)num3;

        bool flag = rectangle.Contains(new Point(Main.mouseX, Main.mouseY));

        if (lockState == 2) {
            flag = false;
        }

        if (flag || lockState == 1) {
            sb.Draw(TextureAssets.ColorHighlight.Value, destinationRectangle, Main.OurFavoriteColor);
        }

        var colorSlider = TextureAssets.ColorSlider.Value;

        sb.Draw(colorSlider, new Vector2(num2 + 167f * scale * perc, num3 + 4f * scale), null, Color.White, 0f, colorSlider.Size() * 0.5f, scale, SpriteEffects.None, 0f);

        if (Main.mouseX >= rectangle.X && Main.mouseX <= rectangle.X + rectangle.Width) {
            IngameOptions.inBar = flag;
            return (Main.mouseX - rectangle.X) / (float)rectangle.Width;
        }

        IngameOptions.inBar = false;

        if (rectangle.X >= Main.mouseX) {
            return 0f;
        }

        return 1f;
    }

    public int UpdateCount { get; private set; }

    public static void MouseText_DrawItemTooltip_GetLinesInfo(Item item, ref int yoyoLogo, ref int researchLine, float oldKB, ref int numLines, string[] toolTipLine, bool[] preFixLine, bool[] badPreFixLine, string[] toolTipNames, out int prefixlineIndex) {
        prefixlineIndex = -1;
        toolTipLine[0] = item.HoverName;
        toolTipNames[0] = "ItemName";

        int damage = 10;
        toolTipLine[numLines] = damage + item.DamageType.DisplayName.Value;
        toolTipNames[numLines] = "Damage_Druid";

        numLines++;
    }

    public static List<TooltipLine> ModifyTooltips(Item item, ref int numTooltips, string[] names, ref string[] text, ref bool[] modifier, ref bool[] badModifier, ref int oneDropLogo, out Color?[] overrideColor, int prefixlineIndex) {
        var tooltips = new List<TooltipLine>();

        for (int k = 0; k < numTooltips; k++) {
            TooltipLine tooltip = new TooltipLine(RoA.Instance, names[k], text[k]);
            tooltip.IsModifier = modifier[k];
            tooltip.IsModifierBad = badModifier[k];

            //if (k == oneDropLogo) {
            //    tooltip.OneDropLogo = true;
            //}

            tooltips.Add(tooltip);
        }

        if (item.prefix >= PrefixID.Count && prefixlineIndex != -1) {
            var tooltipLines = PrefixLoader.GetPrefix(item.prefix)?.GetTooltipLines(item);
            if (tooltipLines != null) {
                foreach (var line in tooltipLines) {
                    tooltips.Insert(prefixlineIndex, line);
                    prefixlineIndex++;
                }
            }
        }

        item.ModItem?.ModifyTooltips(tooltips);

        var HookModifyTooltips = typeof(ItemLoader).GetField("HookModifyTooltips",
                            BindingFlags.Static |
                            BindingFlags.NonPublic);

        if (!item.IsAir) { // Prevents dummy items used in Main.HoverItem from getting unrelated tooltips
            foreach (var g in (HookModifyTooltips.GetValue(null) as Terraria.ModLoader.Core.GlobalHookList<Terraria.ModLoader.GlobalItem>).Enumerate(item)) {
                if (g.Mod != RoA.Instance) {
                    continue;
                }
                g.ModifyTooltips(item, tooltips);
            }
        }

        tooltips.RemoveAll(x => !x.Visible);

        numTooltips = tooltips.Count;
        text = new string[numTooltips];
        modifier = new bool[numTooltips];
        badModifier = new bool[numTooltips];
        oneDropLogo = -1;
        overrideColor = new Color?[numTooltips];

        for (int k = 0; k < numTooltips; k++) {
            text[k] = tooltips[k].Text;
            modifier[k] = tooltips[k].IsModifier;
            badModifier[k] = tooltips[k].IsModifierBad;

            //if (tooltips[k].OneDropLogo) {
            //    oneDropLogo = k;
            //}

            overrideColor[k] = tooltips[k].OverrideColor;
        }

        return tooltips;
    }

    private void MouseText_DrawItemTooltip(int rare, byte diff, int X, int Y, out int numLines2) {
        bool settingsEnabled_OpaqueBoxBehindTooltips = Main.SettingsEnabled_OpaqueBoxBehindTooltips;
        ref byte mouseTextColor = ref Main.mouseTextColor;
        Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color(mouseTextColor, mouseTextColor, mouseTextColor, mouseTextColor);
        Item hoverItem = _hoverItem;
        int yoyoLogo = -1;
        int researchLine = -1;
        rare = hoverItem.rare;
        float knockBack = hoverItem.knockBack;
        float num = 1f;
        if (hoverItem.DamageType == DamageClass.Melee && Main.player[Main.myPlayer].kbGlove)
            num += 1f;

        if (Main.player[Main.myPlayer].kbBuff)
            num += 0.5f;

        if (num != 1f)
            hoverItem.knockBack *= num;

        if (hoverItem.DamageType == DamageClass.Ranged && Main.player[Main.myPlayer].shroomiteStealth)
            hoverItem.knockBack *= 1f + (1f - Main.player[Main.myPlayer].stealth) * 0.5f;

        int num2 = 30;
        int numLines = 1;
        string[] array = new string[num2];
        bool[] array2 = new bool[num2];
        bool[] array3 = new bool[num2];
        for (int i = 0; i < num2; i++) {
            array2[i] = false;
            array3[i] = false;
        }

        // This array will be filled with internal names assigned to vanilla tooltips.
        string[] tooltipNames = new string[num2];

        MouseText_DrawItemTooltip_GetLinesInfo(hoverItem, ref yoyoLogo, ref researchLine, knockBack, ref numLines, array, array2, array3, tooltipNames, out int prefixlineIndex);
        float num3 = (float)(int)mouseTextColor / 255f;
        float num4 = num3;
        int a = mouseTextColor;

        Vector2 zero = Vector2.Zero;

        // TML's abstractions over tooltip arrays.
        try {
            List<TooltipLine> lines = ModifyTooltips(_hoverItem, ref numLines, tooltipNames, ref array, ref array2, ref array3, ref yoyoLogo, out Color?[] overrideColor, prefixlineIndex);
            List<DrawableTooltipLine> drawableLines = lines.Select((TooltipLine x, int i) => new DrawableTooltipLine(x, i, 0, 0, Color.White)).ToList();
            int num0 = -(!Main.gameMenu ? 1 : 0);
            numLines2 = numLines + num0;
            int num12 = 0;
            for (int j = 0; j < numLines + num0; j++) {
                Vector2 stringSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, array[j], Vector2.One);
                if (stringSize.X > zero.X)
                    zero.X = stringSize.X;

                zero.Y += stringSize.Y + (float)num12;
            }

            if (yoyoLogo != -1)
                zero.Y += 24f;

            X += 6;
            Y += 6;
            int num13 = 4;
            if (settingsEnabled_OpaqueBoxBehindTooltips) {
                X += 8;
                Y += 2;
                num13 = 18;
            }

            int num14 = Main.screenWidth;
            int num15 = Main.screenHeight;
            if ((float)X + zero.X + (float)num13 > (float)num14)
                X = (int)((float)num14 - zero.X - (float)num13);

            if ((float)Y + zero.Y + (float)num13 > (float)num15)
                Y = (int)((float)num15 - zero.Y - (float)num13);

            int num16 = 0;
            num3 = (float)(int)mouseTextColor / 255f;
            if (settingsEnabled_OpaqueBoxBehindTooltips) {
                num3 = MathHelper.Lerp(num3, 1f, 1f);
                int num17 = 14;
                int num18 = 9;
                Utils.DrawInvBG(Main.spriteBatch, new Microsoft.Xna.Framework.Rectangle(X - num17, Y - num18, (int)zero.X + num17 * 2, (int)zero.Y + num18 + num18 / 2), new Microsoft.Xna.Framework.Color(23, 25, 81, 255) * 0.925f);
            }

            bool globalCanDraw = ItemLoader.PreDrawTooltip(_hoverItem, lines.AsReadOnly(), ref X, ref Y);
            for (int k = 0; k < numLines; k++) {
                int x = X;
                int y = Y + num16;
                drawableLines[k].X = x;
                drawableLines[k].Y = y;

                /*
                if (k == yoyoLogo) {
                */

                {
                    Microsoft.Xna.Framework.Color black = Microsoft.Xna.Framework.Color.Black;
                    black = new Microsoft.Xna.Framework.Color(num4, num4, num4, num4);
                    /*
                    if (k == 0) {
                    */
                    if (drawableLines[k].Mod == "Terraria" && drawableLines[k].Name == "ItemName") {
                        /*
                        if (rare == -13)
                            black = new Microsoft.Xna.Framework.DrawColor((byte)(255f * num4), (byte)(masterColor * 200f * num4), 0, a);
                        */

                        if (rare == -11)
                            black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(175f * num4), (byte)(0f * num4), a);

                        if (rare == -1)
                            black = new Microsoft.Xna.Framework.Color((byte)(130f * num4), (byte)(130f * num4), (byte)(130f * num4), a);

                        if (rare == 1)
                            black = new Microsoft.Xna.Framework.Color((byte)(150f * num4), (byte)(150f * num4), (byte)(255f * num4), a);

                        if (rare == 2)
                            black = new Microsoft.Xna.Framework.Color((byte)(150f * num4), (byte)(255f * num4), (byte)(150f * num4), a);

                        if (rare == 3)
                            black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(200f * num4), (byte)(150f * num4), a);

                        if (rare == 4)
                            black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(150f * num4), (byte)(150f * num4), a);

                        if (rare == 5)
                            black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(150f * num4), (byte)(255f * num4), a);

                        if (rare == 6)
                            black = new Microsoft.Xna.Framework.Color((byte)(210f * num4), (byte)(160f * num4), (byte)(255f * num4), a);

                        if (rare == 7)
                            black = new Microsoft.Xna.Framework.Color((byte)(150f * num4), (byte)(255f * num4), (byte)(10f * num4), a);

                        if (rare == 8)
                            black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(255f * num4), (byte)(10f * num4), a);

                        if (rare == 9)
                            black = new Microsoft.Xna.Framework.Color((byte)(5f * num4), (byte)(200f * num4), (byte)(255f * num4), a);

                        if (rare == 10)
                            black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(40f * num4), (byte)(100f * num4), a);

                        /*
                        if (rare >= 11)
                        */
                        if (rare == 11)
                            black = new Microsoft.Xna.Framework.Color((byte)(180f * num4), (byte)(40f * num4), (byte)(255f * num4), a);
                        if (rare > 11)
                            black = RarityLoader.GetRarity(rare).RarityColor * num4;
                        if (diff == 1)
                            black = new Microsoft.Xna.Framework.Color((byte)((float)(int)Main.mcColor.R * num4), (byte)((float)(int)Main.mcColor.G * num4), (byte)((float)(int)Main.mcColor.B * num4), a);
                        if (diff == 2)
                            black = new Microsoft.Xna.Framework.Color((byte)((float)(int)Main.hcColor.R * num4), (byte)((float)(int)Main.hcColor.G * num4), (byte)((float)(int)Main.hcColor.B * num4), a);

                        if (hoverItem.expert || rare == -12)
                            black = new Microsoft.Xna.Framework.Color((byte)((float)Main.DiscoR * num4), (byte)((float)Main.DiscoG * num4), (byte)((float)Main.DiscoB * num4), a);

                        // Handle new master mode field.
                        if (hoverItem.master || rare == -13)
                            black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(Main.masterColor * 200f * num4), 0, a);
                    }
                    else if (array2[k]) {
                        black = ((!array3[k]) ? new Microsoft.Xna.Framework.Color((byte)(120f * num4), (byte)(190f * num4), (byte)(120f * num4), a) : new Microsoft.Xna.Framework.Color((byte)(190f * num4), (byte)(120f * num4), (byte)(120f * num4), a));
                    }
                    /*
                    else if (k == numLines - 1) {
                    */
                    else if (drawableLines[k].Mod == "Terraria" && drawableLines[k].Name == "Price") {
                        black = color;
                    }

                    /*
                    if (k == researchLine)
                    */
                    if (drawableLines[k].Mod == "Terraria" && drawableLines[k].Name == "JourneyResearch")
                        black = Terraria.ID.Colors.JourneyMode;

                    /*
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, array[k], new Vector2(X, Y + num16), black, 0f, Vector2.Zero, Vector2.One);
                    */
                    //drawableLines[k].DrawColor = black;
                    Color realLineColor = black;

                    if (overrideColor[k].HasValue) {
                        // TODO: Some way for mods to bypass mouseTextColor pulsing for a TooltipLine. Apply to UnloadedPrefix once implemented.
                        realLineColor = overrideColor[k].Value * num4;
                        //drawableLines[k].OverrideColor = realLineColor;
                    }

                    ItemLoader.PreDrawTooltipLine(_hoverItem, drawableLines[k], ref num12);

                    if (drawableLines[k].Name != "ItemName" &&
                        drawableLines[k].Name != "Damage_Druid" &&
                        drawableLines[k].Name != "DruidDamageTip" &&
                        drawableLines[k].Name != "PotentialDamage") {
                        continue;
                    }

                    ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, drawableLines[k].Font, drawableLines[k].Text,
                        new Vector2(drawableLines[k].X, drawableLines[k].Y), realLineColor, drawableLines[k].Rotation, drawableLines[k].Origin, drawableLines[k].BaseScale, drawableLines[k].MaxWidth, drawableLines[k].Spread);
                }

            PostDraw:
                ItemLoader.PostDrawTooltipLine(_hoverItem, drawableLines[k]);

                num16 += (int)(FontAssets.MouseText.Value.MeasureString(drawableLines[k].Text).Y + (float)num12);
            }

            ItemLoader.PostDrawTooltip(_hoverItem, drawableLines.AsReadOnly());
        }
        catch {
            numLines2 = 0;
        }
    }

    public override void Draw(SpriteBatch spriteBatch) {
        Label = Language.GetTextValue("Mods.RoA.Configs.RoAClientConfig.DamageTooltipOption.Label2");

        Width.Set(0, 1f);

        CalculatedStyle dimensions = base.GetDimensions();
        float settingsWidth = dimensions.Width + 1f;
        Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
        Vector2 baseScale = new Vector2(0.8f);
        Color color = IsMouseHovering ? Color.White : Color.White;

        if (!MemberInfo.CanWrite)
            color = Color.Gray;

        color = Color.Lerp(color, Color.White, base.IsMouseHovering ? 1f : 0f);
        Color panelColor = base.IsMouseHovering ? UICommon.DefaultUIBlue : UICommon.DefaultUIBlue.MultiplyRGBA(new Color(180, 180, 180));
        Vector2 position = vector;

        if (Flashing) {
            float ratio = Utils.Turn01ToCyclic010(((UpdateCount % flashRate) / (float)flashRate)) * 0.5f + 0.5f;
            panelColor = Color.Lerp(panelColor, Color.White, MathF.Pow(ratio, 2));
        }
        DrawPanel2(spriteBatch, position, TextureAssets.SettingsPanel.Value, settingsWidth, dimensions.Height, panelColor);

        if (DrawLabel) {
            position.X += 8f;
            position.Y += 8f;

            string label = TextDisplayFunction();
            if (ReloadRequired && ValueChanged) {
                label += " - [c/FF0000:" + Language.GetTextValue("tModLoader.ModReloadRequired") + "]";
            }
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, label, position, color, 0f, Vector2.Zero, baseScale, settingsWidth, 2f);
        }

        position += new Vector2(6f, 30);
        int X = (int)position.X;
        int Y = (int)position.Y;

        if (Main.ThickMouse) {
            X += 6;
            Y += 6;
        }

        if (!Main.mouseItem.IsAir)
            X += 34;

        if (_hoverItem == null) {
            _hoverItem = new Item();
            _hoverItem.SetDefaults(ModContent.ItemType<TectonicCane>());
            _hoverItem.stack = 1;
        }
        string cursorText = "123";
        new Microsoft.Xna.Framework.Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
        vector = ChatManager.GetStringSize(FontAssets.MouseText.Value, cursorText, Vector2.One);
        KeywordSystem.UpdateLogic(true);
        MouseText_DrawItemTooltip(0, 0, X, Y, out int numLines2);

        int height = numLines2 <= 2 ? 122 : 150;
        Height.Set(145 + 20 + 8, 0f);
        _lastHeight = height;

        Recalculate();
    }

    public override void Update(GameTime gameTime) {
        UpdateCount++;
    }

    internal class EnumElement : RangeElement {
        private Func<object> _getValue;
        private Func<object> _getValueString;
        private Func<int> _getIndex;
        private Action<int> _setValue;
        private int max;
        private string[] valueStrings;

        public override int NumberTicks => valueStrings.Length;
        public override float TickIncrement => 1f / (valueStrings.Length - 1);

        protected override float Proportion {
            get => _getIndex() / (float)(max - 1);
            set => _setValue((int)(Math.Round(value * (max - 1))));
        }

        public override void OnBind() {
            base.OnBind();
            valueStrings = Enum.GetNames(MemberInfo.Type);

            // Retrieve individual Enum member labels
            for (int i = 0; i < valueStrings.Length; i++) {
                var enumFieldFieldInfo = MemberInfo.Type.GetField(valueStrings[i]);
                if (enumFieldFieldInfo != null) {
                    valueStrings[i] = "";
                }
            }

            max = valueStrings.Length;

            //valueEnums = Enum.GetValues(variable.Type);

            TextDisplayFunction = () => MemberInfo.Name + ": " + _getValueString();
            _getValue = () => DefaultGetValue();
            _getValueString = () => DefaultGetStringValue();
            _getIndex = () => DefaultGetIndex();
            _setValue = (int value) => DefaultSetValue(value);

            /*
            if (array != null) {
                _GetValue = () => array[index];
                _SetValue = (int valueIndex) => { array[index] = (Enum)Enum.GetValues(memberInfo.Type).GetValue(valueIndex); Interface.modConfig.SetPendingChanges(); };
                _TextDisplayFunction = () => index + 1 + ": " + _GetValueString();
            }
            */

            if (Label != null) {
                TextDisplayFunction = () => Label + ": " + _getValueString();
            }
        }

        private void DefaultSetValue(int index) {
            if (!MemberInfo.CanWrite)
                return;

            MemberInfo.SetValue(Item, Enum.GetValues(MemberInfo.Type).GetValue(index));
        }

        private object DefaultGetValue() {
            return MemberInfo.GetValue(Item);
        }

        private int DefaultGetIndex() {
            return Array.IndexOf(Enum.GetValues(MemberInfo.Type), _getValue());
        }

        private string DefaultGetStringValue() {
            int index = _getIndex();
            if (index < 0) // User manually entered invalid enum number into json or loading future Enum value saved as int.
                return Language.GetTextValue("tModLoader.ModConfigUnknownEnum");
            return valueStrings[index];
        }
    }
}

sealed class DamageTooltipOptionConfigElement2 : ConfigElement {
    private static int _lastHeight = 150;

    private int max;
    private string[] valueStrings;
    private Func<object> _getValue;
    private Func<object> _getValueString;
    private Func<int> _getIndex;
    private Action<int> _setValue;
    private Item _hoverItem;

    private EnumElement _lock;

    protected Color SliderColor { get; set; } = Color.White;
    protected Utils.ColorLerpMethod ColorMethod { get; set; }

    protected float Proportion {
        get => _getIndex() / (float)(max - 1);
        set => _setValue((int)(Math.Round(value * (max - 1))));
    }

    //private ref RoAClientConfig.DamageTooltipOptions modifying => ref ModContent.GetInstance<RoAClientConfig>().DamageTooltipOption;

    internal static RoAClientConfig.DamageTooltipOptions modifying = RoAClientConfig.DamageTooltipOptions.Option1;

    public DamageTooltipOptionConfigElement2() {
        Width.Set(0, 1f);
        float ratio = Main.screenHeight / (float)Main.screenWidth;
        Height.Set(30, 0f);

        valueStrings = Enum.GetNames(typeof(RoAClientConfig.DamageTooltipOptions));
        for (int i = 0; i < valueStrings.Length; i++) {
            var enumFieldFieldInfo = modifying;
            string name = Language.GetTextValue($"Mods.RoA.Configs.DamageTooltipOptions.Option{i + 1}.Label");
            valueStrings[i] = name;
        }

        max = valueStrings.Length;

        ColorMethod = new Utils.ColorLerpMethod((percent) => Color.Lerp(Color.Black, SliderColor, percent));

        _getValue = () => DefaultGetValue();
        _getValueString = () => DefaultGetStringValue();
        _getIndex = () => DefaultGetIndex();
        _setValue = (int value) => DefaultSetValue(value);

        _lock = new EnumElement();

        modifying = ModContent.GetInstance<RoAClientConfig>().DamageTooltipOption;

        Recalculate();
    }

    private void DefaultSetValue(int index) {
        if (!MemberInfo.CanWrite)
            return;

        modifying = (RoAClientConfig.DamageTooltipOptions)Enum.GetValues(typeof(RoAClientConfig.DamageTooltipOptions)).GetValue(index);
        SetObject(modifying);
    }

    private object DefaultGetValue() {
        return GetObject();
    }

    private int DefaultGetIndex() {
        return Array.IndexOf(Enum.GetValues(typeof(RoAClientConfig.DamageTooltipOptions)), _getValue());
    }

    private string DefaultGetStringValue() {
        int index = _getIndex();
        if (index < 0) // User manually entered invalid enum number into json or loading future Enum value saved as int.
            return Language.GetTextValue("tModLoader.ModConfigUnknownEnum");
        return valueStrings[index];
    }

    public float DrawValueBar(SpriteBatch sb, float scale, float perc, int lockState = 0, Utils.ColorLerpMethod colorMethod = null) {
        perc = Utils.Clamp(perc, -.05f, 1.05f);

        if (colorMethod == null)
            colorMethod = new Utils.ColorLerpMethod(Utils.ColorLerp_BlackToWhite);

        Texture2D colorBarTexture = TextureAssets.ColorBar.Value;
        Vector2 vector = new Vector2((float)colorBarTexture.Width, (float)colorBarTexture.Height) * scale;
        IngameOptions.valuePosition.X -= (float)((int)vector.X);
        Rectangle rectangle = new Rectangle((int)IngameOptions.valuePosition.X, (int)IngameOptions.valuePosition.Y - (int)vector.Y / 2, (int)vector.X, (int)vector.Y);
        Rectangle destinationRectangle = rectangle;
        int num = 167;
        float num2 = rectangle.X + 5f * scale;
        float num3 = rectangle.Y + 4f * scale;

        if (true) {
            int numTicks = valueStrings.Length;
            if (numTicks > 1) {
                for (int tick = 0; tick < numTicks; tick++) {
                    float percent = tick * 1f / (valueStrings.Length - 1);

                    if (percent <= 1f)
                        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(num2 + num * percent * scale), rectangle.Y - 2, 2, rectangle.Height + 4), Color.White);
                }
            }
        }

        sb.Draw(colorBarTexture, rectangle, Color.White);

        for (float num4 = 0f; num4 < (float)num; num4 += 1f) {
            float percent = num4 / (float)num;
            sb.Draw(TextureAssets.ColorBlip.Value, new Vector2(num2 + num4 * scale, num3), null, colorMethod(percent), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        rectangle.Inflate((int)(-5f * scale), 2);

        //rectangle.X = (int)num2;
        //rectangle.Y = (int)num3;

        bool flag = rectangle.Contains(new Point(Main.mouseX, Main.mouseY));

        if (lockState == 2) {
            flag = false;
        }

        if (flag || lockState == 1) {
            sb.Draw(TextureAssets.ColorHighlight.Value, destinationRectangle, Main.OurFavoriteColor);
        }

        var colorSlider = TextureAssets.ColorSlider.Value;

        sb.Draw(colorSlider, new Vector2(num2 + 167f * scale * perc, num3 + 4f * scale), null, Color.White, 0f, colorSlider.Size() * 0.5f, scale, SpriteEffects.None, 0f);

        if (Main.mouseX >= rectangle.X && Main.mouseX <= rectangle.X + rectangle.Width) {
            IngameOptions.inBar = flag;
            return (Main.mouseX - rectangle.X) / (float)rectangle.Width;
        }

        IngameOptions.inBar = false;

        if (rectangle.X >= Main.mouseX) {
            return 0f;
        }

        return 1f;
    }

    public int UpdateCount { get; private set; }

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "rightLock")]
    public extern static ref RangeElement RangeElement_rightLock(RangeElement self);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "rightHover")]
    public extern static ref RangeElement RangeElement_rightHover(RangeElement self);

    public static void MouseText_DrawItemTooltip_GetLinesInfo(Item item, ref int yoyoLogo, ref int researchLine, float oldKB, ref int numLines, string[] toolTipLine, bool[] preFixLine, bool[] badPreFixLine, string[] toolTipNames, out int prefixlineIndex) {
        prefixlineIndex = -1;
        toolTipLine[0] = item.HoverName;
        toolTipNames[0] = "ItemName";

        int damage = 10;
        toolTipLine[numLines] = damage + item.DamageType.DisplayName.Value;
        toolTipNames[numLines] = "Damage_Druid";

        numLines++;
    }

    private void MouseText_DrawItemTooltip(int rare, byte diff, int X, int Y, out int numLines2) {
        bool settingsEnabled_OpaqueBoxBehindTooltips = Main.SettingsEnabled_OpaqueBoxBehindTooltips;
        ref byte mouseTextColor = ref Main.mouseTextColor;
        Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color(mouseTextColor, mouseTextColor, mouseTextColor, mouseTextColor);
        Item hoverItem = _hoverItem;
        int yoyoLogo = -1;
        int researchLine = -1;
        rare = hoverItem.rare;
        float knockBack = hoverItem.knockBack;
        float num = 1f;
        if (hoverItem.DamageType == DamageClass.Melee && Main.player[Main.myPlayer].kbGlove)
            num += 1f;

        if (Main.player[Main.myPlayer].kbBuff)
            num += 0.5f;

        if (num != 1f)
            hoverItem.knockBack *= num;

        if (hoverItem.DamageType == DamageClass.Ranged && Main.player[Main.myPlayer].shroomiteStealth)
            hoverItem.knockBack *= 1f + (1f - Main.player[Main.myPlayer].stealth) * 0.5f;

        int num2 = 30;
        int numLines = 1;
        string[] array = new string[num2];
        bool[] array2 = new bool[num2];
        bool[] array3 = new bool[num2];
        for (int i = 0; i < num2; i++) {
            array2[i] = false;
            array3[i] = false;
        }

        // This array will be filled with internal names assigned to vanilla tooltips.
        string[] tooltipNames = new string[num2];

        MouseText_DrawItemTooltip_GetLinesInfo(hoverItem, ref yoyoLogo, ref researchLine, knockBack, ref numLines, array, array2, array3, tooltipNames, out int prefixlineIndex);
        float num3 = (float)(int)mouseTextColor / 255f;
        float num4 = num3;
        int a = mouseTextColor;

        Vector2 zero = Vector2.Zero;

        // TML's abstractions over tooltip arrays.
        List<TooltipLine> lines = ItemLoader.ModifyTooltips(_hoverItem, ref numLines, tooltipNames, ref array, ref array2, ref array3, ref yoyoLogo, out Color?[] overrideColor, prefixlineIndex);
        List<DrawableTooltipLine> drawableLines = lines.Select((TooltipLine x, int i) => new DrawableTooltipLine(x, i, 0, 0, Color.White)).ToList();
        int num0 = -(!Main.gameMenu ? 1 : 0);
        numLines2 = numLines + num0;
        int num12 = 0;
        for (int j = 0; j < numLines + num0; j++) {
            Vector2 stringSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, array[j], Vector2.One);
            if (stringSize.X > zero.X)
                zero.X = stringSize.X;

            zero.Y += stringSize.Y + (float)num12;
        }

        if (yoyoLogo != -1)
            zero.Y += 24f;

        X += 6;
        Y += 6;
        int num13 = 4;
        if (settingsEnabled_OpaqueBoxBehindTooltips) {
            X += 8;
            Y += 2;
            num13 = 18;
        }

        int num14 = Main.screenWidth;
        int num15 = Main.screenHeight;
        if ((float)X + zero.X + (float)num13 > (float)num14)
            X = (int)((float)num14 - zero.X - (float)num13);

        if ((float)Y + zero.Y + (float)num13 > (float)num15)
            Y = (int)((float)num15 - zero.Y - (float)num13);

        int num16 = 0;
        num3 = (float)(int)mouseTextColor / 255f;
        if (settingsEnabled_OpaqueBoxBehindTooltips) {
            num3 = MathHelper.Lerp(num3, 1f, 1f);
            int num17 = 14;
            int num18 = 9;
            Utils.DrawInvBG(Main.spriteBatch, new Microsoft.Xna.Framework.Rectangle(X - num17, Y - num18, (int)zero.X + num17 * 2, (int)zero.Y + num18 + num18 / 2), new Microsoft.Xna.Framework.Color(23, 25, 81, 255) * 0.925f);
        }

        bool globalCanDraw = ItemLoader.PreDrawTooltip(_hoverItem, lines.AsReadOnly(), ref X, ref Y);
        for (int k = 0; k < numLines; k++) {
            int x = X;
            int y = Y + num16;
            drawableLines[k].X = x;
            drawableLines[k].Y = y;

            /*
			if (k == yoyoLogo) {
			*/

            {
                Microsoft.Xna.Framework.Color black = Microsoft.Xna.Framework.Color.Black;
                black = new Microsoft.Xna.Framework.Color(num4, num4, num4, num4);
                /*
				if (k == 0) {
				*/
                if (drawableLines[k].Mod == "Terraria" && drawableLines[k].Name == "ItemName") {
                    /*
					if (rare == -13)
						black = new Microsoft.Xna.Framework.DrawColor((byte)(255f * num4), (byte)(masterColor * 200f * num4), 0, a);
					*/

                    if (rare == -11)
                        black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(175f * num4), (byte)(0f * num4), a);

                    if (rare == -1)
                        black = new Microsoft.Xna.Framework.Color((byte)(130f * num4), (byte)(130f * num4), (byte)(130f * num4), a);

                    if (rare == 1)
                        black = new Microsoft.Xna.Framework.Color((byte)(150f * num4), (byte)(150f * num4), (byte)(255f * num4), a);

                    if (rare == 2)
                        black = new Microsoft.Xna.Framework.Color((byte)(150f * num4), (byte)(255f * num4), (byte)(150f * num4), a);

                    if (rare == 3)
                        black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(200f * num4), (byte)(150f * num4), a);

                    if (rare == 4)
                        black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(150f * num4), (byte)(150f * num4), a);

                    if (rare == 5)
                        black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(150f * num4), (byte)(255f * num4), a);

                    if (rare == 6)
                        black = new Microsoft.Xna.Framework.Color((byte)(210f * num4), (byte)(160f * num4), (byte)(255f * num4), a);

                    if (rare == 7)
                        black = new Microsoft.Xna.Framework.Color((byte)(150f * num4), (byte)(255f * num4), (byte)(10f * num4), a);

                    if (rare == 8)
                        black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(255f * num4), (byte)(10f * num4), a);

                    if (rare == 9)
                        black = new Microsoft.Xna.Framework.Color((byte)(5f * num4), (byte)(200f * num4), (byte)(255f * num4), a);

                    if (rare == 10)
                        black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(40f * num4), (byte)(100f * num4), a);

                    /*
					if (rare >= 11)
					*/
                    if (rare == 11)
                        black = new Microsoft.Xna.Framework.Color((byte)(180f * num4), (byte)(40f * num4), (byte)(255f * num4), a);
                    if (rare > 11)
                        black = RarityLoader.GetRarity(rare).RarityColor * num4;
                    if (diff == 1)
                        black = new Microsoft.Xna.Framework.Color((byte)((float)(int)Main.mcColor.R * num4), (byte)((float)(int)Main.mcColor.G * num4), (byte)((float)(int)Main.mcColor.B * num4), a);
                    if (diff == 2)
                        black = new Microsoft.Xna.Framework.Color((byte)((float)(int)Main.hcColor.R * num4), (byte)((float)(int)Main.hcColor.G * num4), (byte)((float)(int)Main.hcColor.B * num4), a);

                    if (hoverItem.expert || rare == -12)
                        black = new Microsoft.Xna.Framework.Color((byte)((float)Main.DiscoR * num4), (byte)((float)Main.DiscoG * num4), (byte)((float)Main.DiscoB * num4), a);

                    // Handle new master mode field.
                    if (hoverItem.master || rare == -13)
                        black = new Microsoft.Xna.Framework.Color((byte)(255f * num4), (byte)(Main.masterColor * 200f * num4), 0, a);
                }
                else if (array2[k]) {
                    black = ((!array3[k]) ? new Microsoft.Xna.Framework.Color((byte)(120f * num4), (byte)(190f * num4), (byte)(120f * num4), a) : new Microsoft.Xna.Framework.Color((byte)(190f * num4), (byte)(120f * num4), (byte)(120f * num4), a));
                }
                /*
				else if (k == numLines - 1) {
				*/
                else if (drawableLines[k].Mod == "Terraria" && drawableLines[k].Name == "Price") {
                    black = color;
                }

                /*
				if (k == researchLine)
				*/
                if (drawableLines[k].Mod == "Terraria" && drawableLines[k].Name == "JourneyResearch")
                    black = Terraria.ID.Colors.JourneyMode;

                /*
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, array[k], new Vector2(X, Y + num16), black, 0f, Vector2.Zero, Vector2.One);
				*/
                //drawableLines[k].DrawColor = black;
                Color realLineColor = black;

                if (overrideColor[k].HasValue) {
                    // TODO: Some way for mods to bypass mouseTextColor pulsing for a TooltipLine. Apply to UnloadedPrefix once implemented.
                    realLineColor = overrideColor[k].Value * num4;
                    //drawableLines[k].OverrideColor = realLineColor;
                }

                ItemLoader.PreDrawTooltipLine(_hoverItem, drawableLines[k], ref num12);

                if (drawableLines[k].Name != "ItemName" &&
                    drawableLines[k].Name != "Damage_Druid" &&
                    drawableLines[k].Name != "DruidDamageTip" &&
                    drawableLines[k].Name != "PotentialDamage") {
                    continue;
                }

                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, drawableLines[k].Font, drawableLines[k].Text,
                    new Vector2(drawableLines[k].X, drawableLines[k].Y), realLineColor, drawableLines[k].Rotation, drawableLines[k].Origin, drawableLines[k].BaseScale, drawableLines[k].MaxWidth, drawableLines[k].Spread);
            }

        PostDraw:
            ItemLoader.PostDrawTooltipLine(_hoverItem, drawableLines[k]);

            num16 += (int)(FontAssets.MouseText.Value.MeasureString(drawableLines[k].Text).Y + (float)num12);
        }

        ItemLoader.PostDrawTooltip(_hoverItem, drawableLines.AsReadOnly());
    }

    public override void Draw(SpriteBatch spriteBatch) {
        Label = Language.GetTextValue("Mods.RoA.Configs.RoAClientConfig.DamageTooltipOption.Label") + ": " + Language.GetTextValue($"Mods.RoA.Configs.DamageTooltipOptions.Option{DefaultGetIndex() + 1}.Label");

        Width.Set(0, 1f);

        CalculatedStyle dimensions = base.GetDimensions();
        float settingsWidth = dimensions.Width + 1f;
        Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
        Vector2 baseScale = new Vector2(0.8f);
        Color color = IsMouseHovering ? Color.White : Color.White;

        if (!MemberInfo.CanWrite)
            color = Color.Gray;

        color = Color.Lerp(color, Color.White, base.IsMouseHovering ? 1f : 0f);
        Color panelColor = base.IsMouseHovering ? UICommon.DefaultUIBlue : UICommon.DefaultUIBlue.MultiplyRGBA(new Color(180, 180, 180));
        Vector2 position = vector;

        if (Flashing) {
            float ratio = Utils.Turn01ToCyclic010(((UpdateCount % flashRate) / (float)flashRate)) * 0.5f + 0.5f;
            panelColor = Color.Lerp(panelColor, Color.White, MathF.Pow(ratio, 2));
        }
        DrawPanel2(spriteBatch, position, TextureAssets.SettingsPanel.Value, settingsWidth, dimensions.Height, panelColor);

        if (DrawLabel) {
            position.X += 8f;
            position.Y += 8f;

            string label = TextDisplayFunction();
            if (ReloadRequired && ValueChanged) {
                label += " - [c/FF0000:" + Language.GetTextValue("tModLoader.ModReloadRequired") + "]";
            }
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, label, position, color, 0f, Vector2.Zero, baseScale, settingsWidth, 2f);
        }

        float num = 6f;
        int num2 = 0;

        ref RangeElement rightHover = ref RangeElement_rightHover(null);
        ref RangeElement rightLock = ref RangeElement_rightLock(null);

        rightHover = null;

        if (!Main.mouseLeft) {
            rightLock = null;
        }

        if (rightLock == _lock) {
            num2 = 1;
        }
        else if (rightLock != null) {
            num2 = 2;
        }

        dimensions = GetDimensions();
        float num3 = dimensions.Width + 1f;
        vector = new Vector2(dimensions.X, dimensions.Y);
        bool flag2 = IsMouseHovering;

        if (num2 == 1) {
            flag2 = true;
        }

        if (num2 == 2) {
            flag2 = false;
        }

        Vector2 vector2 = vector;
        vector2.X += 8f;
        vector2.Y += 2f + num;
        vector2.X -= 17f;
        //TextureAssets.ColorBar.Value.CurrentState(1, 1, 0, 0);
        vector2 = new Vector2(dimensions.X + dimensions.Width - 10f, dimensions.Y + 10f + num);
        IngameOptions.valuePosition = vector2;
        float obj = DrawValueBar(spriteBatch, 1f, Proportion, num2, ColorMethod);

        if (IngameOptions.inBar || rightLock == _lock) {
            rightHover = _lock;
            if (PlayerInput.Triggers.Current.MouseLeft && rightLock == _lock) {
                Proportion = obj;
            }
        }

        if (rightHover != null && rightLock == null && PlayerInput.Triggers.JustPressed.MouseLeft) {
            rightLock = rightHover;
        }

        Height.Set(30, 0f);
        //_lastHeight = height;

        Recalculate();
    }

    public override void Update(GameTime gameTime) {
        UpdateCount++;
    }


    internal class EnumElement : RangeElement {
        private Func<object> _getValue;
        private Func<object> _getValueString;
        private Func<int> _getIndex;
        private Action<int> _setValue;
        private int max;
        private string[] valueStrings;

        public override int NumberTicks => valueStrings.Length;
        public override float TickIncrement => 1f / (valueStrings.Length - 1);

        protected override float Proportion {
            get => _getIndex() / (float)(max - 1);
            set => _setValue((int)(Math.Round(value * (max - 1))));
        }

        public override void OnBind() {
            base.OnBind();
            valueStrings = Enum.GetNames(MemberInfo.Type);

            // Retrieve individual Enum member labels
            for (int i = 0; i < valueStrings.Length; i++) {
                var enumFieldFieldInfo = MemberInfo.Type.GetField(valueStrings[i]);
                if (enumFieldFieldInfo != null) {
                    valueStrings[i] = "";
                }
            }

            max = valueStrings.Length;

            //valueEnums = Enum.GetValues(variable.Type);

            TextDisplayFunction = () => MemberInfo.Name + ": " + _getValueString();
            _getValue = () => DefaultGetValue();
            _getValueString = () => DefaultGetStringValue();
            _getIndex = () => DefaultGetIndex();
            _setValue = (int value) => DefaultSetValue(value);

            /*
            if (array != null) {
                _GetValue = () => array[index];
                _SetValue = (int valueIndex) => { array[index] = (Enum)Enum.GetValues(memberInfo.Type).GetValue(valueIndex); Interface.modConfig.SetPendingChanges(); };
                _TextDisplayFunction = () => index + 1 + ": " + _GetValueString();
            }
            */

            if (Label != null) {
                TextDisplayFunction = () => Label + ": " + _getValueString();
            }
        }

        private void DefaultSetValue(int index) {
            if (!MemberInfo.CanWrite)
                return;

            MemberInfo.SetValue(Item, Enum.GetValues(MemberInfo.Type).GetValue(index));
        }

        private object DefaultGetValue() {
            return MemberInfo.GetValue(Item);
        }

        private int DefaultGetIndex() {
            return Array.IndexOf(Enum.GetValues(MemberInfo.Type), _getValue());
        }

        private string DefaultGetStringValue() {
            int index = _getIndex();
            if (index < 0) // User manually entered invalid enum number into json or loading future Enum value saved as int.
                return Language.GetTextValue("tModLoader.ModConfigUnknownEnum");
            return valueStrings[index];
        }
    }
}

sealed class DamageTooltipOptionConfigElement3 : ConfigElement {
    private static int _lastHeight = 150;

    private int max;
    private string[] valueStrings;
    private Func<object> _getValue;
    private Func<object> _getValueString;
    private Func<int> _getIndex;
    private Action<int> _setValue;
    private Item _hoverItem;

    private EnumElement _lock;

    protected Color SliderColor { get; set; } = Color.White;
    protected Utils.ColorLerpMethod ColorMethod { get; set; }

    protected float Proportion {
        get => _getIndex() / (float)(max - 1);
        set => _setValue((int)(Math.Round(value * (max - 1))));
    }

    //private ref RoAClientConfig.DamageTooltipOptions modifying => ref ModContent.GetInstance<RoAClientConfig>().DamageTooltipOption;

    internal static RoAClientConfig.HighlightModes modifying = RoAClientConfig.HighlightModes.Normal;

    public DamageTooltipOptionConfigElement3() {
        Width.Set(0, 1f);
        float ratio = Main.screenHeight / (float)Main.screenWidth;
        Height.Set(30, 0f);

        valueStrings = Enum.GetNames(typeof(RoAClientConfig.HighlightModes));
        for (int i = 0; i < valueStrings.Length; i++) {
            var enumFieldFieldInfo = modifying;
            string name = Language.GetTextValue($"Mods.RoA.Configs.DamageTooltipOptions.Option{i + 1}.Label");
            valueStrings[i] = name;
        }

        max = valueStrings.Length;

        ColorMethod = new Utils.ColorLerpMethod((percent) => Color.Lerp(Color.Black, SliderColor, percent));

        _getValue = () => DefaultGetValue();
        _getValueString = () => DefaultGetStringValue();
        _getIndex = () => DefaultGetIndex();
        _setValue = (int value) => DefaultSetValue(value);

        _lock = new EnumElement();

        modifying = ModContent.GetInstance<RoAClientConfig>().HighlightMode;

        Recalculate();
    }

    private void DefaultSetValue(int index) {
        if (!MemberInfo.CanWrite)
            return;

        modifying = (RoAClientConfig.HighlightModes)Enum.GetValues(typeof(RoAClientConfig.HighlightModes)).GetValue(index);
        SetObject(modifying);
    }

    private object DefaultGetValue() {
        return GetObject();
    }

    private int DefaultGetIndex() {
        return Array.IndexOf(Enum.GetValues(typeof(RoAClientConfig.HighlightModes)), _getValue());
    }

    private string DefaultGetStringValue() {
        int index = _getIndex();
        if (index < 0) // User manually entered invalid enum number into json or loading future Enum value saved as int.
            return Language.GetTextValue("tModLoader.ModConfigUnknownEnum");
        return valueStrings[index];
    }

    public float DrawValueBar(SpriteBatch sb, float scale, float perc, int lockState = 0, Utils.ColorLerpMethod colorMethod = null) {
        perc = Utils.Clamp(perc, -.05f, 1.05f);

        if (colorMethod == null)
            colorMethod = new Utils.ColorLerpMethod(Utils.ColorLerp_BlackToWhite);

        Texture2D colorBarTexture = TextureAssets.ColorBar.Value;
        Vector2 vector = new Vector2((float)colorBarTexture.Width, (float)colorBarTexture.Height) * scale;
        IngameOptions.valuePosition.X -= (float)((int)vector.X);
        Rectangle rectangle = new Rectangle((int)IngameOptions.valuePosition.X, (int)IngameOptions.valuePosition.Y - (int)vector.Y / 2, (int)vector.X, (int)vector.Y);
        Rectangle destinationRectangle = rectangle;
        int num = 167;
        float num2 = rectangle.X + 5f * scale;
        float num3 = rectangle.Y + 4f * scale;

        if (true) {
            int numTicks = valueStrings.Length;
            if (numTicks > 1) {
                for (int tick = 0; tick < numTicks; tick++) {
                    float percent = tick * 1f / (valueStrings.Length - 1);

                    if (percent <= 1f)
                        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(num2 + num * percent * scale), rectangle.Y - 2, 2, rectangle.Height + 4), Color.White);
                }
            }
        }

        sb.Draw(colorBarTexture, rectangle, Color.White);

        for (float num4 = 0f; num4 < (float)num; num4 += 1f) {
            float percent = num4 / (float)num;
            sb.Draw(TextureAssets.ColorBlip.Value, new Vector2(num2 + num4 * scale, num3), null, colorMethod(percent), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        rectangle.Inflate((int)(-5f * scale), 2);

        //rectangle.X = (int)num2;
        //rectangle.Y = (int)num3;

        bool flag = rectangle.Contains(new Point(Main.mouseX, Main.mouseY));

        if (lockState == 2) {
            flag = false;
        }

        if (flag || lockState == 1) {
            sb.Draw(TextureAssets.ColorHighlight.Value, destinationRectangle, Main.OurFavoriteColor);
        }

        var colorSlider = TextureAssets.ColorSlider.Value;

        sb.Draw(colorSlider, new Vector2(num2 + 167f * scale * perc, num3 + 4f * scale), null, Color.White, 0f, colorSlider.Size() * 0.5f, scale, SpriteEffects.None, 0f);

        if (Main.mouseX >= rectangle.X && Main.mouseX <= rectangle.X + rectangle.Width) {
            IngameOptions.inBar = flag;
            return (Main.mouseX - rectangle.X) / (float)rectangle.Width;
        }

        IngameOptions.inBar = false;

        if (rectangle.X >= Main.mouseX) {
            return 0f;
        }

        return 1f;
    }

    public int UpdateCount { get; private set; }

    public override void Draw(SpriteBatch spriteBatch) {
        string name = "Normal";
        switch (modifying) {
            case RoAClientConfig.HighlightModes.Always:
                name = "Always";
                break;
            case RoAClientConfig.HighlightModes.Off:
                name = "Off";
                break;
        }
        Label =
            Language.GetTextValue("Mods.RoA.Configs.RoAClientConfig.HighlightMode.Label") + ": " +
            Language.GetTextValue($"Mods.RoA.Configs.HighlightModes.{name}.Label");

        Width.Set(0, 1f);

        CalculatedStyle dimensions = base.GetDimensions();
        float settingsWidth = dimensions.Width + 1f;
        Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
        Vector2 baseScale = new Vector2(0.8f);
        Color color = IsMouseHovering ? Color.White : Color.White;

        if (!MemberInfo.CanWrite)
            color = Color.Gray;

        color = Color.Lerp(color, Color.White, base.IsMouseHovering ? 1f : 0f);
        Color panelColor = base.IsMouseHovering ? UICommon.DefaultUIBlue : UICommon.DefaultUIBlue.MultiplyRGBA(new Color(180, 180, 180));
        Vector2 position = vector;

        if (Flashing) {
            float ratio = Utils.Turn01ToCyclic010(((UpdateCount % flashRate) / (float)flashRate)) * 0.5f + 0.5f;
            panelColor = Color.Lerp(panelColor, Color.White, MathF.Pow(ratio, 2));
        }
        DrawPanel2(spriteBatch, position, TextureAssets.SettingsPanel.Value, settingsWidth, dimensions.Height, panelColor);

        if (DrawLabel) {
            position.X += 8f;
            position.Y += 8f;

            string label = TextDisplayFunction();
            if (ReloadRequired && ValueChanged) {
                label += " - [c/FF0000:" + Language.GetTextValue("tModLoader.ModReloadRequired") + "]";
            }
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, label, position, color, 0f, Vector2.Zero, baseScale, settingsWidth, 2f);
        }

        float num = 6f;
        int num2 = 0;

        ref RangeElement rightHover = ref DamageTooltipOptionConfigElement2.RangeElement_rightHover(null);
        ref RangeElement rightLock = ref DamageTooltipOptionConfigElement2.RangeElement_rightLock(null);

        rightHover = null;

        if (!Main.mouseLeft) {
            rightLock = null;
        }

        if (rightLock == _lock) {
            num2 = 1;
        }
        else if (rightLock != null) {
            num2 = 2;
        }

        dimensions = GetDimensions();
        float num3 = dimensions.Width + 1f;
        vector = new Vector2(dimensions.X, dimensions.Y);
        bool flag2 = IsMouseHovering;

        if (num2 == 1) {
            flag2 = true;
        }

        if (num2 == 2) {
            flag2 = false;
        }

        Vector2 vector2 = vector;
        vector2.X += 8f;
        vector2.Y += 2f + num;
        vector2.X -= 17f;
        //TextureAssets.ColorBar.Value.CurrentState(1, 1, 0, 0);
        vector2 = new Vector2(dimensions.X + dimensions.Width - 10f, dimensions.Y + 10f + num);
        IngameOptions.valuePosition = vector2;
        float obj = DrawValueBar(spriteBatch, 1f, Proportion, num2, ColorMethod);

        if (IngameOptions.inBar || rightLock == _lock) {
            rightHover = _lock;
            if (PlayerInput.Triggers.Current.MouseLeft && rightLock == _lock) {
                Proportion = obj;
            }
        }

        if (rightHover != null && rightLock == null && PlayerInput.Triggers.JustPressed.MouseLeft) {
            rightLock = rightHover;
        }

        Height.Set(30, 0f);
        //_lastHeight = height;

        Recalculate();
    }

    public override void Update(GameTime gameTime) {
        UpdateCount++;
    }


    internal class EnumElement : RangeElement {
        private Func<object> _getValue;
        private Func<object> _getValueString;
        private Func<int> _getIndex;
        private Action<int> _setValue;
        private int max;
        private string[] valueStrings;

        public override int NumberTicks => valueStrings.Length;
        public override float TickIncrement => 1f / (valueStrings.Length - 1);

        protected override float Proportion {
            get => _getIndex() / (float)(max - 1);
            set => _setValue((int)(Math.Round(value * (max - 1))));
        }

        public override void OnBind() {
            base.OnBind();
            valueStrings = Enum.GetNames(MemberInfo.Type);

            // Retrieve individual Enum member labels
            for (int i = 0; i < valueStrings.Length; i++) {
                var enumFieldFieldInfo = MemberInfo.Type.GetField(valueStrings[i]);
                if (enumFieldFieldInfo != null) {
                    valueStrings[i] = "";
                }
            }

            max = valueStrings.Length;

            //valueEnums = Enum.GetValues(variable.Type);

            TextDisplayFunction = () => MemberInfo.Name + ": " + _getValueString();
            _getValue = () => DefaultGetValue();
            _getValueString = () => DefaultGetStringValue();
            _getIndex = () => DefaultGetIndex();
            _setValue = (int value) => DefaultSetValue(value);

            /*
            if (array != null) {
                _GetValue = () => array[index];
                _SetValue = (int valueIndex) => { array[index] = (Enum)Enum.GetValues(memberInfo.Type).GetValue(valueIndex); Interface.modConfig.SetPendingChanges(); };
                _TextDisplayFunction = () => index + 1 + ": " + _GetValueString();
            }
            */

            if (Label != null) {
                TextDisplayFunction = () => Label + ": " + _getValueString();
            }
        }

        private void DefaultSetValue(int index) {
            if (!MemberInfo.CanWrite)
                return;

            MemberInfo.SetValue(Item, Enum.GetValues(MemberInfo.Type).GetValue(index));
        }

        private object DefaultGetValue() {
            return MemberInfo.GetValue(Item);
        }

        private int DefaultGetIndex() {
            return Array.IndexOf(Enum.GetValues(MemberInfo.Type), _getValue());
        }

        private string DefaultGetStringValue() {
            int index = _getIndex();
            if (index < 0) // User manually entered invalid enum number into json or loading future Enum value saved as int.
                return Language.GetTextValue("tModLoader.ModConfigUnknownEnum");
            return valueStrings[index];
        }
    }
}