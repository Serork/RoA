using Microsoft.Xna.Framework;

using RoA.Common.Players;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Miscellaneous;

[AutoloadEquip(EquipType.Head)]
sealed class HornetSkull : ModItem, IDoubleTap {
    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
    }

    public override void SetDefaults() {
        int width = 22, height = 28;
        Item.SetSizeValues(width, height);
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<CarcassChestguard>() && legs.type == ModContent.ItemType<CarcassSandals>();

    void IDoubleTap.OnDoubleTap(Player player, IDoubleTap.TapDirection direction) => player.GetCommon().DoHornetDash(direction);
}
