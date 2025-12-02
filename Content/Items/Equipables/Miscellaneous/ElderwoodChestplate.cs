using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Body)]
sealed class ElderwoodChestplate : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Elderwood Chestplate");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 30; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.White;
        Item.value = Item.sellPrice(silver: 7);

        Item.defense = 3;
    }
}