using Microsoft.Xna.Framework.Graphics;

using Terraria.ModLoader;

namespace RoA.Content.Tiles.Other;

sealed class BackwoodsNightMusicBox : MusicBox {
    protected override int CursorItemType => ModContent.ItemType<Items.Placeable.BackwoodsNightMusicBox>();

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        
    }
}