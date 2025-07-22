using Terraria.ModLoader;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class BackwoodsDayMusicBox : MusicBox {
    protected override int CursorItemType => ModContent.ItemType<Items.Placeable.MusicBoxes.BackwoodsDayMusicBox>();
}