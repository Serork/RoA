using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Common.Tiles.TileHooks;

namespace RoA.Content.Tiles.Walls;

sealed class ElderwoodWall3 : ElderwoodWall {
    public override void SetStaticDefaults() {
        base.SetStaticDefaults();

        Main.wallHouse[Type] = false;

        WallID.Sets.WallSpreadStopsAtAir[Type] = true;
        WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;
    }

    public override string Texture => base.Texture[..^1];
}

sealed class ElderwoodWall2 : ElderwoodWall {
    public override string Texture => base.Texture[..^1];
}

class ElderwoodWall : ModWall, IRequireMinHammerPower, IResistToHammer {
    int IRequireMinHammerPower.MinHammer => 55;

    bool IResistToHammer.CanBeApplied(int i, int j) => true;
    float IResistToHammer.ResistToPick => 0.25f;

    public override void SetStaticDefaults() {
        Main.wallHouse[Type] = true;
        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Furniture>();
        AddMapEntry(new Color(72, 56, 52));
    }

    public override bool CanExplode(int i, int j) => NPC.downedBoss2;
}