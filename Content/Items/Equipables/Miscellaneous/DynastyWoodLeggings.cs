using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Legs)]
sealed class DynastyWoodLeggings : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Dynasty Wood Greaves");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.White;
        Item.value = Item.sellPrice(0, 0, 2, 0);

        Item.defense = 2;
    }
}