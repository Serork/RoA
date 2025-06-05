using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class DullDaikatana : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 44; int height = 46;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;
        Item.maxStack = Item.CommonMaxStack;

        Item.value = Item.sellPrice(0, 1, 25, 0);
    }
}