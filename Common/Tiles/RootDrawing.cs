using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Tiles;

sealed class RootsDrawing : GlobalTile {
    private static Asset<Texture2D> _rootsTexture = null!;

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _rootsTexture = ModContent.Request<Texture2D>(ResourceManager.AmbientTileTextures + "Roots");
    }

    public static bool[] ShouldDraw = TileID.Sets.Factory.CreateBoolSet();

    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j - 1);
        if (!ShouldDraw[tile.TileType]) {
            return;
        }

        DrawRoots(i, j, spriteBatch);
    }

    public static void DrawRoots(int i, int j, SpriteBatch spriteBatch) {
        byte frameCount = 3;
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        Color color = Lighting.GetColor(i, j);
        SpriteFrame frame = new(frameCount, 1);
        Asset<Texture2D> texture = _rootsTexture;
        Rectangle rectangle = frame.GetSourceRectangle(texture.Value);
        int width = texture.Width() / frameCount;
        ulong seedForRandomness = (ulong)((long)j << 32 | (long)(uint)i);
        rectangle.X = width * (byte)Utils.RandomInt(ref seedForRandomness, 3);
        spriteBatch.Draw(texture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X - 2, j * 16 - (int)Main.screenPosition.Y) + zero, rectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
    }
}