using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Walls;

sealed class SolidifiedTarWall : ModWall {
    public override bool IsLoadingEnabled(Mod mod) => RoA.HasRoALiquidMod();

    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = false;

        WallID.Sets.WallSpreadStopsAtAir[Type] = true;
        WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;

        DustType = RoA.RoALiquidMod.Find<ModDust>("SolidifiedTar").Type;
        AddMapEntry(new Color(48, 40, 54));
    }
}
