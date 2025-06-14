using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.PopupTexts;

[Autoload(Side = ModSide.Client)]
sealed class CustomPopupTextLoader : ILoadable {
    public static CustomPopupText[] popupText = new CustomPopupText[20];

    public void Load(Mod mod) {
        for (int num5 = 0; num5 < 20; num5++) {
            CustomPopupTextLoader.popupText[num5] = new CustomPopupText();
        }

        On_Main.DoUpdateInWorld += On_Main_DoUpdateInWorld;
        On_Main.DrawItemTextPopups += On_Main_DrawItemTextPopups;
    }

    private void On_Main_DoUpdateInWorld(On_Main.orig_DoUpdateInWorld orig, Main self, System.Diagnostics.Stopwatch sw) {
        orig(self, sw);

        if (Main.netMode != NetmodeID.Server) {
            CustomPopupText.UpdateItemText();
        }
    }

    public void Unload() {
        popupText = null;
    }

    private void On_Main_DrawItemTextPopups(On_Main.orig_DrawItemTextPopups orig, float scaleTarget) {
        orig(scaleTarget);

        for (int i = 0; i < 20; i++) {
            CustomPopupText popupText = CustomPopupTextLoader.popupText[i];
            if (popupText == null || !popupText.active)
                continue;

            string text = popupText.name;
            if (popupText.stack > 1)
                text = text + " (" + popupText.stack + ")";

            Vector2 vector = FontAssets.MouseText.Value.MeasureString(text);
            Vector2 origin = new Vector2(vector.X * 0.5f, vector.Y * 0.5f);
            float num = popupText.scale / scaleTarget;
            int num2 = (int)(255f - 255f * num);
            float num3 = (int)popupText.color.R;
            float num4 = (int)popupText.color.G;
            float num5 = (int)popupText.color.B;
            float num6 = (int)popupText.color.A;
            num3 *= num * popupText.alpha * 0.3f;
            num5 *= num * popupText.alpha * 0.3f;
            num4 *= num * popupText.alpha * 0.3f;
            num6 *= num * popupText.alpha;
            Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color((int)num3, (int)num4, (int)num5, (int)num6);
            Microsoft.Xna.Framework.Color color2 = Microsoft.Xna.Framework.Color.Black;
            float num7 = 1f;
            Texture2D texture2D = null;
            switch (popupText.context) {
                case CustomPopupTextContext.PettyBag:
                    color2 = new Microsoft.Xna.Framework.Color(92, 98, 152) * 0.4f;
                    num7 = 0.8f;
                    break;
                case CustomPopupTextContext.ItemPickupToVoidContainer:
                    color2 = new Microsoft.Xna.Framework.Color(127, 20, 255) * 0.4f;
                    num7 = 0.8f;
                    break;
                case CustomPopupTextContext.SonarAlert:
                    color2 = Microsoft.Xna.Framework.Color.Blue * 0.4f;
                    if (popupText.npcNetID != 0)
                        color2 = Microsoft.Xna.Framework.Color.Red * 0.4f;
                    num7 = 1f;
                    break;
            }

            float num8 = (float)num2 / 255f;
            for (int j = 0; j < 5; j++) {
                color = color2;
                float num9 = 0f;
                float num10 = 0f;
                switch (j) {
                    case 0:
                        num9 -= scaleTarget * 2f;
                        break;
                    case 1:
                        num9 += scaleTarget * 2f;
                        break;
                    case 2:
                        num10 -= scaleTarget * 2f;
                        break;
                    case 3:
                        num10 += scaleTarget * 2f;
                        break;
                    default:
                        color = popupText.color * num * popupText.alpha * num7;
                        break;
                }

                if (j < 4) {
                    num6 = (float)(int)popupText.color.A * num * popupText.alpha;
                    color = new Microsoft.Xna.Framework.Color(0, 0, 0, (int)num6);
                }

                if (color2 != Microsoft.Xna.Framework.Color.Black && j < 4) {
                    num9 *= 1.3f + 1.3f * num8;
                    num10 *= 1.3f + 1.3f * num8;
                }

                float num11 = popupText.position.Y - Main.screenPosition.Y + num10;
                if (Main.player[Main.myPlayer].gravDir == -1f)
                    num11 = (float)Main.screenHeight - num11;

                if (color2 != Microsoft.Xna.Framework.Color.Black && j < 4) {
                    Microsoft.Xna.Framework.Color color3 = color2;
                    color3.A = (byte)MathHelper.Lerp(60f, 127f, Utils.GetLerpValue(0f, 255f, num6, clamped: true));
                    Main.spriteBatch.DrawString(FontAssets.MouseText.Value, text, new Vector2(popupText.position.X - Main.screenPosition.X + num9 + origin.X, num11 + origin.Y), Microsoft.Xna.Framework.Color.Lerp(color, color3, 0.5f), popupText.rotation, origin, popupText.scale, SpriteEffects.None, 0f);
                    Main.spriteBatch.DrawString(FontAssets.MouseText.Value, text, new Vector2(popupText.position.X - Main.screenPosition.X + num9 + origin.X, num11 + origin.Y), color3, popupText.rotation, origin, popupText.scale, SpriteEffects.None, 0f);
                }
                else {
                    Main.spriteBatch.DrawString(FontAssets.MouseText.Value, text, new Vector2(popupText.position.X - Main.screenPosition.X + num9 + origin.X, num11 + origin.Y), color, popupText.rotation, origin, popupText.scale, SpriteEffects.None, 0f);
                }

                if (texture2D != null) {
                    float scale = (1.3f - num8) * popupText.scale * 0.7f;
                    Vector2 vector2 = new Vector2(popupText.position.X - Main.screenPosition.X + num9 + origin.X, num11 + origin.Y);
                    Microsoft.Xna.Framework.Color color4 = color2 * 0.6f;
                    if (j == 4)
                        color4 = Microsoft.Xna.Framework.Color.White * 0.6f;

                    color4.A = (byte)((float)(int)color4.A * 0.5f);
                    int num12 = 25;
                    Main.spriteBatch.Draw(texture2D, vector2 + new Vector2(origin.X * -0.5f - (float)num12 - texture2D.Size().X / 2f, 0f), null, color4 * popupText.scale, 0f, texture2D.Size() / 2f, scale, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(texture2D, vector2 + new Vector2(origin.X * 0.5f + (float)num12 + texture2D.Size().X / 2f, 0f), null, color4 * popupText.scale, 0f, texture2D.Size() / 2f, scale, SpriteEffects.None, 0f);
                }
            }
        }
    }
}

class CustomPopupText {
    /// <summary>
    /// The position of this <see cref="PopupText"/> in world coordinates.
    /// </summary>
    public Vector2 position;

