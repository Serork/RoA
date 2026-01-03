using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsRoots2_3 : BackwoodsRocks0 {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileID.Sets.BreakableWhenPlacing[Type] = true;
        TileID.Sets.IgnoredInHouseScore[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(110, 91, 74));

        MineResist = 0.01f;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        Dust.NewDustDirect(new Vector2(i * 16f, j * 16f), 16, 16, ModContent.DustType<WoodTrash>());

        return false;
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        if (!WorldGenHelper.GetTileSafely(i - 1, j).HasTile) {
            WorldGen.KillTile(i, j);
        }
        else if ((Main.tile[i, j].TileFrameY == 0 && !(Main.tile[i, j + 1].TileType == Type && Main.tile[i, j + 1].TileFrameY == Main.tile[i, j].TileFrameY + 18)) ||
            (Main.tile[i, j].TileFrameY != 0 && !(Main.tile[i, j - 1].TileType == Type && Main.tile[i, j - 1].TileFrameY == Main.tile[i, j].TileFrameY - 18))) {
            WorldGen.KillTile(i, j);
        }

        return false;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        width = 24;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) { }
}