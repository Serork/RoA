using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;

using Stubble.Core.Imported;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace RoA.Common.UI;

sealed class UIClickableNPCButton : UIElement {
    private object _text = "";
    private float _textScale = 1f;
    private Vector2 _textSize = Vector2.Zero;
    private bool _isLarge;
    private Color _color = Color.White;
    private Color _shadowColor = Color.Black;
    private bool _isWrapped;
    public bool DynamicallyScaleDownToWidth;
    private string _visibleText;
    private string _lastTextReference;
    private bool _focus;

    public string Text => _text.ToString();

    public float TextOriginX { get; set; }

    public float TextOriginY { get; set; }

    public float WrappedTextBottomPadding { get; set; }

    public bool IsWrapped {
        get {
            return _isWrapped;
        }
        set {
            _isWrapped = value;
            if (value)
                MinWidth.Set(0, 0); // TML: IsWrapped when true should prevent changing MinWidth, otherwise can't shrink in width due to CreateWrappedText+GetInnerDimensions logic. IsWrapped is false in ctor, so need to undo changes.
            InternalSetText(_text, _textScale, _isLarge);
        }
    }

    public Color TextColor {
        get {
            return _color;
        }
        set {
            _color = value;
        }
    }

    public Color ShadowColor {
        get {
            return _shadowColor;
        }
        set {
            _shadowColor = value;
        }
    }

    public event Action OnInternalTextChange;

    public UIClickableNPCButton(string text, float textScale = 1f, bool large = false) {
        TextOriginX = 0.5f;
        TextOriginY = 0f;
        IsWrapped = false;
        WrappedTextBottomPadding = 20f;
        InternalSetText(text, textScale, large);
    }

    public UIClickableNPCButton(LocalizedText text, float textScale = 1f, bool large = false) {
        TextOriginX = 0.5f;
        TextOriginY = 0f;
        IsWrapped = false;
        WrappedTextBottomPadding = 20f;
        InternalSetText(text, textScale, large);
    }

    public override void Recalculate() {
        InternalSetText(_text, _textScale, _isLarge);
        base.Recalculate();
    }

    public void SetText(string text) {
        InternalSetText(text, _textScale, _isLarge);
    }

    public void SetText(LocalizedText text) {
        InternalSetText(text, _textScale, _isLarge);
    }

    public void SetText(string text, float textScale, bool large) {
        InternalSetText(text, textScale, large);
    }

    public void SetText(LocalizedText text, float textScale, bool large) {
        InternalSetText(text, textScale, large);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);
        VerifyTextState();
        CalculatedStyle innerDimensions = GetInnerDimensions();
        Vector2 position = innerDimensions.Position();
        if (_isLarge)
            position.Y -= 10f * _textScale;
        else
            position.Y -= 2f * _textScale;

        position.X += (innerDimensions.Width - _textSize.X) * TextOriginX;
        position.Y += (innerDimensions.Height - _textSize.Y) * TextOriginY;
        float num = _textScale;
        if (DynamicallyScaleDownToWidth && _textSize.X > innerDimensions.Width)
            num *= innerDimensions.Width / _textSize.X;

        DynamicSpriteFont value = (_isLarge ? FontAssets.DeathText : FontAssets.MouseText).Value;
        Vector2 vector = value.MeasureString(_visibleText);
        Color baseColor = _shadowColor * ((float)(int)_color.A / 255f);
        Vector2 origin = new Vector2(0.5f, 0.5f) * vector;
        Vector2 baseScale = new Vector2(num);
        TextSnippet[] snippets = ChatManager.ParseMessage(_visibleText, _color).ToArray();
        Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color(200, 200, 200, 200);
        int mouseTextColor = (Main.mouseTextColor * 2 + 255) / 3;
        Microsoft.Xna.Framework.Color black = Microsoft.Xna.Framework.Color.Black;
        Microsoft.Xna.Framework.Color brown = Microsoft.Xna.Framework.Color.Brown;
        Microsoft.Xna.Framework.Color color2 = new Microsoft.Xna.Framework.Color(mouseTextColor, (int)((double)mouseTextColor / 1.1), mouseTextColor / 2, mouseTextColor);
        Player player = Main.LocalPlayer;
        float num2 = 1.2f;
        Vector2 vec = new Vector2(Main.mouseX, Main.mouseY);
        if (vec.Between(position, position + vector * baseScale) && !PlayerInput.IgnoreMouseInterface) {
            player.mouseInterface = true;
            player.releaseUseItem = false;
            baseScale *= num2;

            if (!_focus)
                SoundEngine.PlaySound(SoundID.MenuTick);

            _focus = true;
        }
        else {
            if (_focus)
                SoundEngine.PlaySound(SoundID.MenuTick);

            _focus = false;
        }
        ChatManager.ConvertNormalSnippets(snippets);
        position += origin;
        ChatManager.DrawColorCodedStringShadow(spriteBatch, value, snippets, position, (!_focus) ? Microsoft.Xna.Framework.Color.Black : brown, 0f, origin, baseScale, -1f, 1.5f);
        ChatManager.DrawColorCodedString(spriteBatch, value, snippets, position, color2, 0f, origin, baseScale, out var _, -1f, ignoreColors: true);
    }

    private void VerifyTextState() {
        if ((object)_lastTextReference != Text)
            InternalSetText(_text, _textScale, _isLarge);
    }

    private void InternalSetText(object text, float textScale, bool large) {
        DynamicSpriteFont dynamicSpriteFont = (large ? FontAssets.DeathText.Value : FontAssets.MouseText.Value);
        _text = text;
        _isLarge = large;
        _textScale = textScale;
        _lastTextReference = _text.ToString();
        if (IsWrapped)
            _visibleText = dynamicSpriteFont.CreateWrappedText(_lastTextReference, GetInnerDimensions().Width / _textScale);
        else
            _visibleText = _lastTextReference;

        // TML: Changed to use ChatManager.GetStringSize() since using DynamicSpriteFont.MeasureString() ignores chat tags,
        // giving the UI element a much larger calculated size than it should have.
        Vector2 vector = ChatManager.GetStringSize(dynamicSpriteFont, _visibleText, new Vector2(1));

        Vector2 vector2 = (_textSize = ((!IsWrapped) ? (new Vector2(vector.X, large ? 32f : 16f) * textScale) : (new Vector2(vector.X, vector.Y + WrappedTextBottomPadding) * textScale)));
        if (!IsWrapped) { // TML: IsWrapped when true should prevent changing MinWidth, otherwise can't shrink in width due to logic.
            MinWidth.Set(vector2.X + PaddingLeft + PaddingRight, 0f);
        }
        MinHeight.Set(vector2.Y + PaddingTop + PaddingBottom, 0f);
        if (this.OnInternalTextChange != null)
            this.OnInternalTextChange();
    }
}