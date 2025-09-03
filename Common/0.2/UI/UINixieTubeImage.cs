using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

namespace RoA.Common.UI;

class UINixieTubeImage : UIFramedImage {
    private byte _myIndex;

    public UINixieTubeImage(Asset<Texture2D> texture, Rectangle frame, byte index) : base(texture, frame) {
        _myIndex = index;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        if (NixieTubePicker_Remade.PickedIndex == _myIndex || IsMouseHovering) {
            Color = Color.White;
        }
        else {
            Color = Color.White * 0.5f;
        }

        base.DrawSelf(spriteBatch);
    }
}
