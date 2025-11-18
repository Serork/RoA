using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;

using RoA.Common.Cache;
using RoA.Common.Configs;
using RoA.Common.Druid.Wreath;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

using static RoA.Common.ShaderLoader;

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

        WreathHandler stats = player.GetWreathHandler();
        Vector2 playerPosition = Utils.Floor(new Vector2((int)(drawInfo.Position.X + (float)(drawInfo.drawPlayer.width / 2)),
            (int)(drawInfo.Position.Y + ((float)drawInfo.drawPlayer.height - 40f) * player.gravDir)));
        var formHandler = player.GetFormHandler();
        var currentForm = formHandler.CurrentForm;
        if (formHandler.IsInADruidicForm && currentForm != null && currentForm.BaseForm.IsDrawing) {
            playerPosition += currentForm.BaseForm.SetWreathOffset(player);
        }
        playerPosition.Y += stats.YAdjustAmountInPixels * player.gravDir;
        playerPosition.Y -= 12f * player.gravDir;
        if (player.gravDir < 0) {
            playerPosition.Y += 10f;
        }
        Vector2 position;
        bool breathUI = player.breath < player.breathMax || player.lavaTime < player.lavaMax;
        float offsetX = -_wreathSpriteData.FrameWidth / 2f + 2,
              offsetY = _wreathSpriteData.FrameHeight * player.gravDir;
        playerPosition.X += offsetX;
        var storage = player.GetModPlayer<ValuesStorage>();
        ref Vector2 oldPosition = ref storage.OldPosition;
        playerPosition.Y += breathUI ? (float)(-(float)offsetY * ((player.breathMax - 1) / 200 + 1)) : -offsetY;
        if (/*player.dead || */player.ghost || player.ShouldNotDraw || !stats.ShouldDrawItself) {
            oldPosition = playerPosition;

            return;
        }

        SpriteBatch batch = Main.spriteBatch;
        SpriteBatchSnapshot snapshot = batch.CaptureSnapshot();

        var phoenixHandler = player.GetFormHandler();
        bool shouldRotateWithPlayer = ModContent.GetInstance<RoAClientConfig>().RotateWreathWithPlayer;
        float baseRotation = !shouldRotateWithPlayer ? 0f : drawInfo.rotation;
        float rotation = baseRotation + MathHelper.Pi;
        bool flag2 = phoenixHandler.Dashed || phoenixHandler.Dashed || phoenixHandler.IsPreparing || player.sleeping.isSleeping;
        if (stats.LockWreathPosition) {
            flag2 = true;
        }
        if (flag2) {
            rotation = MathHelper.Pi;
        }
        oldPosition = playerPosition;
        if (!flag2) {
            Vector2 vector = drawInfo.Position + drawInfo.rotationOrigin;
            Matrix matrix = Matrix.CreateRotationZ(baseRotation);
            Vector2 newPosition = oldPosition - vector;
            newPosition = Vector2.Transform(newPosition, matrix);
            oldPosition.X = (newPosition + vector).X;
            float progress4 = Math.Abs(baseRotation) / MathHelper.Pi;
            float offsetY2 = progress4 * (player.height / 2f + 10f);
            oldPosition.Y += offsetY2;
        }
        position = oldPosition/*Vector2.Lerp(_oldPosition, playerPosition, 0.3f)*/;
        position -= Main.screenPosition;
        position -= new Vector2(1f, 0f);

        WreathDrawing2.DrawText(Vector2.UnitY * 15f);

        DrawWreath(player, batch, snapshot, position, rotation);
    }

    public static void DrawWreath(Player player, SpriteBatch batch, SpriteBatchSnapshot snapshot, Vector2 position, float rotation, bool applyGravityForText = false) {
        WreathHandler stats = player.GetWreathHandler();
        var storage = player.GetModPlayer<ValuesStorage>();
        float progress = MathHelper.Clamp(stats.ActualProgress2, 0f, 1f);
        //float alpha = Lighting.Brightness((int)Stats.LightingPosition.X / 16, (int)Stats.LightingPosition.Y / 16);
        //alpha = (alpha + 1f) / 2f;
        //DrawColor color = DrawColor.Multiply(Stats.DrawColor, alpha);
        Color color = WreathHandler.BaseColor;
        if (stats.IsAetherWreath) {
            color = WreathHandler.AetherBaseColor;
        }
        float opacity = Math.Max(Utils.GetLerpValue(1f, 0.75f, progress, true), 0.7f);
        //position = position.Floor();
        // dark border
        SpriteData wreathSpriteData = _wreathSpriteData;
        if (!applyGravityForText) {
            if (player.gravDir < 0) {
                wreathSpriteData.Effects = SpriteEffects.FlipVertically;
            }
            else {
                wreathSpriteData.Effects = SpriteEffects.None;
            }
        }
        batch.End();
        batch.Begin(SpriteSortMode.Deferred, snapshot.blendState, SamplerState.PointClamp, snapshot.depthStencilState, snapshot.rasterizerState, snapshot.effect, snapshot.transformationMatrix);
        wreathSpriteData.Color = color * opacity;
        wreathSpriteData.VisualPosition = position;
        wreathSpriteData.Rotation = rotation;
        wreathSpriteData.DrawSelf();
        batch.Begin(in snapshot, true);
        float gravOffset = 0f;
        // filling
        byte x = (byte)(0 + stats.IsAetherWreath.ToInt() * 2 + stats.IsPhoenixWreath.ToInt());
        byte y = 1;
        SpriteData wreathSpriteData2 = wreathSpriteData.Framed(x, y);
        int frameOffsetY = 0;
        int frameHeight = wreathSpriteData2.FrameHeight + frameOffsetY;
        VerticalAppearanceShader.Min = 0.5f;
        bool soulOfTheWoods = stats.SoulOfTheWoods;
        VerticalAppearanceShader.Size2 = 0.025f * (1f - Utils.GetLerpValue(0.8f, 1f, progress, true));
        Rectangle sourceRectangle = new(wreathSpriteData2.FrameX, wreathSpriteData2.FrameY + frameOffsetY, wreathSpriteData2.FrameWidth, frameHeight);
        float progress2 = stats.ActualProgress2 - 1f;
        float value = progress2;
        float mult = 0.5f; // second transition mult
        float progress3 = 1f - MathHelper.Clamp(progress2 * mult, 0f, mult);
        VerticalAppearanceShader.Max = soulOfTheWoods ? (0.875f + 0.125f * Utils.Remap(progress2, 0f, 0.1f, 0f, 1f, true)) : 0.875f;
        void drawFilling(Rectangle sourceRectangle, float progress, float progress2, Vector2? offset = null, float opacity = 1f) {
            batch.End();
            batch.Begin(SpriteSortMode.Immediate, snapshot.blendState, SamplerState.PointClamp, snapshot.depthStencilState, snapshot.rasterizerState, snapshot.effect, snapshot.transformationMatrix);
            VerticalAppearanceShader.Progress = progress2;
            wreathSpriteData2.VisualPosition = position - Vector2.UnitY * frameOffsetY;
            wreathSpriteData2.Color = color * opacity;
            VerticalAppearanceShader.DrawColor = wreathSpriteData2.Color;
            VerticalAppearanceShader.Effect?.CurrentTechnique.Passes[0].Apply();
            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
            batch.Begin(in snapshot, true);
        }
        Rectangle sourceRectangle2 = sourceRectangle;
        sourceRectangle2.X = 0;
        sourceRectangle2.Y += frameHeight + 2 - frameOffsetY;
        sourceRectangle2.Height = (int)(frameHeight * 1f/*progress2*/);
        Vector2 offset = Vector2.Zero;
        float value3 = progress3 * (1f - Utils.GetLerpValue(0.6f, 1f, progress2, true));
        bool flag = true;
        if (soulOfTheWoods && progress2 > 0.9f) {
            flag = false;
        }
        if (flag) {
            drawFilling(sourceRectangle, progress, 1f - Utils.Remap(progress, 0f, 1f, 0.35f, 0.675f, true), opacity: value3);
        }
        if (soulOfTheWoods) {
            color = WreathHandler.SoulOfTheWoodsBaseColor;
            float filling2Progress = 1f - Utils.Remap(progress2, 0f, 1f, 0.725f, 1f, true);
            drawFilling(sourceRectangle2, progress2, filling2Progress, offset);
        }
        float mult2 = 5f; // first transition mult
        progress3 = 1f - MathHelper.Clamp(progress2 * mult2, 0f, 1f);
        // effect
        void drawEffect(float progress, float progress2, Rectangle sourceRectangle, Vector2? offset = null, float opacity = 1f, byte frameX = 3, byte frameY = 1,
            bool soulOfTheWoods = false) {
            batch.End();
            batch.Begin(SpriteSortMode.Immediate, snapshot.blendState, SamplerState.AnisotropicClamp, snapshot.depthStencilState, snapshot.rasterizerState, snapshot.effect, snapshot.transformationMatrix);
            //color = DrawColor.Multiply(Stats.DrawColor, alpha);
            color = WreathHandler.BaseColor;
            if (stats.IsAetherWreath) {
                color = WreathHandler.AetherBaseColor;
            }
            if (soulOfTheWoods) {
                color = WreathHandler.SoulOfTheWoodsBaseColor;
            }
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
            VerticalAppearanceShader.Progress = progress2;
            wreathSpriteData2.Color = color * factor * opacity * 2f;
            wreathSpriteData2.Scale = factor + 0.475f;
            VerticalAppearanceShader.DrawColor = wreathSpriteData2.Color;
            VerticalAppearanceShader.Effect?.CurrentTechnique.Passes[0].Apply();
            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
            wreathSpriteData2.Scale += 0.13f * progress * 2f;
            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
            SpriteData wreathSpriteData3 = wreathSpriteData.Framed(frameX, frameY);
            opacity = Math.Min(progress * 1.15f, 0.7f);
            wreathSpriteData3.Color = color * opacity;
            VerticalAppearanceShader.DrawColor = wreathSpriteData3.Color;
            VerticalAppearanceShader.Effect?.CurrentTechnique.Passes[0].Apply();
            wreathSpriteData3.VisualPosition.Y += gravOffset;
            wreathSpriteData3.DrawSelf(offset: offset);
            batch.Begin(in snapshot, true);
        }
        if (flag) {
            byte x2 = (byte)(3 + stats.IsAetherWreath.ToInt() * 2 + stats.IsPhoenixWreath.ToInt());
            byte y2 = 1;
            drawEffect(progress, 1f - Utils.Remap(progress, 0f, 1f, 0.35f, 0.675f, true), sourceRectangle, opacity: progress3, 
                frameX: x2, frameY: y2);
        }
        if (soulOfTheWoods) {
            float filling2Progress = 1f - Utils.Remap(progress2, 0f, 1f, 0.725f, 1f, true);
            drawEffect(progress2, filling2Progress, sourceRectangle2, offset, frameY: 2);
        }

        // on hit special visuals 
        if (player.GetDruidStats().IsDruidsEyesEffectActive) {
            batch.DrawWithSnapshot(() => {
                wreathSpriteData = wreathSpriteData.Framed(1, 0);
                int fluff = WreathHandler.GETHITEFFECTTIME / 4;
                float opacity = Utils.GetLerpValue(0, fluff, stats.GetHitTimer, true) * Utils.GetLerpValue(WreathHandler.GETHITEFFECTTIME, WreathHandler.GETHITEFFECTTIME - fluff, stats.GetHitTimer, true);
                wreathSpriteData.Color = Color.White * opacity;
                wreathSpriteData.VisualPosition = position;
                wreathSpriteData.Rotation = rotation;
                wreathSpriteData.Scale = 1f + 0.1f * opacity;
                ShaderLoader.WreathShaderData.UseSaturation(opacity * 2f);
                ShaderLoader.WreathShaderData.UseColor(new Color(57, 197, 71));
                ShaderLoader.WreathShaderData.Apply(player, wreathSpriteData.AsDrawData());
                wreathSpriteData.DrawSelf();
            }, sortMode: SpriteSortMode.Immediate);
        }

        // adapted vanilla
        Microsoft.Xna.Framework.Rectangle mouseRectangle = new Microsoft.Xna.Framework.Rectangle((int)((float)Main.mouseX + Main.screenPosition.X), (int)((float)Main.mouseY + Main.screenPosition.Y), 1, 1);
        if (applyGravityForText && player.gravDir == -1f)
            mouseRectangle.Y = (int)Main.screenPosition.Y + Main.screenHeight - Main.mouseY;
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
    }
}