    /// <summary>
    /// The velocity of this <see cref="PopupText"/> in world coordinates per tick.
    /// </summary>
    public Vector2 velocity;

    /// <summary>
    /// The opacity of this <see cref="PopupText"/> in the range [0f, 1f], where <c>0f</c> is transparent and <c>1f</c> is opaque.
    /// </summary>
    public float alpha;

    /// <summary>
    /// The direction this <see cref="PopupText"/>'s <see cref="alpha"/> changes in.
    /// </summary>
    public int alphaDir = 1;

    /// <summary>
    /// The text displayed by this <see cref="PopupText"/>.
    /// </summary>
    public string name;

    /// <summary>
    /// The optional stack size appended to <see cref="name"/>.
    /// <br/> Will only be displayed is <c><see cref="stack"/> &gt; 1</c>.
    /// </summary>
    public long stack;

    /// <summary>
    /// The scale this <see cref="PopupText"/> draws at.
    /// </summary>
    public float scale = 1f;

    /// <summary>
    /// The clockwise _rotation of this <see cref="PopupText"/> in radians.
    /// </summary>
    public float rotation;

    /// <summary>
    /// The color of this <see cref="PopupText"/>'s text.
    /// </summary>
    public Color color;

    /// <summary>
    /// If <see langword="true"/>, this <see cref="PopupText"/> is visible in the world.
    /// </summary>
    public bool active;

    /// <summary>
    /// The time in ticks this <see cref="PopupText"/> will remain for until it starts to disappear.
    /// </summary>
    public int lifeTime;

