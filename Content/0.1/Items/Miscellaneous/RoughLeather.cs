using Microsoft.Xna.Framework;

using Terraria;

namespace RoA.Content.Items.Miscellaneous;

sealed class RoughLeather : AnimalLeather {
    public override void SetDefaults() {
        base.SetDefaults();

        int width = 24; int height = 26;
        Item.Size = new Vector2(width, height);
    }
}