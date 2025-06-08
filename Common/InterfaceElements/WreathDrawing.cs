﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;

using RoA.Common.Cache;
using RoA.Common.Configs;
using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Content.Forms;
using RoA.Core;
using RoA.Core.Data;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.InterfaceElements;

sealed class WreathDrawing : PlayerDrawLayer {
    internal const byte HORIZONTALFRAMECOUNT = 6;

    internal static SpriteData _wreathSpriteData;

    internal sealed class ValuesStorage : ModPlayer {
        public float MainFactor;
        public Vector2 OldPosition;
    }

    public static bool JustDrawn { get; internal set; }

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _wreathSpriteData = new SpriteData(ModContent.Request<Texture2D>(ResourceManager.Textures + "Wreath"), new SpriteFrame(HORIZONTALFRAMECOUNT, 3));
    }

    public override bool IsHeadLayer => false;

    public override Position GetDefaultPosition() => PlayerDrawLayers.AfterLastVanillaLayer;

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        var config = ModContent.GetInstance<RoAClientConfig>();
        if (config.WreathDrawingMode != RoAClientConfig.WreathDrawingModes.Normal &&
            config.WreathDrawingMode != RoAClientConfig.WreathDrawingModes.Normal2) {
            return;
        }

        bool shouldDrawNearHPBar = config.WreathPosition == RoAClientConfig.WreathPositions.Health;
        if (shouldDrawNearHPBar) {
            return;
        }

        Player player = drawInfo.drawPlayer;
        if (drawInfo.shadow != 0f || !player.active || player.whoAmI != Main.myPlayer) {
            return;
        }

        WreathHandler stats = player.GetModPlayer<WreathHandler>();
        Vector2 playerPosition = Utils.Floor(new Vector2((int)(drawInfo.Position.X + (float)(drawInfo.drawPlayer.width / 2)),
            (int)(drawInfo.Position.Y + (float)drawInfo.drawPlayer.height - 40f)));
        var formHandler = player.GetModPlayer<BaseFormHandler>();
        var currentForm = formHandler.CurrentForm;
        if (formHandler.IsInADruidicForm && currentForm != null) {
            playerPosition += currentForm.BaseForm.WreathOffset;
        }
        playerPosition.Y -= 12f;
        Vector2 position;
        bool breathUI = player.breath < player.breathMax || player.lavaTime < player.lavaMax;
        float offsetX = -_wreathSpriteData.FrameWidth / 2f + 2,
              offsetY = _wreathSpriteData.FrameHeight;
        playerPosition.X += offsetX;
        var storage = player.GetModPlayer<ValuesStorage>();
        ref Vector2 oldPosition = ref storage.OldPosition;
        playerPosition.Y += breathUI ? (float)(-(float)offsetY * ((player.breathMax - 1) / 200 + 1)) : -offsetY;
        if (player.dead || player.ghost || player.ShouldNotDraw || !stats.ShouldDrawItself) {
            oldPosition = playerPosition;

            return;
        }


        SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

        position = oldPosition/*Vector2.Lerp(_oldPosition, playerPosition, 0.3f)*/;
        position -= Main.screenPosition;
        position -= new Vector2(1f, 0f);
        var phoenixHandler = player.GetModPlayer<LilPhoenixForm.LilPhoenixFormHandler>();
        float rotation = drawInfo.rotation + MathHelper.Pi;
        bool flag2 = phoenixHandler._dashed || phoenixHandler._dashed || phoenixHandler._isPreparing || player.sleeping.isSleeping;
        if (flag2) {
            rotation = MathHelper.Pi;
        }
        oldPosition = playerPosition;
        if (!flag2) {
            Vector2 vector = drawInfo.Position + drawInfo.rotationOrigin;
            Matrix matrix = Matrix.CreateRotationZ(drawInfo.rotation);
            Vector2 newPosition = oldPosition - vector;
            newPosition = Vector2.Transform(newPosition, matrix);
            oldPosition.X = (newPosition + vector).X;
            float progress4 = Math.Abs(drawInfo.rotation) / MathHelper.Pi;
            float offsetY2 = progress4 * (player.height / 2f + 10f);
            oldPosition.Y += offsetY2;
        }

        WreathDrawing2.DrawText(Vector2.UnitY * 15f);

        float progress = MathHelper.Clamp(stats.ActualProgress2, 0f, 1f);
        //float alpha = Lighting.Brightness((int)Stats.LightingPosition.X / 16, (int)Stats.LightingPosition.Y / 16);
        //alpha = (alpha + 1f) / 2f;
        //Color color = Color.Multiply(Stats.DrawColor, alpha);
        Color color = stats.BaseColor;
        float opacity = Math.Max(Utils.GetLerpValue(1f, 0.75f, progress, true), 0.7f);
        //position = position.Floor();
        // dark border
        SpriteData wreathSpriteData = _wreathSpriteData;
        wreathSpriteData.Color = color * opacity;
        wreathSpriteData.VisualPosition = position;
        wreathSpriteData.Rotation = rotation;
        wreathSpriteData.DrawSelf();
        // filling
        SpriteData wreathSpriteData2 = wreathSpriteData.Framed((byte)(0 + stats.IsPhoenixWreath.ToInt()), 1);
        int frameOffsetY = 0;
        int frameHeight = wreathSpriteData2.FrameHeight + frameOffsetY;
        void drawFilling(Rectangle sourceRectangle, Vector2? offset = null, float opacity = 1f) {
            wreathSpriteData2.VisualPosition = position - Vector2.UnitY * frameOffsetY;
            wreathSpriteData2.Color = color * opacity;
            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
        }
        Rectangle sourceRectangle = new(wreathSpriteData2.FrameX, wreathSpriteData2.FrameY + frameOffsetY, wreathSpriteData2.FrameWidth, (int)(frameHeight * progress));
        bool soulOfTheWoods = stats.SoulOfTheWoods;
        float progress2 = stats.ActualProgress2 - 1f;
        float value = progress2;
        float mult = 0.5f; // second transition mult
        float progress3 = 1f - MathHelper.Clamp(progress2 * mult, 0f, mult);
        Rectangle sourceRectangle2 = sourceRectangle;
        sourceRectangle2.X = 0;
        sourceRectangle2.Y += frameHeight + 2 - frameOffsetY;
        sourceRectangle2.Height = (int)(frameHeight * progress2);
        Vector2 offset = Vector2.Zero;
        float value3 = progress3 * (1f - Utils.GetLerpValue(0.6f, 1f, progress2, true));
        bool flag = true;
        if (soulOfTheWoods && progress2 > 0.9f) {
            flag = false;
        }
        if (flag) {
            drawFilling(sourceRectangle, opacity: value3);
        }
        if (soulOfTheWoods) {
            drawFilling(sourceRectangle2, offset);
        }
        float mult2 = 2.25f; // first transition mult
        progress3 = 1f - MathHelper.Clamp(progress2 * mult2, 0f, 1f);
        // effect
        void drawEffect(float progress, Rectangle sourceRectangle, Vector2? offset = null, float opacity = 1f, byte frameX = 3, byte frameY = 1) {
            //color = Color.Multiply(Stats.DrawColor, alpha);
            color = stats.BaseColor;
            color *= 1.4f;
            color.A = 80;
            color *= opacity;
            opacity = progress < 1f ? Ease.CubeInOut(progress) : 1f;
            float factor = Ease.CircOut((float)(Main.GlobalTimeWrappedHourly % 1.0) / 7f) * Math.Min(opacity > 0.75f ? 0.75f - opacity * (1f - opacity) : 0.925f, 0.925f);
            if (progress > 0f && progress < 0.5f) {
                factor *= 0.1f;
            }
            ref float mainFactor = ref storage.MainFactor;
            mainFactor = MathHelper.Lerp(mainFactor, factor, mainFactor < factor ? 0.1f : 0.025f);
            factor = mainFactor * stats.PulseIntensity;
            wreathSpriteData2.Color = color * factor * opacity * 2f;
            wreathSpriteData2.Scale = factor + 0.475f;
            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
            wreathSpriteData2.Scale += 0.13f * progress * 2f;
            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
            SpriteData wreathSpriteData3 = wreathSpriteData.Framed(frameX, frameY);
            opacity = Math.Min(progress * 1.15f, 0.7f);
            wreathSpriteData3.Color = color * opacity;
            wreathSpriteData3.DrawSelf(offset: offset);
        }
        if (flag) {
            drawEffect(progress, sourceRectangle, opacity: progress3, frameX: (byte)(3 + stats.IsPhoenixWreath.ToInt()), frameY: 1);
        }
        if (soulOfTheWoods) {
            drawEffect(progress2, sourceRectangle2, offset, frameY: 2);
        }

        // adapted vanilla
        Microsoft.Xna.Framework.Rectangle mouseRectangle = new Microsoft.Xna.Framework.Rectangle((int)((float)Main.mouseX + Main.screenPosition.X), (int)((float)Main.mouseY + Main.screenPosition.Y), 1, 1);
        //if (player.gravDir == -1f)
        //    mouseRectangle.Y = (int)Main.screenPosition.Y + Main.screenHeight - Main.mouseY;
        Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle((int)((double)wreathSpriteData.VisualPosition.X + Main.screenPosition.X), (int)(wreathSpriteData.VisualPosition.Y + Main.screenPosition.Y), (int)(29 * Main.UIScale), (int)(29 * Main.UIScale));
        if (!Main.mouseText && mouseRectangle.Intersects(value2)) {
            player.cursorItemIconEnabled = false;

            string text2 = "[kw/n:" + stats.CurrentResource + "]" + "/" + stats.TotalResource;

            Main.instance.MouseTextHackZoom(text2);
            Main.mouseText = true;
            JustDrawn = true;
        }
        else {
            JustDrawn = false;
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(in snapshot);
    }
}

