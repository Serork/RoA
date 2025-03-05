using RoA.Content.Tiles.Decorations;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class LothorEnrageMonolith : ModItem {
    public override void SetDefaults() {
        Item.CloneDefaults(ItemID.BloodMoonMonolith);
        Item.accessory = true;
        Item.vanity = true;
        Item.hasVanityEffects = true;
        Item.createTile = ModContent.TileType<Tiles.Decorations.LothorEnrageMonolith>();
        Item.placeStyle = 0;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<EnragedVisuals>()._isActive2 = true;
    }

    public override void UpdateVanity(Player player) {
        player.GetModPlayer<EnragedVisuals>()._isActive2 = true;
    }
}