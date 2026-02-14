using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

using ReLogic.Graphics;
using RoA.Core;

namespace RoA.Common.CombatTexts;

sealed class CustomCombatTextLoader : ILoadable {
    public static CustomCombatText[] customCombatText = new CustomCombatText[100];
    public static bool init;

    void ILoadable.Load(Mod mod) {
        for (int num4 = 0; num4 < 100; num4++) {
            customCombatText[num4] = new CustomCombatText();
        }

        init = true;

        On_CombatText.UpdateCombatText += On_CombatText_UpdateCombatText;
        On_Main.DrawPlayerChatBubbles += On_Main_DrawPlayerChatBubbles;
        On_CombatText.clearAll += On_CombatText_clearAll;
    }

    private void On_CombatText_clearAll(On_CombatText.orig_clearAll orig) {
        orig();

        if (!init) {
            return;
        }
        CustomCombatText.clearAll();
    }

    private void On_Main_DrawPlayerChatBubbles(On_Main.orig_DrawPlayerChatBubbles orig, Main self) {
        orig(self);

        if (!init) {
            return;
        }

        SpriteBatch spriteBatch = Main.spriteBatch;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.GameViewMatrix.ZoomMatrix);
        float targetScale = CombatText.TargetScale;
        for (int l = 0; l < 100; l++) {
            if (!customCombatText[l].active)
                continue;

            int num10 = 0;
            if (customCombatText[l].crit)
                num10 = 1;

            Vector2 vector2 = FontAssets.CombatText[num10].Value.MeasureString(customCombatText[l].text);
            Vector2 origin = new Vector2(vector2.X * 0.5f, vector2.Y * 0.5f);
            float num11 = customCombatText[l].scale / targetScale;
            float num12 = (int)customCombatText[l].color.R;
            float num13 = (int)customCombatText[l].color.G;
            float num14 = (int)customCombatText[l].color.B;
            float num15 = (int)customCombatText[l].color.A;
            num12 *= num11 * customCombatText[l].alpha * 0.3f;
            num14 *= num11 * customCombatText[l].alpha * 0.3f;
            num13 *= num11 * customCombatText[l].alpha * 0.3f;
            num15 *= num11 * customCombatText[l].alpha;
            Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color((int)num12, (int)num13, (int)num14, (int)num15);
            for (int m = 0; m < 5; m++) {
                float num16 = 0f;
                float num17 = 0f;
                switch (m) {
                    case 0:
                        num16 -= targetScale;
                        break;
                    case 1:
                        num16 += targetScale;
                        break;
                    case 2:
                        num17 -= targetScale;
                        break;
                    case 3:
                        num17 += targetScale;
                        break;
                    default:
                        num12 = (float)(int)customCombatText[l].color.R * num11 * customCombatText[l].alpha;
                        num14 = (float)(int)customCombatText[l].color.B * num11 * customCombatText[l].alpha;
                        num13 = (float)(int)customCombatText[l].color.G * num11 * customCombatText[l].alpha;
                        num15 = (float)(int)customCombatText[l].color.A * num11 * customCombatText[l].alpha;
                        color = new Microsoft.Xna.Framework.Color((int)num12, (int)num13, (int)num14, (int)num15);
                        break;
                }

                if (Main.player[Main.myPlayer].gravDir == -1f) {
                    float num18 = customCombatText[l].position.Y - Main.screenPosition.Y;
                    num18 = (float)Main.screenHeight - num18;
                    spriteBatch.DrawString(FontAssets.CombatText[num10].Value, customCombatText[l].text, new Vector2(customCombatText[l].position.X - Main.screenPosition.X + num16 + origin.X, num18 + num17 + origin.Y), color, customCombatText[l].rotation, origin, customCombatText[l].scale, SpriteEffects.None, 0f);
                }
                else {
                    spriteBatch.DrawString(FontAssets.CombatText[num10].Value, customCombatText[l].text, new Vector2(customCombatText[l].position.X - Main.screenPosition.X + num16 + origin.X, customCombatText[l].position.Y - Main.screenPosition.Y + num17 + origin.Y), color, customCombatText[l].rotation, origin, customCombatText[l].scale, SpriteEffects.None, 0f);
                }
            }
        }

