using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.ID;

using Microsoft.Xna.Framework;

using RoA.Common.Tiles;
using RoA.Content.Tiles.Solid.Backwoods;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class LuminousFlower : SimpleTileBaseToGenerateOverTime {
    protected override int[] AnchorValidTiles => [TileID.Grass, TileID.JungleGrass, TileID.CorruptGrass, TileID.CrimsonGrass, (ushort)ModContent.TileType<BackwoodsGrass>()];

    protected override ushort ExtraChance => 30;

    protected override ushort DropItem => (ushort)ModContent.ItemType<Items.Miscellaneous.LuminousFlower>();

    protected override byte XSize => 2;
	protected override byte YSize => 3;

    protected override Color MapColor => new(211, 141, 162);

    protected override void SafeSetStaticDefaults() {
        Main.tileLighted[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileWaterDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
		TileObjectData.newTile.AnchorValidTiles = AnchorValidTiles;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.Width = XSize;
        TileObjectData.newTile.Height = YSize;
        TileObjectData.newTile.DrawYOffset = 6;
        TileObjectData.addTile(Type);

        AnimationFrameHeight = 54;
    }

	public override void NumDust(int i, int j, bool fail, ref int num) => num = 6;

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
		r = 0.9f;
		g = 0.7f;
		b = 0.3f;
	}

	public override void AnimateTile(ref int frame, ref int frameCounter) {
		frameCounter++;
		if (frameCounter > 5) {
			frameCounter = 0;

			frame++;
			if (frame > 14) {
				frame = 0;
			}
		}
	}
}
