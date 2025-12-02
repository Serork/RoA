using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace RoA.Common.Players;

sealed partial class PlayerCommon : ModPlayer {
    public bool IsBloodCursorEffectActive;
    public float DistanceProgressToRootboundChest;

    public partial void CursorEffectsResetEffects() {
        IsBloodCursorEffectActive = false;

        DistanceProgressToRootboundChest = 0f;
    }

    public partial void CursorEffectsLoad() {
        On_Main.DrawCursor += On_Main_DrawCursor;
    }

    private static void On_Main_DrawCursor(On_Main.orig_DrawCursor orig, Vector2 bonus, bool smart) {
        if (Main.gameMenu && Main.alreadyGrabbingSunOrMoon)
            return;

        if (Main.player[Main.myPlayer].dead || Main.player[Main.myPlayer].mouseInterface) {
            Main.ClearSmartInteract();
            Main.TileInteractionLX = (Main.TileInteractionHX = (Main.TileInteractionLY = (Main.TileInteractionHY = -1)));
        }

        Microsoft.Xna.Framework.Color color = Main.cursorColor;
        if (!Main.gameMenu && Main.LocalPlayer.hasRainbowCursor)
            color = Main.hslToRgb(Main.GlobalTimeWrappedHourly * 0.25f % 1f, 1f, 0.5f);

        bool bloodCursor = false;
        bool shouldDrawRootBoundEffect = false;
        if (!Main.gameMenu) {
            var handler = Main.LocalPlayer.GetCommon();
            if (handler.IsBloodCursorEffectActive) {
                bloodCursor = true;
            }
            if (handler.DistanceProgressToRootboundChest > 0f) {
                shouldDrawRootBoundEffect = true;
            }
        }

        if (shouldDrawRootBoundEffect) {

        }

        if (!bloodCursor) {
            drawCursor(Vector2.Zero);
        }
        else {
            drawCursor(Vector2.Zero);
            SpriteBatchSnapshot snapshot = Main.spriteBatch.CaptureSnapshot();
            Main.spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
            for (float i = -MathHelper.Pi; i <= MathHelper.Pi; i += MathHelper.PiOver2) {
                drawCursor(Utils.RotatedBy(Utils.ToRotationVector2(i), Main.GlobalTimeWrappedHourly * 10.0, new Vector2())
                        * Helper.Wave(0f, 3f, 12f, 0.5f) * 1f,
                        Main.rand.NextFloatRange(0.05f) * 1f,
                        Helper.Wave(0.5f, 0.75f, 12f, 0.5f) * 1f);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(in snapshot);
        }

        void drawCursor(Vector2 extraPosition, float extraRotation = 0f, float alpha = 1f) {
            bool flag = UILinkPointNavigator.Available && !PlayerInput.InBuildingMode;
            float rotation = 0f + extraRotation;
            if (PlayerInput.SettingsForUI.ShowGamepadCursor) {
                if ((Main.player[Main.myPlayer].dead && !Main.player[Main.myPlayer].ghost && !Main.gameMenu) || PlayerInput.InvisibleGamepadInMenus)
                    return;

                Vector2 t = new Vector2(Main.mouseX, Main.mouseY);
                Vector2 t2 = Vector2.Zero;
                bool flag2 = Main.SmartCursorIsUsed;
                if (flag2) {
                    PlayerInput.smartSelectPointer.UpdateCenter(Main.ScreenSize.ToVector2() / 2f);
                    t2 = PlayerInput.smartSelectPointer.GetPointerPosition();
                    if (Vector2.Distance(t2, t) < 1f)
                        flag2 = false;
                    else
                        Utils.Swap(ref t, ref t2);
                }

                float num = 1f;
                if (flag2) {
                    num = 0.3f;
                    color = Microsoft.Xna.Framework.Color.White * Main.GamepadCursorAlpha;
                    int num2 = 17;
                    int frameX = 0;
                    Main.spriteBatch.Draw(TextureAssets.Cursors[num2].Value, t2 + bonus + extraPosition, TextureAssets.Cursors[num2].Frame(1, 1, frameX), color.MultiplyAlpha(alpha), rotation + (float)Math.PI / 2f * Main.GlobalTimeWrappedHourly, TextureAssets.Cursors[num2].Frame(1, 1, frameX).Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);
                }

                if (smart && !flag) {
                    color = Microsoft.Xna.Framework.Color.White * Main.GamepadCursorAlpha * num;
                    int num3 = 13;
                    int frameX2 = 0;
                    Main.spriteBatch.Draw(TextureAssets.Cursors[num3].Value, t + bonus + extraPosition, TextureAssets.Cursors[num3].Frame(2, 1, frameX2), color.MultiplyAlpha(alpha), rotation, TextureAssets.Cursors[num3].Frame(2, 1, frameX2).Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);
                }
                else {
                    color = Microsoft.Xna.Framework.Color.White;
                    int num4 = 15;
                    Main.spriteBatch.Draw(TextureAssets.Cursors[num4].Value, new Vector2(Main.mouseX, Main.mouseY) + bonus + extraPosition, null, color.MultiplyAlpha(alpha), rotation, TextureAssets.Cursors[num4].Value.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);
                }
            }
            else {
                int num5 = smart.ToInt();
                Main.spriteBatch.Draw(TextureAssets.Cursors[num5].Value, new Vector2(Main.mouseX, Main.mouseY) + bonus + extraPosition + Vector2.One, null, new Microsoft.Xna.Framework.Color((int)((float)(int)color.R * 0.2f), (int)((float)(int)color.G * 0.2f), (int)((float)(int)color.B * 0.2f), (int)((float)(int)color.A * 0.5f)), rotation, default(Vector2), Main.cursorScale * 1.1f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(TextureAssets.Cursors[num5].Value, new Vector2(Main.mouseX, Main.mouseY) + bonus + extraPosition, null, color.MultiplyAlpha(alpha), rotation, default(Vector2), Main.cursorScale, SpriteEffects.None, 0f);
            }
        }
    }
}
