using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Magic;

[AutoloadEquip(EquipType.Legs)]
sealed class CopperAcolyteLeggings : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Acolyte Leggings");
        // Tooltip.SetDefault("+8% magic crititical strike chance");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.defense = 2;

        Item.value = Item.sellPrice(0, 0, 4, 0);
    }

    public override void UpdateEquip(Player player) => player.GetCritChance(DamageClass.Magic) += 8;
}