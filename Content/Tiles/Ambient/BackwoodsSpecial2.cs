using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts.Backwoods;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class BackwoodsSpecial2 : BackwoodsRocks0 {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.BreakableWhenPlacing[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        DustType = ModContent.DustType<Stone>();

        AddMapEntry(new Color(34, 37, 46));
        AddMapEntry(new Color(110, 91, 74));

        MineResist = 0.01f;
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        height = 20;
        offsetY -= 4;
    }

    public override ushort GetMapOption(int i, int j) {
        var tileFrameX = Main.tile[i, j].TileFrameX;
        if (tileFrameX >= 108) {
            tileFrameX -= 108;
        }
        if (tileFrameX < 36)
            return 0;
        else
            return 1;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) { }

    public override bool CreateDust(int i, int j, ref int type) {
        Tile tile = Main.tile[i, j];
        short tileFrameX = tile.TileFrameX;
        if (tileFrameX >= 108) {
            tileFrameX -= 108;
        }
        if (tileFrameX < 36) {
            type = ModContent.DustType<Stone>();
        }
        else if (tileFrameX < 72) {
            type = Main.rand.NextBool() ? ModContent.DustType<WoodTrash>() : ModContent.DustType<Stone>();
        }
        else {
            type = ModContent.DustType<WoodTrash>();
        }

        return true;
    }

    public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) {
        wormChance = 6;
    }
}