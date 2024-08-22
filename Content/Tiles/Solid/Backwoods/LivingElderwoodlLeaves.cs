using FullSerializer.Internal;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Gores;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class LivingElderwoodlLeaves : ModTile {
	public override void SetStaticDefaults() {
        TileHelper.Solid(Type, false, false);
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<LivingElderwood>());
        TileHelper.MergeWith(Type, TileID.Dirt);

        TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        HitSound = SoundID.Grass;
        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
		AddMapEntry(new Color(0, 128, 0));
	}

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        // adapted vanilla
        IEntitySource entitySource = new EntitySource_TileUpdate(i, j);
        ushort leafGoreType = (ushort)ModContent.GoreType<BackwoodsLeaf>();
        int x = i, y = j;
        if (Main.rand.NextBool(2500)) {
            Tile tile = Main.tile[x, y + 1];
            if (!WorldGen.SolidTile(tile) && !tile.AnyLiquid()) {
                float windForVisuals = Main.WindForVisuals;
                if ((!(windForVisuals < -0.2f) || (!WorldGen.SolidTile(Main.tile[x - 1, y + 1]) && !WorldGen.SolidTile(Main.tile[x - 2, y + 1]))) && (!(windForVisuals > 0.2f) || (!WorldGen.SolidTile(Main.tile[x + 1, y + 1]) && !WorldGen.SolidTile(Main.tile[x + 2, y + 1]))))
                    Gore.NewGorePerfect(entitySource, new Vector2(x * 16, y * 16 + 16), Vector2.Zero, leafGoreType).Frame.CurrentColumn = Main.tile[x, y].TileColor;
            }
            if (Main.rand.NextBool()) {
                int num = 0;
                if (Main.WindForVisuals > 0.2f)
                    num = 1;
                else if (Main.WindForVisuals < -0.2f)
                    num = -1;

                tile = Main.tile[x + num, y];
                if (!WorldGen.SolidTile(tile) && !tile.AnyLiquid()) {
                    int num2 = 0;
                    if (num == -1)
                        num2 = -10;

                    Gore.NewGorePerfect(entitySource, new Vector2(x * 16 + 8 + 4 * num + num2, y * 16 + 8), Vector2.Zero, leafGoreType).Frame.CurrentColumn = Main.tile[x, y].TileColor;
                }
            }
        }
    }
}