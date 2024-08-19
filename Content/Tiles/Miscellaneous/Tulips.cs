using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria;
using RoA.Core;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class ExoticTulip : TulipBase {
    protected override int[] AnchorValidTiles => [TileID.Sand];

    protected override ushort ExtraChance => 300;

    protected override ushort DropItem => (ushort)ModContent.ItemType<Items.Weapons.Druidic.Rods.ExoticTulip>();

    protected override Color MapColor => new(216, 78, 142);
}

sealed class SweetTulip : TulipBase {
    protected override int[] AnchorValidTiles => [TileID.JungleGrass];

    protected override ushort ExtraChance => 30;

    protected override byte StyleX => 1;

    protected override ushort DropItem => (ushort)ModContent.ItemType<Items.Weapons.Druidic.Rods.SweetTulip>();

    protected override Color MapColor => new(255, 165, 0);
}

sealed class WeepingTulip : TulipBase {
    protected override int[] AnchorValidTiles => [TileID.PinkDungeonBrick, TileID.GreenDungeonBrick, TileID.BlueDungeonBrick];

    protected override ushort ExtraChance => 30;

    protected override byte StyleX => 2;

    protected override byte Amount => 3;

    protected override bool InDungeon => true;

    protected override ushort DropItem => (ushort)ModContent.ItemType<Items.Weapons.Druidic.Rods.WeepingTulip>();

    protected override Color MapColor => new(0, 0, 255);

    protected override void SafeSetStaticDefaults() => DustType = DustID.Bone;

    private sealed class RootsDrawing : GlobalTile {
        private int _tileFrameX;

        public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
            if (_tileFrameX == 0)
                _tileFrameX = Main.rand.Next(0, 3);
            Tile tile = Main.tile[i, j];
            if (tile.TileType != TileID.PinkDungeonBrick && tile.TileType != TileID.GreenDungeonBrick && tile.TileType != TileID.BlueDungeonBrick)
                return;
            if (Main.tile[i, j - 1].TileType != ModContent.TileType<WeepingTulip>())
                return;
            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
                zero = Vector2.Zero;
            Color color = Lighting.GetColor(i, j);
            SpriteFrame frame = new(3, 1);
            Asset<Texture2D> texture = ModContent.Request<Texture2D>(ResourceManager.TilesTextures + "Roots");
            Rectangle rectangle = frame.GetSourceRectangle(texture.Value);
            int width = texture.Width() / 3;
            rectangle.X = width * _tileFrameX;
            spriteBatch.Draw(texture.Value,
            new Vector2(i * 16 - (int)Main.screenPosition.X - 2, j * 16 - (int)Main.screenPosition.Y) + zero, rectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }
    }
}