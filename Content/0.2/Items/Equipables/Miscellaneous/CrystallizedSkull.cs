using RoA.Core.Defaults;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head, EquipType.Face)]
sealed class CrystallizedSkull : ModItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Face.Sets.PreventHairDraw[Item.faceSlot] = true;
        ArmorIDs.Face.Sets.OverrideHelmet[Item.faceSlot] = true;
    }

    public override void SetDefaults() {
        int width = 32, height = 28;
        Item.SetSizeValues(width, height);

        Item.accessory = true;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<CarcassChestguard>() && legs.type == ModContent.ItemType<CarcassSandals>();

    public override void UpdateArmorSet(Player player) {
        player.GetCommon().ApplyCrystallizedSkullSetBonus = true;
    }

    public override bool CanEquipAccessory(Player player, int slot, bool modded) => player.GetCommon().PerfectClotActivated;

    public override void UpdateEquip(Player player) {
        if (player.GetCommon().PerfectClotActivated) {
            player.GetCommon().ApplyCrystallizedSkullSetBonus = true;
        }
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().ApplyCrystallizedSkullSetBonus = true;
    }
}
