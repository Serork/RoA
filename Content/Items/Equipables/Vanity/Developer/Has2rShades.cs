using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Head)]
[AutoloadGlowMask2("_Head_Glow")]
sealed class Has2rShades : ModItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        int width = 28; int height = 14;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
        if (drawPlayer.active && drawPlayer.hair == 26) {
            glowMask = VanillaGlowMaskHandler.GetID(Texture + "_Head_Glow");
            glowMaskColor = Color.White * 0.9f;
        }
    }

    public override bool IsVanitySet(int head, int body, int legs)
       => (head == EquipLoader.GetEquipSlot(Mod, nameof(Has2rMask), EquipType.Head) || head == EquipLoader.GetEquipSlot(Mod, nameof(Has2rShades), EquipType.Head)) &&
          body == EquipLoader.GetEquipSlot(Mod, nameof(Has2rJacket), EquipType.Body) &&
          legs == EquipLoader.GetEquipSlot(Mod, nameof(Has2rPants), EquipType.Legs);

    public override void UpdateVanitySet(Player player) => player.yoraiz0rDarkness = true;

    public override void UpdateArmorSet(Player player) => player.yoraiz0rDarkness = true;
}
