using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Tiles;
using RoA.Content.Dusts;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

class BackwoodsRocks02 : BackwoodsRocks0 {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;

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

        MineResist = 0.01f;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) { }

    public override bool CreateDust(int i, int j, ref int type) {
        type = ModContent.DustType<Stone>();

        return true;
    }

    public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) {
        wormChance = 6;
    }
}

class BackwoodsRocks01 : BackwoodsRocks0 {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;

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

        MineResist = 0.01f;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) { }

    public override bool CreateDust(int i, int j, ref int type) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        bool flag = (tile.TileFrameX >= 1116 && tile.TileFrameX <= 1188) && tile.TileFrameY > 0;
        if (flag) {
            type = ModContent.DustType<WoodTrash>();
        }
        else if (tile.TileFrameX <= 108) {
            type = ModContent.DustType<Stone>();
        }

        return true;
    }

    public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) {
        wormChance = 6;
    }
}

class BackwoodsRocks0 : BackwoodsRocks1 {
    public override bool CreateDust(int i, int j, ref int type) {
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        if (tile.TileFrameX <= 108) {
            type = ModContent.DustType<Stone>();
        }
        else {
            type = ModContent.DustType<WoodTrash>();
        }

        return true;
    }
}

sealed class BackwoodsRocks2 : BackwoodsRocks1, TileHooks.IGetTileDrawData {
    public static Asset<Texture2D> GlowTexture { get; private set; } = null!;

    protected override void SafeSetStaticDefaults() {
        if (!Main.dedServ) {
            GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }
    }

    public void GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {
        glowTexture = GlowTexture.Value;
        glowColor = TileDrawingExtra.BackwoodsMossGlowColor;
        glowSourceRect = new Rectangle(tileFrameX, tileFrameY, tileWidth, tileHeight);
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => BackwoodsGreenMoss.SetupLight(ref r, ref g, ref b);

    public override bool CreateDust(int i, int j, ref int type) {
        if (Main.rand.NextBool()) {
            type = ModContent.DustType<TealMossDust>();
        }

        return base.CreateDust(i, j, ref type);
    }
}

class BackwoodsRocks1 : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 1;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        DustType = ModContent.DustType<Stone>();
        AddMapEntry(new Color(34, 37, 46));

        MineResist = 0.01f;

        SafeSetStaticDefaults();
    }

    protected virtual void SafeSetStaticDefaults() { }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
        if (i % 2 != 1) {
            return;
        }

        spriteEffects = SpriteEffects.FlipHorizontally;
    }

    public override void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) {
        wormChance = 8;
    }
}
