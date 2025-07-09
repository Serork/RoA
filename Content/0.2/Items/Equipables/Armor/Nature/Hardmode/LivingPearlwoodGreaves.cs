using RoA.Core.Defaults;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature.Hardmode;

[AutoloadEquip(EquipType.Legs)]
sealed class LivingPearlwoodGreaves : NatureItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(22, 18);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.LightPurple6, Item.sellPrice());
    }

    public override void UpdateEquip(Player player) {

    }
}