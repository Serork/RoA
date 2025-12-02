using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.WorldEvents;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace RoA.Common.Tiles;

sealed class TileDrawingExtra : ILoadable {
    public static Color BackwoodsMossGlowColor => new Color(0, 180, 250, 0) * 0.5f * BackwoodsFogHandler.Opacity;

    public void Load(Mod mod) {
        On_TileDrawing.GetTileDrawData += On_TileDrawing_GetTileDrawData;

        TileHelper.Load();
    }

    public void Unload() {
        TileHelper.Unload();
    }

    private static void On_TileDrawing_GetTileDrawData(On_TileDrawing.orig_GetTileDrawData orig, TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, out int tileWidth, out int tileHeight, out int tileTop, out int halfBrickHeight, out int addFrX, out int addFrY, out SpriteEffects tileSpriteEffect, out Texture2D glowTexture, out Rectangle glowSourceRect, out Color glowColor) {
        orig(self, x, y, tileCache, typeCache, ref tileFrameX, ref tileFrameY, out tileWidth, out tileHeight, out tileTop, out halfBrickHeight, out addFrX, out addFrY, out tileSpriteEffect, out glowTexture, out glowSourceRect, out glowColor);

        if (TileLoader.GetTile(typeCache) is TileHooks.IGetTileDrawData getTileDrawData) {
            getTileDrawData.GetTileDrawData(self, x, y, tileCache, typeCache, ref tileFrameX, ref tileFrameY, ref tileWidth, ref tileHeight, ref tileTop, ref halfBrickHeight, ref addFrX, ref addFrY, ref tileSpriteEffect, ref glowTexture, ref glowSourceRect, ref glowColor);
        }
        //glowColor = BackwoodsMossGlowColor;
        //glowSourceRect = new Rectangle(tileFrameX, tileFrameY, tileWidth, tileHeight);
    }
}
