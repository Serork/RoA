using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Druid.Wreath;
using RoA.Common.Players;
using RoA.Content.Items.Equipables.Wreaths;
using RoA.Core;
using RoA.Core.Data;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.InterfaceElements;

sealed class WreathDrawing() : InterfaceElement(RoA.ModName + ": Wreath", InterfaceScaleType.Game) {
    private const byte HORIZONTALFRAMECOUNT = 6;

    private static SpriteData _wreathSpriteData;
    private static float _factor;

    private Vector2 _oldPosition;

    private static Player Player => Main.LocalPlayer;
    private static WreathHandler Stats => Player.GetModPlayer<WreathHandler>();

    public static bool DrawingAmount { get; private set; }

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Active && layer.Name.Equals("Vanilla: Ingame Options"));

    public override void Load(Mod mod) {
        if (Main.dedServ) {
            return;
        }

        _wreathSpriteData = new SpriteData(ModContent.Request<Texture2D>(ResourceManager.Textures + "Wreath"), new SpriteFrame(HORIZONTALFRAMECOUNT, 3));
    }

    protected override bool DrawSelf() {
        Vector2 playerPosition = Utils.Floor(Player.Top + Vector2.UnitY * Player.gfxOffY);
        playerPosition.Y -= Player.fullRotationOrigin.Y;
        playerPosition.Y -= 12f;
        Vector2 position;
        bool breathUI = Player.breath < Player.breathMax || Player.lavaTime < Player.lavaMax;
        float offsetX = -_wreathSpriteData.FrameWidth / 2f + 2, offsetY = _wreathSpriteData.FrameHeight;
        playerPosition.X += offsetX;
        playerPosition.Y += breathUI ? (float)(-(float)offsetY * ((Player.breathMax - 1) / 200 + 1)) : -offsetY;

        if (Player.dead || Player.ghost || Player.ShouldNotDraw || !Stats.ShouldDrawItself) {
            _oldPosition = playerPosition;

            return true;
        }

        position = Vector2.Lerp(_oldPosition, playerPosition, 0.3f) - Main.screenPosition;
        _oldPosition = playerPosition;

        float progress = MathHelper.Clamp(Stats.ActualProgress2, 0f, 1f);
        //float alpha = Lighting.Brightness((int)Stats.LightingPosition.X / 16, (int)Stats.LightingPosition.Y / 16);
        //alpha = (alpha + 1f) / 2f;
        //Color color = Color.Multiply(Stats.DrawColor, alpha);
        Color color = Stats.BaseColor;
        float opacity = Math.Max(Utils.GetLerpValue(1f, 0.75f, progress, true), 0.7f);

        // dark border
        SpriteData wreathSpriteData = _wreathSpriteData;
        wreathSpriteData.Rotation = MathHelper.Pi;
        wreathSpriteData.Color = color * opacity;
        wreathSpriteData.VisualPosition = position;
        wreathSpriteData.DrawSelf();
        
        // filling
        SpriteData wreathSpriteData2 = wreathSpriteData.Framed((byte)(0 + Stats.IsPhoenixWreath.ToInt()), 1);
        int frameOffsetY = 0;
        int frameHeight = wreathSpriteData2.FrameHeight + frameOffsetY;
        void drawFilling(Rectangle sourceRectangle, Vector2? offset = null, float opacity = 1f) {
            wreathSpriteData2.VisualPosition = position - Vector2.UnitY * frameOffsetY;
            wreathSpriteData2.Color = color * opacity;
            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
        }
        Rectangle sourceRectangle = new(wreathSpriteData2.FrameX, wreathSpriteData2.FrameY + frameOffsetY, wreathSpriteData2.FrameWidth, (int)(frameHeight * progress));
        bool soulOfTheWoods = Stats.SoulOfTheWoods;
        float progress2 = Stats.ActualProgress2 - 1f;
        float value = progress2;
        float progress3 = 1f - MathHelper.Clamp(progress2 * 0.7f, 0f, 0.7f);
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

        progress3 = 1f - MathHelper.Clamp(progress2 * 2.25f, 0f, 1f);
        // effect
        void drawEffect(float progress, Rectangle sourceRectangle, Vector2? offset = null, float opacity = 1f, byte frameX = 3, byte frameY = 1) {
            //color = Color.Multiply(Stats.DrawColor, alpha);
            color = Stats.BaseColor;
            color *= 1.4f;
            color.A = 80;
            color *= opacity;
            opacity = progress < 1f ? Ease.CubeInOut(progress) : 1f;
            float factor = Ease.CircOut((float)(Main.GlobalTimeWrappedHourly % 1.0) / 7f) * Math.Min(opacity > 0.75f ? 0.75f - opacity * (1f - opacity) : 0.925f, 0.925f);
            if (progress > 0f && progress < 0.5f) {
                factor *= 0.1f;
            }
            _factor = MathHelper.Lerp(_factor, factor, _factor < factor ? 0.1f : 0.025f);
            factor = _factor * Stats.PulseIntensity;
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
            drawEffect(progress, sourceRectangle, opacity: progress3, frameX: (byte)(3 + Stats.IsPhoenixWreath.ToInt()), frameY: 1);
        }
        if (soulOfTheWoods) {
            drawEffect(progress2, sourceRectangle2, offset, frameY: 2);
        }

        // adapted vanilla
        Microsoft.Xna.Framework.Rectangle mouseRectangle = new Microsoft.Xna.Framework.Rectangle((int)((float)Main.mouseX + Main.screenPosition.X), (int)((float)Main.mouseY + Main.screenPosition.Y), 1, 1);
        if (Player.gravDir == -1f)
            mouseRectangle.Y = (int)Main.screenPosition.Y + Main.screenHeight - Main.mouseY;
        Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle((int)((double)wreathSpriteData.VisualPosition.X + Main.screenPosition.X), (int)(wreathSpriteData.VisualPosition.Y + Main.screenPosition.Y), (int)(29 * Main.UIScale), (int)(29 * Main.UIScale));
        if (!Main.mouseText && mouseRectangle.Intersects(value2)) {
            Player.cursorItemIconEnabled = false;

            string text2 = "[kw/n:" + Stats.CurrentResource + "]" + "/" + Stats.TotalResource;

            Main.instance.MouseTextHackZoom(text2);
            Main.mouseText = true;
            DrawingAmount = true;
        }
        else {
            DrawingAmount = false;
        }

        return true;
    }
}
