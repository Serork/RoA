using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Common.Configs;
using RoA.Common.Druid.Wreath;
using RoA.Content.Forms;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.InterfaceElements;

sealed class WreathDrawing : PlayerDrawLayer {
    private const byte HORIZONTALFRAMECOUNT = 6;

    private static SpriteData _wreathSpriteData;

    private sealed class ValuesStorage : ModPlayer {
        public float MainFactor;
        public Vector2 OldPosition;
    }

    public static bool JustDrawn { get; private set; }

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _wreathSpriteData = new SpriteData(ModContent.Request<Texture2D>(ResourceManager.Textures + "Wreath"), new SpriteFrame(HORIZONTALFRAMECOUNT, 3));
    }

    public override bool IsHeadLayer => false;

    public override Position GetDefaultPosition() => PlayerDrawLayers.AfterLastVanillaLayer;

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        if (ModContent.GetInstance<RoAClientConfig>().WreathDrawingMode != RoAClientConfig.WreathDrawingModes.Normal) {
            return;
        }

        Player player = drawInfo.drawPlayer;
        if (drawInfo.shadow != 0f || !player.active || player.whoAmI != Main.myPlayer) {
            return;
        }

        WreathHandler stats = player.GetModPlayer<WreathHandler>();
        Vector2 playerPosition = Utils.Floor(new Vector2((int)(drawInfo.Position.X + (float)(drawInfo.drawPlayer.width / 2)),
            (int)(drawInfo.Position.Y + (float)drawInfo.drawPlayer.height - 40f)));
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
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.Transform);

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
        if (player.gravDir == -1f)
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

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(in snapshot);
    }
}

//sealed class WreathDrawing : ModSystem {
//    private const byte HORIZONTALFRAMECOUNT = 6;

//    private static SpriteData _wreathSpriteData;
//    private static float _factor;

//    private Vector2 _oldPosition;

//    public static bool JustDrawn { get; private set; }

//    public override void SetStaticDefaults() {
//        if (Main.dedServ) {
//            return;
//        }

//        _wreathSpriteData = new SpriteData(ModContent.Request<Texture2D>(ResourceManager.Textures + "Wreath"), new SpriteFrame(HORIZONTALFRAMECOUNT, 3));
//    }

//    public override void Load() {
//        On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += On_PlayerDrawLayers_DrawPlayer_RenderAllLayers;
//    }

//    private void On_PlayerDrawLayers_DrawPlayer_RenderAllLayers(On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref PlayerDrawSet drawinfo) {
//        var drawInfo = drawinfo;
//        float rotation = 0f;
//        Vector2 position = Vector2.Zero;
//        if (drawInfo.shadow == 0f) {
//            Player player = drawInfo.drawPlayer;
//            WreathHandler stats = player.GetModPlayer<WreathHandler>();
//            Vector2 playerPosition = Utils.Floor(new Vector2((int)(drawinfo.Position.X + (float)(drawInfo.drawPlayer.width / 2)),
//                (int)(drawinfo.Position.Y + (float)drawInfo.drawPlayer.height - 40f)));
//            playerPosition.Y -= 12f;
//            bool breathUI = player.breath < player.breathMax || player.lavaTime < player.lavaMax;
//            float offsetX = -_wreathSpriteData.FrameWidth / 2f + 2,
//                  offsetY = _wreathSpriteData.FrameHeight;
//            playerPosition.X += offsetX;
//            playerPosition.Y += breathUI ? (float)(-(float)offsetY * ((player.breathMax - 1) / 200 + 1)) : -offsetY;

//            if (player.dead || player.ghost || player.ShouldNotDraw || !stats.ShouldDrawItself) {
//                _oldPosition = playerPosition;
//            }
//            else {
//                position = Vector2.Lerp(_oldPosition, playerPosition, 0.3f);

