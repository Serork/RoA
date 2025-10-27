using RoA.Common.Players;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head, EquipType.Face)]
sealed class HornetSkull : ModItem, IDoubleTap {
    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;

        ArmorIDs.Face.Sets.PreventHairDraw[Item.faceSlot] = true;
        ArmorIDs.Face.Sets.OverrideHelmet[Item.faceSlot] = true;
    }

    public override void SetDefaults() {
        int width = 22, height = 28;
        Item.SetSizeValues(width, height);

        Item.accessory = true;
    }

    public override bool CanEquipAccessory(Player player, int slot, bool modded) => player.GetCommon().PerfectClotActivated;

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<CarcassChestguard>() && legs.type == ModContent.ItemType<CarcassSandals>();

    public override void UpdateArmorSet(Player player) {
        player.GetCommon().ApplyHornetSkullSetBonus = true;
    }

    public override void UpdateEquip(Player player) {
        if (player.GetCommon().PerfectClotActivated) {
            player.GetCommon().ApplyHornetSkullSetBonus = true;
            player.GetCommon().StopFaceDrawing = true;
        }
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().ApplyHornetSkullSetBonus = true;
    }

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) => player.GetCommon().DoHornetDash(direction);
}
