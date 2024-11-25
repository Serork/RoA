using Microsoft.Xna.Framework;

using RoA.Content.Dusts.Backwoods;
using RoA.Content.Items.Materials;
using RoA.Core.Utility;

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
	public override void SetStaticDefaults() {
        TileHelper.Solid(Type, false, false);
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<BackwoodsGrass>());
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<BackwoodsStone>());
        TileHelper.MergeWith(Type, TileID.Dirt);
        TileHelper.MergeWith(Type, TileID.Grass);
        TileHelper.MergeWith(Type, TileID.JungleGrass);
        TileHelper.MergeWith(Type, TileID.Mud);
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<BackwoodsGreenMoss>());

        TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        RegisterItemDrop(ModContent.ItemType<Elderwood>());
        DustType = (ushort)ModContent.DustType<WoodTrash>();
		AddMapEntry(new Color(162, 82, 45), CreateMapEntryName());
	}
}