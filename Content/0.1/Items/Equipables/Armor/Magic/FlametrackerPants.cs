using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Magic;

[AutoloadEquip(EquipType.Legs)]
sealed class FlametrackerPants : ModItem {
    public override void SetStaticDefaults() {
        // Tooltip.SetDefault("8% reduced mana usage\n5% increased magic damage");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;

        Item.defense = 6;

        Item.value = Item.sellPrice(0, 0, 60, 0);
    }

    public override void UpdateEquip(Player player) {
        player.manaCost -= 0.08f;
        player.GetDamage(DamageClass.Magic) += 0.05f;
    }
}