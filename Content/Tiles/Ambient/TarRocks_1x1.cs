using RoA.Common.Tiles;
using RoA.Content.Items.LiquidsSpecific;
using RoA.Core.Utility;

using Terraria.GameContent;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Ambient;

sealed class TarRocks1 : Rubble_1x1 {
    protected override void SafeSetStaticDefaults() {
        DustType = ModContent.DustType<Dusts.SolidifiedTar>();

        AddMapEntry(new Microsoft.Xna.Framework.Color(43, 31, 47));
    }
}

sealed class TarRocks1Rubble : Rubble1x1_RubbleMaker {
    public override string Texture => TileHelper.GetTileTexture<TarRocks1>();

    protected override void SafeSetStaticDefaults() {
        DustType = ModContent.DustType<Dusts.SolidifiedTar>();

        AddMapEntry(new Microsoft.Xna.Framework.Color(43, 31, 47));

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<SolidifiedTar>(), Type, 0, 1, 2, 3, 4, 5);
    }
}
