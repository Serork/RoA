using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Walls;

sealed class BackwoodsRootWall : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() {
        Item.DefaultToPlaceableWall(ModContent.WallType<Tiles.Walls.BackwoodsRootWall>());

        Item.Size = Vector2.One * 24;
    }
}
