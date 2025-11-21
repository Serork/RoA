using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Items;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature;

[AutoloadEquip(EquipType.Head)]
sealed class LivingMahoganyHelmet : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Living Mahogany Helmet");
        //Tooltip.SetDefault("4% increased nature critical strike chance");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 26; int height = 26;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 15, 0);

        Item.defense = 1;
    }

    public override void UpdateEquip(Player player) => player.GetAttackSpeed(DruidClass_Claws.Nature) += 0.04f; //player.GetCritChance(DruidClass.Nature) += 3;

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<LivingMahoganyChestplate>() && legs.type == ModContent.ItemType<LivingMahoganyGreaves>();

    public override void UpdateArmorSet(Player player) {
        player.setBonus = Language.GetText("Mods.RoA.Items.Tooltips.LivingMahoganySetBonus").WithFormatArgs(Helper.ArmorSetBonusKey).Value;
        player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier += 0.1f;
    }
}