using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;
using ReLogic.Graphics;

using RoA.Common.Configs;
using RoA.Common.Druid.Wreath;
using RoA.Core;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;

namespace RoA.Common.InterfaceElements;

sealed class FancyWreathDrawing() : InterfaceElement(RoA.ModName + ": Wreath Drawing Fancy", InterfaceScaleType.UI) {
    public static bool MapEnabled { get; private set; }
    public static bool IsHoveringUI { get; private set; }

    private static Asset<Texture2D> _mainTexture, _mainTexture2;

    public override bool ShouldDrawSelfInMenu() => true;

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) {
        int preferredIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Resource Bars");
        return preferredIndex < 0 ? 0 : preferredIndex;
    }

    public override void Load(Mod mod) {
        On_Main.DrawInventory += On_Main_DrawInventory;
        On_PlayerResourceSetsManager.Draw += On_PlayerResourceSetsManager_Draw; ;

        if (Main.dedServ) {
            return;
        }

        string textureName = ResourceManager.UITextures + "Wreath_Fancy";
        _mainTexture = ModContent.Request<Texture2D>(textureName);
        textureName = ResourceManager.UITextures + "Wreath_Bars";
        _mainTexture2 = ModContent.Request<Texture2D>(textureName);
    }

    private void On_PlayerResourceSetsManager_Draw(On_PlayerResourceSetsManager.orig_Draw orig, PlayerResourceSetsManager self) {
        orig(self);
        if (RoAConfig.IsFancy) {
            DrawInner1();
        }
        else if (RoAConfig.IsBars) {
            DrawInner2();
        }
    }

    private void On_Main_DrawInventory(On_Main.orig_DrawInventory orig, Main self) {
        if (ModContent.GetInstance<RoAConfig>().WreathDrawingMode == RoAConfig.WreathDrawingModes.Normal) {
            orig(self);
            return;
        }

        bool mapEnabled = Main.mapEnabled;
        Main.mapEnabled = false;
        orig(self);
        Main.mapEnabled = mapEnabled;
        MapEnabled = Main.mapEnabled;

        int num = 0;
        int num2 = 0;
        int num3 = Main.screenWidth;
        int num4 = 44;
        int num5 = Main.screenWidth;
        int num6 = 0;
        if (Main.mapEnabled) {
            bool flag = false;
            int num12 = num3 - 440;
            int num13 = 40 + num4;
            if (Main.screenWidth < 940)
                flag = true;

            if (flag) {
                num12 = num5 - 40;
                num13 = num6 - 200;
            }

            int num14 = 0;
            for (int k = 0; k < 4; k++) {
                int num15 = 255;
                int num16 = num12 + k * 32 - num14;
                int num17 = num13;
                if (flag) {
                    num16 = num12;
                    num17 = num13 + k * 32 - num14;
                }

                int num18 = k;
                num15 = 120;
                if (k > 0 && Main.mapStyle == k - 1)
                    num15 = 200;

                if (Main.mouseX >= num16 && Main.mouseX <= num16 + 32 && Main.mouseY >= num17 && Main.mouseY <= num17 + 30 && !PlayerInput.IgnoreMouseInterface) {
                    num15 = 255;
                    num18 += 4;
                    Main.player[Main.myPlayer].mouseInterface = true;
                    if (Main.mouseLeft && Main.mouseLeftRelease) {
                        if (k == 0) {
                            Main.playerInventory = false;
                            Main.player[Main.myPlayer].SetTalkNPC(-1);
                            Main.npcChatCornerItem = 0;
                            SoundEngine.PlaySound(SoundID.MenuOpen);
                            //SoundEngine.PlaySound(10);
                            Main.mapFullscreenScale = 2.5f;
                            Main.mapFullscreen = true;
                            Main.resetMapFull = true;
                        }

                        if (k == 1) {
                            Main.mapStyle = 0;
                            SoundEngine.PlaySound(SoundID.MenuTick);
                            //SoundEngine.PlaySound(12);
                        }

                        if (k == 2) {
                            Main.mapStyle = 1;
                            SoundEngine.PlaySound(SoundID.MenuTick);
                            //SoundEngine.PlaySound(12);
                        }

                        if (k == 3) {
                            Main.mapStyle = 2;
                            SoundEngine.PlaySound(SoundID.MenuTick);
                            //SoundEngine.PlaySound(12);
                        }
                    }
                }

                Main.spriteBatch.Draw(TextureAssets.MapIcon[num18].Value, new Vector2(num16, num17), new Microsoft.Xna.Framework.Rectangle(0, 0, TextureAssets.MapIcon[num18].Width(), TextureAssets.MapIcon[num18].Height()), new Microsoft.Xna.Framework.Color(num15, num15, num15, num15), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
            }
        }
    }

    protected override bool DrawSelf() {
        //DrawInner();
        return true;
    }

    private void DrawInner1() {
        if (!RoAConfig.IsFancy) {
            return;
        }

        Texture2D mainTexture = _mainTexture.Value;

        Player player = Main.LocalPlayer;
        var stats = player.GetModPlayer<WreathHandler>();

        // border
        SpriteBatch spriteBatch = Main.spriteBatch;
        int width = 44, height = 46;
        int UI_ScreenAnchorX = Main.screenWidth - 870;
        Vector2 position = new Vector2(500 + UI_ScreenAnchorX + width / 2, 15f + height / 2);
        if (ModContent.GetInstance<RoAConfig>().WreathDrawingMode == RoAConfig.WreathDrawingModes.Fancy2) {
            Vector2 vector3 = new Vector2(Main.screenWidth - 300 + 4, 15f);
            Vector2 vector = vector3 + new Vector2(-4f, 3f) + new Vector2(-48f, -18f);
            Player localPlayer = Main.LocalPlayer;
            Color textColor = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
            string text3 = Language.GetTextValue("Mods.RoA.WreathSlot") + ":";
            string text4 = " ";
            bool flag = stats.CurrentResource < 100;
            bool flag2 = stats.CurrentResource < 10;
            bool flag3 = stats.CurrentResource < 200;
            if (flag) {
                text4 = string.Empty;
            }
            string text = text3 + text4 + stats.TotalResource + "/" + stats.TotalResource;
            Vector2 vector2 = FontAssets.MouseText.Value.MeasureString(text);
            if (flag2) {
                vector2.X *= 0.9f;
            }
            else if (flag3) {
                vector2.X *= 0.95f;
            }
            spriteBatch.DrawString(FontAssets.MouseText.Value, text3, vector + new Vector2((0f - vector2.X) * 0.5f, 0f), textColor, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(FontAssets.MouseText.Value, stats.CurrentResource + "/" + stats.TotalResource, vector + new Vector2(vector2.X * 0.5f, 0f), textColor, 0f,
                new Vector2(FontAssets.MouseText.Value.MeasureString(stats.CurrentResource + "/" + stats.TotalResource).X, 0f), 1f, SpriteEffects.None, 0f);
            position.Y += 6;
        }
        Rectangle frame = new(0, 0, width, height);
        int num5 = 255;
        int a = (int)((double)num5 * 0.9);
        Color color = new(num5, num5, num5, a);
        Vector2 origin = new(width / 2, height / 2);
        float scale = Main.UIScale;
        spriteBatch.Draw(mainTexture, position, frame, color, 0f, origin, scale, SpriteEffects.None, 0f);

        IsHoveringUI = false;
        Vector2 mouseScreen = Main.MouseScreen;
        if (mouseScreen.X > (float)(500 + UI_ScreenAnchorX) && mouseScreen.X < (float)(500 + width + UI_ScreenAnchorX) && 
            mouseScreen.Y > 15f && mouseScreen.Y < (float)(15f + height)) {
            if (!Main.mouseText) {
                player.cursorItemIconEnabled = false;
                string text = "[kw/n:" + stats.CurrentResource + "]" + "/" + stats.TotalResource;
                Main.instance.MouseTextHackZoom(text);
                Main.mouseText = true;
                IsHoveringUI = true;
            }
        }

        // fill
        frame.X = 6;
        frame.Y = stats.IsPhoenixWreath ? 102 : 54;
        frame.Width = 30;
        float progress = MathHelper.Clamp(stats.ActualProgress2 * 1.1f, 0f, 1f);
        frame.Height = (int)(30 * progress);
        origin = new Vector2(30, 30) / 2f;
        position -= Vector2.One * 2f;
        position.X += 1f;
        spriteBatch.Draw(mainTexture, position, frame, color, MathHelper.Pi, origin, scale, SpriteEffects.None, 0f);

        // full icon
        if (stats.IsFull) {
            frame.X = 170;
            frame.Y = stats.IsPhoenixWreath ? 130 : 82;
            frame.Width = 6;
            frame.Height = 6;
            Vector2 origin2 = origin;
            origin = frame.Size() / 2f;
            Vector2 position2 = position;
            position2 -= Vector2.One * 2f;
            position2.X += 1f;
            position2 += origin2;
            position2.Y += 3f;
            spriteBatch.Draw(mainTexture, position2, frame, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        // soul of the woods fill
        float value = stats.ActualProgress2 - 1f;
        value = MathHelper.Clamp(value * 1.1f, 0f, 1f);
        if (stats.SoulOfTheWoods && value >= 0f) {
            frame.X = 52;
            frame.Y = 54;
            frame.Width = 30;
            progress = value;
            frame.Height = (int)(30 * progress);
            origin = new Vector2(30, 30) / 2f;
            spriteBatch.Draw(mainTexture, position, frame, color, MathHelper.Pi, origin, scale, SpriteEffects.None, 0f);
        }

        // soul of the woods full icon
        if (stats.IsFull2) {
            frame.X = 216;
            frame.Y = 82;
            frame.Width = 6;
            frame.Height = 6;
            Vector2 origin2 = origin;
            origin = frame.Size() / 2f;
            Vector2 position2 = position;
            position2 -= Vector2.One * 2f;
            position2.X += 1f;
            position2 += origin2;
            position2.Y += 3f;
            spriteBatch.Draw(mainTexture, position2, frame, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }

    private void DrawInner2() {
        if (!RoAConfig.IsBars) {
            return;
        }

        Texture2D mainTexture = _mainTexture2.Value;

        // border
        Player player = Main.LocalPlayer;
        var stats = player.GetModPlayer<WreathHandler>();
        SpriteBatch spriteBatch = Main.spriteBatch;
        int width = 44, height = 44;
        int UI_ScreenAnchorX = Main.screenWidth - 870;
        Vector2 position = new Vector2(500 + UI_ScreenAnchorX + width / 2, 15f + height / 2);
        if (ModContent.GetInstance<RoAConfig>().WreathDrawingMode == RoAConfig.WreathDrawingModes.Bars2) {
            Vector2 vector3 = new Vector2(Main.screenWidth - 300 + 4, 15f);
            Vector2 vector = vector3 + new Vector2(-4f, 3f) + new Vector2(-48f, -18f);
            Player localPlayer = Main.LocalPlayer;
            Color textColor = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
            string text3 = Language.GetTextValue("Mods.RoA.WreathSlot") + ":";
            string text4 = " ";
            bool flag = stats.CurrentResource < 100;
            bool flag2 = stats.CurrentResource < 10;
            bool flag3 = stats.CurrentResource < 200;
            if (flag) {
                text4 = string.Empty;
            }
            string text = text3 + text4 + stats.TotalResource + "/" + stats.TotalResource;
            Vector2 vector2 = FontAssets.MouseText.Value.MeasureString(text);
            if (flag2) {
                vector2.X *= 0.9f;
            }
            else if (flag3) {
                vector2.X *= 0.95f;
            }
            spriteBatch.DrawString(FontAssets.MouseText.Value, text3, vector + new Vector2((0f - vector2.X) * 0.5f, 0f), textColor, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(FontAssets.MouseText.Value, stats.CurrentResource + "/" + stats.TotalResource, vector + new Vector2(vector2.X * 0.5f, 0f), textColor, 0f,
                new Vector2(FontAssets.MouseText.Value.MeasureString(stats.CurrentResource + "/" + stats.TotalResource).X, 0f), 1f, SpriteEffects.None, 0f);
            position.Y += 6;
        }
        Rectangle frame = new(0, 0, width, height);
        int num5 = 255;
        int a = (int)((double)num5 * 0.9);
        Color color = new(num5, num5, num5, a);
        Vector2 origin = new(width / 2, height / 2);
        float scale = Main.UIScale;
        spriteBatch.Draw(mainTexture, position, frame, color, 0f, origin, scale, SpriteEffects.None, 0f);

        IsHoveringUI = false;
        Vector2 mouseScreen = Main.MouseScreen;
        if (mouseScreen.X > (float)(500 + UI_ScreenAnchorX) && mouseScreen.X < (float)(500 + width + UI_ScreenAnchorX) &&
            mouseScreen.Y > 15f && mouseScreen.Y < (float)(15f + height)) {
            if (!Main.mouseText) {
                player.cursorItemIconEnabled = false;
                string text = "[kw/n:" + stats.CurrentResource + "]" + "/" + stats.TotalResource;
                Main.instance.MouseTextHackZoom(text);
                Main.mouseText = true;
                IsHoveringUI = true;
            }
        }

        // fill
        frame.X = 8;
        frame.Y = stats.IsPhoenixWreath ? 104 : 56;
        frame.Width = 28;
        float progress = MathHelper.Clamp(stats.ActualProgress2 * 1.1f, 0f, 1f);
        frame.Height = (int)(28 * progress);
        origin = new Vector2(28, 28) / 2f;
        position -= Vector2.One * 2f;
        position.X += 1f;
        position += new Vector2(1f, 2f);
        spriteBatch.Draw(mainTexture, position, frame, color, MathHelper.Pi, origin, scale, SpriteEffects.None, 0f);

        // full icon
        if (stats.IsFull) {
            frame.X = 144;
            frame.Y = stats.IsPhoenixWreath ? 104 : 56;
            frame.Width = 28;
            frame.Height = 28;
            Vector2 origin2 = origin;
            origin = frame.Size() / 2f;
            Vector2 position2 = position;
            spriteBatch.Draw(mainTexture, position2, frame, color, MathHelper.Pi, origin, scale, SpriteEffects.None, 0f);
        }

        // soul of the woods fill
        float value = stats.ActualProgress2 - 1f;
        value = MathHelper.Clamp(value * 1.1f, 0f, 1f);
        if (stats.SoulOfTheWoods && value >= 0f) {
            frame.X = 54;
            frame.Y = 56;
            frame.Width = 28;
            progress = value;
            frame.Height = (int)(28 * progress);
            origin = new Vector2(28, 28) / 2f;
            spriteBatch.Draw(mainTexture, position, frame, color, MathHelper.Pi, origin, scale, SpriteEffects.None, 0f);
        }

        // soul of the woods icon
        if (stats.IsFull2) {
            frame.X = 190;
            frame.Y = 56;
            frame.Width = 28;
            frame.Height = 28;
            Vector2 origin2 = origin;
            origin = frame.Size() / 2f;
            Vector2 position2 = position;
            spriteBatch.Draw(mainTexture, position2, frame, color, MathHelper.Pi, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
