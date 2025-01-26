using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Ambient.LargeTrees;
using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class BigTreeSpawner : ModItem {
    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetDefaults() {
        int width = 20; int height = width;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 10;
        Item.autoReuse = false;
        Item.useTurn = true;
    }

    public override bool? UseItem(Player player) {
        if (player.ItemAnimationJustStarted) {
            BackwoodsBigTree.Place((int)(Main.MouseWorld.X / 16f), (int)(Main.MouseWorld.Y / 16f), Main.rand.Next(14, 20));
        }

        return true;
    }
}
