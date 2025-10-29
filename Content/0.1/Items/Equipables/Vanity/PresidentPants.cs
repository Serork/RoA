using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Legs)]
sealed class PresidentPants : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("President's Pants");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.vanity = true;

        Item.value = Item.sellPrice(0, 4, 0, 0);
    }
}
