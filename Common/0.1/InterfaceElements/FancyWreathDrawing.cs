using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;
using ReLogic.Graphics;

using RoA.Common.Cache;
using RoA.Common.Configs;
using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.InterfaceElements;

sealed class FancyWreathDrawing() : InterfaceElement(RoA.ModName + ": Wreath Drawing Fancy", InterfaceScaleType.UI) {
    public static bool MapEnabled { get; private set; }
    public static bool IsHoveringUI { get; private set; }

    private static Asset<Texture2D> _mainTexture, _mainTexture2;

    public override bool ShouldDrawSelfInMenu() => true;

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) {
        int preferredIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Resource Bars");
        return preferredIndex < 0 ? 0 : preferredIndex + 1;
    }

    public override void Load(Mod mod) {
        On_Main.DrawInventory += On_Main_DrawInventory;
        On_PlayerResourceSetsManager.Draw += On_PlayerResourceSetsManager_Draw;

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

        var config = ModContent.GetInstance<RoAClientConfig>();
        var wreathPosition = config.WreathPosition;
        if (Main.InGameUI.IsVisible || Main.ingameOptionsWindow) {
            wreathPosition = RoAClientConfig.WreathPositions.Health;
        }
        bool flag = wreathPosition == RoAClientConfig.WreathPositions.Player;
        SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
        if (flag) {
            Main.spriteBatch.End();
            Main.spriteBatch.BeginWorld();
        }

        if (RoAClientConfig.IsFancy) {
            DrawInner1();
        }
        else if (RoAClientConfig.IsBars) {
            DrawInner2();
        }

        if (flag) {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(in snapshot);
        }
    }

    private void On_Main_DrawInventory(On_Main.orig_DrawInventory orig, Main self) {
        var config = ModContent.GetInstance<RoAClientConfig>();
        bool flag5 = config.WreathDrawingMode == RoAClientConfig.WreathDrawingModes.Normal;
        if (flag5 && config.WreathPosition != RoAClientConfig.WreathPositions.Health) {
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
        if (!RoAClientConfig.IsFancy) {
            return;
        }

        Texture2D mainTexture = _mainTexture.Value;

        Player player = Main.LocalPlayer;
        var stats = player.GetWreathHandler();

        // border
        SpriteBatch spriteBatch = Main.spriteBatch;
        int width = 44, height = 46;
        int UI_ScreenAnchorX = Main.screenWidth - 870;
        Vector2 position = new Vector2(500 + UI_ScreenAnchorX + width / 2, 15f + height / 2);
        var config = ModContent.GetInstance<RoAClientConfig>();
        var wreathPosition = config.WreathPosition;
        if (Main.InGameUI.IsVisible || Main.ingameOptionsWindow) {
            wreathPosition = RoAClientConfig.WreathPositions.Health;
        }
        bool reversedGravity = player.gravDir == -1f;
        if (Main.InGameUI.IsVisible || Main.ingameOptionsWindow) {
            reversedGravity = false;
        }
        Vector2 screenSize = Main.ScreenSize.ToVector2();
        screenSize *= Main.UIScale;
        var formHandler = player.GetFormHandler();
        var currentForm = formHandler.CurrentForm;
        if (wreathPosition == RoAClientConfig.WreathPositions.Player) {
            position = screenSize / 2f;
            position.Y -= player.height * 1.5f;

            if (!player.GetWreathHandler().ShouldDrawItself) {
                return;
            }
        }
        bool fancy2 = config.WreathDrawingMode == RoAClientConfig.WreathDrawingModes.Fancy2;
        bool onPlayer = wreathPosition == RoAClientConfig.WreathPositions.Player;
        bool defaultResources = !onPlayer && Main.ResourceSetsManager.ActiveSetKeyName == "Default";
        bool horizontalBarsWithText = !onPlayer && Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBarsWithText";
        if (fancy2) {
            Vector2 vector3 = new Vector2(Main.screenWidth - 300, 15f);
            Vector2 vector = vector3 + new Vector2(-4f, 3f) + new Vector2(-48f, -18f);
            if (onPlayer) {
                vector3 = screenSize / 2f;
                vector3.Y -= player.height * 1.5f;

                vector = vector3 + new Vector2(-4f, 3f) + new Vector2(0f, -40f);
            }
            if (defaultResources) {
                vector.Y += 6f;
            }
            if (horizontalBarsWithText) {
                vector.Y += 2f;
            }
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
                vector2.X *= 0.93f;
            }
            else if (flag3) {
                vector2.X *= 1f;
            }
            if (reversedGravity) {
                vector.Y += 28f;
            }
            if (formHandler.IsInADruidicForm && currentForm != null && currentForm.BaseForm.IsDrawing) {
                vector += currentForm.BaseForm.SetWreathOffset2(player);
            }
            spriteBatch.DrawString(FontAssets.MouseText.Value, text3,
                reversedGravity ? Main.ReverseGravitySupport(vector + new Vector2((0f - vector2.X) * 0.5f, 0f)) : (vector + new Vector2((0f - vector2.X) * 0.5f, 0f)), textColor, 0f, default(Vector2), 1f,
                reversedGravity ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
            spriteBatch.DrawString(FontAssets.MouseText.Value, stats.CurrentResource + "/" + stats.TotalResource,
                reversedGravity ? Main.ReverseGravitySupport(vector + new Vector2(vector2.X * 0.5f, 0f)) : (vector + new Vector2(vector2.X * 0.5f, 0f)), textColor, 0f,
                new Vector2(FontAssets.MouseText.Value.MeasureString(stats.CurrentResource + "/" + stats.TotalResource).X, 0f), 1f,
                reversedGravity ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
            if (!defaultResources) {
                position.Y += 6;
            }
        }

        Rectangle frame = new(0, 0, width, height);
        Color color = WreathHandler.BaseColor;
        Vector2 origin = new(width / 2, height / 2);
        float scale = Main.UIScale;
        if (wreathPosition == RoAClientConfig.WreathPositions.Player) {
            scale = 1f;
        }
        else {
            position.Y += 24f * (Main.UIScale - 1f);
        }

        if (reversedGravity) {
            position = Main.ReverseGravitySupport(position);
        }
        float rotation = 0f;
        if (reversedGravity) {
            rotation = MathHelper.Pi;
        }
        SpriteEffects effects = SpriteEffects.None;
        if (reversedGravity) {
            effects = SpriteEffects.FlipHorizontally;
            position.Y -= 0f;
        }
        if (formHandler.IsInADruidicForm && currentForm != null && currentForm.BaseForm.IsDrawing) {
            position += currentForm.BaseForm.SetWreathOffset2(player);
        }

        position += new Vector2(defaultResources ? -4 : 0, defaultResources ? 18f : 0f);
        if (horizontalBarsWithText) {
            position.Y += 2f;
        }

        spriteBatch.Draw(mainTexture, position, frame, color, rotation, origin, scale, effects, 0f);

        IsHoveringUI = false;
        Matrix matrix2 = Matrix.Invert(Main.GameViewMatrix.ZoomMatrix);
        Vector2 position3 = Main.ReverseGravitySupport(Main.MouseScreen);
        Vector2.Transform(Main.screenPosition, matrix2);
        Vector2 v2 = Vector2.Transform(position3, matrix2);
        float startX = (float)(500 + UI_ScreenAnchorX);
        float endX = (float)(500 + width + UI_ScreenAnchorX);
        float startY = 15f;
        float endY = (float)(15f + height);
        if (wreathPosition == RoAClientConfig.WreathPositions.Player) {
            startX = position.X - width / 2;
            endX = startX + width;
            startY = position.Y - height / 2;
            endY = startY + height;
        }
        else {
            v2 = Main.MouseScreen;
        }
        if (v2.X > startX && v2.X < endX &&
            v2.Y > startY && v2.Y < endY) {
            if (!Main.mouseText) {
                player.cursorItemIconEnabled = false;
                string text = "[kw/n:" + stats.CurrentResource + "]" + "/" + stats.TotalResource;
                Main.instance.MouseTextHackZoom(text);
                Main.mouseText = true;
                IsHoveringUI = true;
            }
        }

        if (stats.IsAetherWreath) {
            color = WreathHandler.AetherBaseColor;
        }

        // fill
        frame.X = stats.IsAetherWreath ? 52 : 6;
        frame.Y = stats.IsPhoenixWreath || stats.IsAetherWreath ? 102 : 54;
        frame.Width = 30;
        float progress = MathHelper.Clamp(stats.ActualProgress2 * 1.1f, 0f, 1f);
        frame.Height = (int)(30 * progress);
        origin = new Vector2(30, 30) / 2f;
        position -= Vector2.One * 2f;
        position.X += 1f;
        if (reversedGravity) {
            position.Y += 4f;
        }
        spriteBatch.Draw(mainTexture, position, frame, color, MathHelper.Pi + rotation, origin, scale, effects, 0f);

        // full icon
        if (stats.IsFull1) {
            frame.X = stats.IsAetherWreath ? 216 : 170;
            frame.Y = stats.IsPhoenixWreath || stats.IsAetherWreath ? 130 : 82;
            frame.Width = 6;
            frame.Height = 6;
            Vector2 origin2 = origin;
            origin = frame.Size() / 2f;
            Vector2 position2 = position;
            position2 -= Vector2.One * 2f;
            position2.X += 1f;
            position2 += origin2;
            position2.Y += 3f;
            Vector2 offset = Vector2.Zero;
            if (reversedGravity) {
                offset.Y -= 32f;
            }
            spriteBatch.Draw(mainTexture, position2 + offset, frame, color, rotation, origin, scale, effects, 0f);
        }

        color = WreathHandler.SoulOfTheWoodsBaseColor;
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
            spriteBatch.Draw(mainTexture, position, frame, color, MathHelper.Pi + rotation, origin, scale, effects, 0f);
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
            spriteBatch.Draw(mainTexture, position2, frame, color, rotation, origin, scale, effects, 0f);
        }
    }

    private void DrawInner2() {
        if (!RoAClientConfig.IsBars) {
            return;
        }

        Texture2D mainTexture = _mainTexture2.Value;

        // border
        Player player = Main.LocalPlayer;
        var stats = player.GetWreathHandler();
        SpriteBatch spriteBatch = Main.spriteBatch;
        int width = 44, height = 44;
        int UI_ScreenAnchorX = Main.screenWidth - 870;
        Vector2 position = new Vector2(500 + UI_ScreenAnchorX + width / 2, 15f + height / 2);
        var config = ModContent.GetInstance<RoAClientConfig>();
        var wreathPosition = config.WreathPosition;
        if (Main.InGameUI.IsVisible || Main.ingameOptionsWindow) {
            wreathPosition = RoAClientConfig.WreathPositions.Health;
        }
        Vector2 screenSize = Main.ScreenSize.ToVector2();
        screenSize *= Main.UIScale;
        if (wreathPosition == RoAClientConfig.WreathPositions.Player) {
            position = screenSize / 2f;
            position.Y -= player.height * 1.5f;

            if (!player.GetWreathHandler().ShouldDrawItself) {
                return;
            }
        }
        bool reversedGravity = player.gravDir == -1f;
        if (Main.InGameUI.IsVisible || Main.ingameOptionsWindow) {
            reversedGravity = false;
        }
        var formHandler = player.GetFormHandler();
        var currentForm = formHandler.CurrentForm;
        bool bars2 = config.WreathDrawingMode == RoAClientConfig.WreathDrawingModes.Bars2;
        bool onPlayer = wreathPosition == RoAClientConfig.WreathPositions.Player;
        bool defaultResources = !onPlayer && Main.ResourceSetsManager.ActiveSetKeyName == "Default";
        bool horizontalBarsWithText = !onPlayer && Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBarsWithText";
        if (bars2) {
            Vector2 vector3 = new Vector2(Main.screenWidth - 300 + 4, 15f);
            Vector2 vector = vector3 + new Vector2(-4f, 3f) + new Vector2(-48f, -18f);
            if (onPlayer) {
                vector3 = screenSize / 2f;
                vector3.Y -= player.height * 1.5f;

                vector = vector3 + new Vector2(-4f, 3f) + new Vector2(0f, -40f);
            }
            if (defaultResources) {
                vector.Y += 6f;
            }
            if (horizontalBarsWithText) {
                vector.Y += 2f;
            }
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
                vector2.X *= 0.93f;
            }
            else if (flag3) {
                vector2.X *= 1f;
            }
            if (reversedGravity) {
                vector.Y += 28f;
            }
            if (formHandler.IsInADruidicForm && currentForm != null && currentForm.BaseForm.IsDrawing) {
                vector2 += currentForm.BaseForm.SetWreathOffset2(player);
            }
            spriteBatch.DrawString(FontAssets.MouseText.Value, text3,
                reversedGravity ? Main.ReverseGravitySupport(vector + new Vector2((0f - vector2.X) * 0.5f, 0f)) : (vector + new Vector2((0f - vector2.X) * 0.5f, 0f)), textColor, 0f, default(Vector2), 1f,
                reversedGravity ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
            spriteBatch.DrawString(FontAssets.MouseText.Value, stats.CurrentResource + "/" + stats.TotalResource,
                reversedGravity ? Main.ReverseGravitySupport(vector + new Vector2(vector2.X * 0.5f, 0f)) : (vector + new Vector2(vector2.X * 0.5f, 0f)), textColor, 0f,
                new Vector2(FontAssets.MouseText.Value.MeasureString(stats.CurrentResource + "/" + stats.TotalResource).X, 0f), 1f,
                reversedGravity ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
            if (!defaultResources) {
                position.Y += 6;
            }
        }

        Rectangle frame = new(0, 0, width, height);
        Color color = WreathHandler.BaseColor;
        Vector2 origin = new(width / 2, height / 2);
        float scale = Main.UIScale;
        if (wreathPosition == RoAClientConfig.WreathPositions.Player) {
            scale = 1f;
        }
        else {
            position.Y += 24f * (Main.UIScale - 1f);
        }
        if (reversedGravity) {
            position = Main.ReverseGravitySupport(position);
        }
        float rotation = 0f;
        if (reversedGravity) {
            rotation = MathHelper.Pi;
        }
        SpriteEffects effects = SpriteEffects.None;
        if (reversedGravity) {
            effects = SpriteEffects.FlipHorizontally;
            position.Y -= 1f;
        }
        if (reversedGravity) {
            position.Y += 1f;
        }
        if (formHandler.IsInADruidicForm && currentForm != null && currentForm.BaseForm.IsDrawing) {
            position += currentForm.BaseForm.SetWreathOffset2(player);
        }

        position += new Vector2(defaultResources ? -4 : 0, defaultResources ? 18f : 0f);
        if (horizontalBarsWithText) {
            position.Y += 2f;
        }

        spriteBatch.Draw(mainTexture, position, frame, color, rotation, origin, scale, effects, 0f);

        IsHoveringUI = false;
        Matrix matrix2 = Matrix.Invert(Main.GameViewMatrix.ZoomMatrix);
        Vector2 position3 = Main.ReverseGravitySupport(Main.MouseScreen);
        Vector2.Transform(Main.screenPosition, matrix2);
        Vector2 v2 = Vector2.Transform(position3, matrix2);
        float startX = (float)(500 + UI_ScreenAnchorX);
        float endX = (float)(500 + width + UI_ScreenAnchorX);
        float startY = 15f;
        float endY = (float)(15f + height);
        if (wreathPosition == RoAClientConfig.WreathPositions.Player) {
            startX = position.X - width / 2;
            endX = startX + width;
            startY = position.Y - height / 2;
            endY = startY + height;
        }
        else {
            v2 = Main.MouseScreen;
        }
        if (v2.X > startX && v2.X < endX &&
            v2.Y > startY && v2.Y < endY) {
            if (!Main.mouseText) {
                player.cursorItemIconEnabled = false;
                string text = "[kw/n:" + stats.CurrentResource + "]" + "/" + stats.TotalResource;
                Main.instance.MouseTextHackZoom(text);
                Main.mouseText = true;
                IsHoveringUI = true;
            }
        }

        if (stats.IsAetherWreath) {
            color = WreathHandler.AetherBaseColor;
        }

        // fill
        frame.X = stats.IsAetherWreath ? 54 : 8;
        frame.Y = stats.IsPhoenixWreath || stats.IsAetherWreath ? 104 : 56;
        frame.Width = 28;
        float progress = MathHelper.Clamp(stats.ActualProgress2 * 1.1f, 0f, 1f);
        frame.Height = (int)(28 * progress);
        origin = new Vector2(28, 28) / 2f;
        position -= Vector2.One * 2f;
        position.X += 1f;
        position += new Vector2(1f, 2f);
        spriteBatch.Draw(mainTexture, position, frame, color, MathHelper.Pi + rotation, origin, scale, effects, 0f);

        // full icon
        if (stats.IsFull1) {
            frame.X = stats.IsAetherWreath ? 190 : 144;
            frame.Y = stats.IsPhoenixWreath || stats.IsAetherWreath ? 104 : 56;
            frame.Width = 28;
            frame.Height = 28;
            Vector2 origin2 = origin;
            origin = frame.Size() / 2f;
            Vector2 position2 = position;
            Vector2 offset = Vector2.Zero;
            //if (reversedGravity) {
            //    offset.Y -= 32f;
            //}
            spriteBatch.Draw(mainTexture, position2 + offset, frame, color, MathHelper.Pi + rotation, origin, scale, effects, 0f);
        }

        color = WreathHandler.SoulOfTheWoodsBaseColor;
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
            spriteBatch.Draw(mainTexture, position, frame, color, MathHelper.Pi + rotation, origin, scale, effects, 0f);
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
            spriteBatch.Draw(mainTexture, position2, frame, color, MathHelper.Pi + rotation, origin, scale, effects, 0f);
        }
    }
}
