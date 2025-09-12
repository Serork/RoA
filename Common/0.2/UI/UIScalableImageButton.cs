using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core.Utility;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace RoA.Common.UI;

class UIImageButton : UIElement {
    protected Asset<Texture2D> texture;
    protected float visibilityActive = 1f;
    protected float visibilityInactive = 0.4f;
    protected Asset<Texture2D> borderTexture;

    public float Scale = 1f;

    public UIImageButton(Asset<Texture2D> texture) {
        this.texture = texture;
        Width.Set(this.texture.Width() * Scale, 0f);
        Height.Set(this.texture.Height() * Scale, 0f);
    }

    public void SetHoverImage(Asset<Texture2D> texture) {
        borderTexture = texture;
    }

    public void SetImage(Asset<Texture2D> texture) {
        this.texture = texture;
        Width.Set(this.texture.Width() * Scale, 0f);
        Height.Set(this.texture.Height() * Scale, 0f);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        CalculatedStyle dimensions = GetDimensions();
        spriteBatch.Draw(texture.Value, dimensions.Position().Floor(), null, Color.White * (base.IsMouseHovering ? visibilityActive : visibilityInactive), 0f, Vector2.Zero, Scale, 0, 0f);
        if (borderTexture != null && base.IsMouseHovering)
            spriteBatch.Draw(borderTexture.Value, dimensions.Position().Floor(), null, Color.White, 0f, Vector2.Zero, Scale, 0, 0f);
    }

    public override void MouseOver(UIMouseEvent evt) {
        base.MouseOver(evt);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void MouseOut(UIMouseEvent evt) {
        base.MouseOut(evt);
    }

    public void SetVisibility(float whenActive, float whenInactive) {
        visibilityActive = MathHelper.Clamp(whenActive, 0f, 1f);
        visibilityInactive = MathHelper.Clamp(whenInactive, 0f, 1f);
    }
}