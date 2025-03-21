using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class BunnyHat : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Bunny Hat");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 22; int height = 26;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Green;
        Item.vanity = true;

        Item.value = Item.sellPrice(silver: 50);
    }

    public override bool IsVanitySet(int head, int body, int legs) => body == ModContent.ItemType<BunnyJacket>() && legs == ModContent.ItemType<BunnyPants>();
}

