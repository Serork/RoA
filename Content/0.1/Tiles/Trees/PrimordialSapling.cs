using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts.Backwoods;
using RoA.Content.Tiles.Ambient.LargeTrees;
using RoA.Content.Tiles.Platforms;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

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
        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<BackwoodsGrass>()];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.DrawFlipHorizontal = true;
        TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.addTile(Type);

        AdjTiles = [TileID.Saplings];

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(new Color(163, 116, 81), name);
        DustType = ModContent.DustType<WoodTrash>();
        HitSound = SoundID.Dig;
    }

    public override void RandomUpdate(int i, int j) {
        if (Main.rand.NextBool(15)) {
            GrowTree(i, j);
        }
    }

    internal static void GrowTree(int i, int j) {
        bool success = false;
        if (Main.hardMode) {
            success = BackwoodsBigTree.TryGrowBigTree(i, j + 2, placeRand: Main.rand);
        }
        if (!success && !TileID.Sets.TreeSapling[WorldGenHelper.GetTileSafely(i + 1, j).TileType] && !TileID.Sets.TreeSapling[WorldGenHelper.GetTileSafely(i - 1, j).TileType]) {
            success = WorldGenHelper.GrowTreeWithBranches<TreeBranch>(i, j + 2, branchChance: 10);
        }
        bool isPlayerNear = WorldGen.PlayerLOS(i, j);
        if (success && isPlayerNear) {
            WorldGen.TreeGrowFXCheck(i, j);
        }
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects) {
        if (i % 2 == 1) {
            effects = SpriteEffects.FlipHorizontally;
        }
    }
}