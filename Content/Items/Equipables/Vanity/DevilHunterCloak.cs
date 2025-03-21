using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Body)]
sealed class DevilHunterCloak : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Devil Hunter's Cloak");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 30; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Orange;
        Item.vanity = true;

        Item.value = Item.sellPrice(0, 0, 30, 0);
    }
}