    /// <summary>
    /// The default <see cref="lifeTime"/> of a <see cref="PopupText"/>.
    /// </summary>
    public static int activeTime = 60;

    /// <summary>
    /// The number of <see cref="active"/> <see cref="PopupText"/>s in <see cref="CustomPopupTextLoader.popupText"/>.
    /// <br/> Assigned after <see cref="UpdateItemText"/> runs.
    /// </summary>
    public static int numActive;

    /// <summary>
    /// If <see langword="true"/>, this <see cref="PopupText"/> can't be modified when creating a new item <see cref="PopupText"/>.
    /// </summary>
    public bool NoStack;

    /// <summary>
    /// If <see langword="true"/>, this <see cref="PopupText"/> is specifically for coins.
    /// </summary>
    public bool coinText;

    /// <summary>
    /// The value of coins this <see cref="PopupText"/> represents in the range [0, 999999999].
    /// </summary>
    public long coinValue;

    /// <summary>
    /// The index in <see cref="CustomPopupTextLoader.popupText"/> of the last known sonar text.
    /// <br/> Assign and clear using <see cref="AssignAsSonarText(int)"/> and <see cref="ClearSonarText"/>.
    /// </summary>
    public static int sonarText = -1;

    /// <summary>
    /// If <see langword="true"/>, this <see cref="PopupText"/> will draw in the Expert Mode rarity color.
    /// </summary>
    public bool expert;

    /// <summary>
    /// If <see langword="true"/>, this <see cref="PopupText"/> will draw in the Master Mode rarity color.
    /// </summary>
    public bool master;

    /// <summary>
    /// Marks this <see cref="PopupText"/> as this player's Sonar Potion text.
    /// </summary>
    public bool sonar;

    /// <summary>
    /// The context in which this <see cref="PopupText"/> was created.
    /// </summary>
    public CustomPopupTextContext context;

    /// <summary>
    /// The NPC type (<see cref="NPC.type"/>) this <see cref="PopupText"/> is bound to, or <c>0</c> if not bound to an NPC.
    /// </summary>
    public int npcNetID;

    /// <summary>
    /// If <see langword="true"/>, this <see cref="PopupText"/> is not bound to an item or NPC.
    /// </summary>
    public bool freeAdvanced;

    // Added by TML.
    /// <summary>
    /// The <see cref="ItemRarityID"/> this <see cref="PopupText"/> uses for its main color.
    /// </summary>
    public int rarity;

    /// <summary>
    /// If <see langword="true"/>, this <see cref="PopupText"/> is not for an item.
    /// </summary>
    public bool notActuallyAnItem {
        get {
            if (npcNetID == 0)
                return freeAdvanced;

            return true;
        }
    }

    public static float TargetScale => Main.UIScale / Main.GameViewMatrix.Zoom.X;

    /// <summary>
    /// Destroys the <see cref="PopupText"/> in <see cref="CustomPopupTextLoader.popupText"/> at the index <see cref="sonarText"/> if that text has <see cref="sonar"/> set to <see langword="true"/>.
    /// <para/> Note that multiple fishing lures is a common feature of many popular mods, so this method won't clear all PopupText in that situation because only 1 index is tracked. The Projectile.localAI[2] of a bobber projectile is used to store the index of its sonar PopupText. That stored value is offset by +1.
    /// </summary>
    public static void ClearSonarText() {
        if (sonarText >= 0 && CustomPopupTextLoader.popupText[sonarText].sonar) {
            CustomPopupTextLoader.popupText[sonarText].active = false;
            sonarText = -1;
        }
    }

    /// <summary>
    /// Resets a <see cref="PopupText"/> to its default values.
    /// </summary>
    /// <param name="text">The <see cref="PopupText"/> to reset.</param>
    public static void ResetText(CustomPopupText text) {
        text.NoStack = false;
        text.coinText = false;
        text.coinValue = 0L;
        text.sonar = false;
        text.npcNetID = 0;
        text.expert = false;
        text.master = false;
        text.freeAdvanced = false;
        text.scale = 0f;
        text.rotation = 0f;
        text.alpha = 1f;
        text.alphaDir = -1;
        text.rarity = 0;
    }