        targetScale = PopupText.TargetScale;
        if (targetScale == 0f)
            targetScale = 1f;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, null, null, null, Main.UIScaleMatrix);
    }

    private void On_CombatText_UpdateCombatText(On_CombatText.orig_UpdateCombatText orig) {
        orig();

        if (!init) {
            return;
        }
        CustomCombatText.UpdateCombatText();
    }

    void ILoadable.Unload() {

    }
}

public class CustomCombatText {
    public static readonly Color DamagedFriendly = new Color(255, 80, 90, 255);
    public static readonly Color DamagedFriendlyCrit = new Color(255, 100, 30, 255);
    public static readonly Color DamagedHostile = new Color(255, 160, 80, 255);
    public static readonly Color DamagedHostileCrit = new Color(255, 100, 30, 255);
    public static readonly Color OthersDamagedHostile = DamagedHostile * 0.4f;
    public static readonly Color OthersDamagedHostileCrit = DamagedHostileCrit * 0.4f;
    public static readonly Color HealLife = new Color(100, 255, 100, 255);
    public static readonly Color HealMana = new Color(100, 100, 255, 255);
    public static readonly Color LifeRegen = new Color(255, 60, 70, 255);
    public static readonly Color LifeRegenNegative = new Color(255, 140, 40, 255);

    public static readonly Color AmmoReceiveRed = new Color(255, 93, 95);
    public static readonly Color AmmoReceiveOrange = new Color(255, 130, 51);
    public static readonly Color AmmorReceiveLightOrange = new Color(255, 168, 5);
    public static readonly Color AmmoReceiveYellow = new Color(255, 243, 12);
    public static readonly Color AmmoReceiveGreen = new Color(32, 248, 108);

    public Vector2 position;
    public Vector2 velocity;
    public float alpha;
    public int alphaDir = 1;
    public string text = "";
    public float scale = 1f;
    public float rotation;
    public Color color;
    public bool active;
    public int lifeTime;
    public bool crit;
    public bool dot;
    public bool customAmmoReceive;

    public static float TargetScale => 1f;

    public static int NewText(Rectangle location, Color color, int amount, bool dramatic = false, bool dot = false, bool customAmmoReceive = false) => NewText(location, color, amount.ToString(), dramatic, dot, customAmmoReceive);

