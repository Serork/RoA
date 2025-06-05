using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Ambient.LargeTrees;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class BigTreeSpawner : ModItem {
    public override void SetDefaults() {
        int width = 32; int height = 50;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = Item.useAnimation = 10;
        Item.autoReuse = false;
        Item.useTurn = true;
    }

    public override bool? UseItem(Player player) {
        if (player.ItemAnimationJustStarted) {
            BackwoodsBigTree.TryGrowBigTree((int)(Main.MouseWorld.X / 16f), (int)(Main.MouseWorld.Y / 16f));
        }

        return true;
    }
}
