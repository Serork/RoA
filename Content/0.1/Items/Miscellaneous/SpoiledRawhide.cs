using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class SpoiledRawhide : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Spoiled Rawhide");
        // Tooltip.SetDefault("This hide wasn't treated fast enough");

        Item.ResearchUnlockCount = 50;
    }

    public override void SetDefaults() {
        int width = 26; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Item.CommonMaxStack;
        Item.rare = ItemRarityID.Gray;
    }
}