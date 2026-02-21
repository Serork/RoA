using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.BackwoodsSystems;
using RoA.Common.Utilities.Extensions;
using RoA.Common.World;
using RoA.Content.Biomes.Backwoods;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Capture;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RoA.Content.Backgrounds;

sealed class BackwoodsBackgroundSurface : ModSurfaceBackgroundStyle {
    public static bool IsDrawingBackwoodsBackground { get; private set; }
    public static bool IsDrawingSurfaceBackground { get; private set; }

    private static float _minDepth;

    public static byte THEMBGTEXTURECOUNT => 3;
    private static ushort THEMACTIVETIME => (ushort)MathUtils.SecondsToFrames(30);
    private static byte MAXTHEMCOUNT => 50;

    public static Asset<Texture2D>[] ThemBGTextures { get; private set; } = null!;
    public static ThemBGInfo[] ThemBG { get; private set; } = null!;

    public record struct ThemBGInfo(byte TextureIndex, Vector2 Position, ushort ActiveTime, float Opacity = 0f, bool FacedRight = true) {
        public readonly ushort MaxActiveTime = ActiveTime;
    }

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        ThemBGTextures = new Asset<Texture2D>[THEMBGTEXTURECOUNT];
        for (int i = 0; i < ThemBGTextures.Length; i++) {
            ThemBGTextures[i] = ModContent.Request<Texture2D>(ResourceManager.AmbienceTextures + $"them_BG_{i + 1}");
        }

