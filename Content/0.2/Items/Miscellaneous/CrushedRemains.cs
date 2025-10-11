using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class CrushedRemains : ModItem {
    public override void SetDefaults() {
        Item.maxStack = Item.CommonMaxStack;
        int width = 36, height = 24;
        Item.Size = new Vector2(width, height);
        Item.rare = 1;
    }
}
