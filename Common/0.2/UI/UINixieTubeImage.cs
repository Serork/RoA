using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;

namespace RoA.Common.UI;

class UINixieTubeImage : UIFramedImage {
    private byte _myIndex;

    public UINixieTubeImage(Asset<Texture2D> texture, Asset<Texture2D> borderTexture, Rectangle frame, byte index) : base(texture, frame) {
        _myIndex = index;
    }

    protected override bool ShouldDrawBorder() => IsMouseHovering;

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        if (NixieTubePicker_RemadePicker.PickedIndex == _myIndex || IsMouseHovering) {
            Color = Color.White;
        }
        else {
            Color = Color.White * 0.5f;
        }

        if (IsMouseHovering) {
            Main.hoverItemName = NixieTubePicker_RemadePicker.GetHoverText(_myIndex);
        }

        base.DrawSelf(spriteBatch);
    }
}
