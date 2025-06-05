using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts.Backwoods;

using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsRoots2_2 : BackwoodsRocks0 {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
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
        if (!WorldGen.SolidTile(Main.tile[i, j - 1])) {
            WorldGen.KillTile(i, j);
        }
        else if ((Main.tile[i, j].TileFrameX % 36 == 0 && !(Main.tile[i + 1, j].TileType == Type && Main.tile[i + 1, j].TileFrameX == Main.tile[i, j].TileFrameX + 18)) ||
            (Main.tile[i, j].TileFrameX % 36 != 0 && !(Main.tile[i - 1, j].TileType == Type && Main.tile[i - 1, j].TileFrameX == Main.tile[i, j].TileFrameX - 18))) {
            WorldGen.KillTile(i, j);
        }

        return false;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        height = 22;
        offsetY -= 4;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) { }
}