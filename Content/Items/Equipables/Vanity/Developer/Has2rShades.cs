using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using RoA.Common.GlowMasks;

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
            glowMaskColor = Color.White;
        }
    }

    public override bool IsVanitySet(int head, int body, int legs)
		=> body == ModContent.ItemType<Has2rJacket>() && legs == ModContent.ItemType<Has2rPants>();

	public override void UpdateVanitySet(Player player) => player.yoraiz0rDarkness = true;

	public override void UpdateArmorSet(Player player) => player.yoraiz0rDarkness = true;
}
