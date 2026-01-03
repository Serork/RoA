using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Common.Tiles;

abstract class Rubble2x1_RubbleMaker : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.BreakableWhenPlacing[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);

        MineResist = 0.01f;

        SafeSetStaticDefaults();
    }

    protected virtual void SafeSetStaticDefaults() { }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        if (!WorldGenHelper.GetTileSafely(i, j + 1).HasTile) {
            WorldGen.KillTile(i, j);
        }

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }
}

abstract class Rubble1x1_RubbleMaker : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.BreakableWhenPlacing[Type] = true;

        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 1;
        TileObjectData.newTile.Origin = new Point16(0, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);

        MineResist = 0.01f;

        SafeSetStaticDefaults();
    }

    protected virtual void SafeSetStaticDefaults() { }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        if (!WorldGenHelper.GetTileSafely(i, j + 1).HasTile) {
            WorldGen.KillTile(i, j);
        }

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }
}