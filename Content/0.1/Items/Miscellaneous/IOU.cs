using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class IOU : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 42; int height = 26;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Yellow;
        Item.maxStack = 1;

        Item.value = Item.sellPrice(0, 20, 0, 0);
    }
}