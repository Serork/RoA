using Microsoft.Xna.Framework;

using RoA.Common.Tiles;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Plants;

sealed class Bonerose : TulipLikeTileBaseButPlant {
    public override int[] AnchorValidTiles => [TileID.PinkDungeonBrick, TileID.GreenDungeonBrick, TileID.BlueDungeonBrick];
    public override Predicate<ushort> ConditionForWallToBeValid => (wallType) => { return Main.wallDungeon[wallType]; };

    public override Color MapColor => new(178, 178, 137);

    public override byte Amount => 5;

    public override bool InUnderground => true;

    public override ushort DropItem => (ushort)ModContent.ItemType<Items.Materials.Bonerose>();

    protected override void SafeSetStaticDefaults() {
        base.SafeSetStaticDefaults();

        DustType = DustID.Bone;
        HitSound = SoundID.NPCHit1;

        RootsDrawing.ShouldDraw[Type] = true;
    }
}
