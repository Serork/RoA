using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class FenethStatueFlowers : ModTile, TileHooks.IGetTileDrawData {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileLighted[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;

        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Style = 0;
        TileObjectData.newTile.LavaDeath = false;
        List<int> anchorValidTileTypes = [TileID.Ash, 2, 23, 109, 199, 477, 492, 633, ModContent.TileType<BackwoodsGrass>()];
        if (ModLoader.TryGetMod("TheDepths", out Mod theDepths) && theDepths.TryFind<ModTile>("NightmareGrass", out ModTile NightmareGrass)) {
            anchorValidTileTypes.Add(NightmareGrass.Type);
        }
        TileObjectData.newTile.AnchorValidTiles = anchorValidTileTypes.ToArray();
        TileObjectData.addTile(Type);

        DustType = (ushort)ModContent.DustType<Dusts.Fireblossom2>();
        HitSound = SoundID.Grass;
        AddMapEntry(new Microsoft.Xna.Framework.Color(243, 138, 3));

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ItemID.Fireblossom, Type, 0, 1, 2, 3);
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        Color color = Color.Orange;
        color *= 0.5f;
        r = color.R / 255f;
        g = color.G / 255f;
        b = color.B / 255f;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        spriteEffects = i % 2 == 0 ? SpriteEffects.FlipHorizontally : spriteEffects;
    }

    void TileHooks.IGetTileDrawData.GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {
        glowTexture = this.GetTileGlowTexture();
        glowColor = Color.White;
        glowSourceRect = new Rectangle(tileFrameX, tileFrameY, tileWidth, tileHeight);
    }

    //public override void PostDraw(int i, int j, SpriteBatch spriteBatch) => BackwoodsGrass.EmitDusts(i, j);
}