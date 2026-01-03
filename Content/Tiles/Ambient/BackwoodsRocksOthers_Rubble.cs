using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Dusts;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.Items.Placeable.Solid;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsRocks01Rubble : BackwoodsRocks1Rubble {
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

        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<Grimstone>(), Type, 0, 1, 2, 3, 4, 5);

        RegisterItemDrop(ModContent.ItemType<Grimstone>());

        DustType = ModContent.DustType<Stone>();
        AddMapEntry(new Color(34, 37, 46));

        MineResist = 0.01f;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        bool flag = (tile.TileFrameX >= 1116 && tile.TileFrameX <= 1188) && tile.TileFrameY > 0;
        if (flag) {
            type = ModContent.DustType<WoodTrash>();
        }
        else if (tile.TileFrameX <= 108) {
            type = ModContent.DustType<Stone>();
        }

        return true;
    }
}

class BackwoodsRocks0Rubble : BackwoodsRocks1Rubble {
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

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<Grimstone>(), Type, 0, 1, 2, 3, 4, 5);
        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<Elderwood>(), Type, 6, 7, 8, 9, 10, 11);

        // Tiles placed by Rubblemaker drop the item used to place them.
        RegisterItemDrop(ModContent.ItemType<Grimstone>());

        DustType = ModContent.DustType<Stone>();
        AddMapEntry(new Color(34, 37, 46));

        MineResist = 0.01f;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        if (tile.TileFrameX <= 108) {
            type = ModContent.DustType<Stone>();
        }
        else {
            type = ModContent.DustType<WoodTrash>();
        }

        return true;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        if (!WorldGenHelper.GetTileSafely(i, j + 1).HasTile) {
            WorldGen.KillTile(i, j);
        }

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }
}

sealed class BackwoodsRocks2Rubble : BackwoodsRocks1Rubble, TileHooks.IGetTileDrawData {
    public override string Texture => TileLoader.GetTile(ModContent.TileType<BackwoodsRocks2>()).Texture;

    public void GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {
        glowTexture = BackwoodsRocks2.GlowTexture.Value;
        glowColor = TileDrawingExtra.BackwoodsMossGlowColor;
        glowSourceRect = new Rectangle(tileFrameX, tileFrameY, tileWidth, tileHeight);
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => BackwoodsGreenMoss.SetupLight(ref r, ref g, ref b);

    public override bool CreateDust(int i, int j, ref int type) {
        if (Main.rand.NextBool()) {
            type = ModContent.DustType<TealMossDust>();
        }

        return base.CreateDust(i, j, ref type);
    }
}

class BackwoodsRocks1Rubble : ModTile {
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

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<Grimstone>(), Type, 0, 1, 2, 3, 4, 5);

        // Tiles placed by Rubblemaker drop the item used to place them.
        RegisterItemDrop(ModContent.ItemType<Grimstone>());

        DustType = ModContent.DustType<Stone>();
        AddMapEntry(new Color(34, 37, 46));

        MineResist = 0.01f;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {

    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        if (!WorldGenHelper.GetTileSafely(i, j + 1).HasTile) {
            WorldGen.KillTile(i, j);
        }

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }
}
