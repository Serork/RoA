using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.UI;

sealed class UINixieTubePickButton : UIElement {
    private UINixieTubeImage _image;
    private NixieTubeInfo _nixieTubeInfo;

    public UINixieTubePickButton(NixieTubeInfo nixieTubeInfo) {
        Width.Set(28f, 0f);
        Height.Set(48f, 0f);
        SetPadding(0f);

        _nixieTubeInfo = nixieTubeInfo;

        UIElement uIElement = new UIElement {
            Width = new StyleDimension(0f, 1f),
            Height = new StyleDimension(0f, 1f),
            HAlign = 0.5f,
            VAlign = 0.5f
        };
        uIElement.SetPadding(0f);
        var texture = ModContent.Request<Texture2D>(ResourceManager.UITextures + "NixieTube_PickButton2");
        var borderTexture = ModContent.Request<Texture2D>(ResourceManager.UITextures + "NixieTube_PickButton2_Border");
        SpriteFrame spriteFrame = new(41, 2, nixieTubeInfo.Index, 0);
        if (nixieTubeInfo.Index > 41) {
            spriteFrame.CurrentColumn = 0;
            spriteFrame.CurrentRow = 1;
        }
        Rectangle frame = spriteFrame.GetSourceRectangle(texture.Value);
        _image = new UINixieTubeImage(texture, frame, _nixieTubeInfo.Index, borderTexture) {
            VAlign = 0.5f,
            HAlign = 0.5f,
            ImageScale = 0.75f
        };
        uIElement.Append(_image);
        Append(uIElement);

        OnLeftClick += UINixieTubePickButton_OnLeftClick;
        OnMouseOut += UINixieTubePickButton_OnMouseOut;
        OnMouseOver += UINixieTubePickButton_OnMouseOver;
    }

    private void UINixieTubePickButton_OnMouseOver(UIMouseEvent evt, UIElement listeningElement) {
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    private void UINixieTubePickButton_OnMouseOut(UIMouseEvent evt, UIElement listeningElement) {
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    private void UINixieTubePickButton_OnLeftClick(UIMouseEvent evt, UIElement listeningElement) {
        SoundEngine.PlaySound(SoundID.MenuTick);
        NixieTubePicker_RemadePicker.ChangeNixieTubeSymbol(_nixieTubeInfo.Index);
    }
}
