using RoA.Common.Players;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Armor.Nature.Hardmode;

[AutoloadEquip(EquipType.Head)]
sealed class ChlorophyteCowl : NatureItem, IDoubleTap {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26, 26);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.LightPurple6, Item.sellPrice());
    }

    public override void UpdateArmorSet(Player player) {
        player.AddBuff(BuffID.LeafCrystal, 18000);

        player.GetCommon().IsChlorophyteCowlArmorSetActive = true;
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) {

    }

    public override void ArmorSetShadows(Player player) {
        player.armorEffectDrawShadowSubtle = true;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ItemID.ChlorophytePlateMail && legs.type == ItemID.ChlorophyteGreaves;

    public override void EquipFrameEffects(Player player, EquipType type) {

    }
}