using RoA.Common.Tiles;

using Terraria.ModLoader;

namespace RoA.Content.Tiles.Ambient;

sealed class TarRocks1 : Rubble_1x1 {
    public override bool IsLoadingEnabled(Mod mod) => RoA.HasRoALiquidMod();

    protected override void SafeSetStaticDefaults() {
        DustType = RoA.RoALiquidMod.Find<ModDust>("SolidifiedTar").Type;

        AddMapEntry(new Microsoft.Xna.Framework.Color(88, 74, 91));
    }
}
