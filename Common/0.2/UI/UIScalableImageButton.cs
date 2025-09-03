﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace RoA.Common.UI;

class UIImageButton : UIElement {
    protected Asset<Texture2D> _texture;
    private float _visibilityActive = 1f;
    private float _visibilityInactive = 0.4f;
    private Asset<Texture2D> _borderTexture;

    public float Scale = 1f;

    public UIImageButton(Asset<Texture2D> texture) {
        _texture = texture;
        Width.Set(_texture.Width() * Scale, 0f);
        Height.Set(_texture.Height() * Scale, 0f);
    }

    public void SetHoverImage(Asset<Texture2D> texture) {
        _borderTexture = texture;
    }

    public void SetImage(Asset<Texture2D> texture) {
        _texture = texture;
        Width.Set(_texture.Width() * Scale, 0f);
        Height.Set(_texture.Height() * Scale, 0f);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        CalculatedStyle dimensions = GetDimensions();
        spriteBatch.Draw(_texture.Value, dimensions.Position().Floor(), null, Color.White * (base.IsMouseHovering ? _visibilityActive : _visibilityInactive), 0f, Vector2.Zero, Scale, 0, 0f);
        if (_borderTexture != null && base.IsMouseHovering)
            spriteBatch.Draw(_borderTexture.Value, dimensions.Position().Floor(), null, Color.White, 0f, Vector2.Zero, Scale, 0, 0f);
    }

    public override void MouseOver(UIMouseEvent evt) {
        base.MouseOver(evt);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void MouseOut(UIMouseEvent evt) {
        base.MouseOut(evt);
    }

    public void SetVisibility(float whenActive, float whenInactive) {
        _visibilityActive = MathHelper.Clamp(whenActive, 0f, 1f);
        _visibilityInactive = MathHelper.Clamp(whenInactive, 0f, 1f);
    }
}