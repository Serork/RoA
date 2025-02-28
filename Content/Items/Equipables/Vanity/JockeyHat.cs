using Microsoft.Xna.Framework;

using RoA.Common.CustomConditions;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class JockeyHat : ModItem {
    private sealed class JockeyHatInMerchantShop : GlobalNPC {
        public override void ModifyShop(NPCShop shop) {
            if (shop.NpcType != NPCID.Merchant) {
                return;
            }

            shop.InsertAfter(ItemID.MiningHelmet, ModContent.ItemType<JockeyHat>(), RoAConditions.HasAnySaddle);
        }
    }

    public override void SetStaticDefaults(){
		//Tooltip.SetDefault("-100% running speed");
		ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 22; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Blue;
		Item.value = Item.buyPrice(gold: 2, silver: 50);
		Item.value = Item.sellPrice(silver: 50);
		Item.vanity = true;
	}
}
