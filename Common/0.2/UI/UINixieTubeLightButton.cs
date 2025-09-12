using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.UI;

namespace RoA.Common.UI;

sealed class UINixieTubeLightButton : UIImageButton {
    public UINixieTubeLightButton(Asset<Texture2D> texture) : base(texture) { }

    public override void Update(GameTime gameTime) {
        Width.Set(texture.Width() * Scale, 0f);
        Height.Set(texture.Height() / 2f * Scale, 0f);

        if (IsMouseHovering) {
            Main.instance.MouseText(Language.GetTextValue($"Mods.RoA.NixieTubeFlickerButton{(NixieTubePicker_RemadePicker.IsFlickerOff ? 2 : 1)}"));
        }
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        CalculatedStyle dimensions = GetDimensions();
        SpriteFrame frame = new(1, 2, 0, (byte)(NixieTubePicker_RemadePicker.IsFlickerOff ? 1 : 0));
        Rectangle sourceRectangle = frame.GetSourceRectangle(texture.Value);
        sourceRectangle.Height += 2;
        spriteBatch.Draw(texture.Value, dimensions.Position().Floor(), sourceRectangle, Color.White * (base.IsMouseHovering ? visibilityActive : visibilityInactive), 0f, Vector2.Zero, Scale, 0, 0f);
        if (borderTexture != null && base.IsMouseHovering)
            spriteBatch.Draw(borderTexture.Value, dimensions.Position().Floor(), null, Color.White, 0f, Vector2.Zero, Scale, 0, 0f);
    }
}
