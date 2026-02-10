using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;

namespace RoA.Common.UI;

class UINixieTubeImage : UIFramedImage {
    private NixieTubeInfo _nixieTubeInfo;

    public UINixieTubeImage(Asset<Texture2D> texture, Rectangle frame, NixieTubeInfo nixieTubeInfo, Asset<Texture2D>? borderTexture = null) : base(texture, frame, borderTexture) {
        _nixieTubeInfo = nixieTubeInfo;
    }

    protected override bool ShouldDrawBorder() => IsMouseHovering;

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        if (NixieTubePicker_RemadePicker.PickedIndex == _nixieTubeInfo.Index || IsMouseHovering) {
            Color = Color.White;
        }
        else {
            Color = Color.White * 0.5f;
        }

        if (IsMouseHovering) {
            int indexToShow = _nixieTubeInfo.Index;
            int engRusCount = NixieTubePicker_RemadePicker.GetEngRusCount() + NixieTubePicker_RemadePicker.NUMCOUNT;
            if (indexToShow >= NixieTubePicker_RemadePicker.NUMCOUNT && indexToShow < engRusCount) {
                indexToShow -= NixieTubePicker_RemadePicker.NUMCOUNT - 1;
            }
            if (indexToShow >= engRusCount && indexToShow < engRusCount + NixieTubePicker_RemadePicker.MISCCOUNT) {
                indexToShow -= engRusCount - 1;
            }
            Main.hoverItemName = NixieTubePicker_RemadePicker.GetHoverText(_nixieTubeInfo.Index) + $" ({indexToShow - 1})";
        }

        base.DrawSelf(spriteBatch);
    }
}
