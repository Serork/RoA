using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.HandsOn)]
sealed class BandOfPurity : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Band of Purity");
        //Tooltip.SetDefault("Increases nature potential damage by 5%" + "\nRegenerates life while wreath is charged");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 28; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.accessory = true;

        Item.value = Item.sellPrice(0, 1, 50, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier += 0.2f;
        if (player.GetWreathHandler().IsFull1) {
            player.lifeRegen += 10;
        }
    }
}
