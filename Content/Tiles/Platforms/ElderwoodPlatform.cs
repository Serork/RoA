using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Platforms;

sealed class ElderwoodPlatform : ModTile {
    public override void SetStaticDefaults() {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileTable[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.Platforms[Type] = true;

        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleMultiplier = 27;
        TileObjectData.newTile.StyleWrapLimit = 27;
        TileObjectData.newTile.UsesCustomCanPlace = false;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);
        AddMapEntry(new Color(191, 142, 111));

        AdjTiles = [TileID.Platforms];
    }

    public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;
    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;
}