//                position -= Main.screenPosition;
//                position += new Vector2(Main.screenWidth, Main.screenHeight) / 3f;
//                position.Y -= 25f;
//                position += player.PlayerMovementOffset();
//                var phoenixHandler = player.GetModPlayer<LilPhoenixForm.LilPhoenixFormHandler>();
//                rotation = drawInfo.rotation + MathHelper.Pi;
//                bool flag2 = phoenixHandler._dashed || phoenixHandler._dashed || phoenixHandler._isPreparing;
//                if (flag2) {
//                    rotation = MathHelper.Pi;
//                }
//                _oldPosition = playerPosition;
//                if (!flag2) {
//                    Vector2 vector = drawInfo.Position + drawInfo.rotationOrigin;
//                    Matrix matrix = Matrix.CreateRotationZ(drawInfo.rotation);
//                    Vector2 newPosition = _oldPosition - vector;
//                    newPosition = Vector2.Transform(newPosition, matrix);
//                    _oldPosition.X = (newPosition + vector).X;
//                    float progress4 = Math.Abs(drawInfo.rotation) / MathHelper.Pi;
//                    float offsetY2 = progress4 * (player.height / 2f + 10f);
//                    _oldPosition.Y += offsetY2;
//                }
//            }
//        }

//        orig(ref drawinfo);

//        SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
//        Main.spriteBatch.End();
//        Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.Transform);
//        if (drawInfo.shadow == 0f) {
//            Player player = drawInfo.drawPlayer;
//            WreathHandler stats = player.GetModPlayer<WreathHandler>();
//            //position -= new Vector2(2f, 0f);
//            var phoenixHandler = player.GetModPlayer<LilPhoenixForm.LilPhoenixFormHandler>();

//            float progress = MathHelper.Clamp(stats.ActualProgress2, 0f, 1f);
//            //float alpha = Lighting.Brightness((int)Stats.LightingPosition.X / 16, (int)Stats.LightingPosition.Y / 16);
//            //alpha = (alpha + 1f) / 2f;
//            //Color color = Color.Multiply(Stats.DrawColor, alpha);
//            Color color = stats.BaseColor;
//            float opacity = Math.Max(Utils.GetLerpValue(1f, 0.75f, progress, true), 0.7f);

//            //position = position.Floor();
//            // dark border
//            SpriteData wreathSpriteData = _wreathSpriteData;
//            wreathSpriteData.Color = color * opacity;
//            wreathSpriteData.VisualPosition = position;
//            wreathSpriteData.Rotation = rotation;
//            wreathSpriteData.DrawSelf();

//            // filling
//            SpriteData wreathSpriteData2 = wreathSpriteData.Framed((byte)(0 + stats.IsPhoenixWreath.ToInt()), 1);
//            int frameOffsetY = 0;
//            int frameHeight = wreathSpriteData2.FrameHeight + frameOffsetY;
//            void drawFilling(Rectangle sourceRectangle, Vector2? offset = null, float opacity = 1f) {
//                wreathSpriteData2.VisualPosition = position - Vector2.UnitY * frameOffsetY;
//                wreathSpriteData2.Color = color * opacity;
//                wreathSpriteData2.DrawSelf(sourceRectangle, offset);
//            }
//            Rectangle sourceRectangle = new(wreathSpriteData2.FrameX, wreathSpriteData2.FrameY + frameOffsetY, wreathSpriteData2.FrameWidth, (int)(frameHeight * progress));
//            bool soulOfTheWoods = stats.SoulOfTheWoods;
//            float progress2 = stats.ActualProgress2 - 1f;
//            float value = progress2;
//            float progress3 = 1f - MathHelper.Clamp(progress2 * 0.7f, 0f, 0.7f);
//            Rectangle sourceRectangle2 = sourceRectangle;
//            sourceRectangle2.X = 0;
//            sourceRectangle2.Y += frameHeight + 2 - frameOffsetY;
//            sourceRectangle2.Height = (int)(frameHeight * progress2);
//            Vector2 offset = Vector2.Zero;
//            float value3 = progress3 * (1f - Utils.GetLerpValue(0.6f, 1f, progress2, true));
//            bool flag = true;
//            if (soulOfTheWoods && progress2 > 0.9f) {
//                flag = false;
//            }
//            if (flag) {
//                drawFilling(sourceRectangle, opacity: value3);
//            }
//            if (soulOfTheWoods) {
//                drawFilling(sourceRectangle2, offset);
//            }

