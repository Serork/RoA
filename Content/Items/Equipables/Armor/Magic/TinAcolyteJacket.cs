using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Magic;

[AutoloadEquip(EquipType.Body)]
sealed class TinAcolyteJacket : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Acolyte Jacket");
        // Tooltip.SetDefault("+40 maximum mana");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 26; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.value = Item.sellPrice(silver: 60);
        Item.rare = ItemRarityID.Blue;
        Item.defense = 3;
    }

    public override void UpdateEquip(Player player) => player.statManaMax2 += 40;
}