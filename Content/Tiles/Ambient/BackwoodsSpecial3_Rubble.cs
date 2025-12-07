using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Items.Placeable.Solid;
using RoA.Content.Tiles.Solid.Backwoods;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsSpecial3_Rubble : ModTile, TileHooks.IGetTileDrawData {
    public override string Texture => TileLoader.GetTile(ModContent.TileType<BackwoodsSpecial3>()).Texture;

    public void GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {
        glowTexture = BackwoodsSpecial3.GlowTexture.Value;
        glowColor = TileDrawingExtra.BackwoodsMossGlowColor;
        glowSourceRect = new Rectangle(tileFrameX, tileFrameY, tileWidth, tileHeight);
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileLavaDeath[Type] = true;

        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.BreakableWhenPlacing[Type] = true;

        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(1, 1);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);

        DustType = ModContent.DustType<Dusts.Backwoods.Stone>();

        AddMapEntry(new Color(178, 178, 137));
        AddMapEntry(new Color(95, 98, 113));
        AddMapEntry(new Color(110, 91, 74));

        MineResist = 0.01f;

        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Elderwood>(), Type, 0, 1, 2, 3, 4, 5, 6, 7);
        RegisterItemDrop(ModContent.ItemType<Elderwood>());
    }

    public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) {

    }

    public override ushort GetMapOption(int i, int j) {
        var tileFrameX = Main.tile[i, j].TileFrameX;
        if (tileFrameX >= 214) {
            tileFrameX -= 214;
        }
        if (tileFrameX < 54)
            return 0;
        else if (tileFrameX < 108) {
            return 1;
        }
        else {
            return 2;
        }
    }

    public override bool CreateDust(int i, int j, ref int type) {
        short tileFrame = Main.tile[i, j].TileFrameX;
        if (tileFrame >= 214) {
            tileFrame -= 214;
        }
        if (tileFrame < 54) {
            type = DustID.Bone;
        }
        else if (tileFrame < 108) {
            type = ModContent.DustType<Dusts.Backwoods.Stone>();
        }
        else {
            type = ModContent.DustType<Dusts.Backwoods.WoodTrash>();
        }

        return base.CreateDust(i, j, ref type);
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        var tileFrameX = Main.tile[i, j].TileFrameX;
        if (tileFrameX >= 214) {
            tileFrameX -= 214;
        }
        if (tileFrameX < 54) {
        }
        else if (tileFrameX < 108) {
            BackwoodsGreenMoss.SetupLight(ref r, ref g, ref b);
        }
        else {

        }
    }
}
