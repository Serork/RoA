using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.GameContent.Drawing;

namespace RoA.Common.Tiles;

sealed class TileHooks {
    public interface IGetTileDrawData {
        void GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor);
    }

    public interface IGlobalRandomUpdate {
        public void OnGlobalRandomUpdate(int i, int j);
    }

    public interface ITileFluentlyDrawn {
        public struct BasicDrawInfo {
            public Vector2 DrawCenterPos;
            public SpriteBatch SpriteBatch;
            public TileDrawing TileDrawing;
        }

        void FluentDraw(Vector2 screenPosition, Point pos, SpriteBatch spriteBatch, TileDrawing tileDrawing);
    }

    public interface ITileFlameData {
        public struct TileFlameData {
            public Texture2D flameTexture;
            public ulong flameSeed;
            public int flameCount;
            public Color flameColor;
            public int flameRangeXMin;
            public int flameRangeXMax;
            public int flameRangeYMin;
            public int flameRangeYMax;
            public float flameRangeMultX;
            public float flameRangeMultY;
        }

        TileFlameData GetTileFlameData(int tileX, int tileY, int type, int tileFrameY);
    }
}
