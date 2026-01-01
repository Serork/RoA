using RoA.Common.Tiles;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Ambient;

sealed class BookRubble1 : Rubble_1x1 {
    protected override void SafeSetStaticDefaults() {
        DustType = DustID.Pot;

        AddMapEntry(new Microsoft.Xna.Framework.Color(170, 85, 144));
    }
}

sealed class BookRubble2 : Rubble_2x1 {
    protected override void SafeSetStaticDefaults() {
        DustType = DustID.Pot;

        AddMapEntry(new Microsoft.Xna.Framework.Color(170, 85, 144));
    }
}