    public static int NewText(Rectangle location, Color color, string text, bool dramatic = false, bool dot = false, bool customAmmoReceive = false) {
        if (Main.netMode == 2)
            return 100;

        if (!CustomCombatTextLoader.init) {
            return 100;
        }

        for (int i = 0; i < 100; i++) {
            if (CustomCombatTextLoader.customCombatText[i].active)
                continue;

            int num = 0;
            if (dramatic)
                num = 1;

            Vector2 vector = FontAssets.CombatText[num].Value.MeasureString(text);
            CustomCombatTextLoader.customCombatText[i].alpha = 1f;
            CustomCombatTextLoader.customCombatText[i].alphaDir = -1;
            CustomCombatTextLoader.customCombatText[i].active = true;
            CustomCombatTextLoader.customCombatText[i].scale = 0f;
            CustomCombatTextLoader.customCombatText[i].rotation = 0f;
            CustomCombatTextLoader.customCombatText[i].position.X = (float)location.X + (float)location.Width * 0.5f - vector.X * 0.5f;
            CustomCombatTextLoader.customCombatText[i].position.Y = (float)location.Y + (float)location.Height * 0.25f - vector.Y * 0.5f;
            CustomCombatTextLoader.customCombatText[i].position.X += Main.rand.Next(-(int)((double)location.Width * 0.5), (int)((double)location.Width * 0.5) + 1);
            CustomCombatTextLoader.customCombatText[i].position.Y += Main.rand.Next(-(int)((double)location.Height * 0.5), (int)((double)location.Height * 0.5) + 1);
            CustomCombatTextLoader.customCombatText[i].color = color;
            CustomCombatTextLoader.customCombatText[i].text = text;
            CustomCombatTextLoader.customCombatText[i].velocity.Y = -7f;
            if (Main.player[Main.myPlayer].gravDir == -1f) {
                CustomCombatTextLoader.customCombatText[i].velocity.Y *= -1f;
                CustomCombatTextLoader.customCombatText[i].position.Y = (float)location.Y + (float)location.Height * 0.75f + vector.Y * 0.5f;
            }

            CustomCombatTextLoader.customCombatText[i].lifeTime = 60;
            CustomCombatTextLoader.customCombatText[i].crit = dramatic;
            CustomCombatTextLoader.customCombatText[i].dot = dot;
            if (dramatic) {
                CustomCombatTextLoader.customCombatText[i].text = text;
                CustomCombatTextLoader.customCombatText[i].lifeTime *= 2;
                CustomCombatTextLoader.customCombatText[i].velocity.Y *= 2f;
                CustomCombatTextLoader.customCombatText[i].velocity.X = (float)Main.rand.Next(-25, 26) * 0.05f;
                CustomCombatTextLoader.customCombatText[i].rotation = (float)(CustomCombatTextLoader.customCombatText[i].lifeTime / 2) * 0.002f;
                if (CustomCombatTextLoader.customCombatText[i].velocity.X < 0f)
                    CustomCombatTextLoader.customCombatText[i].rotation *= -1f;
            }

            if (dot) {
                CustomCombatTextLoader.customCombatText[i].velocity.Y = -4f;
                CustomCombatTextLoader.customCombatText[i].lifeTime = 40;
            }

            CustomCombatTextLoader.customCombatText[i].customAmmoReceive = customAmmoReceive;

            return i;
        }

        return 100;
    }

    public static void clearAll() {
        for (int i = 0; i < 100; i++) {
            CustomCombatTextLoader.customCombatText[i].active = false;
        }
    }

    public void Update() {
        if (!active)
            return;

        float targetScale = TargetScale;
        alpha += (float)alphaDir * 0.05f;
        if ((double)alpha <= 0.6)
            alphaDir = 1;

        if (alpha >= 1f) {
            alpha = 1f;
            alphaDir = -1;
        }

        if (dot) {
            velocity.Y += 0.15f;
        }
        else {
            if (customAmmoReceive) {
                velocity.Y *= 0.92f;
                velocity.Y *= 0.92f + 0.08f * Ease.CubeIn(lifeTime / 60f);

                velocity.Y += 0.15f * Ease.CubeIn(1f - lifeTime / 60f);
            }
            else {
                velocity.Y *= 0.92f;
                if (crit)
                    velocity.Y *= 0.92f;
            }
        }

        velocity.X *= 0.93f;
        position += velocity;
        lifeTime--;
        if (lifeTime <= 0) {
            scale -= 0.1f * targetScale;
            if ((double)scale < 0.1)
                active = false;

            lifeTime = 0;
            if (crit) {
                alphaDir = -1;
                scale += 0.07f * targetScale;
            }

            return;
        }

        if (crit) {
            if (velocity.X < 0f)
                rotation += 0.001f;
            else
                rotation -= 0.001f;
        }

        if (dot) {
            scale += 0.5f * targetScale;
            if ((double)scale > 0.8 * (double)targetScale)
                scale = 0.8f * targetScale;

            return;
        }

        if (scale < targetScale)
            scale += 0.1f * targetScale;

        if (scale > targetScale)
            scale = targetScale;
    }

    public static void UpdateCombatText() {
        for (int i = 0; i < 100; i++) {
            if (CustomCombatTextLoader.customCombatText[i].active)
                CustomCombatTextLoader.customCombatText[i].Update();
        }
    }
}
