using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Summon;

[AutoloadEquip(EquipType.Legs)]
sealed class WorshipperGarb : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Worshipper Garb");
        // Tooltip.SetDefault("8% increased minion damage" + "\n10% increased movement speed");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 26; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;

        Item.defense = 5;


        Item.value = Item.sellPrice(0, 0, 60, 0);
    }

    public override void UpdateEquip(Player player) {
        player.GetDamage(DamageClass.Summon) += 0.08f;
        player.moveSpeed += 0.1f;
    }
}