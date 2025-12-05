using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Materials;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

class TreeDryadDecoration2Rubble_Jungle : TreeDryadDecoration2Rubble {
    public override string Texture => TileLoader.GetTile(ModContent.TileType<TreeDryadDecoration2_Jungle>()).Texture;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.DrawYOffset = 0;
        TileObjectData.newTile.CoordinateHeights = [18];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.addTile(Type);

        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<NaturesHeart>(), Type, 0, 1);

        DustType = DustID.RichMahogany;
        AddMapEntry(new Color(184, 118, 124));

        RegisterItemDrop(ModContent.ItemType<NaturesHeart>());

        MineResist = 0.01f;
    }
}

class TreeDryadDecoration2Rubble_Spirit : TreeDryadDecoration2Rubble {
    public override string Texture => TileLoader.GetTile(ModContent.TileType<TreeDryadDecoration2_Spirit>()).Texture;

    public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("SpiritReforged");

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.DrawYOffset = 0;
        TileObjectData.newTile.CoordinateHeights = [18];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.addTile(Type);

        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<NaturesHeart>(), Type, 0, 1);

        DustType = DustID.t_PearlWood;
        AddMapEntry(new Color(168, 153, 136));

        RegisterItemDrop(ModContent.ItemType<NaturesHeart>());

        MineResist = 0.01f;
    }
}

class TreeDryadDecoration2Rubble : ModTile {
    public override string Texture => TileLoader.GetTile(ModContent.TileType<TreeDryadDecoration2>()).Texture;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.DrawYOffset = 0;
        TileObjectData.newTile.CoordinateHeights = [18];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.addTile(Type);

        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<NaturesHeart>(), Type, 0, 1);

        DustType = DustID.WoodFurniture;
        AddMapEntry(new Color(191, 143, 111));

        RegisterItemDrop(ModContent.ItemType<NaturesHeart>());

        MineResist = 0.01f;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        height = 20;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) { }
}
