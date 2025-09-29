using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.Waist)]
sealed class GiantTreeSapling : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Giant Tree Sapling");
        //Tooltip.SetDefault("Increases wreath filling rate by 10%\nIncreases maximum life by 20 while wreath is charged");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 28; int height = 32;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.accessory = true;

        Item.value = Item.sellPrice(0, 0, 75, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<DruidStats>().DischargeTimeDecreaseMultiplier -= 0.15f;
        if (player.GetWreathHandler().IsFull1) {
            player.statDefense += 5;
        }
    }
}