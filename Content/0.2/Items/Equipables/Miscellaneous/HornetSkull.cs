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

        On_ItemSlot.PickItemMovementAction += On_ItemSlot_PickItemMovementAction;
    }

    private int On_ItemSlot_PickItemMovementAction(On_ItemSlot.orig_PickItemMovementAction orig, Item[] inv, int context, int slot, Item checkItem) {
        int result = orig(inv, context, slot, checkItem);
        bool hornetSkull = checkItem.type == ModContent.ItemType<HornetSkull>() && Main.LocalPlayer.GetCommon().ApplyHornetSkullSetBonus;
        bool devilSkull = checkItem.type == ModContent.ItemType<DevilSkull>() && Main.LocalPlayer.GetCommon().ApplyDevilSkullSetBonus;
        bool vanillaSkull = checkItem.type == ItemID.Skull && Main.LocalPlayer.GetCommon().ApplyVanillaSkullSetBonus;
        bool crystallizedSkull = checkItem.type == ModContent.ItemType<CrystallizedSkull>() && Main.LocalPlayer.GetCommon().ApplyCrystallizedSkullSetBonus;
        if (result == 1 && 
            (hornetSkull || devilSkull || vanillaSkull || crystallizedSkull)) {
            result = -1;
        }
        return result;
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
