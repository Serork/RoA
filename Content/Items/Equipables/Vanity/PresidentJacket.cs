using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Body)]
sealed class PresidentJacket : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("President's Jacket");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 30; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.vanity = true;

        Item.value = Item.sellPrice(0, 4, 0, 0);
    }
}
