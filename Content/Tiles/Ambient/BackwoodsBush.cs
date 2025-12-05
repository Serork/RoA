using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Tiles.Solid.Backwoods;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsBush : ModTile {
    public override void Load() {
        On_WorldGen.PlaceObject += On_WorldGen_PlaceObject;
    }

    private bool On_WorldGen_PlaceObject(On_WorldGen.orig_PlaceObject orig, int x, int y, int type, bool mute, int style, int alternate, int random, int direction) {
        if (!TileObject.CanPlace(x, y, type, style, direction, out var objectData))
            return false;

        objectData.random = random;
        if (TileObject.Place(objectData)) {
            WorldGen.SquareTileFrame(x, y);
            if (type == ModContent.TileType<BackwoodsBush>()) {
                Main.tile[x, y].TileFrameX = (short)(style * 34);
            }
            if (!mute)
                SoundEngine.PlaySound(SoundID.Dig, new Microsoft.Xna.Framework.Vector2(x * 16, y * 16));
        }

        return true;
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.IgnoredInHouseScore[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Style = 0;
        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<BackwoodsGrass>()];
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
        width = 34;
        height = 32;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 3 : 10;

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        spriteEffects = i % 2 == 0 ? SpriteEffects.FlipHorizontally : spriteEffects;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) => BackwoodsGrass.EmitDusts(i, j);
}