    /// <summary>
    /// Creates a new <see cref="PopupText"/> in <see cref="CustomPopupTextLoader.popupText"/> at <paramref name="position"/> using the settings from <paramref name="request"/>.
    /// <br/> The new <see cref="PopupText"/> is not bound to a specific <see cref="Item"/> or <see cref="NPCID"/>.
    /// <br/> All <see cref="PopupText"/>s created using this method have <c><see cref="context"/> == <see cref="CustomPopupTextContext.Advanced"/></c> and <see cref="freeAdvanced"/> set to <see langword="true"/>.
    /// </summary>
    /// <param name="request">The settings for the new <see cref="PopupText"/>.</param>
    /// <param name="position">The position of the new <see cref="PopupText"/> in world coordinates.</param>
    /// <returns>
    /// <c>-1</c> if a new <see cref="PopupText"/> could not be made, if <c><see cref="Main.netMode"/> == <see cref="NetmodeID.Server"/></c>, or if the current player has item text disabled (<see cref="Main.showItemText"/>).
    /// <br/> Otherwise, return the index in <see cref="CustomPopupTextLoader.popupText"/> of the new <see cref="PopupText"/>
    /// </returns>
    public static int NewText(AdvancedPopupRequest request, Vector2 position) {
        if (!Main.showItemText)
            return -1;

        if (Main.netMode == 2)
            return -1;

        int num = FindNextItemTextSlot();
        if (num >= 0) {
            string text = request.Text;
            Vector2 vector = FontAssets.MouseText.Value.MeasureString(text);
            CustomPopupText obj = CustomPopupTextLoader.popupText[num];
            ResetText(obj);
            obj.active = true;
            obj.position = position - vector / 2f;
            obj.name = text;
            obj.stack = 1L;
            obj.velocity = request.Velocity;
            obj.lifeTime = request.DurationInFrames;
            obj.context = CustomPopupTextContext.Advanced;
            obj.freeAdvanced = true;
            obj.color = request.Color;
        }

        return num;
    }

    /// <summary>
    /// Creates a new <see cref="PopupText"/> in <see cref="CustomPopupTextLoader.popupText"/> at <paramref name="position"/> bound to a given <paramref name="npcNetID"/>.
    /// </summary>
    /// <param name="context">
    /// The <see cref="CustomPopupTextContext"/> in which this <see cref="PopupText"/> was created.
    /// <br/> If <c><paramref name="context"/> == <see cref="CustomPopupTextContext.SonarAlert"/></c>, then <see cref="color"/> will be a shade of red.
    /// </param>
    /// <param name="npcNetID">The <see cref="NPCID"/> this <see cref="PopupText"/> represents.</param>
    /// <param name="position"></param>
    /// <param name="stay5TimesLonger">If <see langword="true"/>, then this <see cref="PopupText"/> will spawn with <c><see cref="lifeTime"/> == 5 * 60</c>.</param>
    /// <returns>
    /// <inheritdoc cref="NewText(AdvancedPopupRequest, Vector2)"/>
    /// <br/> Also returns <c>-1</c> if <c><paramref name="npcNetID"/> == 0</c>.
    /// </returns>
    public static int NewText(CustomPopupTextContext context, int npcNetID, Vector2 position, bool stay5TimesLonger) {
        if (!Main.showItemText)
            return -1;

        if (npcNetID == 0)
            return -1;

        if (Main.netMode == 2)
            return -1;

        int num = FindNextItemTextSlot();
        if (num >= 0) {
            NPC nPC = new NPC();
            nPC.SetDefaults(npcNetID);
            string typeName = nPC.TypeName;
            Vector2 vector = FontAssets.MouseText.Value.MeasureString(typeName);
            CustomPopupText popupText = CustomPopupTextLoader.popupText[num];
            ResetText(popupText);
            popupText.active = true;
            popupText.position = position - vector / 2f;
            popupText.name = typeName;
            popupText.stack = 1L;
            popupText.velocity.Y = -7f;
            popupText.lifeTime = 60;
            popupText.context = context;
            if (stay5TimesLonger)
                popupText.lifeTime *= 5;

            popupText.npcNetID = npcNetID;
            popupText.color = Color.White;
            if (context == CustomPopupTextContext.SonarAlert)
                popupText.color = Color.Lerp(Color.White, Color.Crimson, 0.5f);
        }

        return num;
    }

