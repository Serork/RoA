using RoA.Common.Tiles;

using Terraria.ModLoader;

namespace RoA.Content.Tiles.Ambient;

sealed class TarRocks2 : Rubble_2x1 {
    public override bool IsLoadingEnabled(Mod mod) => RoA.HasRoALiquidMod();

    protected override void SafeSetStaticDefaults() {
        DustType = RoA.RoALiquidMod.Find<ModDust>("SolidifiedTar").Type;

        AddMapEntry(new Microsoft.Xna.Framework.Color(43, 31, 47));
    }
}
