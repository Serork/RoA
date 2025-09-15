using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Common.Tiles;
using RoA.Common.WorldEvents;
using RoA.Content.Dusts;
using RoA.Content.Dusts.Backwoods;
using RoA.Content.Tiles.Trees;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Ambient;

sealed class FallenTree : ModTile, TileHooks.IRequireMinAxePower, TileHooks.IPostDraw {
    int TileHooks.IRequireMinAxePower.MinAxe => PrimordialTree.MINAXEREQUIRED;

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileLighted[Type] = true;
        Main.tileAxe[Type] = true;
        Main.tileObsidianKill[Type] = true;

        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.CoordinateHeights = [16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        DustType = ModContent.DustType<WoodTrash>();
        AddMapEntry(new Microsoft.Xna.Framework.Color(91, 74, 67), CreateMapEntryName());
    }

    public override bool CanExplode(int i, int j) {
        if (!NPC.downedBoss2) {
            return false;
        }

        return base.CanExplode(i, j);
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        TileHelper.AddPostSolidTileDrawPoint(this, i, j);

        return true;
    }

    void TileHooks.IPostDraw.PostDrawExtra(SpriteBatch spriteBatch, Point16 pos) {
        int i = pos.X, j = pos.Y;
        Tile tile = WorldGenHelper.GetTileSafely(i, j);
        Vector2 drawPosition = new(i * 16 - (int)Main.screenPosition.X - 18,
                                   j * 16 - (int)Main.screenPosition.Y);
        Color color = Lighting.GetColor(i, j);
        if (BackwoodsFogHandler.Opacity > 0f && TileDrawing.IsVisible(tile)) {
            SpriteBatchSnapshot snapshot = spriteBatch.CaptureSnapshot();
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.Transform);
            int height = tile.TileFrameY == 36 ? 18 : 16;
            ulong speed = (((ulong)j << 32) | (ulong)i);
            float posX = Utils.RandomInt(ref speed, -12, 13) * 0.0875f;
            float posY = Utils.RandomInt(ref speed, -12, 13) * 0.0875f;
            int directionX = Utils.RandomInt(ref speed, 2) == 0 ? 1 : -1;
            int directionY = Utils.RandomInt(ref speed, 2) != 0 ? 1 : -1;
            float opacity = BackwoodsFogHandler.Opacity > 0f ? BackwoodsFogHandler.Opacity : 1f;
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>(TileLoader.GetTile(ModContent.TileType<FallenTree>()).Texture + "_Glow").Value,
                                  new Vector2(i * 16 - (int)Main.screenPosition.X - Helper.Wave(-1.75f, 1.75f, 2f, (i * 16) + (j * 16) + (j << 32) | i) * directionX * posX,
                                  j * 16 - (int)Main.screenPosition.Y + 2 - Helper.Wave(-1.75f, 1.75f, 2f, (i * 16) + (j * 16) + (j << 32) | i) * directionY * posY),
                                  new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, height),
                                  Color.Lerp(Color.White, Lighting.GetColor(i, j), 0.8f) * opacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(in snapshot);
        }

        if (Main.rand.NextBool(1050)) {
            Dust dust = Dust.NewDustPerfect(drawPosition + Main.rand.Random2(0, tile.TileFrameX, 0, tile.TileFrameY), ModContent.DustType<TreeDust>());
            dust.velocity *= 0.5f + Main.rand.NextFloat() * 0.25f;
            dust.scale *= 1.1f;
        }
    }
}
