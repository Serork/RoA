using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class ItemCommon : GlobalItem {
    public override bool InstancePerEntity => true;

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
}
