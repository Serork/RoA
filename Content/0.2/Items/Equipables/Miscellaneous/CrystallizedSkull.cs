using Microsoft.Xna.Framework;

using RoA.Core.Defaults;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head)]
sealed class CrystallizedSkull : ModItem {
    public override void SetStaticDefaults() {
    }

    public override void SetDefaults() {
        int width = 32, height = 28;
        Item.SetSizeValues(width, height);
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<CarcassChestguard>() && legs.type == ModContent.ItemType<CarcassSandals>();

    public override void UpdateArmorSet(Player player) {
        player.GetCommon().ApplyCrystallizedSkullSetBonus = true;
    }
}
