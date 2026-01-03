using RoA.Common.Tiles;
using RoA.Core.Utility;

using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Ambient;

sealed class BookRubble1 : Rubble_1x1 {
    protected override void SafeSetStaticDefaults() {
        DustType = DustID.Pot;

        AddMapEntry(new Microsoft.Xna.Framework.Color(170, 85, 144));
    }
}

sealed class BookRubble1Rubble : Rubble1x1_RubbleMaker {
    protected override void SafeSetStaticDefaults() {
        DustType = ModContent.DustType<Dusts.SolidifiedTar>();

        AddMapEntry(new Microsoft.Xna.Framework.Color(43, 31, 47));

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ItemID.Book, Type, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
    }
}

sealed class BookRubble2 : Rubble_2x1 {
    protected override void SafeSetStaticDefaults() {
        DustType = DustID.Pot;

        AddMapEntry(new Microsoft.Xna.Framework.Color(170, 85, 144));
    }
}

sealed class BookRubble2Rubble : Rubble2x1_RubbleMaker {
    protected override void SafeSetStaticDefaults() {
        DustType = DustID.Pot;

        AddMapEntry(new Microsoft.Xna.Framework.Color(170, 85, 144));

        FlexibleTileWand.RubblePlacementMedium.AddVariations(ItemID.Book, Type, 0, 1, 2, 3, 4, 5);
    }
}