using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class DecorativeBanners : ModBannerTile {
    public enum StyleID : byte {
        SmolderingTapestry,
        Riot,
        Equality
    }

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();

        TileID.Sets.GeneralPlacementTiles[Type] = false;
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
    }

    public override void NearbyEffects(int i, int j, bool closer) {
        
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        if (WorldGen.gen) {
            return false;
        }

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }
}

sealed class DecorativeBanners_Gen : ModBannerTile {
    public override string Texture => TileLoader.GetTile(ModContent.TileType<DecorativeBanners>()).Texture;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.MultiTileSway[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom | AnchorType.PlanterBox, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.DrawYOffset = -2; // Draw this tile 2 pixels up, allowing the banner pole to align visually with the bottom of the tile it is anchored to.

        // This alternate placement supports placing on un-hammered platform tiles. Note how the DrawYOffset accounts for the height adjustment needed for the tile to look correctly attached.
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.Platform, TileObjectData.newTile.Width, 0);
        TileObjectData.newAlternate.DrawYOffset = -10;
        TileObjectData.addAlternate(0);

        TileObjectData.addTile(Type);

        DustType = -1; // No dust when mined
        AddMapEntry(new Color(13, 88, 130), Language.GetText("MapObject.Banner"));

        TileID.Sets.GeneralPlacementTiles[Type] = false;
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        int itemType = ModContent.ItemType<Items.Placeable.Decorations.Equality>();
        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, itemType);
    }

    public override void NearbyEffects(int i, int j, bool closer) {

    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        if (WorldGen.gen) {
            return false;
        }

        if (!WorldGenHelper.GetTileSafely(i, j - 1).HasTile) {
            return true;
        }

        if (WorldGen.SolidTile2(i, j - 1) && !WorldGenHelper.GetTileSafely(i, j + 1).HasTile) {
            return true;
        }

        if (!WorldGen.SolidTile2(i, j - 1) && WorldGenHelper.GetTileSafely(i, j - 1).TileType != Type) {
            return true;
        }

        return false;
    }
}
