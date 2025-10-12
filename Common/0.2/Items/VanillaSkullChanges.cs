using Microsoft.Xna.Framework;

using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed partial class ItemCommon : GlobalItem {
    public override void SetDefaults(Item entity) {
        ResetSkullItemDefaults(entity);
    }

    private void ResetSkullItemDefaults(Item entity) {
        if (entity.type == ItemID.Skull) {
            int width = 20, height = 22;
            entity.Size = new Vector2(width, height);
            entity.headSlot = 93;
            entity.rare = ItemRarityID.White;
            entity.vanity = false;
        }
    }

    public override string IsArmorSet(Item head, Item body, Item legs) {
        if (head.type == ItemID.Skull && body.type == ModContent.ItemType<CarcassChestguard>() && legs.type == ModContent.ItemType<CarcassSandals>()) {
            return "SkullArmorSet";
        }

        return base.IsArmorSet(head, body, legs);
    }

    public override void ArmorSetShadows(Player player, string set) {
        if (set == "SkullArmorSet") {
            player.GetCommon().ApplyBoneArmorVisuals = true;
        }
    }
}
