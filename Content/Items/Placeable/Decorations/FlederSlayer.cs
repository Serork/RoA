using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class FlederSlayerDecoration : ModItem {
    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.BloodMoonMonolith);
        Item.createTile = ModContent.TileType<Tiles.Decorations.FlederSlayerDecoration>();
        Item.placeStyle = 0;
    }
}
