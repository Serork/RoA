using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Body)]
sealed class SoapSellersJacket : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Soap Seller's Jacket");
        Item.ResearchUnlockCount = 1;

        ItemID.Sets.ShimmerTransformToItem[ItemID.FamiliarShirt] = Type;
    }

    public override void SetDefaults() {
        int width = 24; int height = width;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;

        Item.vanity = true;

        Item.value = Item.sellPrice(0, 0, 25, 0);
    }
}