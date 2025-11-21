using Microsoft.Xna.Framework;

using RoA.Common.Items;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Magic;

[AutoloadEquip(EquipType.Head)]
public class TinAcolyteHat : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Acolyte Hat");
        // Tooltip.SetDefault("6% reduced mana usage");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        int width = 28; int height = 16;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.defense = 2;

        Item.value = Item.sellPrice(0, 0, 4, 50);
    }

    public override void UpdateEquip(Player player) => player.manaCost -= 0.06f;

    public override bool IsArmorSet(Item head, Item body, Item legs)
        => (body.type == ModContent.ItemType<CopperAcolyteJacket>() || body.type == ModContent.ItemType<TinAcolyteJacket>()) &&
        (legs.type == ModContent.ItemType<CopperAcolyteLeggings>() || legs.type == ModContent.ItemType<TinAcolyteLeggings>());

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.AcolyteSetBonus").Value;
        if (player.statMana <= 40) {
            player.GetDamage(DruidClass.Nature) += 0.4f;
        }
    }
}