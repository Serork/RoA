using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Microsoft.Xna.Framework;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Legs)]
sealed class Has2rPants : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 18;
        Item.Size = new Vector2(width, height);

        Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Cyan;

        Item.vanity = true;
    }
}
