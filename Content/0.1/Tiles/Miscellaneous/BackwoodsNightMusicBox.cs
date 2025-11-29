using Microsoft.Xna.Framework.Graphics;

using Terraria.ModLoader;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class BackwoodsNightMusicBox : MusicBox {
    protected override int CursorItemType => ModContent.ItemType<Items.Placeable.MusicBoxes.BackwoodsNightMusicBox>();
}