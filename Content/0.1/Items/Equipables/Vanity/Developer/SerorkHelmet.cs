using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Head)]
sealed class SerorkHelmet : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 30; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }

    public override bool IsVanitySet(int head, int body, int legs)
       => (head == EquipLoader.GetEquipSlot(Mod, nameof(SerorkHelmet), EquipType.Head) || head == EquipLoader.GetEquipSlot(Mod, nameof(SerorkMask), EquipType.Head)) &&
          body == EquipLoader.GetEquipSlot(Mod, nameof(SerorkBreastplate), EquipType.Body) &&
          legs == EquipLoader.GetEquipSlot(Mod, nameof(SerorkGreaves), EquipType.Legs);
}
