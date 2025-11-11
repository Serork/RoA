using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Head)]
sealed class LivingWoodHelmet : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Living Wood Helmet");
        //Tooltip.SetDefault("2% increased potential speed");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 24; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 15, 0);

        Item.defense = 2;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<LivingWoodChestplate>() && legs.type == ModContent.ItemType<LivingWoodGreaves>();

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetTextValue("Mods.RoA.Items.Tooltips.LivingWoodSetBonus");
        player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier += 0.1f;
    }
}