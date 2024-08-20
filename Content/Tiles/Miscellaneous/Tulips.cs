using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;

using RoA.Common.Tiles;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class ExoticTulip : TulipTileBase {
    protected override int[] AnchorValidTiles => [TileID.Sand];

    protected override ushort ExtraChance => 300;

    protected override ushort DropItem => (ushort)ModContent.ItemType<Items.Weapons.Druidic.Rods.ExoticTulip>();

    protected override Color MapColor => new(216, 78, 142);
}

sealed class SweetTulip : TulipTileBase {
    protected override int[] AnchorValidTiles => [TileID.JungleGrass];

    protected override ushort ExtraChance => 30;

    protected override byte StyleX => 1;

    protected override ushort DropItem => (ushort)ModContent.ItemType<Items.Weapons.Druidic.Rods.SweetTulip>();

    protected override Color MapColor => new(255, 165, 0);
}

sealed class WeepingTulip : TulipTileBase {
    protected override int[] AnchorValidTiles => [TileID.PinkDungeonBrick, TileID.GreenDungeonBrick, TileID.BlueDungeonBrick];

    protected override ushort ExtraChance => 30;

    protected override byte StyleX => 2;

    protected override byte Amount => 3;

    protected override bool InDungeon => true;

    protected override ushort DropItem => (ushort)ModContent.ItemType<Items.Weapons.Druidic.Rods.WeepingTulip>();

    protected override Color MapColor => new(0, 0, 255);

    protected override void SafeSetStaticDefaults() {
        DustType = DustID.Bone;

        RootsDrawing.ShouldDraw[Type] = true;
    }
}