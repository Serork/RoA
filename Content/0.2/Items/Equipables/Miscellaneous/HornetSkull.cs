using Microsoft.Xna.Framework;

using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head)]
sealed class HornetSkull : ModItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
    }

    public override void SetDefaults() {
        int width = 22; int height = 28;
        Item.Size = new Vector2(width, height);
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<CarcassChestguard>() && (legs.legSlot == CarcassSandals.Alternative || legs.type == ModContent.ItemType<CarcassSandals>());

    public override void ArmorSetShadows(Player player) {
        player.GetCommon().ApplyBoneArmorVisuals = true;
    }
}
