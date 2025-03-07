using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Head)]
[AutoloadGlowMask2("_Head_Glow")]
sealed class Has2rMask : ModItem {
	public override void SetStaticDefaults() {
		ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;

		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
        if (drawPlayer.active && drawPlayer.hair == 26) {
            glowMask = VanillaGlowMaskHandler.GetID(Texture + "_Head_Glow");
            glowMaskColor = Color.White;
        }
    }

    public override void SetDefaults() {
		int width = 18; int height = 24;
		Item.Size = new Vector2(width, height);

		Item.sellPrice(gold: 5);
		Item.rare = ItemRarityID.Cyan;

		Item.vanity = true;
	}

	public override bool IsVanitySet(int head, int body, int legs) => body == ModContent.ItemType<Has2rJacket>() && legs == ModContent.ItemType<Has2rPants>();
}
