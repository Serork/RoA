using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoA.Content.Tiles.Solid.Backwoods;
using Terraria.GameContent.Metadata;
using RoA.Content.Dusts;
using RoA.Content.Tiles.Platforms;
using RoA.Core.Utility;

namespace RoA.Content.Tiles.Trees;

sealed class PrimordialSapling : ModTile {
	public override void SetStaticDefaults() {
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLavaDeath[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;
		TileID.Sets.CommonSapling[Type] = true;
		TileID.Sets.TreeSapling[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        TileObjectData.newTile.Width = 1;
		TileObjectData.newTile.Height = 2;
		TileObjectData.newTile.Origin = new Point16(0, 1);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.UsesCustomCanPlace = true;
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.CoordinateWidth = 16;
		TileObjectData.newTile.CoordinatePadding = 2;
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<Solid.Backwoods.BackwoodsGrass>()];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.DrawFlipHorizontal = true;
		TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
		TileObjectData.newTile.LavaDeath = true;
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.addTile(Type);

		AdjTiles = [TileID.Saplings];

		LocalizedText name = CreateMapEntryName();
		AddMapEntry(new Color(200, 200, 200), name);
		DustType = ModContent.DustType<Dusts.Backwoods.WoodTrash>();
		HitSound = SoundID.Dig;
	}

	public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

	public override void RandomUpdate(int i, int j) {
		if (WorldGen.genRand.NextBool(15)) {
			bool isPlayerNear = WorldGen.PlayerLOS(i, j);
			bool success = WorldGenHelper.GrowTreeWithBranches<TreeBranch>(i, j);
			if (success && isPlayerNear) {
				WorldGen.TreeGrowFXCheck(i, j);
			}
		}
	}

	public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects) {
		if (i % 2 == 1) {
			effects = SpriteEffects.FlipHorizontally;
		}
	}
}