sealed class WreathDrawing2() : InterfaceElement(RoA.ModName + ": Wreath", InterfaceScaleType.Game) {
    private const byte HORIZONTALFRAMECOUNT = 6;

    private static Player Player => Main.LocalPlayer;
    private static WreathHandler Stats => Player.GetModPlayer<WreathHandler>();

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Active && layer.Name.Equals("Vanilla: Ingame Options"));

    public static void DrawText(Vector2 position) {
        Player player = Player;
        var stats = Stats;
        var config = ModContent.GetInstance<RoAClientConfig>();
        var wreathPosition = config.WreathPosition;
        if (config.WreathDrawingMode == RoAClientConfig.WreathDrawingModes.Normal2) {
            Vector2 vector3 = new Vector2(Main.screenWidth - 300 + 4, 15f);
            Vector2 vector = vector3 + new Vector2(-4f, 3f) + new Vector2(-48f, -18f);
            if (wreathPosition == RoAClientConfig.WreathPositions.Player) {
                vector3 = Main.ScreenSize.ToVector2() / 2f;
                vector3.Y -= player.height * 1.5f;

                vector = vector3 + new Vector2(-4f, 3f) + new Vector2(0f, -40f);
            }
            vector += position;
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
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.DrawString(FontAssets.MouseText.Value, text3, vector + new Vector2((0f - vector2.X) * 0.5f, 0f), textColor, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(FontAssets.MouseText.Value, (stats.CurrentResource + "/" + stats.TotalResource).ToString(), vector + new Vector2(vector2.X * 0.5f, 0f), textColor, 0f,
                new Vector2(FontAssets.MouseText.Value.MeasureString(stats.CurrentResource + "/" + stats.TotalResource).X, 0f), 1f, SpriteEffects.None, 0f);
        }
    }

    protected override bool DrawSelf() {
        var config = ModContent.GetInstance<RoAClientConfig>();
        if (config.WreathDrawingMode != RoAClientConfig.WreathDrawingModes.Normal &&
            config.WreathDrawingMode != RoAClientConfig.WreathDrawingModes.Normal2) {
            return true;
        }

        bool shouldDrawNearPlayer = config.WreathPosition == RoAClientConfig.WreathPositions.Player;
        if (shouldDrawNearPlayer) {
            return true;
        }

        Vector2 playerPosition = Utils.Floor(Player.Center - Vector2.UnitY * Player.height / 2f + Vector2.UnitY * Player.gfxOffY);

        int width = 44, height = 46;
        int UI_ScreenAnchorX = Main.screenWidth - 870;
        playerPosition = new Vector2(500 + UI_ScreenAnchorX + width / 2, 42f + height / 2);

        var _wreathSpriteData = WreathDrawing._wreathSpriteData;
        playerPosition.Y -= 12f;
        Vector2 position;
        bool breathUI = Player.breath < Player.breathMax || Player.lavaTime < Player.lavaMax;
        float offsetX = -_wreathSpriteData.FrameWidth / 2f + 2,
              offsetY = _wreathSpriteData.FrameHeight;
        playerPosition.X += offsetX;
        playerPosition.Y += breathUI ? (float)(-(float)offsetY * ((Player.breathMax - 1) / 200 + 1)) : -offsetY;

        var storage = Player.GetModPlayer<WreathDrawing.ValuesStorage>();
        ref Vector2 _oldPosition = ref storage.OldPosition;
        if (Player.ghost) {
            _oldPosition = playerPosition;

            return true;
        }

        if (!Player.mount.Active) {
            position = Vector2.Lerp(_oldPosition, playerPosition, 1f);
        }
        else {
            position = playerPosition;
        }
        //position -= Main.screenPosition;
        position -= new Vector2(/*4f*/1f, 0f);
        float rotation = MathHelper.Pi;
        _oldPosition = playerPosition;

        DrawText(Vector2.Zero);

        var stats = Stats;
        float progress = MathHelper.Clamp(stats.ActualProgress2, 0f, 1f);
        //float alpha = Lighting.Brightness((int)Stats.LightingPosition.X / 16, (int)Stats.LightingPosition.Y / 16);
        //alpha = (alpha + 1f) / 2f;
        //Color color = Color.Multiply(Stats.DrawColor, alpha);
        Color color = stats.BaseColor;
        float opacity = Math.Max(Utils.GetLerpValue(1f, 0.75f, progress, true), 0.7f);
        //position = position.Floor();
        // dark border
        SpriteData wreathSpriteData = _wreathSpriteData;
        wreathSpriteData.Color = color * opacity;
        wreathSpriteData.VisualPosition = position;
        wreathSpriteData.Rotation = rotation;
        wreathSpriteData.DrawSelf();
        // filling
        SpriteData wreathSpriteData2 = wreathSpriteData.Framed((byte)(0 + stats.IsPhoenixWreath.ToInt()), 1);
        int frameOffsetY = 0;
        int frameHeight = wreathSpriteData2.FrameHeight + frameOffsetY;
        void drawFilling(Rectangle sourceRectangle, Vector2? offset = null, float opacity = 1f) {
            wreathSpriteData2.VisualPosition = position - Vector2.UnitY * frameOffsetY;
            wreathSpriteData2.Color = color * opacity;
            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
        }
        Rectangle sourceRectangle = new(wreathSpriteData2.FrameX, wreathSpriteData2.FrameY + frameOffsetY, wreathSpriteData2.FrameWidth, (int)(frameHeight * progress));
        bool soulOfTheWoods = stats.SoulOfTheWoods;
        float progress2 = stats.ActualProgress2 - 1f;
        float value = progress2;
        float mult = 0.5f; // second transition mult
        float progress3 = 1f - MathHelper.Clamp(progress2 * mult, 0f, mult);
        Rectangle sourceRectangle2 = sourceRectangle;
        sourceRectangle2.X = 0;
        sourceRectangle2.Y += frameHeight + 2 - frameOffsetY;
        sourceRectangle2.Height = (int)(frameHeight * progress2);
        Vector2 offset = Vector2.Zero;
        float value3 = progress3 * (1f - Utils.GetLerpValue(0.6f, 1f, progress2, true));
        bool flag = true;
        if (soulOfTheWoods && progress2 > 0.9f) {
            flag = false;
        }
        if (flag) {
            drawFilling(sourceRectangle, opacity: value3);
        }
        if (soulOfTheWoods) {
            drawFilling(sourceRectangle2, offset);
        }
        float mult2 = 2.25f; // first transition mult
        progress3 = 1f - MathHelper.Clamp(progress2 * mult2, 0f, 1f);
        // effect
        void drawEffect(float progress, Rectangle sourceRectangle, Vector2? offset = null, float opacity = 1f, byte frameX = 3, byte frameY = 1) {
            //color = Color.Multiply(Stats.DrawColor, alpha);
            color = stats.BaseColor;
            color *= 1.4f;
            color.A = 80;
            color *= opacity;
            opacity = progress < 1f ? Ease.CubeInOut(progress) : 1f;
            float factor = Ease.CircOut((float)(Main.GlobalTimeWrappedHourly % 1.0) / 7f) * Math.Min(opacity > 0.75f ? 0.75f - opacity * (1f - opacity) : 0.925f, 0.925f);
            if (progress > 0f && progress < 0.5f) {
                factor *= 0.1f;
            }
            ref float mainFactor = ref storage.MainFactor;
            mainFactor = MathHelper.Lerp(mainFactor, factor, mainFactor < factor ? 0.1f : 0.025f);
            factor = mainFactor * stats.PulseIntensity;
            wreathSpriteData2.Color = color * factor * opacity * 2f;
            wreathSpriteData2.Scale = factor + 0.475f;
            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
            wreathSpriteData2.Scale += 0.13f * progress * 2f;
            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
            SpriteData wreathSpriteData3 = wreathSpriteData.Framed(frameX, frameY);
            opacity = Math.Min(progress * 1.15f, 0.7f);
            wreathSpriteData3.Color = color * opacity;
            wreathSpriteData3.DrawSelf(offset: offset);
        }
        if (flag) {
            drawEffect(progress, sourceRectangle, opacity: progress3, frameX: (byte)(3 + stats.IsPhoenixWreath.ToInt()), frameY: 1);
        }
        if (soulOfTheWoods) {
            drawEffect(progress2, sourceRectangle2, offset, frameY: 2);
        }

        var player = Player;
        // adapted vanilla
        Microsoft.Xna.Framework.Rectangle mouseRectangle = new Microsoft.Xna.Framework.Rectangle((int)((float)Main.mouseX + Main.screenPosition.X), (int)((float)Main.mouseY + Main.screenPosition.Y), 1, 1);
        if (player.gravDir == -1f)
            mouseRectangle.Y = (int)Main.screenPosition.Y + Main.screenHeight - Main.mouseY;
        Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle((int)((double)wreathSpriteData.VisualPosition.X + Main.screenPosition.X), (int)(wreathSpriteData.VisualPosition.Y + Main.screenPosition.Y), (int)(29 * Main.UIScale), (int)(29 * Main.UIScale));
        if (!Main.mouseText && mouseRectangle.Intersects(value2)) {
            player.cursorItemIconEnabled = false;

            string text2 = "[kw/n:" + stats.CurrentResource + "]" + "/" + stats.TotalResource;

            Main.instance.MouseTextHackZoom(text2);
            Main.mouseText = true;
            WreathDrawing.JustDrawn = true;
        }
        else {
            WreathDrawing.JustDrawn = false;
        }

        return true;
    }
}
