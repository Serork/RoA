using RoA.Common.Druid;
using RoA.Core.Defaults;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class DruidEmblem : NatureItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[ItemID.SummonerEmblem] = Type;
        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.SorcererEmblem;
        ItemID.Sets.ShimmerTransformToItem[ItemID.SorcererEmblem] = ItemID.RangerEmblem;
        ItemID.Sets.ShimmerTransformToItem[ItemID.RangerEmblem] = ItemID.WarriorEmblem;
        ItemID.Sets.ShimmerTransformToItem[ItemID.WarriorEmblem] = ItemID.SummonerEmblem;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(28);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.LightRed4, Item.sellPrice(0, 2, 0, 0));

        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDamage(DruidClass.Nature) += 0.15f;
        player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.15f;
    }
}
