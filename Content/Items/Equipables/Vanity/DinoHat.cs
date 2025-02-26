using Microsoft.Xna.Framework;

using RoA.Common.CustomConditions;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class DinoHat : ModItem {
    private sealed class DinoHatInMerchantShop : GlobalNPC {
        public override void ModifyShop(NPCShop shop) {
            if (shop.NpcType != NPCID.Merchant) {
                return;
            }

            shop.InsertAfter(ItemID.MiningHelmet, ModContent.ItemType<DinoHat>(), RoAConditions.HasAnySaddle);
        }
    }

    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Dino's Hat");
        //Tooltip.SetDefault("'Scary monsters, super creeps\nKeep me running, running scared'");
        ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
	}

	public override void SetDefaults() {
		int width = 24; int height = 22;
		Item.Size = new Vector2(width, height);

		Item.rare = ItemRarityID.Orange;
		Item.value = Item.buyPrice(gold: 2, silver: 50);
		Item.value = Item.sellPrice(silver: 50);
		Item.vanity = true;
	}
}
