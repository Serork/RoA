using Microsoft.Xna.Framework;

using RoA.Common.WorldEvents;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class FogStarter : ModItem {
    public override void SetDefaults() {
        int width = 36; int height = 42;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = Item.useAnimation = 10;
        Item.autoReuse = false;
        Item.useTurn = true;
    }

    public override bool? UseItem(Player player) {
        if (player.ItemAnimationJustStarted) {
            BackwoodsFogHandler.ToggleBackwoodsFog(false);
        }

        return true;
    }
}
