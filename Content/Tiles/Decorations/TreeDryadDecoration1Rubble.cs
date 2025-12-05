using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Materials;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

class TreeDryadDecoration1Rubble_Jungle : TreeDryadDecoration1Rubble {
    public override string Texture => TileLoader.GetTile(ModContent.TileType<TreeDryadDecoration1_Jungle>()).Texture;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 1;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<NaturesHeart>(), Type, 0, 1);

        DustType = DustID.RichMahogany;
        AddMapEntry(new Color(184, 118, 124));

        RegisterItemDrop(ModContent.ItemType<NaturesHeart>());

        MineResist = 0.01f;
    }
}

class TreeDryadDecoration1Rubble_Spirit : TreeDryadDecoration1Rubble {
    public override string Texture => TileLoader.GetTile(ModContent.TileType<TreeDryadDecoration1_Spirit>()).Texture;

    public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("SpiritReforged");

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 1;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<NaturesHeart>(), Type, 0, 1);

        DustType = DustID.t_PearlWood;
        AddMapEntry(new Color(168, 153, 136));

        RegisterItemDrop(ModContent.ItemType<NaturesHeart>());

        MineResist = 0.01f;
    }
}

class TreeDryadDecoration1Rubble : ModTile {
    public override string Texture => TileLoader.GetTile(ModContent.TileType<TreeDryadDecoration1>()).Texture;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 1;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<NaturesHeart>(), Type, 0, 1);

        DustType = DustID.WoodFurniture;
        AddMapEntry(new Color(191, 143, 111));

        RegisterItemDrop(ModContent.ItemType<NaturesHeart>());

        MineResist = 0.01f;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {

    }
}