//            progress3 = 1f - MathHelper.Clamp(progress2 * 2.25f, 0f, 1f);
//            // effect
//            void drawEffect(float progress, Rectangle sourceRectangle, Vector2? offset = null, float opacity = 1f, byte frameX = 3, byte frameY = 1) {
//                //color = Color.Multiply(Stats.DrawColor, alpha);
//                color = stats.BaseColor;
//                color *= 1.4f;
//                color.A = 80;
//                color *= opacity;
//                opacity = progress < 1f ? Ease.CubeInOut(progress) : 1f;
//                float factor = Ease.CircOut((float)(Main.GlobalTimeWrappedHourly % 1.0) / 7f) * Math.Min(opacity > 0.75f ? 0.75f - opacity * (1f - opacity) : 0.925f, 0.925f);
//                if (progress > 0f && progress < 0.5f) {
//                    factor *= 0.1f;
//                }
//                _factor = MathHelper.Lerp(_factor, factor, _factor < factor ? 0.1f : 0.025f);
//                factor = _factor * stats.PulseIntensity;
//                wreathSpriteData2.Color = color * factor * opacity * 2f;
//                wreathSpriteData2.Scale = factor + 0.475f;
//                wreathSpriteData2.DrawSelf(sourceRectangle, offset);
//                wreathSpriteData2.Scale += 0.13f * progress * 2f;
//                wreathSpriteData2.DrawSelf(sourceRectangle, offset);
//                SpriteData wreathSpriteData3 = wreathSpriteData.Framed(frameX, frameY);
//                opacity = Math.Min(progress * 1.15f, 0.7f);
//                wreathSpriteData3.Color = color * opacity;
//                wreathSpriteData3.DrawSelf(offset: offset);
//            }
//            if (flag) {
//                drawEffect(progress, sourceRectangle, opacity: progress3, frameX: (byte)(3 + stats.IsPhoenixWreath.ToInt()), frameY: 1);
//            }
//            if (soulOfTheWoods) {
//                drawEffect(progress2, sourceRectangle2, offset, frameY: 2);
//            }

//            // adapted vanilla
//            Microsoft.Xna.Framework.Rectangle mouseRectangle = new Microsoft.Xna.Framework.Rectangle((int)((float)Main.mouseX + Main.screenPosition.X), (int)((float)Main.mouseY + Main.screenPosition.Y), 1, 1);
//            if (player.gravDir == -1f)
//                mouseRectangle.Y = (int)Main.screenPosition.Y + Main.screenHeight - Main.mouseY;
//            Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle((int)((double)wreathSpriteData.VisualPosition.X + Main.screenPosition.X), (int)(wreathSpriteData.VisualPosition.Y + Main.screenPosition.Y), (int)(29 * Main.UIScale), (int)(29 * Main.UIScale));
//            if (!Main.mouseText && mouseRectangle.Intersects(value2)) {
//                player.cursorItemIconEnabled = false;

//                string text2 = "[kw/n:" + stats.CurrentResource + "]" + "/" + stats.TotalResource;

//                Main.instance.MouseTextHackZoom(text2);
//                Main.mouseText = true;
//                JustDrawn = true;
//            }
//            else {
//                JustDrawn = false;
//            }
//        }

//        Main.spriteBatch.End();
//        Main.spriteBatch.Begin(in snapshot);
//    }
//}

//sealed class WreathDrawing() : InterfaceElement(RoA.ModName + ": Wreath", InterfaceScaleType.Game) {
//    private const byte HORIZONTALFRAMECOUNT = 6;

//    private static SpriteData _wreathSpriteData;
//    private static float _factor;

//    private Vector2 _oldPosition;

//    private static Player Player => Main.LocalPlayer;
//    private static WreathHandler Stats => Player.GetModPlayer<WreathHandler>();

//    public static bool JustDrawn { get; private set; }

//    public override int GetInsertIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Active && layer.Name.Equals("Vanilla: Ingame Options"));

