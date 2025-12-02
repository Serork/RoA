using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Head)]
sealed class NFAHorns : ModItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        ItemGlowMaskHandler.RegisterArmorGlowMask(EquipType.Head, this);
    }

    public override void SetDefaults() {
        int width = 20; int height = 26;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }

    public override bool IsVanitySet(int head, int body, int legs)
       => head == EquipLoader.GetEquipSlot(Mod, nameof(NFAHorns), EquipType.Head) &&
          body == EquipLoader.GetEquipSlot(Mod, nameof(NFAJacket), EquipType.Body) &&
          legs == EquipLoader.GetEquipSlot(Mod, nameof(NFAPants), EquipType.Legs);

    public override void UpdateVanitySet(Player player) {
        player.armorEffectDrawShadowLokis = true;
    }
}
