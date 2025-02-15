using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class HedgehogTerrarium : ModTile {
    public override void SetStaticDefaults() {
        TileID.Sets.CritterCageLidStyle[Type] = TileID.Sets.CritterCageLidStyle[TileID.BunnyCage]; // This is how vanilla draws the roof of the cage

        Main.tileFrameImportant[Type] = Main.tileFrameImportant[TileID.BunnyCage];
        Main.tileLavaDeath[Type] = Main.tileLavaDeath[TileID.BunnyCage];
        Main.tileSolidTop[Type] = Main.tileSolidTop[TileID.BunnyCage];
        Main.tileTable[Type] = Main.tileTable[TileID.BunnyCage];
        //AdjTiles = new int[] { TileID.BunnyCage, TileID.GoldFrogCage }; // Just in case another mod uses the frog cage to craft
        AnimationFrameHeight = 54;

        // We can copy the TileObjectData directly from an existing tile to copy changes, if any, made to the TileObjectData template the original tile copied from.
        // In this case, the original FrogCage tile is an exact copy of TileObjectData.StyleSmallCage, so either approach works here.
        TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.BunnyCage, 0));
        // or TileObjectData.newTile.CopyFrom(TileObjectData.StyleSmallCage);
        TileObjectData.addTile(Type);

        // Since this tile is only used for a single item, we can reuse the item localization for the map entry.
        AddMapEntry(new Color(122, 217, 232), ModContent.GetInstance<Items.Placeable.Decorations.HedgehogTerrarium>().DisplayName);

        DustType = DustID.Glass;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        offsetY = 2; // From vanilla
        Main.critterCage = true; // Vanilla doesn't run the animation code for critters unless this is checked
    }

    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
        Tile tile = Main.tile[i, j];
        // The GetSmallAnimalCageFrame method utilizes some math to stagger each individual tile. First the top left tile is found, then those coordinates are passed into some math to stagger an index into Main.snail2CageFrame
        // Main.frogCageFrame is used since we want the same animation, but if we wanted a different frame count or a different animation timing, we could write our own by adapting vanilla code and placing the code in AnimateTile
        int tileCageFrameIndex = TileDrawing.GetSmallAnimalCageFrame(i, j, tile.TileFrameX, tile.TileFrameY);
        frameYOffset = Main.bunnyCageFrame[tileCageFrameIndex] * AnimationFrameHeight;
    }
}