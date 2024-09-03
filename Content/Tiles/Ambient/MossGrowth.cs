using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.WorldEvents;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class MossGrowth : ModTile {
    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileLighted[Type] = true;

        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 0, 0);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 0, 0);
        TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.SolidTile, 0, 0);
        TileObjectData.newTile.AnchorLeft = new AnchorData(AnchorType.SolidTile, 0, 0);
        TileObjectData.newTile.CoordinateWidth = 20;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(29, 106, 88));
        DustType = DustID.GreenMoss;
        HitSound = SoundID.Grass;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => BackwoodsGreenMoss.SetupLight(ref r, ref g, ref b);

    public override void NearbyEffects(int i, int j, bool closer) {
        ushort moss = (ushort)ModContent.TileType<BackwoodsGreenMoss>();
        Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
        Tile belowTile = WorldGenHelper.GetTileSafely(i, j + 1);
        Tile leftTile = WorldGenHelper.GetTileSafely(i - 1, j);
        Tile rightTile = WorldGenHelper.GetTileSafely(i + 1, j);
        bool hasTile = false;
        if (aboveTile.ActiveTile(moss)) {
            hasTile = true;
        }
        else if (belowTile.ActiveTile(moss)) {
            hasTile = true;
        }
        else {
            if (leftTile.ActiveTile(moss)) {
                hasTile = true;
            }
            if (rightTile.ActiveTile(moss)) {
                hasTile = true; 
            }
        }
        if (hasTile) {
            return;
        }
        WorldGenHelper.GetTileSafely(i, j).HasTile = false;
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j, 1f);
        }
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        width = 22;
        short framesHeight = 54;
        ushort moss = (ushort)ModContent.TileType<BackwoodsGreenMoss>();
        Tile aboveTile = WorldGenHelper.GetTileSafely(i, j - 1);
        Tile belowTile = WorldGenHelper.GetTileSafely(i, j + 1);
        Tile leftTile = WorldGenHelper.GetTileSafely(i - 1, j);
        Tile rightTile = WorldGenHelper.GetTileSafely(i + 1, j);
        offsetY = 0;
        if (aboveTile.ActiveTile(moss)) {
            tileFrameY += framesHeight;
            offsetY = -2;
        }
        else if (belowTile.ActiveTile(moss)) {
            offsetY = 2;
        }
        else {
            if (leftTile.ActiveTile(moss)) {
                tileFrameY += (short)(framesHeight * 2);
            }
            if (rightTile.ActiveTile(moss)) {
                tileFrameY += (short)(framesHeight * 3);
            }
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (Main.LightingEveryFrame) {
            TileHelper.AddSpecialPoint(i, j, 12);
        }

        return false;
    }
}