        ThemBG = new ThemBGInfo[MAXTHEMCOUNT];
    }

    public readonly int CloseOffset = 700;
    public readonly int MidOffset = 1140;
    public readonly int MidOffset2 = 1740;
    public readonly int FarOffset = 1200;

    private bool IsFogActiveForBackground() => BackwoodsFogHandler.Opacity > 0f && Main.cloudAlpha <= 0f && Main.GraveyardVisualIntensity * 0.92f <= 0f;

    private bool SpawnThem() {
        int availableIndex = -1;
        for (int i = 0; i < ThemBG.Length; i++) {
            if (ThemBG[i].ActiveTime <= 0) {
                availableIndex = i;
                break;
            }
        }
        if (availableIndex >= 0) {
            Vector2 getPosition() => new(Main.rand.Next(Main.screenWidth * 4), -Main.screenHeight / 3 * Main.rand.NextFloatDirection());
            Vector2 position = getPosition();
            int attempts = 100;
            while (attempts-- > 0) {
                bool shouldBreak = true;
                for (int i = 0; i < ThemBG.Length; i++) {
                    if (Vector2.Distance(ThemBG[availableIndex].Position, position) < 600f) {
                        position = getPosition();
                        shouldBreak = false;
                    }
                }
                if (shouldBreak) {
                    break;
                }
            }
            ThemBG[availableIndex] = new ThemBGInfo((byte)Main.rand.Next(THEMBGTEXTURECOUNT), 
                                                    position,
                                                    (ushort)(THEMACTIVETIME * Main.rand.NextFloat(0.75f, 1f)),
                                                    FacedRight: Main.rand.NextBool());
            return true;
        }

        return false;
    }

    private void UpdateThem() {
        for (int i = 0; i < ThemBG.Length; i++) {
            ref ThemBGInfo themBGInfo = ref ThemBG[i];
            if (themBGInfo.ActiveTime <= 0) {
                continue;
            }

            themBGInfo.Opacity = Utils.GetLerpValue(0f, themBGInfo.MaxActiveTime * 0.15f, themBGInfo.ActiveTime, true) * Utils.GetLerpValue(themBGInfo.MaxActiveTime, themBGInfo.MaxActiveTime * 0.85f, themBGInfo.ActiveTime, true);

            themBGInfo.ActiveTime--;
        }
    }

    public override void ModifyFarFades(float[] fades, float transitionSpeed) {
        if (IsFogActiveForBackground()) {
            if (Main.rand.NextBool(300)) {
                SpawnThem();
            }
        }
        UpdateThem();
    }

    public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) => -1;

    public override int ChooseMiddleTexture() => -1;

    public override int ChooseFarTexture() => -1;

    public override void Load() {
        On_SkyManager.DrawRemainingDepth += On_SkyManager_DrawRemainingDepth;
        On_SkyManager.DrawToDepth += On_SkyManager_DrawToDepth;

        On_Main.DrawSurfaceBG += On_Main_DrawSurfaceBG;
        On_SkyManager.DrawDepthRange += On_SkyManager_DrawDepthRange;
    }

    private void On_SkyManager_DrawDepthRange(On_SkyManager.orig_DrawDepthRange orig, SkyManager self, SpriteBatch spriteBatch, float minDepth, float maxDepth) {
        if (!Main.gameMenu) {
            IsDrawingBackwoodsBackground = Main.bgAlphaFrontLayer[ModContent.Find<ModSurfaceBackgroundStyle>(RoA.ModName + "/BackwoodsBackgroundSurface").Slot] > 0f;
            if ((IsDrawingSurfaceBackground && !IsDrawingBackwoodsBackground) ||
                IsDrawingBackwoodsBackground) {
                orig(self, spriteBatch, minDepth, maxDepth);
            }
            return;
        }
        orig(self, spriteBatch, minDepth, maxDepth);
    }

    private void On_Main_DrawSurfaceBG(On_Main.orig_DrawSurfaceBG orig, Main self) {
        IsDrawingSurfaceBackground = true;
        orig(self);

        if (Main.gameMenu) {
            return;
        }
        bool canBGDraw = false;
        if ((!Main.remixWorld || Main.gameMenu && !WorldGen.remixWorldGen) && (!WorldGen.remixWorldGen || !WorldGen.drunkWorldGen)) {
            canBGDraw = true;
        }
        if (Main.mapFullscreen) {
            canBGDraw = false;
        }
        if (!canBGDraw) {
            return;
        }
        float value2 = Main.GraveyardVisualIntensity * 0.92f;
        bool flag = false;
        float backgroundOpacity = 1f;
        Color colorOfSurfaceBackgroundsModified = typeof(Main).GetFieldValue<Color>("ColorOfSurfaceBackgroundsModified", Main.instance);
        Color backgroundColor = Main.ColorOfTheSkies * 1f/* * Math.Max(0.5f, 1f - BackwoodsFogHandler.Opacity)*/;
        if (IsFogActiveForBackground()) {
            backgroundOpacity = Math.Max(BackwoodsFogHandler.Opacity * 0.3f, Math.Max(Main.cloudAlpha, value2) * 0.1f);
            backgroundColor *= backgroundOpacity;
            flag = true;
        }
        bool capturing = CaptureManager.Instance.IsCapturing &&
            CaptureBiome.GetCaptureBiome(CaptureInterface.Settings.BiomeChoiceIndex).BackgroundIndex == ModContent.GetInstance<BackwoodsBackgroundSurface>().Slot;
        float captureOffset = capturing ? 200f : 0f;
        if (Main.LocalPlayer.InModBiome<BackwoodsBiome>()) {
            captureOffset = 0;
        }
        if (flag) {
            Texture2D value = TextureAssets.Background[49].Value;
            float bgScale = 1f;
            float bgGlobalScaleMultiplier = 2f;
            bgScale *= bgGlobalScaleMultiplier;
            int backgroundWidth = Main.screenWidth;
            int bgWidthScaled = (int)(backgroundWidth * bgScale);
            float bgTopY = (int)((double)(0f - Main.screenPosition.Y) / (Main.worldSurface * 16.0 - 600.0) * 200.0);
            double bgParallax = 0.1;
            float bgStartX = 0;
            float bgLoops = Main.screenWidth / bgWidthScaled + 2;
            if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                for (int i = 0; i < bgLoops; i++) {
                    int height = Math.Max(Main.screenHeight + 210, value.Height);
                    Main.spriteBatch.Draw(value, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + captureOffset), new Rectangle(0, 0, bgWidthScaled, height), backgroundColor, 0f, default, bgScale, SpriteEffects.None, 0f);
                }
            }
        }
    }

    private void On_SkyManager_DrawToDepth(On_SkyManager.orig_DrawToDepth orig, SkyManager self, SpriteBatch spriteBatch, float minDepth) {
        if (!IsDrawingBackwoodsBackground) {
            _minDepth = minDepth;
        }

        orig(self, spriteBatch, minDepth);
    }

    private void On_SkyManager_DrawRemainingDepth(On_SkyManager.orig_DrawRemainingDepth orig, SkyManager self, SpriteBatch spriteBatch) {
        orig(self, spriteBatch);
    }

    public override bool PreDrawCloseBackground(SpriteBatch spriteBatch) {
        IsDrawingSurfaceBackground = false;
        DrawSurfaceBackground(spriteBatch);

        return false;
    }

    private void DrawSurfaceBackground(SpriteBatch spriteBatch) {
        int[] slotAArray = [
            BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsFar"),
            BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsMid"),
            BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsClose"),
            BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsMid2")
        ];
        Texture2D close = TextureAssets.Background[slotAArray[2]].Value,
                  mid = TextureAssets.Background[slotAArray[1]].Value,
                  mid2 = TextureAssets.Background[slotAArray[3]].Value,
                  far = TextureAssets.Background[slotAArray[0]].Value;
        Texture2D close2 = TextureAssets.Background[BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsMidMenu")].Value;
        int length = slotAArray.Length;
        float screenOff = typeof(Main).GetFieldValue<float>("screenOff", Main.instance);
        float scAdj = typeof(Main).GetFieldValue<float>("scAdj", Main.instance);
        Color colorOfSurfaceBackgroundsModified = typeof(Main).GetFieldValue<Color>("ColorOfSurfaceBackgroundsModified", Main.instance);
        Color backgroundColor = colorOfSurfaceBackgroundsModified * 1f/* * Math.Max(0.5f, 1f - BackwoodsFogHandler.Opacity)*/;
        bool canBGDraw = false;
        if ((!Main.remixWorld || Main.gameMenu && !WorldGen.remixWorldGen) && (!WorldGen.remixWorldGen || !WorldGen.drunkWorldGen)) {
            canBGDraw = true;
        }
        if (Main.mapFullscreen) {
            canBGDraw = false;
        }
        int offset = 30;
        if (Main.gameMenu) {
        //    offset = 0;
        }
        if (WorldGen.drunkWorldGen) {
            offset = -180;
        }
        bool capturing = CaptureManager.Instance.IsCapturing &&
    CaptureBiome.GetCaptureBiome(CaptureInterface.Settings.BiomeChoiceIndex).BackgroundIndex == ModContent.GetInstance<BackwoodsBackgroundSurface>().Slot;
        float surfacePosition = (float)BackwoodsVars.BackwoodsTileForBackground - 1;
        if (capturing && !Main.LocalPlayer.InModBiome<BackwoodsBiome>()) {
            surfacePosition = (float)Main.worldSurface - 100;
        }
        if (surfacePosition == 0f) {
            surfacePosition = 1f;
        }
        float screenPosition = Main.screenPosition.Y + Main.screenHeight / 2 - 600f;
        if (Main.gameMenu) {
            screenPosition = 4370 + Main.screenHeight / 2 - 600f;
            surfacePosition = 300;
            scAdj = screenOff = 0f;
        }
        double backgroundTopMagicNumber = (0f - screenPosition + screenOff / 2f) / (surfacePosition * 16f);
        float bgGlobalScaleMultiplier = 2f;
        int pushBGTopHack;
        int offset2 = -180;
        int menuOffset = 0;
        if (Main.gameMenu) {
            //menuOffset += 900;
            //menuOffset -= offset2;
            //menuOffset += (int)Main.worldSurface - 100;
        }
        pushBGTopHack = menuOffset;
        pushBGTopHack += offset;
        pushBGTopHack += offset2;
        float captureOffset = capturing ? 200f : 0f;
        if (Main.LocalPlayer.InModBiome<BackwoodsBiome>()) {
            captureOffset = 0;
        }
        float parallaxModifier = 1f;
        if (canBGDraw) {
            SkyManager.Instance.SetStartingDepth(_minDepth * 2f);
            var bgScale = 1.25f;
            var bgParallax = 0.3;
            var bgTopY = (int)(backgroundTopMagicNumber * 1800.0 + 500.0) + (int)scAdj + pushBGTopHack;
            bgScale *= bgGlobalScaleMultiplier * 0.8f;
            var bgWidthScaled = (int)(mid2.Width * bgScale);
            var bgStartX = (int)(0.0 - Math.IEEERemainder(Main.screenPosition.X * bgParallax, bgWidthScaled) - bgWidthScaled / 2);
            //if (Main.gameMenu) {
            //    bgTopY = 320 + pushBGTopHack;
            //}
            var bgLoops = Main.screenWidth / bgWidthScaled + 2;
            SkyManager.Instance.DrawToDepth(spriteBatch, 1.5f / (float)bgParallax * parallaxModifier);
            if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                for (int i = 0; i < bgLoops; i++) {
                    Main.spriteBatch.Draw(mid2, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + MidOffset2 + captureOffset), new Rectangle(0, 0, mid2.Width, mid2.Height), backgroundColor, 0f, default, bgScale, SpriteEffects.None, 0f);
                }
            }

            bgScale = 1.25f;
            bgParallax = 0.4;
            bgTopY = (int)(backgroundTopMagicNumber * 1800.0 + 600.0) + (int)scAdj + pushBGTopHack;
            bgScale *= bgGlobalScaleMultiplier * 0.8f;
            bgWidthScaled = (int)(far.Width * bgScale);
            bgStartX = (int)(0.0 - Math.IEEERemainder(Main.screenPosition.X * bgParallax, bgWidthScaled) - bgWidthScaled / 2);
            //if (Main.gameMenu) {
            //    bgTopY = 320 + pushBGTopHack;
            //}
            bgLoops = Main.screenWidth / bgWidthScaled + 2;
            SkyManager.Instance.DrawToDepth(spriteBatch, 1.4f / (float)bgParallax * parallaxModifier);
            if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                for (int i = 0; i < bgLoops; i++) {
                    Main.spriteBatch.Draw(far, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + FarOffset + captureOffset), new Rectangle(0, 0, far.Width, far.Height), backgroundColor, 0f, default, bgScale, SpriteEffects.None, 0f);
                }
            }

            bgScale = 1.31f;
            bgParallax = 0.43;
            bgTopY = (int)(backgroundTopMagicNumber * 1950.0 + 700.0) + (int)scAdj + pushBGTopHack;
            bgScale *= bgGlobalScaleMultiplier * 0.9f;
            bgWidthScaled = (int)(mid.Width * bgScale);
            bgStartX = (int)(0.0 - Math.IEEERemainder(Main.screenPosition.X * bgParallax, bgWidthScaled) - bgWidthScaled / 2) - 150;
            //if (Main.gameMenu) {
            //    bgTopY = 400 + pushBGTopHack;
            //    bgStartX -= 80;
            //}
            bgLoops = Main.screenWidth / bgWidthScaled + 2;
            SkyManager.Instance.DrawToDepth(spriteBatch, 1.5f / (float)bgParallax * parallaxModifier);
            // line here
            if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                for (int i = 0; i < bgLoops; i++) {
                    Main.spriteBatch.Draw(mid, new Vector2(bgStartX + bgWidthScaled * i + 0.5f * i, bgTopY + MidOffset + captureOffset), new Rectangle(0, 0, mid.Width, mid.Height), backgroundColor, 0f, default, bgScale, SpriteEffects.None, 0f);
                }
            }

            bgScale = 1.31f;
            bgParallax = 0.43;
            bgTopY = (int)(backgroundTopMagicNumber * 1950.0 + 700.0) + (int)scAdj + pushBGTopHack;
            bgScale *= bgGlobalScaleMultiplier * 0.9f;
            bgWidthScaled = (int)(mid.Width * bgScale);
            bgStartX = (int)(0.0 - Math.IEEERemainder(Main.screenPosition.X * bgParallax, bgWidthScaled) - bgWidthScaled / 2) - 150;
            //if (Main.gameMenu) {
            //    bgTopY = 400 + pushBGTopHack;
            //    bgStartX -= 80;
            //}
            bgLoops = Main.screenWidth / bgWidthScaled + 2;
            SkyManager.Instance.DrawToDepth(spriteBatch, 1.5f / (float)bgParallax * parallaxModifier);
            // line here
            if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                for (int i = 0; i < bgLoops; i++) {
                    Main.spriteBatch.Draw(mid, new Vector2(bgStartX + bgWidthScaled * i + 0.5f * i, bgTopY + MidOffset + captureOffset), new Rectangle(0, 0, mid.Width, mid.Height), backgroundColor, 0f, default, bgScale, SpriteEffects.None, 0f);
                }
            }

            bgScale = 1.34f;
            bgParallax = 0.46;
            bgTopY = (int)(backgroundTopMagicNumber * 2100.0 + 1100.0) + (int)scAdj + pushBGTopHack;
            bgScale *= bgGlobalScaleMultiplier;
            bgWidthScaled = (int)(close2.Width * bgScale);
            bgStartX = (int)(0.0 - Math.IEEERemainder(Main.screenPosition.X * bgParallax, bgWidthScaled) - bgWidthScaled / 2);
            //if (Main.gameMenu) {
            //    bgTopY = 480 + pushBGTopHack;
            //    bgStartX -= 120;
            //}
            bgLoops = Main.screenWidth / bgWidthScaled + 2;
            SkyManager.Instance.DrawToDepth(spriteBatch, 1.5f / (float)bgParallax * parallaxModifier);
            int close2Offset = 600;
            if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                for (int i = 0; i < bgLoops; i++) {
                    //Main.EntitySpriteDraw(ModContent.Request<Texture2D>(ResourceManager.TexturesPerType + "BackwoodsBackground").Value, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + CloseOffset + close.Height * 3 - 186), new Rectangle(0, 0, close.Width, 300), backgroundColor, 0f, default(Vector2), bgScale, SpriteEffects.None);
                    //Main.spriteBatch.DrawSelf(close2, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + close2Offset + captureOffset), new Rectangle(0, 0, close2.Width, close2.Height), backgroundColor, 0f, default, bgScale, SpriteEffects.None, 0f);
                }
            }

            bgScale = 1.34f;
            bgParallax = 0.49;
            bgTopY = (int)(backgroundTopMagicNumber * 2100.0 + 1100.0) + (int)scAdj + pushBGTopHack;
            bgScale *= bgGlobalScaleMultiplier;
            bgWidthScaled = (int)(close.Width * bgScale);
            bgStartX = (int)(0.0 - Math.IEEERemainder(Main.screenPosition.X * bgParallax, bgWidthScaled) - bgWidthScaled / 2);
            //if (Main.gameMenu) {
            //    bgTopY = 480 + pushBGTopHack;
            //    bgStartX -= 120;
            //}
            bgLoops = Main.screenWidth / bgWidthScaled + 2;
            SkyManager.Instance.DrawToDepth(spriteBatch, 1.5f / (float)bgParallax);
            if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                for (int i = 0; i < bgLoops; i++) {
                    //Main.EntitySpriteDraw(ModContent.Request<Texture2D>(ResourceManager.TexturesPerType + "BackwoodsBackground").Value, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + CloseOffset + close.Height * 3 - 186), new Rectangle(0, 0, close.Width, 300), backgroundColor, 0f, default(Vector2), bgScale, SpriteEffects.None);
                    Main.spriteBatch.Draw(close, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + CloseOffset + captureOffset), new Rectangle(0, 0, close.Width, close.Height), backgroundColor, 0f, default, bgScale, SpriteEffects.None, 0f);
                }
            }
            float value2 = Main.GraveyardVisualIntensity * 0.92f;
            bool flag = false;
            float backgroundOpacity = 1f;
            if (IsFogActiveForBackground()) {
                backgroundOpacity = Math.Max(BackwoodsFogHandler.Opacity * 0.3f, Math.Max(Main.cloudAlpha, value2) * 0.1f);
                backgroundColor *= backgroundOpacity;
                flag = true;
            }

            if (flag) {
                foreach (ThemBGInfo themBGInfo in ThemBG) {
                    Texture2D value = ThemBGTextures[themBGInfo.TextureIndex].Value;
                    bgScale = 1f;
                    bgScale *= bgGlobalScaleMultiplier;
                    Rectangle clip = value.Bounds;
                    Vector2 origin = clip.Centered();
                    int width = value.Width;
                    bgWidthScaled = (int)(BackwoodsVars.BackwoodsHalfSizeX * 3f * 16 * bgScale * 1f);
                    bgParallax = 1.5f;
                    bgStartX = (int)(0.0 - Math.IEEERemainder(Main.screenPosition.X * bgParallax, bgWidthScaled));
                    Vector2 vector = Main.screenPosition + new Vector2(Main.screenWidth >> 1, Main.screenHeight >> 1);
                    Vector2 vector2 = new Vector2(1f / (float)bgParallax, 1f / (float)bgParallax);
                    Vector2 position = themBGInfo.Position;
                    position = (position - vector) * vector2 + vector - Main.screenPosition;
                    position.X = (position.X + 500f) % 4000f;
                    position.Y += BackwoodsVars.BackwoodsCenterY * 6.25f;
                    if (position.X < 0f)
                        position.X += 4000f;

                    position.X -= 500f;
                    //if (bgStartX < 0f)
                    //    bgStartX += bgWidthScaled;
                    bgTopY = (int)(backgroundTopMagicNumber * 2100.0 + 2500.0) + (int)scAdj + pushBGTopHack;
                    if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                        SpriteEffects effects = themBGInfo.FacedRight.ToInt().ToSpriteEffects();
                        for (int i = 0; i < 2; i++) {
                            Main.spriteBatch.Draw(value, position + Main.rand.RandomPointInArea(0.2f), clip, Color.White with { A = 187 } * backgroundOpacity * 5f * themBGInfo.Opacity, 0f, origin, bgScale, effects, 0f);
                            Main.spriteBatch.Draw(value, position + Main.rand.RandomPointInArea(20f), clip, Color.White * 0.1f * backgroundOpacity * 5f * themBGInfo.Opacity, 0f, origin, bgScale, effects, 0f);
                        }
                    }
                }
            }

            //if (flag) {
            //    Texture2D value = TextureAssets.Background[49].Value;
            //    bgScale = 1f;
            //    bgScale *= bgGlobalScaleMultiplier;
            //    int backgroundWidth = Main.screenWidth;
            //    bgWidthScaled = (int)(backgroundWidth * bgScale);
            //    bgTopY = (int)((double)(0f - Main.screenPosition.Y) / (Main.worldSurface * 16.0 - 600.0) * 200.0);
            //    bgParallax = 0.1;
            //    bgStartX = 0;
            //    bgLoops = Main.screenWidth / bgWidthScaled + 2;
            //    if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
            //        for (int i = 0; i < bgLoops; i++) {
            //            int height = Math.Max(Main.screenHeight + 210, value.Height);
            //            Main.spriteBatch.Draw(value, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + captureOffset), new Rectangle(0, 0, bgWidthScaled, height), backgroundColor, 0f, default, bgScale, SpriteEffects.None, 0f);
            //        }
            //    }
            //}
        }
    }
}