using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class LivingElderwood3 : LivingElderwood {
    public override string Texture => base.Texture[..^1];

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();

        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
    }

    public override bool CanExplode(int i, int j) => false;

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
}

sealed class LivingElderwood2 : LivingElderwood {
    public override string Texture => base.Texture[..^1];
}

class LivingElderwood : ModTile {
    public const int MINTILEREQUIRED = 65;

    public override void SetStaticDefaults() {
        TileHelper.Solid(Type, false, false);
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<BackwoodsGrass>());
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<BackwoodsStone>());
        TileHelper.MergeWith(Type, TileID.Dirt);
        TileHelper.MergeWith(Type, TileID.Grass);
        TileHelper.MergeWith(Type, TileID.JungleGrass);
        TileHelper.MergeWith(Type, TileID.Mud);
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<BackwoodsGreenMoss>());

        TileID.Sets.ChecksForMerge[Type] = true;

        TileID.Sets.GeneralPlacementTiles[Type] = false;
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Furniture>();
        AddMapEntry(new Color(107, 89, 73), CreateMapEntryName());

        MineResist = 1.5f;
        MinPick = MINTILEREQUIRED;
    }

    public override bool CanExplode(int i, int j) => NPC.downedBoss2;

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        yield return new Item(ModContent.ItemType<Items.Placeable.Solid.Elderwood>());
    }

    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
        WorldGen.TileMergeAttempt(-2, ModContent.TileType<LivingElderwoodlLeaves>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
        WorldGen.TileMergeAttempt(ModContent.TileType<LivingElderwood>(), TileID.Sets.Dirt, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
    }
}