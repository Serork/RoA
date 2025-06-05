using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Magic;

[AutoloadEquip(EquipType.Body)]
sealed class CopperAcolyteJacket : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Acolyte Jacket");
        // Tooltip.SetDefault("+40 maximum mana");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 26; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.defense = 3;

        Item.value = Item.sellPrice(0, 0, 5, 0);
    }

    public override void UpdateEquip(Player player) => player.statManaMax2 += 40;
}