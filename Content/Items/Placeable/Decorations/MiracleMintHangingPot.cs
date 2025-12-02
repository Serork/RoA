using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class MiracleMintHangingPot : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Decorations.MiracleMintHangingPot>(), 0);
        Item.value = Item.sellPrice(0, 0, 25);
        Item.Size = new Microsoft.Xna.Framework.Vector2(16, 36);
    }
}