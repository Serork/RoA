using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Head)]

sealed class LivingPalmHelmet : NatureItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 30; int height = 26;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 15, 0);

        Item.defense = 1;
    }

    public override void UpdateEquip(Player player) => player.GetDamage(DruidClass.Nature) += 0.03f;

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<LivingPalmChestplate>() && legs.type == ModContent.ItemType<LivingPalmGreaves>();

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetTextValue("Mods.RoA.Items.Tooltips.LivingPalmSetBonus");
        player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier += 0.1f;
    }
}