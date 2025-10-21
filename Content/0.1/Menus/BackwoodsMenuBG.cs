using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Utilities.Extensions;
using RoA.Core;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RoA.Content.Menus;

// it stinks
sealed class BackwoodsMenuBG : ModSurfaceBackgroundStyle {
    public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b) => -1;

    public override int ChooseMiddleTexture() => -1;

    public override int ChooseFarTexture() => -1;

    public override bool PreDrawCloseBackground(SpriteBatch spriteBatch) {
        DrawDarkerBackground(spriteBatch);

        float num1 = 1200f;
        float num2 = 1550f;
        int[] numArray = new int[4] {
            BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsFar"),
            BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsMid"),
            BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsMidMenu"),
            BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "BackwoodsClose")
        };
        Color color = typeof(Main).GetFieldValue<Color>("ColorOfSurfaceBackgroundsModified", Main.instance);

        int length = numArray.Length;
        for (int index1 = 0; index1 < numArray.Length; ++index1) {
            if (index1 == 2) {
                continue;
            }
            float num3 = (float)(0.5 - 0.1 * (length - index1));
            int i = numArray[index1];
            Main.instance.LoadBackground(i);
            float scale = 2.2f;
            int num4 = (int)(Main.backgroundWidth[i] * (double)scale);
            SkyManager.Instance.DrawToDepth(spriteBatch, 1f / num3);
            float fieldValue1 = typeof(Main).GetFieldValue<float>("screenOff", Main.instance);
            float fieldValue2 = typeof(Main).GetFieldValue<float>("scAdj", Main.instance);
            int num5 = (int)(-Math.IEEERemainder(Main.screenPosition.X * (double)num3, num4) - num4 / 2);
            int num6 = (int)((-(double)Main.screenPosition.Y + (double)fieldValue1 / 2.0) / (Main.worldSurface * 16.0) * (double)num1 + (double)num2) + (int)fieldValue2 - length * 1000;
            if (Main.gameMenu) {
                num6 = -500;
            }
            float extraY = index1 == 2 ? -50f : 0;
            if (index1 == 0) {
                extraY -= 25;
            }
            if (index1 == 1) {
                extraY += 40;
            }
            if (index1 == 3) {
                extraY -= 5;
            }
            int num7 = Main.screenWidth / num4 + 2;
            if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
                for (int index2 = 0; index2 < num7; ++index2) {
                    spriteBatch.Draw(TextureAssets.Background[i].Value, new Vector2(num5 + num4 * index2, MathHelper.Clamp(num6, -50f, 0f) + extraY),
                        new Rectangle?(new Rectangle(0, 0, Main.backgroundWidth[i], Main.backgroundHeight[i])), color, 0.0f, new Vector2(), scale, SpriteEffects.None, 0.0f);
                }
            }
        }

        return false;
    }

    private void DrawDarkerBackground(SpriteBatch spriteBatch) {
        Texture2D texture = ModContent.Request<Texture2D>(ResourceManager.BackgroundTextures + "BackwoodsSky").Value;
        float scale = 2.2f;
        int width = ModContent.Request<Texture2D>(ResourceManager.BackgroundTextures + "BackwoodsMid2").Value.Width;
        int num4 = (int)(width * (double)scale);
        float num1 = 1200f;
        float num2 = 1550f;
        Color color = typeof(Main).GetFieldValue<Color>("ColorOfSurfaceBackgroundsModified", Main.instance) * 1.35f;
        int num7 = Main.screenWidth / num4 + 2;
        float fieldValue1 = typeof(Main).GetFieldValue<float>("screenOff", Main.instance);
        float fieldValue2 = typeof(Main).GetFieldValue<float>("scAdj", Main.instance);
        float num3 = 0.07f;
        spriteBatch.Draw(texture, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), color);
        if (Main.screenPosition.Y < Main.worldSurface * 16.0 + 16.0) {
            for (int index2 = 0; index2 < num7; ++index2) {
                int num5 = (int)(-Math.IEEERemainder(Main.screenPosition.X * (double)num3, num4) - num4 / 2);
                int num6 = (int)((-(double)Main.screenPosition.Y + (double)fieldValue1 / 2.0) / (Main.worldSurface * 16.0) * (double)num1 + (double)num2) + (int)fieldValue2;
                if (Main.gameMenu) {
                    num6 = -500;
                }
                Vector2 position = new Vector2(num5 + num4 * index2, MathHelper.Clamp(num6, -50f, 0f));
                texture = ModContent.Request<Texture2D>(ResourceManager.BackgroundTextures + "BackwoodsMid2").Value;
                //spriteBatch.DrawSelf(texture, position, new Rectangle(0, 0, texture.Width * 3, texture.Height * 3), color);
                spriteBatch.Draw(texture, new Vector2(num5 + num4 * index2, MathHelper.Clamp(num6, -50f, 0f) + 500),
                    new Rectangle?(new Rectangle(0, 0, texture.Width, 1160)), color, 0.0f, new Vector2(), scale, SpriteEffects.None, 0.0f);
            }
        }
        float opacity = (color.R + color.G + color.B) / 255f / 3f;
        spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(30, 30, 30).MultiplyRGB(color) * 0.5f * opacity);
    }

    public override void ModifyFarFades(float[] fades, float transitionSpeed) {
        for (int index = 0; index < fades.Length; ++index) {
            if (index == Slot) {
                fades[index] += transitionSpeed;
                if (fades[index] > 1.0)
                    fades[index] = 1f;
            }
            else {
                fades[index] -= transitionSpeed;
                if (fades[index] < 0.0)
                    fades[index] = 0.0f;
            }
        }
    }
}