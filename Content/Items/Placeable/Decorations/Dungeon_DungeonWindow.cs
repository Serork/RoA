using RoA.Core.Defaults;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class DungeonWindow : ModItem {
    public override void SetDefaults() {
        Item.SetSizeValues(16, 16);
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Decorations.DungeonWindow>());
    }
}
