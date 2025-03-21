using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class Them : ModTile {
	public override void SetStaticDefaults() {
		Main.tileFrameImportant[Type] = true;
		Main.tileLavaDeath[Type] = true;

        Main.tileSpelunker[Type] = true;

        TileID.Sets.FramesOnKillWall[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;
        TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;
        TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
		TileObjectData.newTile.Height = 3;
		TileObjectData.newTile.Width = 2;
		TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
		TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.Origin = new Terraria.DataStructures.Point16(0, 1);
        TileObjectData.addTile(Type);

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(new Microsoft.Xna.Framework.Color(99, 50, 30), name);
    }
}