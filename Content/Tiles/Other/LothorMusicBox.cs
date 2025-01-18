using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Other;

sealed class LothorMusicBox : MusicBox {
    protected override int CursorItemType => ModContent.ItemType<Items.Special.Lothor.LothorMusicBox>();

    protected override int GoreOffsetX => 4;

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX > 18) {
            float value = 1f;
            Lighting.AddLight(new Vector2(i, j).ToWorldCoordinates(), new Vector3(1f, 0.2f, 0.2f) * value * 0.75f);
        }
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        int height = tile.TileFrameY == 36 ? 18 : 16;
        Main.spriteBatch.Draw(ModContent.Request<Texture2D>(Texture + "_Glow").Value,
                              new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2) + zero,
                              new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
                              Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }
}