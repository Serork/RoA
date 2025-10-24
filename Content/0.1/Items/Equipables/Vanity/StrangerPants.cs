using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[Autoload(false)]
[AutoloadEquip(EquipType.Legs)]
sealed class StrangerPants : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Stranger's Pants");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;
        Item.value = Item.sellPrice(gold: 5);
        Item.vanity = true;
    }
}
