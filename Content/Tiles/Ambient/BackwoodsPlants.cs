using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Placeable.Seeds;
using RoA.Content.Tiles.Solid.Backwoods;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsPlants : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.IgnoredInHouseScore[Type] = true;
        //TileID.Sets.SlowlyDiesInWater[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Style = 0;
        //TileObjectData.newTile.AnchorValidTileTypes = [ModContent.TileType<BackwoodsGrass>()];
        TileObjectData.addTile(Type);

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
        HitSound = SoundID.Grass;
        AddMapEntry(new Microsoft.Xna.Framework.Color(19, 82, 44));
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        WorldGen.PlantCheck(i, j);

        return base.TileFrame(i, j, ref resetFrame, ref noBreak);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        offsetY = -14;
        height = 32;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        if (Main.rand.NextBool(100)) {
            yield return new Item(ModContent.ItemType<BackwoodsGrassSeeds>());
        }
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        spriteEffects = i % 2 == 0 ? SpriteEffects.FlipHorizontally : spriteEffects;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) => BackwoodsGrass.EmitDusts(i, j);
}