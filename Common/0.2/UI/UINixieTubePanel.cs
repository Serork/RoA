using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core;

using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.UI;

sealed class UINixieTubePanel : UIElement {
    private int _cornerSize = 12;
    private int _barSize = 4;
    private Asset<Texture2D> _nineSlicedTexture;

    // Added by TML.
    private bool _needsTextureLoading;

    private void LoadTextures() {
        // These used to be moved to OnActivate in order to avoid texture loading on JIT thread.
        // Doing so caused issues with missing backgrounds and borders because Activate wasn't always being called.
        if (_nineSlicedTexture == null)
            _nineSlicedTexture = ModContent.Request<Texture2D>(ResourceManager.UITextures + "NixieTube_Panel");
    }

    public UINixieTubePanel() {
        SetPadding(_cornerSize);
        _needsTextureLoading = true;
    }

    public UINixieTubePanel(Asset<Texture2D> customBackground, Asset<Texture2D> customborder, int customCornerSize = 12, int customBarSize = 4) {
        if (_nineSlicedTexture == null)
            _nineSlicedTexture = customborder;

        _cornerSize = customCornerSize;
        _barSize = customBarSize;
        SetPadding(_cornerSize);
    }

    private void DrawPanel(SpriteBatch spriteBatch, Texture2D texture) {
        CalculatedStyle dimensions = GetDimensions();
        Point point = new Point((int)dimensions.X, (int)dimensions.Y);
        Point point2 = new Point(point.X + (int)dimensions.Width - _cornerSize, point.Y + (int)dimensions.Height - _cornerSize);
        int width = point2.X - point.X - _cornerSize;
        int height = point2.Y - point.Y - _cornerSize;
        Color color = Color.White;
        //spriteBatch.Draw(texture, new Rectangle(point.X, point.Y, _cornerSize, _cornerSize), new Rectangle(0, 0, _cornerSize, _cornerSize), color);
        //spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y, _cornerSize, _cornerSize), new Rectangle(_cornerSize + _barSize, 0, _cornerSize, _cornerSize), color);
        //spriteBatch.Draw(texture, new Rectangle(point.X, point2.Y, _cornerSize, _cornerSize), new Rectangle(0, _cornerSize + _barSize, _cornerSize, _cornerSize), color);
        //spriteBatch.Draw(texture, new Rectangle(point2.X, point2.Y, _cornerSize, _cornerSize), new Rectangle(_cornerSize + _barSize, _cornerSize + _barSize, _cornerSize, _cornerSize), color);
        //spriteBatch.Draw(texture, new Rectangle(point.X + _cornerSize, point.Y, width, _cornerSize), new Rectangle(_cornerSize, 0, _barSize, _cornerSize), color);
        //spriteBatch.Draw(texture, new Rectangle(point.X + _cornerSize, point2.Y, width, _cornerSize), new Rectangle(_cornerSize, _cornerSize + _barSize, _barSize, _cornerSize), color);
        //spriteBatch.Draw(texture, new Rectangle(point.X, point.Y + _cornerSize, _cornerSize, height), new Rectangle(0, _cornerSize, _cornerSize, _barSize), color);
        //spriteBatch.Draw(texture, new Rectangle(point2.X, point.Y + _cornerSize, _cornerSize, height), new Rectangle(_cornerSize + _barSize, _cornerSize, _cornerSize, _barSize), color);
        //spriteBatch.Draw(texture, new Rectangle(point.X + _cornerSize, point.Y + _cornerSize, width, height), new Rectangle(_cornerSize, _cornerSize, _barSize, _barSize), color);

        DrawCustomBox(spriteBatch, texture, dimensions.ToRectangle(), color, 6);
    }

    public static void DrawCustomBox(SpriteBatch sb, Texture2D tex, Rectangle target, Color color, int cornerSize) {
        int centerSize = tex.Width - cornerSize * 2;

        if (target.Width < cornerSize * 2 || target.Height < cornerSize * 2)
            return;

        var sourceCorner = new Rectangle(0, 0, cornerSize, cornerSize);
        var sourceCorner1 = new Rectangle(tex.Width - cornerSize, 0, cornerSize, cornerSize);
        var sourceCorner2 = new Rectangle(tex.Width - cornerSize, tex.Height - cornerSize, cornerSize, cornerSize);
        var sourceCorner3 = new Rectangle(0, tex.Height - cornerSize, cornerSize, cornerSize);

        var sourceEdge = new Rectangle(cornerSize, 0, centerSize, cornerSize);
        var sourceEdge1 = new Rectangle(0, cornerSize, cornerSize, centerSize);
        var sourceEdge2 = new Rectangle(tex.Width - cornerSize, cornerSize, cornerSize, centerSize);
        var sourceEdge3 = new Rectangle(cornerSize, tex.Height - cornerSize, centerSize, cornerSize);

        var sourceCenter = new Rectangle(cornerSize, cornerSize, centerSize, centerSize);

        Rectangle inner = target;
        inner.Inflate(-centerSize, -centerSize);

        sb.Draw(tex, inner, sourceCenter, color);

        sb.Draw(tex, new Rectangle(target.X + cornerSize, target.Y, target.Width - cornerSize * 2, cornerSize), sourceEdge, color, 0, Vector2.Zero, 0, 0);
        sb.Draw(tex, new Rectangle(target.X, target.Y + cornerSize, cornerSize, target.Height - cornerSize * 2), sourceEdge1, color, 0, Vector2.Zero, 0, 0);
        sb.Draw(tex, new Rectangle(target.X - cornerSize + target.Width, target.Y + cornerSize, cornerSize, target.Height - cornerSize * 2), sourceEdge2, color, 0, Vector2.Zero, 0, 0);
        sb.Draw(tex, new Rectangle(target.X + cornerSize, target.Y + target.Height - cornerSize, target.Width - cornerSize * 2, cornerSize), sourceEdge3, color, 0, Vector2.Zero, 0, 0);

        sb.Draw(tex, new Rectangle(target.X, target.Y, cornerSize, cornerSize), sourceCorner, color, 0, Vector2.Zero, 0, 0);
        sb.Draw(tex, new Rectangle(target.X + target.Width - cornerSize, target.Y, cornerSize, cornerSize), sourceCorner1, color, 0, Vector2.Zero, 0, 0);
        sb.Draw(tex, new Rectangle(target.X + target.Width - cornerSize, target.Y + target.Height - cornerSize, cornerSize, cornerSize), sourceCorner2, color, 0, Vector2.Zero, 0, 0);
        sb.Draw(tex, new Rectangle(target.X, target.Y + target.Height - cornerSize, cornerSize, cornerSize), sourceCorner3, color, 0, Vector2.Zero, 0, 0);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        if (_needsTextureLoading) {
            _needsTextureLoading = false;
            LoadTextures();
        }

        if (_nineSlicedTexture != null)
            DrawPanel(spriteBatch, _nineSlicedTexture.Value);
    }
}