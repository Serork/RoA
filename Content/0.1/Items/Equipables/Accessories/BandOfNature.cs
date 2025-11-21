using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

[AutoloadEquip(EquipType.HandsOn)]
sealed class BandOfNature : NatureItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Band of Nature_Claws");
        //Tooltip.SetDefault("Increases nature potential damage by 5%" + "\nIncreases defense by 3, when wreath is charged");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    protected override void SafeSetDefaults() {
        int width = 28; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.accessory = true;

        Item.value = Item.sellPrice(0, 1, 0, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier += 0.15f;
    }
}
