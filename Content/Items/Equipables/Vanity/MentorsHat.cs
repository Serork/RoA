using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class MentorsHat : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Mentor's Hat");
		//Tooltip.SetDefault("");
		ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 26; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(silver: 50);
		Item.vanity = true;
	}
}
