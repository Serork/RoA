using RoA.Content.Items.Weapons.Melee;
using RoA.Core.Defaults;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class FlederSlayerDecoration : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<FlederSlayer>();
    }

    public override void SetDefaults() {
        Item.SetSizeValues(20, 38);

        Item.CloneDefaults(ItemID.BloodMoonMonolith);
        Item.createTile = ModContent.TileType<Tiles.Decorations.FlederSlayerDecoration>();
        Item.placeStyle = 0;

        Item.accessory = false;
        Item.vanity = false;

        Item.rare = ItemRarityID.LightRed;

        Item.value = Item.sellPrice(0, 3, 50, 0);
    }
}
