using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsBush : ModTile {
	public override void SetStaticDefaults () {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileSolid[Type] = false;

        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Style = 0;
        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<BackwoodsGrass>()];
        TileObjectData.addTile(Type);

        DustType = (ushort)ModContent.DustType<Dusts.BackwoodsGrass>();
        HitSound = SoundID.Grass;
        AddMapEntry(new Microsoft.Xna.Framework.Color(19, 82, 44));
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        offsetY = -14;
        width = 34;
        height = 32;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = Main.rand.Next(3, 6);

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        if (i % 2 == 1) {
            spriteEffects = SpriteEffects.FlipHorizontally;
        }
    }
}