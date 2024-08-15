using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class ExoticTulip : TulipBase {
    protected override int[] AnchorValidTiles => [TileID.Sand];

    protected override ushort DropItem => (ushort)ModContent.ItemType<Items.Weapons.Druidic.Staffs.ExoticTulip>();

    protected override Color MapColor => new(216, 78, 142);
}

sealed class SweetTulip : TulipBase {
    protected override int[] AnchorValidTiles => [TileID.JungleGrass];

    protected override byte StyleX => 1;

    protected override ushort DropItem => (ushort)ModContent.ItemType<Items.Weapons.Druidic.Staffs.SweetTulip>();

    protected override Color MapColor => new(255, 165, 0);
}

sealed class WeepingTulip : TulipBase {
    protected override int[] AnchorValidTiles => [TileID.PinkDungeonBrick, TileID.GreenDungeonBrick, TileID.BlueDungeonBrick];

    protected override byte StyleX => 2;

    protected override byte Amount => 3;

    protected override bool InDungeon => true;

    protected override ushort DropItem => (ushort)ModContent.ItemType<Items.Weapons.Druidic.Staffs.WeepingTulip>();

    protected override Color MapColor => new(0, 0, 255);

    protected override void SafeSetStaticDefaults() => DustType = DustID.Bone;
}