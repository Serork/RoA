using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class EmptyPlanterBox : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
    }

    public override void SetDefaults() {
        int width = 28; int height = 16;
        Item.Size = new Microsoft.Xna.Framework.Vector2(width, height);

        Item.value = Item.sellPrice(0, 0, 0, 20);
        Item.rare = ItemRarityID.White;
        Item.maxStack = Item.CommonMaxStack;
    }
}
