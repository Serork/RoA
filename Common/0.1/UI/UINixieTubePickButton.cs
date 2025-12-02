using Microsoft.Xna.Framework;

using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.UI;

namespace RoA.Common.UI;

sealed class UINixieTubePickButton : UIElement {
    private UINixieTubeImage _image;
    private NixieTubeInfo _nixieTubeInfo;
    private float _scale;

    public UINixieTubePickButton(NixieTubeInfo nixieTubeInfo, float scale = 1f) {
        _scale = scale;

        Width.Set(28f * _scale, 0f);
        Height.Set(48f * _scale, 0f);
        SetPadding(0f);

        _nixieTubeInfo = nixieTubeInfo;

        UIElement uIElement = new UIElement {
            Width = new StyleDimension(0f, 1f),
            Height = new StyleDimension(0f, 1f),
            HAlign = 0.5f,
            VAlign = 0.5f
        };
        uIElement.SetPadding(0f);
        var texture = NixieTubePicker_TextureLoader.NixieTubePickButton;
        var borderTexture = NixieTubePicker_TextureLoader.NixieTubePickButtonBorder;
        SpriteFrame spriteFrame = new(41, 3, nixieTubeInfo.Index, 0);
        if (NixieTubePicker_RemadePicker.IsRussian()) {
            bool flag = false;
            int checkCount = NixieTubePicker_RemadePicker.NUMCOUNT + NixieTubePicker_RemadePicker.RUSCOUNT;
            if (nixieTubeInfo.Index >= checkCount) {
                byte column = (byte)(NixieTubePicker_RemadePicker.NUMCOUNT + NixieTubePicker_RemadePicker.ENGCOUNT + (nixieTubeInfo.Index - (checkCount)));
                if (column >= 41) {
                    column -= 41;
                    spriteFrame.CurrentRow = 1;
                }
                else {
                    spriteFrame.CurrentRow = 0;
                }
                    spriteFrame.CurrentColumn = column;
                flag = true;
            }
            if (!flag && nixieTubeInfo.Index >= NixieTubePicker_RemadePicker.NUMCOUNT) {
                spriteFrame.CurrentColumn -= NixieTubePicker_RemadePicker.NUMCOUNT;
                spriteFrame.CurrentRow = 2;
            }
        }
        else if (nixieTubeInfo.Index >= 41) {
            spriteFrame.CurrentColumn -= 41;
            spriteFrame.CurrentRow = 1;
        }
        Rectangle frame = spriteFrame.GetSourceRectangle(texture.Value);
        _image = new UINixieTubeImage(texture, frame, _nixieTubeInfo, borderTexture) {
            VAlign = 0.5f,
            HAlign = 0.5f,
            ImageScale = 0.75f * _scale
        };
        uIElement.Append(_image);
        Append(uIElement);

        OnLeftClick += UINixieTubePickButton_OnLeftClick;
        OnMouseOut += UINixieTubePickButton_OnMouseOut;
        OnMouseOver += UINixieTubePickButton_OnMouseOver;
    }

    public override void Update(GameTime gameTime) {
        Width.Set(28f * _scale, 0f);
        Height.Set(48f * _scale, 0f);
        SetPadding(0f);
    }

    private void UINixieTubePickButton_OnMouseOver(UIMouseEvent evt, UIElement listeningElement) {
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    private void UINixieTubePickButton_OnMouseOut(UIMouseEvent evt, UIElement listeningElement) {
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    private void UINixieTubePickButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
        NixieTubePicker_RemadePicker.ResetPickedIndex();
        //SoundEngine.PlaySound(SoundID.MenuTick);
        NixieTubePicker_RemadePicker.ChangeNixieTubeSymbol(_nixieTubeInfo.Index);
    }
}
