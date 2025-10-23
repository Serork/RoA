using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.BackwoodsSystems;
using RoA.Common.Utilities.Extensions;
using RoA.Common.WorldEvents;
using RoA.Content.Biomes.Backwoods;
using RoA.Core;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Capture;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RoA.Content.Backgrounds;

sealed class BackwoodsBackgroundSurface : ModSurfaceBackgroundStyle {
    public readonly int CloseOffset = 700;
    public readonly int MidOffset = 1140;
    public readonly int MidOffset2 = 1740;
    public readonly int FarOffset = 1200;

    public override void ModifyFarFades(float[] fades, float transitionSpeed) {
        for (int i = 0; i < fades.Length; i++) {
            if (i == Slot) {
                fades[i] += transitionSpeed;
                if (fades[i] > 1f) {
                    fades[i] = 1f;
                }
            }
            else {
                fades[i] -= transitionSpeed;
                if (fades[i] < 0f) {
                    fades[i] = 0f;
                }
            }
        }
    }

    public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) => -1;

    public override int ChooseMiddleTexture() => -1;

    public override int ChooseFarTexture() => -1;

    public override bool PreDrawCloseBackground(SpriteBatch spriteBatch) {
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
        Color backgroundColor = colorOfSurfaceBackgroundsModified/* * Math.Max(0.5f, 1f - BackwoodsFogHandler.Opacity)*/;
        bool canBGDraw = false;
        if ((!Main.remixWorld || Main.gameMenu && !WorldGen.remixWorldGen) && (!WorldGen.remixWorldGen || !WorldGen.drunkWorldGen)) {
            canBGDraw = true;
        }
        if (Main.mapFullscreen) {
            canBGDraw = false;
        }
        int offset = 30;
        if (Main.gameMenu) {
            offset = 0;
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
        double backgroundTopMagicNumber = (0f - screenPosition + screenOff / 2f) / (surfacePosition * 16f);
        float bgGlobalScaleMultiplier = 2f;
        int pushBGTopHack;
        int offset2 = -180;
        int menuOffset = 0;
        if (Main.gameMenu) {
            menuOffset -= offset2;
            menuOffset -= 1300;
        }
        pushBGTopHack = menuOffset;
        pushBGTopHack += offset;
        pushBGTopHack += offset2;
        float captureOffset = capturing ? 200f : 0f;
        if (Main.LocalPlayer.InModBiome<BackwoodsBiome>()) {
            captureOffset = 0;
        }
        if (canBGDraw) {
            var bgScale = 1.25f;
            var bgParallax = 0.3;
            var bgTopY = (int)(backgroundTopMagicNumber * 1800.0 + 500.0) + (int)scAdj + pushBGTopHack;
            bgScale *= bgGlobalScaleMultiplier * 0.8f;
            var bgWidthScaled = (int)(mid2.Width * bgScale);
            var bgStartX = (int)(0.0 - Math.IEEERemainder(Main.screenPosition.X * bgParallax, bgWidthScaled) - bgWidthScaled / 2);
            if (Main.gameMenu) {
                bgTopY = 320 + pushBGTopHack;
            }
            var bgLoops = Main.screenWidth / bgWidthScaled + 2;
            SkyManager.Instance.DrawToDepth(spriteBatch, 1.5f / (float)bgParallax);
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
            if (Main.gameMenu) {
                bgTopY = 320 + pushBGTopHack;
            }
            bgLoops = Main.screenWidth / bgWidthScaled + 2;
            SkyManager.Instance.DrawToDepth(spriteBatch, 1.4f / (float)bgParallax);
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
            if (Main.gameMenu) {
                bgTopY = 400 + pushBGTopHack;
                bgStartX -= 80;
            }
            bgLoops = Main.screenWidth / bgWidthScaled + 2;
            SkyManager.Instance.DrawToDepth(spriteBatch, 1.5f / (float)bgParallax);
            if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                for (int i = 0; i < bgLoops; i++) {
                    Main.spriteBatch.Draw(mid, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + MidOffset + captureOffset), new Rectangle(0, 0, mid.Width, mid.Height), backgroundColor, 0f, default, bgScale, SpriteEffects.None, 0f);
                }
            }

            bgScale = 1.31f;
            bgParallax = 0.43;
            bgTopY = (int)(backgroundTopMagicNumber * 1950.0 + 700.0) + (int)scAdj + pushBGTopHack;
            bgScale *= bgGlobalScaleMultiplier * 0.9f;
            bgWidthScaled = (int)(mid.Width * bgScale);
            bgStartX = (int)(0.0 - Math.IEEERemainder(Main.screenPosition.X * bgParallax, bgWidthScaled) - bgWidthScaled / 2) - 150;
            if (Main.gameMenu) {
                bgTopY = 400 + pushBGTopHack;
                bgStartX -= 80;
            }
            bgLoops = Main.screenWidth / bgWidthScaled + 2;
            SkyManager.Instance.DrawToDepth(spriteBatch, 1.5f / (float)bgParallax);
            if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                for (int i = 0; i < bgLoops; i++) {
                    Main.spriteBatch.Draw(mid, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + MidOffset + captureOffset), new Rectangle(0, 0, mid.Width, mid.Height), backgroundColor, 0f, default, bgScale, SpriteEffects.None, 0f);
                }
            }

            bgScale = 1.34f;
            bgParallax = 0.46;
            bgTopY = (int)(backgroundTopMagicNumber * 2100.0 + 1100.0) + (int)scAdj + pushBGTopHack;
            bgScale *= bgGlobalScaleMultiplier;
            bgWidthScaled = (int)(close2.Width * bgScale);
            bgStartX = (int)(0.0 - Math.IEEERemainder(Main.screenPosition.X * bgParallax, bgWidthScaled) - bgWidthScaled / 2);
            if (Main.gameMenu) {
                bgTopY = 480 + pushBGTopHack;
                bgStartX -= 120;
            }
            bgLoops = Main.screenWidth / bgWidthScaled + 2;
            SkyManager.Instance.DrawToDepth(spriteBatch, 1.5f / (float)bgParallax);
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
            if (Main.gameMenu) {
                bgTopY = 480 + pushBGTopHack;
                bgStartX -= 120;
            }
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
            if (/*Main.cloudAlpha > 0f || value2 > 0f || */BackwoodsFogHandler.Opacity > 0f && Main.cloudAlpha <= 0f && value2 <= 0f) {
                backgroundColor *= Math.Max(BackwoodsFogHandler.Opacity * 0.25f, Math.Max(Main.cloudAlpha, value2) * 0.1f);
                flag = true;
            }
            if (flag) {
                Texture2D value = TextureAssets.Background[49].Value;
                bgScale = 1f;
                bgScale *= bgGlobalScaleMultiplier;
                int backgroundWidth = Main.screenWidth;
                bgWidthScaled = (int)(backgroundWidth * bgScale);
                bgTopY = (int)((double)(0f - Main.screenPosition.Y) / (Main.worldSurface * 16.0 - 600.0) * 200.0);
                bgParallax = 0.1;
                bgStartX = 0;
                bgLoops = Main.screenWidth / bgWidthScaled + 2;
                if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                    for (int i = 0; i < bgLoops; i++) {
                        int height = Math.Max(Main.screenHeight + 210, value.Height);
                        Main.spriteBatch.Draw(value, new Vector2(bgStartX + bgWidthScaled * i, bgTopY + captureOffset), new Rectangle(0, 0, bgWidthScaled, height), backgroundColor, 0f, default, bgScale, SpriteEffects.None, 0f);
                    }
                }
            }
        }
        return false;
    }
}