using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class DryadStatue : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileLighted[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(124, 127, 140), CreateMapEntryName());

        DustType = ModContent.DustType<Dusts.Backwoods.Stone>();
        MineResist = 1.5f;
    }

    public override bool IsTileSpelunkable(int i, int j) => WorldGenHelper.GetTileSafely(i, j).TileFrameX <= 72;

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        if (WorldGenHelper.GetTileSafely(i, j).TileFrameX <= 72) {
            yield return new Item(ModContent.ItemType<Items.Placeable.Decorations.DryadStatue>());
        }
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => false;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 2 : 5;
}


sealed class DryadStatue_Rubble : ModTile {
    public override string Texture => TileLoader.GetTile(ModContent.TileType<DryadStatue>()).Texture;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(124, 127, 140), CreateMapEntryName());

        DustType = ModContent.DustType<Dusts.Backwoods.Stone>();

        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Items.Placeable.Decorations.DryadStatue>(), Type, 2, 3, 4, 5);
    }

    public override bool IsTileSpelunkable(int i, int j) => WorldGenHelper.GetTileSafely(i, j).TileFrameX <= 72;

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Decorations.DryadStatue>());
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => false;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 2 : 5;
}