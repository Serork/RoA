using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using System.Collections.Generic;

using RoA.Content.Tiles.Miscellaneous;
using RoA.Common.Tiles;

using Terraria.GameContent.Metadata;

using RoA.Core.Utility;
using RoA.Core.Data;
using System;

namespace RoA.Content.Tiles.Plants;

sealed class Bonerose : TulipLikeTileBase {
    public override int[] AnchorValidTiles => [TileID.PinkDungeonBrick, TileID.GreenDungeonBrick, TileID.BlueDungeonBrick];
    public override Predicate<ushort> ConditionForWallToBeValid => (wallType) => { return Main.wallDungeon[wallType]; };

    public override Color MapColor => new(178, 178, 137);

    public override byte Amount => 3;

    public override bool InUnderground => true;

    private short FrameWidth => 18;
    private PlantStage GetStage(int i, int j) => (PlantStage)(WorldGenHelper.GetTileSafely(i, j).TileFrameX / FrameWidth);
    private bool IsGrowing(int i, int j) => GetStage(i, j) == PlantStage.Growing;
    private bool IsGrown(int i, int j) => GetStage(i, j) == PlantStage.Grown;

    public override string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');

    protected override void SafeSetStaticDefaults() {
        DustType = DustID.Bone;
        HitSound = SoundID.NPCHit1;

        RootsDrawing.ShouldDraw[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY = 0;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = IsGrown(i, j) ? Main.rand.Next(3, 6) : IsGrowing(i, j) ? Main.rand.Next(2, 5) : Main.rand.Next(1, 3);

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        if (IsGrown(i, j)) {
            yield return new Item(ModContent.ItemType<Items.Materials.Bonerose>());
        }
    }

    public override bool IsTileSpelunkable(int i, int j) => IsGrown(i, j);

    public override void RandomUpdate(int i, int j) {
        if (!IsGrown(i, j)) {
            WorldGenHelper.GetTileSafely(i, j).TileFrameX += 18;

            if (Main.netMode != NetmodeID.SinglePlayer) {
                NetMessage.SendTileSquare(-1, i, j, 1);
            }
        }
    }

    public override void MouseOver(int i, int j) { }
    public override bool RightClick(int x, int y) => false;
}
