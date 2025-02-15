using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
using RoA.Common.WorldEvents;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class WreathCharger : ModItem {
    public override void SetDefaults() {
        int width = 30; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = Item.useAnimation = 10;
        Item.autoReuse = false;
        Item.useTurn = true;
    }

    public override bool? UseItem(Player player) {
        if (player.ItemAnimationJustStarted) {
            var handler = player.GetModPlayer<WreathHandler>();
            handler.IncreaseResourceValue();
            handler.MakeDustsOnHit();
        }

        return true;
    }
}
