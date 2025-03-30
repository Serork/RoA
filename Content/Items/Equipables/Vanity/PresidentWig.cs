using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[Autoload(false)]
[AutoloadEquip(EquipType.Head)]
sealed class PresidentWig : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("President's Wig");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;
        Item.vanity = true;

        Item.value = Item.sellPrice(0, 4, 0, 0);
    }
}

