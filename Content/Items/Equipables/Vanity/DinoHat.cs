using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class DinoHat : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Dino's Hat");
        //Tooltip.SetDefault("'Scary monsters, super creeps\nKeep me running, running scared'");
        ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 24; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Blue;
        Item.vanity = true;

        Item.value = Item.sellPrice(0, 3, 0, 0);
    }
}
