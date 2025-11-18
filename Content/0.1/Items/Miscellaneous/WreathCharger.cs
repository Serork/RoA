using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Miscellaneous;

sealed class WreathCharger : NatureItem {
    protected override void SafeSetDefaults() {
        int width = 30; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = Item.useAnimation = 10;
        Item.autoReuse = false;
        Item.useTurn = true;
    }

    public override bool? UseItem(Player player) {
        if (player.ItemAnimationJustStarted) {
            WreathHandler handler = player.GetWreathHandler();
            handler.IncreaseResourceValue();
            handler.MakeDustsOnHit();
        }

        return true;
    }
}