//    public override void Load(Mod mod) {
//        if (Main.dedServ) {
//            return;
//        }

//        _wreathSpriteData = new SpriteData(ModContent.Request<Texture2D>(ResourceManager.Textures + "Wreath"), new SpriteFrame(HORIZONTALFRAMECOUNT, 3));
//    }

//    protected override bool DrawSelf() {
//        Vector2 playerPosition = Utils.Floor(Player.RotatedRelativePoint(Player.Center - Vector2.UnitY * Player.height / 2f) + Vector2.UnitY * Player.gfxOffY);
//        if (Player.GetModPlayer<BaseFormHandler>().IsInDruidicForm) {
//            playerPosition.Y -= Player.fullRotationOrigin.Y;
//        }
//        playerPosition.Y -= 12f;
//        Vector2 position;
//        bool breathUI = Player.breath < Player.breathMax || Player.lavaTime < Player.lavaMax;
//        float offsetX = -_wreathSpriteData.FrameWidth / 2f + 2,
//              offsetY = _wreathSpriteData.FrameHeight;
//        playerPosition.Y -= Player.gfxOffY;
//        playerPosition.X += offsetX;
//        playerPosition.Y += breathUI ? (float)(-(float)offsetY * ((Player.breathMax - 1) / 200 + 1)) : -offsetY;

//        if (Player.dead || Player.ghost || Player.ShouldNotDraw || !Stats.ShouldDrawItself) {
//            _oldPosition = playerPosition;

//            return true;
//        }

//        if (!Player.mount.Active) {
//            position = Vector2.Lerp(_oldPosition, playerPosition, 0.3f);
//        }
//        else {
//            position = playerPosition;
//        }
//        position -= Main.screenPosition;
//        position -= new Vector2(/*4f*/0f, 0f);
//        float rotation = Player.fullRotation + MathHelper.Pi;
//        _oldPosition = playerPosition;

//        //Vector2 vector = Player.Center - Main.screenPosition + Player.fullRotationOrigin;
//        //Matrix matrix = Matrix.CreateRotationZ(Player.bodyRotation);
//        //Vector2 newPosition = position - vector;
//        //newPosition = Vector2.Transform(newPosition - new Vector2(2f, 0f), matrix);
//        //position = newPosition + vector;

//        float progress = MathHelper.Clamp(Stats.ActualProgress2, 0f, 1f);
//        //float alpha = Lighting.Brightness((int)Stats.LightingPosition.X / 16, (int)Stats.LightingPosition.Y / 16);
//        //alpha = (alpha + 1f) / 2f;
//        //Color color = Color.Multiply(Stats.DrawColor, alpha);
//        Color color = Stats.BaseColor;
//        float opacity = Math.Max(Utils.GetLerpValue(1f, 0.75f, progress, true), 0.7f);

//        position = position.Floor();
//        // dark border
//        SpriteData wreathSpriteData = _wreathSpriteData;
//        wreathSpriteData.Rotation = MathHelper.Pi;
//        wreathSpriteData.Color = color * opacity;
//        wreathSpriteData.VisualPosition = position;
//        wreathSpriteData.Rotation = rotation;
//        wreathSpriteData.DrawSelf();

//        // filling
//        SpriteData wreathSpriteData2 = wreathSpriteData.Framed((byte)(0 + Stats.IsPhoenixWreath.ToInt()), 1);
//        int frameOffsetY = 0;
//        int frameHeight = wreathSpriteData2.FrameHeight + frameOffsetY;
//        void drawFilling(Rectangle sourceRectangle, Vector2? offset = null, float opacity = 1f) {
//            wreathSpriteData2.VisualPosition = position - Vector2.UnitY * frameOffsetY;
//            wreathSpriteData2.Color = color * opacity;
//            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
//        }
//        Rectangle sourceRectangle = new(wreathSpriteData2.FrameX, wreathSpriteData2.FrameY + frameOffsetY, wreathSpriteData2.FrameWidth, (int)(frameHeight * progress));
//        bool soulOfTheWoods = Stats.SoulOfTheWoods;
//        float progress2 = Stats.ActualProgress2 - 1f;
//        float value = progress2;
//        float progress3 = 1f - MathHelper.Clamp(progress2 * 0.7f, 0f, 0.7f);
//        Rectangle sourceRectangle2 = sourceRectangle;
//        sourceRectangle2.X = 0;
//        sourceRectangle2.Y += frameHeight + 2 - frameOffsetY;
//        sourceRectangle2.Height = (int)(frameHeight * progress2);
//        Vector2 offset = Vector2.Zero;
//        float value3 = progress3 * (1f - Utils.GetLerpValue(0.6f, 1f, progress2, true));
//        bool flag = true;
//        if (soulOfTheWoods && progress2 > 0.9f) {
//            flag = false;
//        }
//        if (flag) {
//            drawFilling(sourceRectangle, opacity: value3);
//        }
//        if (soulOfTheWoods) {
//            drawFilling(sourceRectangle2, offset);
//        }

