using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core.Utility;

using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Decorations;

sealed class Him : Painting {
    protected override Point16 Size() => new(6, 4);

    protected override bool IsPicture() => false;
}

sealed class NightsShroud : Painting {
    protected override Point16 Size() => new(3, 3);
}

sealed class Her : Painting {
    private static Asset<Texture2D>? _glowTexture;

    protected override void SafeSetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

        _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
    }

    protected override Point16 Size() => new(2, 2);

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        if (_glowTexture?.IsLoaded != true) {
            return;
        }

        Tile tile = Main.tile[i, j];
        Texture2D texture = _glowTexture.Value;
        Color glowColor = Color.White;
        glowColor.A = 100;
        spriteBatch.Draw(texture,
                         new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + TileHelper.ScreenOffset,
                         new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16),
                         glowColor * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }
}

sealed class FourPixels : Painting {
    protected override Point16 Size() => new(4, 3);

    protected override bool IsPicture() => false;
}

abstract class Painting : ModTile {
    protected abstract Point16 Size();
    protected virtual bool IsPicture() => true;

    public sealed override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;

        Main.tileSpelunker[Type] = true;

        TileID.Sets.FramesOnKillWall[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;
        TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;
        TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;

        TileID.Sets.Paintings[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
        Point16 size = Size();
        int width = size.X, height = size.Y;
        TileObjectData.newTile.Height = height;
        TileObjectData.newTile.Width = width;
        TileObjectData.newTile.CoordinateHeights = [.. Enumerable.Repeat(16, height)];
        TileObjectData.newTile.LavaDeath = true;
        Point16 origin = Point16.Zero;
        if (width == 4 && height == 3) {
            origin = new Point16(1, 1);
        }
        else if (width == 2 && height == 2) {
            origin = new Point16(1, 0);
        }
        else if (width == 3 && height == 3) {
            origin = new Point16(1, 1);
        }
        else if (width == 6 && height == 4) {
            origin = new Point16(2, 2);
        }
        TileObjectData.newTile.Origin = origin;
        TileObjectData.addTile(Type);

        string key = IsPicture() ? "Picture" : "Painting";
        AddMapEntry(new Color(99, 50, 30), Language.GetText($"MapObject.{key}"));

        SafeSetStaticDefaults();
    }

    protected virtual void SafeSetStaticDefaults() { }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;
}