    /// <summary>
    /// Creates a new <see cref="PopupText"/> in <see cref="CustomPopupTextLoader.popupText"/> at the center of the picked-up <paramref name="newItem"/>.
    /// <br/> If a <see cref="PopupText"/> already exists with the <see cref="Item.AffixName"/> of <paramref name="newItem"/>, that text will instead be modified unless <paramref name="noStack"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="context">The <see cref="CustomPopupTextContext"/> in which this <see cref="PopupText"/> was created.</param>
    /// <param name="newItem">The <see cref="Item"/> to create the new text from.</param>
    /// <param name="stack">The stack of <paramref name="newItem"/>.</param>
    /// <param name="noStack">If <see langword="true"/>, always create a new <see cref="PopupText"/> instead of modifying an existing one.</param>
    /// <param name="longText">If <see langword="true"/>, then this <see cref="PopupText"/> will spawn with <c><see cref="lifeTime"/> == 5 * 60</c>.</param>
    /// <returns>
    /// <inheritdoc cref="NewText(AdvancedPopupRequest, Vector2)"/>
    /// <br/> Also returns <c>-1</c> if <see cref="Item.Name"/> is <see langword="null"/>.
    /// </returns>
    public static int NewText(CustomPopupTextContext context, Item newItem, int stack, bool noStack = false, bool longText = false) {
        if (!Main.showItemText)
            return -1;

        if (newItem.Name == null || !newItem.active)
            return -1;

        if (Main.netMode == 2)
            return -1;

        bool flag = newItem.type >= 71 && newItem.type <= 74;
        for (int i = 0; i < 20; i++) {
            CustomPopupText popupText = CustomPopupTextLoader.popupText[i];
            if (popupText == null || !popupText.active || popupText.notActuallyAnItem || (!(popupText.name == newItem.AffixName()) && (!flag || !popupText.coinText)) || popupText.NoStack || noStack)
                continue;

            string text = newItem.Name + " (" + (popupText.stack + stack) + ")";
            string text2 = newItem.Name;
            if (popupText.stack > 1)
                text2 = text2 + " (" + popupText.stack + ")";

            Vector2 vector = FontAssets.MouseText.Value.MeasureString(text2);
            vector = FontAssets.MouseText.Value.MeasureString(text);
            if (popupText.lifeTime < 0)
                popupText.scale = 1f;

            if (popupText.lifeTime < 60)
                popupText.lifeTime = 60;

            if (flag && popupText.coinText) {
                long num = 0L;
                if (newItem.type == 71)
                    num += stack;
                else if (newItem.type == 72)
                    num += 100 * stack;
                else if (newItem.type == 73)
                    num += 10000 * stack;
                else if (newItem.type == 74)
                    num += 1000000 * stack;

                popupText.AddToCoinValue(num);
                text = ValueToName(popupText.coinValue);
                vector = FontAssets.MouseText.Value.MeasureString(text);
                popupText.name = text;
                if (popupText.coinValue >= 1000000) {
                    if (popupText.lifeTime < 300)
                        popupText.lifeTime = 300;

                    popupText.color = new Color(220, 220, 198);
                }
                else if (popupText.coinValue >= 10000) {
                    if (popupText.lifeTime < 240)
                        popupText.lifeTime = 240;

                    popupText.color = new Color(224, 201, 92);
                }
                else if (popupText.coinValue >= 100) {
                    if (popupText.lifeTime < 180)
                        popupText.lifeTime = 180;

                    popupText.color = new Color(181, 192, 193);
                }
                else if (popupText.coinValue >= 1) {
                    if (popupText.lifeTime < 120)
                        popupText.lifeTime = 120;

                    popupText.color = new Color(246, 138, 96);
                }
            }

            popupText.stack += stack;
            popupText.scale = 0f;
            popupText.rotation = 0f;
            popupText.position.X = newItem.position.X + (float)newItem.width * 0.5f - vector.X * 0.5f;
            popupText.position.Y = newItem.position.Y + (float)newItem.height * 0.25f - vector.Y * 0.5f;
            popupText.velocity.Y = -7f;
            popupText.context = context;
            popupText.npcNetID = 0;
            if (popupText.coinText)
                popupText.stack = 1L;

            return i;
        }

        int num2 = FindNextItemTextSlot();
        if (num2 >= 0) {
            string text3 = newItem.AffixName();
            if (stack > 1)
                text3 = text3 + " (" + stack + ")";

            Vector2 vector2 = FontAssets.MouseText.Value.MeasureString(text3);
            CustomPopupText popupText2 = CustomPopupTextLoader.popupText[num2];
            ResetText(popupText2);
            popupText2.active = true;
            popupText2.position.X = newItem.position.X + (float)newItem.width * 0.5f - vector2.X * 0.5f;
            popupText2.position.Y = newItem.position.Y + (float)newItem.height * 0.25f - vector2.Y * 0.5f;
            popupText2.color = Color.White;
            if (newItem.rare == 1)
                popupText2.color = new Color(150, 150, 255);
            else if (newItem.rare == 2)
                popupText2.color = new Color(150, 255, 150);
            else if (newItem.rare == 3)
                popupText2.color = new Color(255, 200, 150);
            else if (newItem.rare == 4)
                popupText2.color = new Color(255, 150, 150);
            else if (newItem.rare == 5)
                popupText2.color = new Color(255, 150, 255);
            else if (newItem.rare == -13)
                popupText2.master = true;
            else if (newItem.rare == -11)
                popupText2.color = new Color(255, 175, 0);
            else if (newItem.rare == -1)
                popupText2.color = new Color(130, 130, 130);
            else if (newItem.rare == 6)
                popupText2.color = new Color(210, 160, 255);
            else if (newItem.rare == 7)
                popupText2.color = new Color(150, 255, 10);
            else if (newItem.rare == 8)
                popupText2.color = new Color(255, 255, 10);
            else if (newItem.rare == 9)
                popupText2.color = new Color(5, 200, 255);
            else if (newItem.rare == 10)
                popupText2.color = new Color(255, 40, 100);
            else if (newItem.rare == 11)
                popupText2.color = new Color(180, 40, 255);
            else if (newItem.rare >= ItemRarityID.Count)
                popupText2.color = RarityLoader.GetRarity(newItem.rare).RarityColor;

            popupText2.rarity = newItem.rare; // Added by TML
            popupText2.expert = newItem.expert;
            popupText2.master = newItem.master; // Added by TML
            popupText2.name = newItem.AffixName();
            popupText2.stack = stack;
            popupText2.velocity.Y = -7f;
            popupText2.lifeTime = 60;
            popupText2.context = context;
            if (longText)
                popupText2.lifeTime *= 5;

            popupText2.coinValue = 0L;
            popupText2.coinText = newItem.type >= 71 && newItem.type <= 74;
            if (popupText2.coinText) {
                long num3 = 0L;
                if (newItem.type == 71)
                    num3 += popupText2.stack;
                else if (newItem.type == 72)
                    num3 += 100 * popupText2.stack;
                else if (newItem.type == 73)
                    num3 += 10000 * popupText2.stack;
                else if (newItem.type == 74)
                    num3 += 1000000 * popupText2.stack;

                popupText2.AddToCoinValue(num3);
                popupText2.ValueToName();
                popupText2.stack = 1L;
                if (popupText2.coinValue >= 1000000) {
                    if (popupText2.lifeTime < 300)
                        popupText2.lifeTime = 300;

                    popupText2.color = new Color(220, 220, 198);
                }
                else if (popupText2.coinValue >= 10000) {
                    if (popupText2.lifeTime < 240)
                        popupText2.lifeTime = 240;

                    popupText2.color = new Color(224, 201, 92);
                }
                else if (popupText2.coinValue >= 100) {
                    if (popupText2.lifeTime < 180)
                        popupText2.lifeTime = 180;

                    popupText2.color = new Color(181, 192, 193);
                }
                else if (popupText2.coinValue >= 1) {
                    if (popupText2.lifeTime < 120)
                        popupText2.lifeTime = 120;

                    popupText2.color = new Color(246, 138, 96);
                }
            }
        }

        return num2;
    }

