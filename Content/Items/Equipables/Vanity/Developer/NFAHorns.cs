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
	}

    public override void SetDefaults() {
		int width = 20; int height = 26;
		Item.Size = new Vector2(width, height);

		Item.sellPrice(gold: 5);
		Item.rare = ItemRarityID.Cyan;

		Item.vanity = true;
	}

	public override bool IsVanitySet(int head, int body, int legs) => body == ModContent.ItemType<NFAJacket>() && legs == ModContent.ItemType<NFAPants>();

    public override void ArmorSetShadows(Player player) => player.armorEffectDrawShadowLokis = true;
}
