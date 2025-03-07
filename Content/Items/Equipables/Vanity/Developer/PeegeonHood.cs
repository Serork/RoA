using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Head)]
sealed class PeegeonHood : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Peegeon's Hood");
        //Tooltip.SetDefault("'Great for impersonating RoA devs?' Sure!");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 26; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Cyan;

        Item.vanity = true;
    }

    public override bool IsVanitySet(int head, int body, int legs)
       => head == EquipLoader.GetEquipSlot(Mod, nameof(PeegeonHood), EquipType.Head) &&
          body == EquipLoader.GetEquipSlot(Mod, nameof(PeegeonChestguard), EquipType.Body) &&
          legs == EquipLoader.GetEquipSlot(Mod, nameof(PeegeonGreaves), EquipType.Legs);
}
