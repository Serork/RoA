using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Dusts;
using RoA.Content.Items.Placeable.Solid;
using RoA.Content.Tiles.Solid.Backwoods;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsRocks3x2Rubble : ModTile, TileHooks.IGetTileDrawData {
    public override string Texture => TileLoader.GetTile(ModContent.TileType<BackwoodsRocks3x2>()).Texture;

    public void GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {
        glowTexture = BackwoodsRocks3x2.GlowTexture.Value;
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
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);

        DustType = ModContent.DustType<Dusts.Backwoods.Stone>();
        AddMapEntry(new Color(34, 37, 46));

        MineResist = 1.25f;

        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Grimstone>(), Type, 0, 1, 2, 3, 4, 5);

        // Tiles placed by Rubblemaker drop the item used to place them.
        RegisterItemDrop(ModContent.ItemType<Grimstone>());

        MineResist = 0.01f;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        if (Main.rand.NextBool()) {
            type = ModContent.DustType<TealMossDust>();
        }

        return base.CreateDust(i, j, ref type);
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => BackwoodsGreenMoss.SetupLight(ref r, ref g, ref b);
}
