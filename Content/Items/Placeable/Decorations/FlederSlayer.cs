using RoA.Content.Items.Weapons.Melee;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class FlederSlayerDecoration : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<FlederSlayer>();
    }

    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.BloodMoonMonolith);
        Item.createTile = ModContent.TileType<Tiles.Decorations.FlederSlayerDecoration>();
        Item.placeStyle = 0;
    }
}
