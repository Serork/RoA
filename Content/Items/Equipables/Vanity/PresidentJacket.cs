using Microsoft.Xna.Framework;

using RoA.Common.CustomConditions;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Body)]
sealed class PresidentJacket : ModItem {
    private sealed class PresidentJacketInMerchantShop : GlobalNPC {
        public override void ModifyShop(NPCShop shop) {
            if (shop.NpcType != NPCID.Merchant) {
                return;
            }

            shop.InsertAfter(ItemID.MiningHelmet, ModContent.ItemType<PresidentJacket>(), RoAConditions.Has05LuckOrMore);
        }
    }

    public override void SetStaticDefaults() {
		//DisplayName.SetDefault("President's Jacket");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 30; int height = 22;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Blue;
		Item.vanity = true;

        Item.value = Item.sellPrice(0, 4, 0, 0);
    }
}