    private void AddToCoinValue(long addedValue) {
        long val = coinValue + addedValue;
        coinValue = Math.Min(999999999L, Math.Max(0L, val));
    }

    private static int FindNextItemTextSlot() {
        int num = -1;
        for (int i = 0; i < 20; i++) {
            if (CustomPopupTextLoader.popupText[i] == null || !CustomPopupTextLoader.popupText[i].active) {
                num = i;
                break;
            }
        }

        if (num == -1) {
            double num2 = Main.bottomWorld;
            for (int j = 0; j < 20; j++) {
                if (num2 > (double)CustomPopupTextLoader.popupText[j].position.Y) {
                    num = j;
                    num2 = CustomPopupTextLoader.popupText[j].position.Y;
                }
            }
        }

        return num;
    }

    /// <summary>
    /// Marks the <see cref="PopupText"/> in <see cref="CustomPopupTextLoader.popupText"/> at <paramref name="sonarTextIndex"/> as sonar text, assigning <see cref="sonarText"/> and setting <see cref="sonar"/> to <see langword="true"/>.
    /// </summary>
    /// <param name="sonarTextIndex"></param>
    public static void AssignAsSonarText(int sonarTextIndex) {
        sonarText = sonarTextIndex;
        if (sonarText > -1)
            CustomPopupTextLoader.popupText[sonarText].sonar = true;
    }

