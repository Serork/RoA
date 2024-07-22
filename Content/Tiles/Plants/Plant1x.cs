using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria;

using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

namespace RoA.Content.Tiles.Plants;

abstract class Plant1x : ModTile {
    public enum Stage : byte {
        Planted,
        Growing,
        Grown
    }

    protected virtual short FrameWidth => 18;

    protected virtual int[] AnchorValidTiles => [];

    public sealed override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;

        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.IgnoredInHouseScore[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.AnchorValidTiles = AnchorValidTiles;
        TileObjectData.newTile.AnchorAlternateTiles = [TileID.ClayPot, TileID.PlanterBox];
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.addTile(Type);

        SafeStaticDefaults();
    }

    protected virtual void SafeStaticDefaults() { }

    protected virtual Stage GetStage(int i, int j) => (Stage)(WorldGenHelper.GetTileSafely(i, j).TileFrameX / FrameWidth);
    protected virtual bool IsGrowing(int i, int j) => GetStage(i, j) == Stage.Growing;
    protected virtual bool IsGrown(int i, int j) => GetStage(i, j) == Stage.Grown;

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        if (i % 2 != 1) {
            return;
        }

        spriteEffects = SpriteEffects.FlipHorizontally;
    }
    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY = -2;

    public override bool IsTileSpelunkable(int i, int j) => IsGrown(i, j);

    public override void RandomUpdate(int i, int j) {
        if (!IsGrown(i, j)) {
            WorldGenHelper.GetTileSafely(i, j).TileFrameX += FrameWidth;

            if (Main.netMode != NetmodeID.SinglePlayer) {
                NetMessage.SendTileSquare(-1, i, j, 1);
            }
        }
    }
}
