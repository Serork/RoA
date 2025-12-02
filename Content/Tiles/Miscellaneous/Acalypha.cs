using Microsoft.Xna.Framework;

using RoA.Common.Tiles;
using RoA.Content.Items.Weapons.Nature.Hardmode;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Miscellaneous;

sealed partial class Acalypha : CollectableFlower, IGrowLikeTulip {
    protected override ushort DropItemType => (ushort)ModContent.ItemType<SacredAcalypha>();
    protected override Color MapColor => new(246, 73, 112);
    protected override int[] AnchorValidTileTypes => [109, 117, 116, 164, 402, 403, 115];
    protected override ushort HitDustType => (ushort)DustID.HallowedPlants;

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        width = height = 30;
        offsetY -= 12;
    }

    public override void RandomUpdate(int i, int j) {
        ref short tileFrameY = ref WorldGenHelper.GetTileSafely(i, j).TileFrameY;
        if (tileFrameY != 0) {
            return;
        }

        bool flag = Main.rand.NextBool(40);
        if (!flag) {
            return;
        }

        tileFrameY += 30;
        if (Main.netMode != NetmodeID.SinglePlayer) {
            NetMessage.SendTileSquare(-1, i, j, 1);
        }
    }

    protected override bool CanBeCollected(int i, int j) => WorldGenHelper.GetTileSafely(i, j).TileFrameY != 0;

    Predicate<Point16> IGrowLikeTulip.ShouldGrow => (tilePosition) => {
        int i = tilePosition.X, j = tilePosition.Y;
        Tile tile = Main.tile[i, j];
        if (!(AnchorValidTileTypes.Contains(tile.TileType) && !Main.tile[i, j - 1].AnyWall())) {
            return false;
        }
        bool flag = true;
        if (Main.tile[i, j - 1].LiquidAmount > 0 && Main.tile[i, j - 1].LiquidType != LiquidID.Water) {
            flag = false;
        }
        return flag;
    };
}
