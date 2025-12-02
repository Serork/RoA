using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;

using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class Herbs : ModTile, TileHooks.IGetTileDrawData {
    public void GetTileDrawData(TileDrawing self, int x, int y, Tile tileCache, ushort typeCache, ref short tileFrameX, ref short tileFrameY, ref int tileWidth, ref int tileHeight, ref int tileTop, ref int halfBrickHeight, ref int addFrX, ref int addFrY, ref SpriteEffects tileSpriteEffect, ref Texture2D glowTexture, ref Rectangle glowSourceRect, ref Color glowColor) {
        tileHeight = 20;
        tileTop = -2;
        if (x % 2 == 0)
            tileSpriteEffect = SpriteEffects.FlipHorizontally;
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileSpelunker[Type] = true;
        Main.tileLighted[Type] = true;

        HitSound = SoundID.Grass;

        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.IgnoredInHouseScore[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.addTile(Type);

        LocalizedText name = Lang.GetItemName(ItemID.Daybloom);
        AddMapEntry(new Color(246, 197, 26), name);
        name = Lang.GetItemName(ItemID.Moonglow);
        AddMapEntry(new Color(76, 150, 216), name);
        name = Lang.GetItemName(ItemID.Blinkroot);
        AddMapEntry(new Color(185, 214, 42), name);
        name = Lang.GetItemName(ItemID.Deathweed);
        AddMapEntry(new Color(167, 203, 37), name);
        name = Lang.GetItemName(ItemID.Waterleaf);
        AddMapEntry(new Color(32, 168, 117), name);
        name = Lang.GetItemName(ItemID.Fireblossom);
        AddMapEntry(new Color(177, 69, 49), name);
        name = Lang.GetItemName(ItemID.Shiverthorn);
        AddMapEntry(new Color(40, 152, 240), name);
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        int num2 = Main.tile[i, j].TileFrameX / 18;
        float num3 = 0f;
        switch (num2) {
            case 2:
                num3 = (float)(270 - Main.mouseTextColor) / 800f;
                if (num3 > 1f)
                    num3 = 1f;
                else if (num3 < 0f)
                    num3 = 0f;
                r = num3 * 0.7f;
                g = num3;
                b = num3 * 0.1f;
                break;
            case 5:
                num3 = 0.9f;
                r = num3;
                g = num3 * 0.8f;
                b = num3 * 0.2f;
                break;
            case 6:
                num3 = 0.08f;
                g = num3 * 0.8f;
                b = num3;
                break;
        }
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        var tileFrameX = Main.tile[i, j].TileFrameX;
        if (tileFrameX < 18)
            yield return new Item(ItemID.Daybloom);
        else if (tileFrameX < 36)
            yield return new Item(ItemID.Moonglow);
        else if (tileFrameX < 54)
            yield return new Item(ItemID.Blinkroot);
        else if (tileFrameX < 72)
            yield return new Item(ItemID.Deathweed);
        else if (tileFrameX < 90)
            yield return new Item(ItemID.Waterleaf);
        else if (tileFrameX < 108)
            yield return new Item(ItemID.Fireblossom);
        else
            yield return new Item(ItemID.Shiverthorn);
    }

    public override ushort GetMapOption(int i, int j) {
        var tileFrameX = Main.tile[i, j].TileFrameX;
        if (tileFrameX < 18)
            return 0;
        else if (tileFrameX < 36)
            return 1;
        else if (tileFrameX < 54)
            return 2;
        else if (tileFrameX < 72)
            return 3;
        else if (tileFrameX < 90)
            return 4;
        else if (tileFrameX < 108)
            return 5;
        else
            return 6;

    }

    public override bool CreateDust(int i, int j, ref int type) {
        int num16 = Main.tile[i, j].TileFrameX / 18;
        if (num16 == 0)
            type = 3;

        if (num16 == 1)
            type = 3;

        if (num16 == 2)
            type = 7;

        if (num16 == 3)
            type = 17;

        if (num16 == 4)
            type = 289;

        if (num16 == 5)
            type = 6;

        if (num16 == 6)
            type = 224;

        return base.CreateDust(i, j, ref type);
    }
}