    /// <summary>
    /// Converts a value in copper coins to a formatted string.
    /// </summary>
    /// <param name="coinValue">The value to format in copper coins.</param>
    /// <returns>The formatted text.</returns>
    public static string ValueToName(long coinValue) {
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        int num4 = 0;
        string text = "";
        long num5 = coinValue;
        while (num5 > 0) {
            if (num5 >= 1000000) {
                num5 -= 1000000;
                num++;
            }
            else if (num5 >= 10000) {
                num5 -= 10000;
                num2++;
            }
            else if (num5 >= 100) {
                num5 -= 100;
                num3++;
            }
            else if (num5 >= 1) {
                num5--;
                num4++;
            }
        }

        text = "";
        if (num > 0)
            text = text + num + string.Format(" {0} ", Language.GetTextValue("Currency.Platinum"));

        if (num2 > 0)
            text = text + num2 + string.Format(" {0} ", Language.GetTextValue("Currency.Gold"));

        if (num3 > 0)
            text = text + num3 + string.Format(" {0} ", Language.GetTextValue("Currency.Silver"));

        if (num4 > 0)
            text = text + num4 + string.Format(" {0} ", Language.GetTextValue("Currency.Copper"));

        if (text.Length > 1)
            text = text.Substring(0, text.Length - 1);

        return text;
    }

    private void ValueToName() {
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        int num4 = 0;
        long num5 = coinValue;
        while (num5 > 0) {
            if (num5 >= 1000000) {
                num5 -= 1000000;
                num++;
            }
            else if (num5 >= 10000) {
                num5 -= 10000;
                num2++;
            }
            else if (num5 >= 100) {
                num5 -= 100;
                num3++;
            }
            else if (num5 >= 1) {
                num5--;
                num4++;
            }
        }

        name = "";
        if (num > 0)
            name = name + num + string.Format(" {0} ", Language.GetTextValue("Currency.Platinum"));

        if (num2 > 0)
            name = name + num2 + string.Format(" {0} ", Language.GetTextValue("Currency.Gold"));

        if (num3 > 0)
            name = name + num3 + string.Format(" {0} ", Language.GetTextValue("Currency.Silver"));

        if (num4 > 0)
            name = name + num4 + string.Format(" {0} ", Language.GetTextValue("Currency.Copper"));

        if (name.Length > 1)
            name = name.Substring(0, name.Length - 1);
    }

