using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Legs)]
sealed class SoapSellersJeans : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Soap Seller's Jeans");
        Item.ResearchUnlockCount = 1;

        ItemID.Sets.ShimmerTransformToItem[ItemID.FamiliarPants] = Type;
    }

    public override void SetDefaults() {
        int width = 22; int height = 16;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;

        Item.vanity = true;

        Item.value = Item.sellPrice(0, 0, 25, 0);
    }
}