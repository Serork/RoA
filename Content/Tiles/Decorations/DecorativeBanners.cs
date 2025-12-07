using Microsoft.Xna.Framework;

using Terraria;
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
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawYOffset = -2;

        TileObjectData.addTile(Type);

        DustType = -1; // No dust when mined
        AddMapEntry(new Color(13, 88, 130), Language.GetText("MapObject.Banner"));

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