sealed class WreathDrawing2() : InterfaceElement(RoA.ModName + ": Wreath", InterfaceScaleType.Game) {
    private const byte HORIZONTALFRAMECOUNT = 6;

    private static Player Player => Main.LocalPlayer;
    private static WreathHandler Stats => Player.GetWreathHandler();

    public override void Load(Mod mod) {
        On_PlayerResourceSetsManager.Draw += On_PlayerResourceSetsManager_Draw;
    }

    private void On_PlayerResourceSetsManager_Draw(On_PlayerResourceSetsManager.orig_Draw orig, PlayerResourceSetsManager self) {
        orig(self);

        var config = ModContent.GetInstance<RoAClientConfig>();
        if (config.WreathDrawingMode != RoAClientConfig.WreathDrawingModes.Normal &&
            config.WreathDrawingMode != RoAClientConfig.WreathDrawingModes.Normal2) {
            return;
        }

        bool shouldDrawNearPlayer = config.WreathPosition == RoAClientConfig.WreathPositions.Player;
        if (shouldDrawNearPlayer) {
            return;
        }

        Vector2 playerPosition = Utils.Floor(Player.Center - Vector2.UnitY * Player.height / 2f + Vector2.UnitY * Player.gfxOffY);

        bool horizontalBarsWithText = Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBarsWithText";
        bool normal2 = config.WreathDrawingMode == RoAClientConfig.WreathDrawingModes.Normal2;

        int width = 44, height = 46;
        bool defaultResources = Main.ResourceSetsManager.ActiveSetKeyName == "Default";
        int UI_ScreenAnchorX = Main.screenWidth - (870 + (defaultResources ? 4 : 0));
        playerPosition = new Vector2(500 + UI_ScreenAnchorX + width / 2, 42f + (defaultResources ? Player.statLifeMax <= 100 ? 7f : 17f : 0f) + height / 2);
        if (horizontalBarsWithText) {
            playerPosition.Y += 2f;
        }

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

            return;
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

        if (!normal2 && !defaultResources) {
            position.Y -= 6f;
        }

        SpriteBatch batch = Main.spriteBatch;
        SpriteBatchSnapshot snapshot = batch.CaptureSnapshot();

        //position -= new Vector2(350f, -54f) * (scale - 1f);

        DrawText(Vector2.Zero);

        WreathDrawing.DrawWreath(Player, batch, snapshot, position, rotation, true);
        //float progress = MathHelper.Clamp(stats.ActualProgress2, 0f, 1f);
        ////float alpha = Lighting.Brightness((int)Stats.LightingPosition.X / 16, (int)Stats.LightingPosition.Y / 16);
        ////alpha = (alpha + 1f) / 2f;
        ////DrawColor color = DrawColor.Multiply(Stats.DrawColor, alpha);
        //Color color = stats.BaseColor;
        //float opacity = Math.Max(Utils.GetLerpValue(1f, 0.75f, progress, true), 0.7f);
        ////position = position.Floor();
        //// dark border
        //SpriteData wreathSpriteData = _wreathSpriteData;
        //wreathSpriteData.Color = color * opacity;
        //wreathSpriteData.VisualPosition = position;
        //wreathSpriteData.Rotation = rotation;
        //wreathSpriteData.DrawSelf();
        //// filling
        //SpriteData wreathSpriteData2 = wreathSpriteData.Framed((byte)(0 + stats.IsPhoenixWreath.ToInt()), 1);
        //int frameOffsetY = 0;
        //int frameHeight = wreathSpriteData2.FrameHeight + frameOffsetY;
        //VerticalAppearanceShader.Min = 0.5f;
        //bool soulOfTheWoods = stats.SoulOfTheWoods;
        //VerticalAppearanceShader.Size2 = 0.025f * (1f - Utils.GetLerpValue(0.8f, 1f, progress, true));
        //Rectangle sourceRectangle = new(wreathSpriteData2.FrameX, wreathSpriteData2.FrameY + frameOffsetY, wreathSpriteData2.FrameWidth, (int)(frameHeight * progress));
        //float progress2 = stats.ActualProgress2 - 1f;
        //float value = progress2;
        //float mult = 0.5f; // second transition mult
        //float progress3 = 1f - MathHelper.Clamp(progress2 * mult, 0f, mult);
        //VerticalAppearanceShader.Max = soulOfTheWoods ? (0.875f + 0.125f * Utils.Remap(progress2, 0f, 0.1f, 0f, 1f, true)) : 0.875f;
        //void drawFilling(Rectangle sourceRectangle, float progress, float progress2, Vector2? offset = null, float opacity = 1f) {
        //    batch.End();
        //    batch.Begin(SpriteSortMode.Immediate, snapshot.blendState, SamplerState.PointClamp, snapshot.depthStencilState, snapshot.rasterizerState, snapshot.effect, Main.GameViewMatrix.ZoomMatrix);
        //    VerticalAppearanceShader.ActualProgress = progress2;
        //    wreathSpriteData2.VisualPosition = position - Vector2.UnitY * frameOffsetY;
        //    wreathSpriteData2.Color = color * opacity;
        //    VerticalAppearanceShader.DrawColor = wreathSpriteData2.Color;
        //    VerticalAppearanceShader.Effect?.CurrentTechnique.Passes[0].Apply();
        //    wreathSpriteData2.DrawSelf(sourceRectangle, offset);
        //    batch.Begin(in snapshot, true);
        //}
        //Rectangle sourceRectangle2 = sourceRectangle;
        //sourceRectangle2.X = 0;
        //sourceRectangle2.Y += frameHeight + 2 - frameOffsetY;
        //sourceRectangle2.Height = (int)(frameHeight * 1f/*progress2*/);
        //Vector2 offset = Vector2.Zero;
        //float value3 = progress3 * (1f - Utils.GetLerpValue(0.6f, 1f, progress2, true));
        //bool flag = true;
        //if (soulOfTheWoods && progress2 > 0.9f) {
        //    flag = false;
        //}
        //if (flag) {
        //    drawFilling(sourceRectangle, progress, 1f - Utils.Remap(progress, 0f, 1f, 0.35f, 0.675f, true), opacity: value3);
        //}
        //if (soulOfTheWoods) {
        //    float filling2Progress = 1f - Utils.Remap(progress2, 0f, 1f, 0.725f, 1f, true);
        //    drawFilling(sourceRectangle2, progress2, filling2Progress, offset);
        //}
        //float mult2 = 5f; // first transition mult
        //progress3 = 1f - MathHelper.Clamp(progress2 * mult2, 0f, 1f);
        //// effect
        //void drawEffect(float progress, float progress2, Rectangle sourceRectangle, Vector2? offset = null, float opacity = 1f, byte frameX = 3, byte frameY = 1) {
        //    batch.End();
        //    batch.Begin(SpriteSortMode.Immediate, snapshot.blendState, SamplerState.AnisotropicClamp, snapshot.depthStencilState, snapshot.rasterizerState, snapshot.effect, Main.GameViewMatrix.ZoomMatrix);
        //    //color = DrawColor.Multiply(Stats.DrawColor, alpha);
        //    color = stats.BaseColor;
        //    color *= 1.4f;
        //    color.A = 80;
        //    color *= opacity;
        //    opacity = progress < 1f ? Ease.CubeInOut(progress) : 1f;
        //    float factor = Ease.CircOut((float)(Main.GlobalTimeWrappedHourly % 1.0) / 7f) * Math.Min(opacity > 0.75f ? 0.75f - opacity * (1f - opacity) : 0.925f, 0.925f);
        //    if (progress > 0f && progress < 0.5f) {
        //        factor *= 0.1f;
        //    }
        //    ref float mainFactor = ref storage.MainFactor;
        //    mainFactor = MathHelper.Lerp(mainFactor, factor, mainFactor < factor ? 0.1f : 0.025f);
        //    factor = mainFactor * stats.PulseIntensity;
        //    VerticalAppearanceShader.ActualProgress = progress2;
        //    wreathSpriteData2.Color = color * factor * opacity * 2f;
        //    wreathSpriteData2.Scale = factor + 0.475f;
        //    VerticalAppearanceShader.DrawColor = wreathSpriteData2.Color;
        //    VerticalAppearanceShader.Effect?.CurrentTechnique.Passes[0].Apply();
        //    wreathSpriteData2.DrawSelf(sourceRectangle, offset);
        //    wreathSpriteData2.Scale += 0.13f * progress * 2f;
        //    wreathSpriteData2.DrawSelf(sourceRectangle, offset);
        //    SpriteData wreathSpriteData3 = wreathSpriteData.Framed(frameX, frameY);
        //    opacity = Math.Min(progress * 1.15f, 0.7f);
        //    wreathSpriteData3.Color = color * opacity;
        //    VerticalAppearanceShader.DrawColor = wreathSpriteData3.Color;
        //    VerticalAppearanceShader.Effect?.CurrentTechnique.Passes[0].Apply();
        //    wreathSpriteData3.DrawSelf(offset: offset);
        //    batch.Begin(in snapshot, true);
        //}
        //if (flag) {
        //    drawEffect(progress, 1f - Utils.Remap(progress, 0f, 1f, 0.35f, 0.675f, true), sourceRectangle, opacity: progress3, frameX: (byte)(3 + stats.IsPhoenixWreath.ToInt()), frameY: 1);
        //}
        //if (soulOfTheWoods) {
        //    float filling2Progress = 1f - Utils.Remap(progress2, 0f, 1f, 0.725f, 1f, true);
        //    drawEffect(progress2, filling2Progress, sourceRectangle2, offset, frameY: 2);
        //}

        //var player = Player;
        //// adapted vanilla
        //Microsoft.Xna.Framework.Rectangle mouseRectangle = new Microsoft.Xna.Framework.Rectangle((int)((float)Main.mouseX + Main.screenPosition.X), (int)((float)Main.mouseY + Main.screenPosition.Y), 1, 1);
        //if (player.gravDir == -1f)
        //    mouseRectangle.Y = (int)Main.screenPosition.Y + Main.screenHeight - Main.mouseY;
        //Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle((int)((double)wreathSpriteData.VisualPosition.X + Main.screenPosition.X), (int)(wreathSpriteData.VisualPosition.Y + Main.screenPosition.Y), (int)(29 * Main.UIScale), (int)(29 * Main.UIScale));
        //if (!Main.mouseText && mouseRectangle.Intersects(value2)) {
        //    player.cursorItemIconEnabled = false;

        //    string text2 = "[kw/n:" + stats.CurrentResource + "]" + "/" + stats.TotalResource;

        //    Main.instance.MouseTextHackZoom(text2);
        //    Main.mouseText = true;
        //    WreathDrawing.JustDrawn = true;
        //}
        //else {
        //    WreathDrawing.JustDrawn = false;
        //}
    }

