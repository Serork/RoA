using Microsoft.Xna.Framework;

using RoA.Common.Sets;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class TreeDryad : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.DrawYOffset = -4;
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.CoordinateHeights = [20, 16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.addTile(Type);

        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;

        //TileSets.ShouldKillTileBelow[Type] = false;

        AddMapEntry(new Color(191, 143, 111), CreateMapEntryName());

        DustType = DustID.WoodFurniture;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        if (WorldGenHelper.GetTileSafely(i, j + 1).TileType != Type) {
            height += 4;
        }
    }

    public override bool CanExplode(int i, int j) => false;

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 5;
}