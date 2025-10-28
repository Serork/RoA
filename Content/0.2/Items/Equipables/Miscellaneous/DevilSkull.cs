using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head, EquipType.Face)]
sealed class DevilSkull : ModItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Face.Sets.PreventHairDraw[Item.faceSlot] = true;
    }

    public override void SetDefaults() {
        int width = 24, height = 26;
        Item.SetSizeValues(width, height);

        Item.accessory = true;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<CarcassChestguard>() && legs.type == ModContent.ItemType<CarcassSandals>();

    public override void UpdateArmorSet(Player player) {
        player.GetCommon().ApplyDevilSkullSetBonus = true;
    }

    public override bool CanEquipAccessory(Player player, int slot, bool modded) => player.GetCommon().PerfectClotActivated;

    public override void UpdateEquip(Player player) {
        if (player.GetCommon().PerfectClotActivated) {
            player.GetCommon().ApplyDevilSkullSetBonus = true;
        }
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().ApplyDevilSkullSetBonus = true;
    }
}
