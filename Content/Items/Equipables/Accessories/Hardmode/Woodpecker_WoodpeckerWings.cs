using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

[AutoloadEquip(EquipType.Wings)]
sealed class WoodpeckerWings : ModItem {
    public override void SetStaticDefaults() {
        int flyTime3 = 75;
        float flySpeedOverride5 = 6.75f;
        ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(flyTime3, flySpeedOverride5);
    }

    public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
    ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
        maxAscentMultiplier *= 2f;
    }

    public override void SetDefaults() {
        Item.width = 26;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(0, 40);
        Item.rare = ItemRarityID.Pink;
    }
}
