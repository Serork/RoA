using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature.Hardmode;

[AutoloadEquip(EquipType.Head)]
sealed class LivingPearlwoodHelmet : NatureItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26, 24);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.LightPurple6, Item.sellPrice());
    }

    public override void UpdateEquip(Player player) {

    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<FlamewardenMantle>() && legs.type == ModContent.ItemType<FlamewardenPants>();

    public override void UpdateArmorSet(Player player) {

    }
}