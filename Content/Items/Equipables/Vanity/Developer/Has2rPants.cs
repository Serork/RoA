using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Legs)]
sealed class Has2rPants : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }
}
