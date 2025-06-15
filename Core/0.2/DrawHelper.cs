using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;

namespace RoA.Core;

static class DrawHelper {
    public readonly struct SingleTileDrawInfo(Texture2D texture, Point position, Rectangle clip, Color? color = null, SlopeType slope = SlopeType.Solid, bool isHalfBlock = false) {
        public readonly Texture2D Texture = texture;
        public readonly Point Position = position;
        public readonly Rectangle Clip = clip;
        public readonly Color Color = color ?? Color.White;
        public readonly SlopeType Slope = slope;
        public readonly bool IsHalfBlock = isHalfBlock;
    }

    // vanilla adapted
    public static void DrawSingleTile(in SingleTileDrawInfo singleTileInfo) {
        int num12 = (int)singleTileInfo.Slope;
        bool halfBlock = singleTileInfo.IsHalfBlock;
        Vector2 tilePositionToDraw = singleTileInfo.Position.ToWorldCoordinates() - Vector2.One * 8f;
        Texture2D tileTexture = singleTileInfo.Texture;
        if (num12 == 0 && !halfBlock) {
            Main.spriteBatch.Draw(tileTexture, tilePositionToDraw, DrawInfo.Default with {
                Color = singleTileInfo.Color,
                Clip = singleTileInfo.Clip
            });
        }
        else if (halfBlock) {
            Main.spriteBatch.Draw(tileTexture, tilePositionToDraw + Vector2.UnitY * 8f, DrawInfo.Default with {
                Color = singleTileInfo.Color,
                Clip = singleTileInfo.Clip.AdjustHeight(-singleTileInfo.Clip.Height / 2)
            });
        }
        else {
            int num13 = 2;
            for (int i2 = 0; i2 < 8; i2++) {
                int num14 = i2 * -2;
                int num15 = 16 - i2 * 2;
                int num16 = 16 - num15;
                int num17;
                switch (num12) {
                    case 1:
                        num14 = 0;
                        num17 = i2 * 2;
                        num15 = 14 - i2 * 2;
                        num16 = 0;
                        break;
                    case 2:
                        num14 = 0;
                        num17 = 16 - i2 * 2 - 2;
                        num15 = 14 - i2 * 2;
                        num16 = 0;
                        break;
                    case 3:
                        num17 = i2 * 2;
                        break;
                    default:
                        num17 = 16 - i2 * 2 - 2;
                        break;
                }
                Main.spriteBatch.Draw(tileTexture, tilePositionToDraw + new Vector2(num17, i2 * num13 + num14), DrawInfo.Default with {
                    Color = singleTileInfo.Color,
                    Clip = new Rectangle(singleTileInfo.Clip.X + num17, singleTileInfo.Clip.Y + num16, num13, num15)
                });
            }
            int num18 = ((num12 <= 2) ? 14 : 0);
            Main.spriteBatch.Draw(tileTexture, tilePositionToDraw + new Vector2(0f, num18), DrawInfo.Default with {
                Color = singleTileInfo.Color,
                Clip = new Rectangle(singleTileInfo.Clip.X, singleTileInfo.Clip.Y + num18, 16, 2)
            });
        }
    }
}
