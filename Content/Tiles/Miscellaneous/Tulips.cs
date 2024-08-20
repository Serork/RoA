using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;

using RoA.Common.Tiles;
using System;
using Terraria;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class ExoticTulip : TulipTileBase {
    public override int[] AnchorValidTiles => [TileID.Sand];

    public override ushort ExtraChance => 300;

    public override ushort DropItem => (ushort)ModContent.ItemType<Items.Weapons.Druidic.Rods.ExoticTulip>();

    public override Color MapColor => new(216, 78, 142);
}

sealed class SweetTulip : TulipTileBase {
    public override int[] AnchorValidTiles => [TileID.JungleGrass];

    public override ushort ExtraChance => 30;

    public override byte StyleX => 1;

    public override ushort DropItem => (ushort)ModContent.ItemType<Items.Weapons.Druidic.Rods.SweetTulip>();

    public override Color MapColor => new(255, 165, 0);
}

sealed class WeepingTulip : TulipTileBase {
    public override int[] AnchorValidTiles => [TileID.PinkDungeonBrick, TileID.GreenDungeonBrick, TileID.BlueDungeonBrick];
    public override Predicate<ushort> ConditionForWallToBeValid => (wallType) => { return Main.wallDungeon[wallType]; };

    public override ushort ExtraChance => 30;

    public override byte StyleX => 2;

    public override byte Amount => 3;

    public override bool InUnderground => true;

    public override ushort DropItem => (ushort)ModContent.ItemType<Items.Weapons.Druidic.Rods.WeepingTulip>();

    public override Color MapColor => new(0, 0, 255);

    protected override void SafeSetStaticDefaults() {
        DustType = DustID.Bone;

        RootsDrawing.ShouldDraw[Type] = true;
    }
}