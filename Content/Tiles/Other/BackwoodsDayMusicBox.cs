using Terraria.ModLoader;

namespace RoA.Content.Tiles.Other;

sealed class BackwoodsDayMusicBox : MusicBox {
    protected override int CursorItemType => ModContent.ItemType<Items.Placeable.BackwoodsDayMusicBox>();
}