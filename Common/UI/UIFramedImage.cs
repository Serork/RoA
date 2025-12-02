using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.UI;

namespace RoA.Common.UI;

class UIFramedImage : UIElement {
    private Asset<Texture2D> _texture, _borderTexture;
    public float ImageScale = 1f;
    public float Rotation;
    public bool ScaleToFit;
    public bool AllowResizingDimensions = true;
    public Color Color = Color.White;
    public Vector2 NormalizedOrigin = Vector2.Zero;
    public bool RemoveFloatingPointsFromDrawPosition;
    private Texture2D _nonReloadingTexture;
    private Rectangle _frame;

    protected virtual bool ShouldDrawBorder() => true;

    public UIFramedImage(Asset<Texture2D> texture, Rectangle frame, Asset<Texture2D>? borderTexture = null) {
        SetImage(texture, borderTexture);
        _frame = frame;
    }

    public UIFramedImage(Texture2D nonReloadingTexture, Rectangle frame) {
        SetImage(nonReloadingTexture);
        _frame = frame;
    }

    public void SetImage(Asset<Texture2D> texture, Asset<Texture2D>? borderTexture = null) {
        _texture = texture;
        if (borderTexture != null) {
            _borderTexture = borderTexture;
        }
        _nonReloadingTexture = null;
        if (AllowResizingDimensions) {
            Width.Set(_texture.Width(), 0f);
            Height.Set(_texture.Height(), 0f);
        }
    }

    public void SetImage(Texture2D nonReloadingTexture) {
        _texture = null;
        _nonReloadingTexture = nonReloadingTexture;
        if (AllowResizingDimensions) {
            Width.Set(_nonReloadingTexture.Width, 0f);
            Height.Set(_nonReloadingTexture.Height, 0f);
        }
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        CalculatedStyle dimensions = GetDimensions();
        Texture2D texture2D = null;
        if (_texture != null)
            texture2D = _texture.Value;

        if (_nonReloadingTexture != null)
            texture2D = _nonReloadingTexture;

        Texture2D borderTexture2D = null;
        if (_borderTexture != null)
            borderTexture2D = _borderTexture.Value;

        if (ScaleToFit) {
            spriteBatch.Draw(texture2D, dimensions.ToRectangle(), Color);
            return;
        }

        Vector2 vector = _frame.Size();
        Vector2 vector2 = dimensions.Position() + vector * (1f - ImageScale) / 2f + vector * NormalizedOrigin;
        if (RemoveFloatingPointsFromDrawPosition)
            vector2 = vector2.Floor();

        Color color = Color;

        spriteBatch.Draw(texture2D, vector2.Floor(), _frame, color, Rotation, vector * NormalizedOrigin, ImageScale, SpriteEffects.None, 0f);
        if (ShouldDrawBorder() && borderTexture2D != null) {
            spriteBatch.Draw(borderTexture2D, vector2.Floor(), null, color, Rotation, vector * NormalizedOrigin, ImageScale, SpriteEffects.None, 0f);
        }
    }
}
