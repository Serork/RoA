using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;
using Terraria.Localization;
using Terraria.UI;

namespace RoA.Common.UI;

sealed class UINixieTubeLanguageButton : UIImageButton {
    public UINixieTubeLanguageButton(Asset<Texture2D> texture) : base(texture) { }

    public override void Update(GameTime gameTime) {
        Width.Set(texture.Width() * Scale, 0f);
        Height.Set(texture.Height() * Scale, 0f);

        if (IsMouseHovering) {
            Main.instance.MouseText(Language.GetTextValue("Mods.RoA.NixieTubeLanguageButton"));
        }
    }
}
