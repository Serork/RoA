using RoA.Common.Tiles;

using Terraria.ModLoader;

namespace RoA.Content.Tiles.Ambient;

sealed class TarRocks1 : Rubble_1x1 {
    protected override void SafeSetStaticDefaults() {
        DustType = ModContent.DustType<Dusts.SolidifiedTar>();

        AddMapEntry(new Microsoft.Xna.Framework.Color(43, 31, 47));
    }
}
