using Microsoft.Xna.Framework;

using RoA.Common.CustomConditions;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Legs)]
sealed class PresidentPants : ModItem {
    private sealed class PresidentPantsInMerchantShop : GlobalNPC {
        public override void ModifyShop(NPCShop shop) {
            if (shop.NpcType != NPCID.Merchant) {
                return;
            }

            shop.InsertAfter(ModContent.ItemType<PresidentJacket>(), ModContent.ItemType<PresidentPants>(), RoAConditions.Has05LuckOrMore);
        }
    }

    public override void SetStaticDefaults() {
		//DisplayName.SetDefault("President's Pants");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 30; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(gold: 3, silver: 50);
		Item.vanity = true;
	}
}