    public override int GetInsertIndex(List<GameInterfaceLayer> layers) {
        int preferredIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Resource Bars");
        return preferredIndex < 0 ? 0 : preferredIndex + 1;
    }

    public static void DrawText(Vector2 position) {
        Player player = Player;
        var stats = Stats;
        var config = ModContent.GetInstance<RoAClientConfig>();
        var wreathPosition = config.WreathPosition;
        SpriteBatch batch = Main.spriteBatch;
        SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(batch);
        batch.End();
        batch.Begin(snapshot with { transformationMatrix = Main.UIScaleMatrix });
        bool onPlayer = wreathPosition == RoAClientConfig.WreathPositions.Player;
        if (config.WreathDrawingMode == RoAClientConfig.WreathDrawingModes.Normal2) {
            bool horizontalBarsWithText = !onPlayer && Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBarsWithText";
            bool defaultResources = !onPlayer && Main.ResourceSetsManager.ActiveSetKeyName == "Default";
            Vector2 vector3 = new Vector2(Main.screenWidth - 300 + 4 - (defaultResources ? 4 : 0), 15f + (defaultResources ? 6f : 0f));

            Vector2 vector = vector3 + new Vector2(-4f, 3f) + new Vector2(-48f, -18f);
            if (onPlayer) {
                vector3 = Main.ScreenSize.ToVector2() / 2f;
                vector3.Y -= player.height * 1.5f;

                vector = vector3 + new Vector2(-4f, 3f) + new Vector2(0f, -40f);
            }
            if (horizontalBarsWithText) {
                vector.Y += 2f;
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
                vector2.X *= 0.93f;
            }
            else if (flag3) {
                vector2.X *= 1f;
            }
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.DrawString(FontAssets.MouseText.Value, text3, vector + new Vector2((0f - vector2.X) * 0.5f, 0f), textColor, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(FontAssets.MouseText.Value, (stats.CurrentResource + "/" + stats.TotalResource).ToString(), vector + new Vector2(vector2.X * 0.5f, 0f), textColor, 0f,
                new Vector2(FontAssets.MouseText.Value.MeasureString(stats.CurrentResource + "/" + stats.TotalResource).X, 0f), 1f, SpriteEffects.None, 0f);
        }
        batch.Begin(snapshot, true);
    }

    protected override bool DrawSelf() {
        return true;
    }
}
