using RoA.Common.Tiles;
using RoA.Content.Items.LiquidsSpecific;
using RoA.Core.Utility;

using Terraria.GameContent;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Ambient;

sealed class TarRocks2 : Rubble_2x1 {
    protected override void SafeSetStaticDefaults() {
        DustType = ModContent.DustType<Dusts.SolidifiedTar>();

        AddMapEntry(new Microsoft.Xna.Framework.Color(43, 31, 47));
    }
}

sealed class TarRocks2Rubble : Rubble2x1_RubbleMaker {
    public override string Texture => TileHelper.GetTileTexture<TarRocks2>();

    protected override void SafeSetStaticDefaults() {
        DustType = ModContent.DustType<Dusts.SolidifiedTar>();

        AddMapEntry(new Microsoft.Xna.Framework.Color(43, 31, 47));

        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<SolidifiedTar>(), Type, 0, 1, 2);
    }
}
