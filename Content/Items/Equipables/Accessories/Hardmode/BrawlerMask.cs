using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

[AutoloadEquip(EquipType.Face)]
sealed class BrawlerMask : ModItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Face.Sets.PreventHairDraw[Item.faceSlot] = true;
    }

    public override void SetDefaults() {
        Item.DefaultToAccessory(32, 26);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsBrawlerMaskEffectActive = true;
    }
}
