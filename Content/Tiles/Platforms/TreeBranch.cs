using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using Terraria.GameContent;

using RoA.Core.Utility;

using System;
using RoA.Common.Tiles;
using RoA.Content.Dusts.Backwoods;

namespace RoA.Content.Tiles.Platforms;

class TreeBranch : ModTile {
    protected virtual int FrameCount => 2;

    public override void SetStaticDefaults() {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileAxe[Type] = true;

        TileID.Sets.Platforms[Type] = true;

        CanBeSlopedTileSystem.Included[Type] = true;

        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 32;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.newTile.StyleMultiplier = 2;
        TileObjectData.newTile.UsesCustomCanPlace = false;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);

        AdjTiles = [TileID.Platforms];

        RegisterItemDrop(ModContent.ItemType<Items.Materials.Elderwood>());
        DustType = ModContent.DustType<WoodTrash>();

        AddMapEntry(new Color(162, 82, 45), CreateMapEntryName());
    }

    public override void PostSetDefaults() => Main.tileNoSunLight[Type] = false;

    public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return false;

        Tile leftTile = WorldGenHelper.GetTileSafely(i - 1, j), rightTile = WorldGenHelper.GetTileSafely(i + 1, j);
        if ((!rightTile.HasTile && !leftTile.HasTile) ||
            (rightTile.TileType != TileID.Trees && leftTile.TileType != TileID.Trees)) {
            WorldGen.KillTile(i, j);
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j, 1f);
            }
        }

        return false;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        ulong seedForRandomness = (ulong)(i + j);
        int frame = Math.Min(Utils.RandomInt(ref seedForRandomness, FrameCount + 1), FrameCount - 1);
        bool reversed = true;
        Tile leftTile = WorldGenHelper.GetTileSafely(i - 1, j), rightTile = WorldGenHelper.GetTileSafely(i + 1, j);
        bool hasRightTile = rightTile.HasTile;
        if (hasRightTile && leftTile.TileType != TileID.Trees) {
            reversed = false;
        }
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        Texture2D texture = TextureAssets.Tile[Type].Value;
        int frameWidth = texture.Width, frameHeight = texture.Height / FrameCount;
        Vector2 drawPosition = new Point(i, j).ToVector2() * 16f + zero;
        drawPosition.X += -(!reversed ? (frameWidth / 2 - 2) : 2);
        Rectangle? sourceRectangle = new Rectangle(0, frameHeight * frame, frameWidth, frameHeight);
        Main.EntitySpriteDraw(texture, 
                              drawPosition - Main.screenPosition,
                              sourceRectangle,
                              Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, 
                              !reversed ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

        return false;
    }
}