//        progress3 = 1f - MathHelper.Clamp(progress2 * 2.25f, 0f, 1f);
//        // effect
//        void drawEffect(float progress, Rectangle sourceRectangle, Vector2? offset = null, float opacity = 1f, byte frameX = 3, byte frameY = 1) {
//            //color = Color.Multiply(Stats.DrawColor, alpha);
//            color = Stats.BaseColor;
//            color *= 1.4f;
//            color.A = 80;
//            color *= opacity;
//            opacity = progress < 1f ? Ease.CubeInOut(progress) : 1f;
//            float factor = Ease.CircOut((float)(Main.GlobalTimeWrappedHourly % 1.0) / 7f) * Math.Min(opacity > 0.75f ? 0.75f - opacity * (1f - opacity) : 0.925f, 0.925f);
//            if (progress > 0f && progress < 0.5f) {
//                factor *= 0.1f;
//            }
//            _factor = MathHelper.Lerp(_factor, factor, _factor < factor ? 0.1f : 0.025f);
//            factor = _factor * Stats.PulseIntensity;
//            wreathSpriteData2.Color = color * factor * opacity * 2f;
//            wreathSpriteData2.Scale = factor + 0.475f;
//            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
//            wreathSpriteData2.Scale += 0.13f * progress * 2f;
//            wreathSpriteData2.DrawSelf(sourceRectangle, offset);
//            SpriteData wreathSpriteData3 = wreathSpriteData.Framed(frameX, frameY);
//            opacity = Math.Min(progress * 1.15f, 0.7f);
//            wreathSpriteData3.Color = color * opacity;
//            wreathSpriteData3.DrawSelf(offset: offset);
//        }
//        if (flag) {
//            drawEffect(progress, sourceRectangle, opacity: progress3, frameX: (byte)(3 + Stats.IsPhoenixWreath.ToInt()), frameY: 1);
//        }
//        if (soulOfTheWoods) {
//            drawEffect(progress2, sourceRectangle2, offset, frameY: 2);
//        }

//        // adapted vanilla
//        Microsoft.Xna.Framework.Rectangle mouseRectangle = new Microsoft.Xna.Framework.Rectangle((int)((float)Main.mouseX + Main.screenPosition.X), (int)((float)Main.mouseY + Main.screenPosition.Y), 1, 1);
//        if (Player.gravDir == -1f)
//            mouseRectangle.Y = (int)Main.screenPosition.Y + Main.screenHeight - Main.mouseY;
//        Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle((int)((double)wreathSpriteData.VisualPosition.X + Main.screenPosition.X), (int)(wreathSpriteData.VisualPosition.Y + Main.screenPosition.Y), (int)(29 * Main.UIScale), (int)(29 * Main.UIScale));
//        if (!Main.mouseText && mouseRectangle.Intersects(value2)) {
//            Player.cursorItemIconEnabled = false;

//            string text2 = "[kw/n:" + Stats.CurrentResource + "]" + "/" + Stats.TotalResource;

//            Main.instance.MouseTextHackZoom(text2);
//            Main.mouseText = true;
//            JustDrawn = true;
//        }
//        else {
//            JustDrawn = false;
//        }

//        return true;
//    }
//}
