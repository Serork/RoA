using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Head)]

sealed class LivingBorealWoodHelmet : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Living Boreal Wood Helmet");
        //Tooltip.SetDefault("4% increased nature potential damage");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 24; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 15, 0);

        Item.defense = 1;
    }

    public override void UpdateEquip(Player player) => player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.03f;

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<LivingBorealWoodChestplate>() && legs.type == ModContent.ItemType<LivingBorealWoodGreaves>();

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetTextValue("Mods.RoA.Items.Tooltips.LivingBorealSetBonus");
        player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier += 0.1f;
    }
}