    /// <summary>
    /// Updates this <see cref="PopupText"/>.
    /// </summary>
    /// <param name="whoAmI">The index in <see cref="CustomPopupTextLoader.popupText"/> of this <see cref="PopupText"/>.</param>
    public void Update(int whoAmI) {
        if (!active)
            return;

        float targetScale = TargetScale;
        alpha += (float)alphaDir * 0.01f;
        if ((double)alpha <= 0.7) {
            alpha = 0.7f;
            alphaDir = 1;
        }

        if (alpha >= 1f) {
            alpha = 1f;
            alphaDir = -1;
        }

        if (expert)
            color = new Color((byte)Main.DiscoR, (byte)Main.DiscoG, (byte)Main.DiscoB, Main.mouseTextColor);
        else if (master)
            color = new Color(255, (byte)(Main.masterColor * 200f), 0, Main.mouseTextColor);

        if (rarity > ItemRarityID.Purple)
            color = RarityLoader.GetRarity(rarity).RarityColor;

        bool flag = false;
        Vector2 textHitbox = GetTextHitbox();
        Rectangle rectangle = new Rectangle((int)(position.X - textHitbox.X / 2f), (int)(position.Y - textHitbox.Y / 2f), (int)textHitbox.X, (int)textHitbox.Y);
        for (int i = 0; i < 20; i++) {
            CustomPopupText popupText = CustomPopupTextLoader.popupText[i];
            if (!popupText.active || i == whoAmI)
                continue;

            Vector2 textHitbox2 = popupText.GetTextHitbox();
            Rectangle value = new Rectangle((int)(popupText.position.X - textHitbox2.X / 2f), (int)(popupText.position.Y - textHitbox2.Y / 2f), (int)textHitbox2.X, (int)textHitbox2.Y);
            if (rectangle.Intersects(value) && (position.Y < popupText.position.Y || (position.Y == popupText.position.Y && whoAmI < i))) {
                flag = true;
                int num = numActive;
                if (num > 3)
                    num = 3;

                popupText.lifeTime = activeTime + 15 * num;
                lifeTime = activeTime + 15 * num;
            }
        }

        if (!flag) {
            velocity.Y *= 0.86f;
            if (scale == targetScale)
                velocity.Y *= 0.4f;
        }
        else if (velocity.Y > -6f) {
            velocity.Y -= 0.2f;
        }
        else {
            velocity.Y *= 0.86f;
        }

        velocity.X *= 0.93f;
        position += velocity;
        lifeTime--;
        if (lifeTime <= 0) {
            scale -= 0.03f * targetScale;
            if ((double)scale < 0.1 * (double)targetScale) {
                active = false;
                if (sonarText == whoAmI)
                    sonarText = -1;
            }

            lifeTime = 0;
        }
        else {
            if (scale < targetScale)
                scale += 0.1f * targetScale;

            if (scale > targetScale)
                scale = targetScale;
        }
    }

    private Vector2 GetTextHitbox() {
        string text = name;
        if (stack > 1)
            text = text + " (" + stack + ")";

        Vector2 result = FontAssets.MouseText.Value.MeasureString(text);
        result *= scale;
        result.Y *= 0.8f;
        return result;
    }

    /// <summary>
    /// Calls <see cref="Update(int)"/> on all <see cref="active"/> <see cref="PopupText"/>s in <see cref="CustomPopupTextLoader.popupText"/> and assigns  <see cref="numActive"/>.
    /// </summary>
    public static void UpdateItemText() {
        int num = 0;
        for (int i = 0; i < 20; i++) {
            if (CustomPopupTextLoader.popupText[i] != null && CustomPopupTextLoader.popupText[i].active) {
                num++;
                CustomPopupTextLoader.popupText[i].Update(i);
            }
        }

        numActive = num;
    }

    /// <summary>
    /// Sets all <see cref="PopupText"/>s in <see cref="CustomPopupTextLoader.popupText"/> to a new instance and assigns <see cref="numActive"/> to <c>0</c>.
    /// </summary>
    public static void ClearAll() {
        for (int i = 0; i < 20; i++) {
            CustomPopupTextLoader.popupText[i] = new CustomPopupText();
        }

        numActive = 0;
    }
}
