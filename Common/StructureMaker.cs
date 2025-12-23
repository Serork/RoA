using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common;

sealed class StructureMaker : ModSystem {
    private static bool _captureActive = false, _captureReleased = false;
    private static Point16 _firstCapturedTile = Point16.Zero, _secondCapturedTile = Point16.Zero;

    private static bool StartedCapturing => _firstCapturedTile != Point16.Zero;

    public override void PostUpdateInput() {
        if (!Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Z)) {
            _captureReleased = false;
            return;
        }
        if (_captureReleased) {
            return;
        }
        _captureReleased = true;
        _captureActive = !_captureActive;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        string layerName = "Vanilla: Ruler";
        int rulerLayerIndex = layers.FindIndex(layer => layer.Name == layerName);
        if (rulerLayerIndex < 0) {
            return;
        }
        string structureMakerLayerName = "Structure Layer Layout";
        layers.Insert(rulerLayerIndex, new LegacyGameInterfaceLayer(
           structureMakerLayerName,
            delegate {
                DrawStructureLayerLayout();
                return true;
            },
            InterfaceScaleType.UI)
        );
    }

    private void DrawStructureLayerLayout() {
        if (!_captureActive) {
            return;
        }

        float num = Main.LocalPlayer.velocity.Length();
        float num2 = 6f;
        float num3 = 2f;
        if (!(num <= num2))
            return;

        Main.LocalPlayer.mouseInterface = true;

        if (Main.mouseLeft && !StartedCapturing) {
            _firstCapturedTile = new Point16(Player.tileTargetX, Player.tileTargetY);
        }
        if (!Main.mouseLeft && StartedCapturing) {
            _firstCapturedTile = _secondCapturedTile = Point16.Zero;
        }
        if (StartedCapturing) {
            _secondCapturedTile = new Point16(Player.tileTargetX, Player.tileTargetY);
        }

        float num4 = 1f;
        if (num >= num3)
            num4 = 1f - (num - num3) / num2;

        int num5 = 1;
        if ((float)Main.mouseX + Main.screenPosition.X < Main.player[Main.myPlayer].Center.X)
            num5 = -1;

        int num6 = (int)(Main.player[Main.myPlayer].position.X + (float)(Main.player[Main.myPlayer].width / 2)) / 16;
        int num7 = (int)(Main.player[Main.myPlayer].position.Y + (float)Main.player[Main.myPlayer].height - 2f) / 16;
        if (Main.player[Main.myPlayer].gravDir == -1f)
            num7--;

        Vector2 mouseWorld = Main.MouseWorld;
        if (Math.Abs(num6 - (int)(Main.MouseWorld.X / 16f)) > 0)
            num6 += num5;

        if (Main.player[Main.myPlayer].gravDir == -1f)
            mouseWorld.Y += 16f;

        mouseWorld /= 16f;
        new Vector2(num6, num7);
        int num8 = (int)mouseWorld.X;
        int num9 = (int)mouseWorld.Y;
        Math.Abs(num8);
        Math.Abs(num9);
        if (num8 == 0 && num9 == 0)
            return;

        Texture2D value = TextureAssets.Extra[2].Value;
        Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 16);
        int num10 = num6;
        int num11 = num7;
        if (Main.player[Main.myPlayer].gravDir == -1f)
            num11--;

        float r = 0.8f;
        float g = 0.8f;
        float b = 0.2f;
        float a = 1f;
        float num12 = 0.8f;
        Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color(r, g, b, a) * num12 * num4;

        void drawCapturedTile(Vector2 position) {
            position -= Vector2.One * 8f;
            Main.spriteBatch.Draw(value, Main.ReverseGravitySupport(position - Main.screenPosition, 16f), value2, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        if (!StartedCapturing) {
            drawCapturedTile(new Point16(Player.tileTargetX, Player.tileTargetY).ToWorldCoordinates());
            drawCapturedTile(_firstCapturedTile.ToWorldCoordinates());
            drawCapturedTile(_secondCapturedTile.ToWorldCoordinates());
        }

        int startX = Math.Min(_secondCapturedTile.X, _firstCapturedTile.X),
            startY = Math.Min(_secondCapturedTile.Y, _firstCapturedTile.Y);
        int endX = Math.Max(_secondCapturedTile.X, _firstCapturedTile.X),
            endY = Math.Max(_secondCapturedTile.Y, _firstCapturedTile.Y);
        for (int i = startX; i <= endX; i++) {
            for (int j = startY; j <= endY; j++) {
                drawCapturedTile(new Point16(i, j).ToWorldCoordinates());
            }
        }
    }
}
