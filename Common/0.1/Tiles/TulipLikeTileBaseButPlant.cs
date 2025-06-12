using RoA.Core.Data;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;

namespace RoA.Common.Tiles;

abstract class TulipLikeTileBaseButPlant : TulipLikeTileBase {
    protected short FrameWidth => 18;
    protected PlantStage GetStage(int i, int j) => (PlantStage)(WorldGenHelper.GetTileSafely(i, j).TileFrameX / FrameWidth);
    protected bool IsGrowing(int i, int j) => GetStage(i, j) == PlantStage.Growing;
    protected bool IsGrown(int i, int j) => GetStage(i, j) == PlantStage.Grown;

    public override string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');

    protected override void SafeSetStaticDefaults2() {
        Main.tileSpelunker[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY = 0;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = IsGrown(i, j) ? Main.rand.Next(3, 6) : IsGrowing(i, j) ? Main.rand.Next(2, 5) : Main.rand.Next(1, 3);

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